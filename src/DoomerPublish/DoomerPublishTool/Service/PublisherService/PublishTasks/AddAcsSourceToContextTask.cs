using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all relevant data from the ACS files found in the ACS source and puts it in the main context.
/// </summary>
internal sealed class AddAcsSourceToContextTask : IPublishTask
{
	/// <summary>
	/// Regex to find an ACS file.
	/// </summary>
	private readonly Regex _acsFileRegex = new(@".*\.(acs|bcs)$", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to determine the library.
	/// </summary>
	private readonly Regex _libraryRegex = new(@"#library ""(?<library>[a-z]+)""", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[^\s]+.(acs|bcs))""", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find libdefines, which will also be added to the file.
	/// </summary>
	private readonly Regex _libdefineRegex = new(@"#libdefine (?<key>[^\s]+) (?<value>.+)", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find enum definitions, which will return the inner enum content.
	/// </summary>
	private readonly Regex _enumContentRegex = new(@"enum .* \s*{(?<enumContent>([\sa-z\/,.`\(\)]*))};", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find a todo item.
	/// </summary>
	private readonly Regex _todoItemRegex = new(@"\s*\/\/\s*@todo:?\s*(?<todo>(.*))", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find methods, filtering public and non public, summary, name, actual definition and return type if it's a function.
	/// </summary>
	private readonly Regex _methodRegex = new(@"(\/\/\s*@(?<public>(public))\s*)?(\/\/\s*@summary\s*(?<summary>(.*)))?\s*((?<definition>((function (?<returnType>([a-zA-Z]+)) (?<functionName>(.*))|script\s*""(?<scriptName>(.*))"")\s*\((?<parameters>(.*))\)\s*(?<modifiers>(.*))\s*{)))", RegexOptions.IgnoreCase);

	private readonly ILogger _logger;

	public AddAcsSourceToContextTask(
		ILogger<AddAcsSourceToContextTask> logger)
	{
		this._logger = logger;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested) {
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.CollectLibraryCodeForProjectAsync(projectContext, stoppingToken);
	}

	private async Task CollectLibraryCodeForProjectAsync(ProjectContext project, CancellationToken stoppingToken)
	{
		// This project has no ACS source.
		if (project.AcsSourcePath == null) {
			this._logger.LogInformation("Project has no ACS source.");
			return;
		}

		if (!Directory.Exists(project.AcsSourcePath)) {
			throw new InvalidOperationException($"ACS source not found for project {project.ProjectName}: {project.AcsSourcePath}.");
		}

		// For each folder, find all root .acs and .bcs files.
		var acsLibraryFilePaths = Directory.EnumerateFiles(project.AcsSourcePath, "*.*", SearchOption.TopDirectoryOnly)
				.Where(x => this._acsFileRegex.IsMatch(x) &&
					!x.EndsWith(".g.acs", StringComparison.OrdinalIgnoreCase) &&
					!x.EndsWith(".g.bcs", StringComparison.OrdinalIgnoreCase))
				.ToArray();
		await foreach (var libraryAcsFile in this.CollectLibraryCodeAsync(acsLibraryFilePaths, stoppingToken))
		{
			project.MainAcsLibraryFiles ??= new();
			project.MainAcsLibraryFiles.Add(libraryAcsFile);
		}
	}

	private async IAsyncEnumerable<AcsFile> CollectLibraryCodeAsync(string[] acsLibraryPaths, [EnumeratorCancellation] CancellationToken stoppingToken)
	{
		// TODO: Look into doing this parallel.
		foreach(var acsLibraryPath in acsLibraryPaths)
		{
			// Recursively collect all the ACS files and their inner information.
			var acsFile = await this.RecursiveCollectAcsAsync(acsLibraryPath, stoppingToken);

			// Get library
			var libraryMatch = this._libraryRegex.Match(acsFile.Content);

			var libraryGroup = libraryMatch.Groups.GetValueOrDefault("library") ??
				throw new InvalidOperationException($"Library not found for {acsLibraryPath}");

			acsFile.Library = libraryGroup.Value;
			this._logger.LogDebug("Library found: {Library}", acsFile.Library);

			yield return acsFile;
		}
	}

	private async Task<AcsFile> RecursiveCollectAcsAsync(string acsFilePath, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Collecting {FilePath}", acsFilePath);
		var content = await File.ReadAllTextAsync(acsFilePath, cancellationToken);

		var methods = this.GetPublicAcsMethods(content);
		var libdefines = this.GetLibdefines(content);
		var enumLibdefines = this.GetEnumLibdefines(content)
			.ToList();
		var todos = this.GetTodos(content);

		var includedFilesPaths = this.GetIncludedFiles(content)
			.Select(x => Path.Join(Path.GetDirectoryName(acsFilePath), x));

		var includedFiles = new List<AcsFile>();
		foreach(var path in includedFilesPaths)
		{
			includedFiles.Add(await this.RecursiveCollectAcsAsync(path, cancellationToken));
		}

		var acsFileContext = new AcsFile()
		{
			Name = Path.GetFileName(acsFilePath),
			AbsoluteFolderPath = Path.GetDirectoryName(acsFilePath)!,
			Content = content,
			IncludedFiles = includedFiles,
			Methods = methods,
			LibDefines = libdefines,
			EnumLibdefines = enumLibdefines,
			Todos = todos,
		};

		return acsFileContext;
	}

	private string[] GetIncludedFiles(string content)
	{
		var includedFilesMatchCollection = this._includeRegex.Matches(content);

		var fileGroupCollection = includedFilesMatchCollection.Select(x =>
			x.Groups.GetValueOrDefault("file") ??
				throw new InvalidOperationException($"Error when parsing included file {x.Name}"));

		var includedFiles = fileGroupCollection
			.Select(x => x.Value)

			// Do not include zcommon.
			.Where(x => !x.StartsWith("zcommon", StringComparison.OrdinalIgnoreCase))
			.ToArray();

		return includedFiles;
	}

	private List<AcsLibdefine> GetLibdefines(string content)
	{
		var libdefineMatchCollection = this._libdefineRegex.Matches(content);

		var libdefinesParsed = libdefineMatchCollection.Select(x =>
			new AcsLibdefine()
			{
				Key = x.Groups.GetValueOrDefault("key")?.Value ??
					throw new InvalidOperationException($"Expected a key of libdefine for {x.Name}"),
				Value = x.Groups.GetValueOrDefault("value")?.Value ??
					throw new InvalidOperationException($"Expected a value of libdefine for {x.Name}"),
			});

		return libdefinesParsed.ToList();
	}

	private IEnumerable<AcsLibdefine> GetEnumLibdefines(string content)
	{
		var enumMatchCollection = this._enumContentRegex.Matches(content);

		var filteredLines = enumMatchCollection.Select(x =>
		{
			var enumContent = x.Groups.GetValueOrDefault("enumContent")?.Value ??
				throw new InvalidOperationException($"Expected enum content for {x.Name}");

			// Get individual lines, removing whitespace and comments.
			return enumContent
				.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
				.Where(y =>
				{
					var trimmedString = y.Trim();
					return !trimmedString.StartsWith("//", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(trimmedString);
				})
				.Select(y =>
				{
					if (y.EndsWith(",", StringComparison.OrdinalIgnoreCase))
					{
						return y[..^1].Trim();
					}

					return y.Trim();
				}).ToList();
		}).ToList();

		// Now iterate everything.
		// If the enum has an equality after it, then we must continue with that number.
		foreach(var filteredCollection in filteredLines)
		{
			var value = 0;
			foreach(var line in filteredCollection)
			{
				var parts = line.Split("=");
				if (parts.Length is not 1 or 2)
				{
					throw new InvalidOperationException("Expected one or two parts for enum definition");
				}

				if (parts.Length == 2)
				{
					value = int.Parse(parts.Last(), CultureInfo.InvariantCulture);
				}

				yield return new()
				{
					Key = parts.First(),
					Value = value++.ToString(CultureInfo.InvariantCulture),
				};
			}
		}
	}

	private List<TodoItem> GetTodos(string content)
	{
		static int LineFromPos(string input, int indexPosition)
		{
			int lineNumber = 1;
			for (int i = 0; i < indexPosition; i++)
			{
				if (input[i] == '\n') {
					lineNumber++;
				};
			}
			return lineNumber;
		}

		var todoMatchCollection = this._todoItemRegex.Matches(content);

		return todoMatchCollection.Select(x =>
		{
			var todoGroup = x.Groups.GetValueOrDefault("todo") ??
				throw new InvalidOperationException($"Expected todo content for {x.Name}");

			return new TodoItem()
			{
				Value = todoGroup.Value,
				Line = LineFromPos(content, todoGroup.Index),
			};
		}).ToList();
	}

	private List<AcsMethod> GetPublicAcsMethods(string content)
	{
		var methodMatchCollection = this._methodRegex.Matches(content);

		var acsMethods = methodMatchCollection.Select(x =>
		{
			var isPublicGroup = x.Groups.GetValueOrDefault("public");
			var isPublic = isPublicGroup?.Success ?? false;
			var definitionGroup = x.Groups.GetValueOrDefault("definition");
			if (definitionGroup == null || !definitionGroup.Success)
			{
				throw new InvalidOperationException($"Expected a method definition for {x.Name}");
			}

			var type = definitionGroup.Value.StartsWith("function", StringComparison.OrdinalIgnoreCase) ? AcsMethodType.function :
				definitionGroup.Value.StartsWith("script", StringComparison.OrdinalIgnoreCase) ? AcsMethodType.script :
				throw new InvalidOperationException($"Expected a valid method type for {x.Name}");
			var summaryGroup = x.Groups.GetValueOrDefault("summary");

			// TODO: This has to be done because the regex seems to pass newline characters.
			var summary = summaryGroup?.Value?.Trim();
			if (string.IsNullOrEmpty(summary))
			{
				summary = null;
			}

			var nameGroup = type == AcsMethodType.function ?
				x.Groups.GetValueOrDefault("functionName") :
				x.Groups.GetValueOrDefault("scriptName");
			if (nameGroup == null || !nameGroup.Success)
			{
				throw new InvalidOperationException($"Expected a method name for {x.Name}");
			}

			var returnTypeGroup = x.Groups.GetValueOrDefault("returnType");
			if ((returnTypeGroup == null || !returnTypeGroup.Success) && type == AcsMethodType.function)
			{
				throw new InvalidOperationException($"Expected a return type for function for {x.Name}");
			}

			var returnType = type == AcsMethodType.script ? AcsMethodParameterType.@void :
				MethodParameterTypeFromString(returnTypeGroup!.Value);

			var parametersGroup = x.Groups.GetValueOrDefault("parameters");
			var parameters = parametersGroup == null || !parametersGroup.Success ?
				null : ParseMethodParameters(parametersGroup.Value);

			var modifiersGroup = x.Groups.GetValueOrDefault("modifiers");
			var modifiers = modifiersGroup == null || !modifiersGroup.Success ?
				new List<string>() :
				modifiersGroup.Value
					.Split(" ")
					.Select(x => x.Trim())
					.ToList();

			return new AcsMethod()
			{
				IsPublic = isPublic,
				Type = type,
				Name = nameGroup.Value,
				Definition = definitionGroup.Value,
				Summary = summary,
				ReturnType = returnType,
				Modifiers = modifiers,
				Parameters = parameters,
			};
		});
		return acsMethods.ToList();
	}

	private static AcsMethodParameterType MethodParameterTypeFromString(string type)
	{
		return type.ToUpperInvariant() switch
		{
			"VOID" => AcsMethodParameterType.@void,
			"INT" => AcsMethodParameterType.@int,
			"STR" => AcsMethodParameterType.str,
			"BOOL" => AcsMethodParameterType.@bool,
			"FIXED" => AcsMethodParameterType.@fixed,
			_ => AcsMethodParameterType.special,
		};
	}

	private static List<AcsMethodParameter>? ParseMethodParameters(string parametersJoined)
	{
		// Ignore "void"
		if (parametersJoined.ToUpperInvariant() == "VOID")
		{
			return null;
		}

		// First split at the comma, and then just split at the space.
		// Then it's a matter of determing the type.
		var parameters = parametersJoined.Split(",")
			.Select(x => x.Trim());

		return parameters.Select(x =>
		{
			var parts = x.Split(" ");
			if (parts.Length != 2)
			{
				throw new InvalidOperationException("Expected two parts.");
			}

			return new AcsMethodParameter()
			{
				Type = MethodParameterTypeFromString(parts.First()),
				Name = parts.Last(),
			};
		}).ToList();
	}
}
