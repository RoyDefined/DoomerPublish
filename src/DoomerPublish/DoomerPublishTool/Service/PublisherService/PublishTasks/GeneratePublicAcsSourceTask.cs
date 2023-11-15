using DoomerPublish.Tools.Acs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task generates a public ACS source based on the context that exists for the ACS in the project.
/// </summary>
internal sealed class GeneratePublicAcsSourceTask : IPublishTask
{
	/// <summary>
	/// Header message to put as the header for the generated files.
	/// </summary>
	private readonly string _fileHeader = """
		// ----------------------------------------
		// THIS FILE IS AUTO-GENERATED.
		// Any modifications may be overwritten.
		// ----------------------------------------

		// This file has been automatically generated to maintain data/code consistency.
		// Manual changes could be lost during the next regeneration. It is advised not to modify this file directly.

		// Purpose:
		// This file serves as the base ACS file for the given library, exposing all public scripts/functions that you are allowed to use.
		// This file is generated automatically to ensure accuracy and avoid mistakes with missing/unimplemented scripts/functions.

		// Usage Guidelines:
		// - DO NOT manually edit this file unless you fully understand its purpose and structure.
		// - DO NOT manually expose public scripts/functions. There is no guarantee these scripts/functions persist in the next version.
		// - If changes are necessary, request modification of the source code and regenerate the file.
		// - Reach out to the developer for assistance with any concerns.
		""";

	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public GeneratePublicAcsSourceTask(
		ILogger<GeneratePublicAcsSourceTask> logger)
	{
		this._logger = logger;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		// User does not want an acs source generated.
		if (!context.Configuration.GeneratePublicAcsSource) {
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.CreatePublicAcsSourceForProjectAsync(projectContext, stoppingToken);
	}

	private async Task CreatePublicAcsSourceForProjectAsync(ProjectContext projectContext, CancellationToken stoppingToken)
	{
		var acsFiles = projectContext.MainAcsLibraryFiles;

		// This project has no ACS source files.
		if (acsFiles == null)
		{
			this._logger.LogInformation("Project has no ACS library files.");
			return;
		}

		foreach (var acsFile in acsFiles)
		{
			var content = this.GeneratePublicAcsForLibrary(acsFile);
			var outputFolder = Path.Join(projectContext.ProjectPath, "acs_source_public");

			var outputFile = Path.Join(outputFolder, Path.GetFileNameWithoutExtension(acsFile.Name) + ".g.acs");
			//var outputFile = Path.Join(outputFolder, Path.GetFileNameWithoutExtension(acsFile.Name) + ".g" + Path.GetExtension(acsFile.Name));

			// Save the file.
			if (!Directory.Exists(outputFolder))
			{
				_ = Directory.CreateDirectory(outputFolder);
			}
			await File.WriteAllTextAsync(outputFile, content, stoppingToken);
			this._logger.LogInformation("Created public acs source: {AcsFile}", outputFile);
		}
	}

	// TODO: Do not generate header comments if there is no generated code beneath.
	private string GeneratePublicAcsForLibrary(AcsFile file)
	{
		if (file.Library == null)
		{
			throw new InvalidOperationException("Expected the main acs file to have a library.");
		}

		var stringBuilder = new StringBuilder()
			.AppendLine(this._fileHeader);

		// Library
		var library = $"#library \"{file.Library}\"";
		_ = stringBuilder
			.AppendLine()
			.AppendLine(library);

		// Libdefines, in order of included file.
		_ = stringBuilder
			.AppendLine()
			.AppendLine("/* Libdefines */")
			.AppendLine();
		InsertLibdefines(file, stringBuilder);

		// Libdefines from enums, in order of included file.
		_ = stringBuilder
			.AppendLine()
			.AppendLine("/* Libdefines, generated from enums */")
			.AppendLine();
		InsertEnumLibdefines(file, stringBuilder);

		// Public methods.
		_ = stringBuilder
			.AppendLine()
			.AppendLine("/* Public methods */")
			.AppendLine();
		InsertPublicMethods(file, stringBuilder);

		return stringBuilder.ToString();
	}

	private static void InsertLibdefines(AcsFile file, StringBuilder stringBuilder)
	{
		// File has libdefines
		if (file.LibDefines != null)
		{
			foreach(var libDefine in file.LibDefines)
			{
				_ = stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"#libdefine {libDefine.Key} {libDefine.Value}");
			}
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			InsertLibdefines(includedFile, stringBuilder);
		}
	}

	private static void InsertEnumLibdefines(AcsFile file, StringBuilder stringBuilder)
	{
		// File has libdefines
		if (file.EnumLibdefines != null)
		{
			foreach (var libDefine in file.EnumLibdefines)
			{
				_ = stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"#libdefine {libDefine.Key.ToUpperInvariant()} {libDefine.Value}");
			}
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			InsertEnumLibdefines(includedFile, stringBuilder);
		}
	}

	private static bool ShouldChangeParameterToInt(AcsMethodParameterType parameterType)
	{
		return new[] { AcsMethodParameterType.@fixed, AcsMethodParameterType.raw, AcsMethodParameterType.special }.Contains(parameterType);
	}

	private static void InsertPublicMethods(AcsFile file, StringBuilder stringBuilder)
	{
		// File has libdefines
		if (file.Methods != null)
		{
			foreach (var method in file.Methods)
			{
				// Ignore non public
				if (!method.IsPublic)
				{
					continue;
				}

				// Specific return types (fixed, raw, special) must be converted to an integer since these are not supported by ACC.
				var methodReturnType = ShouldChangeParameterToInt(method.ReturnType) ? "int" : method.ReturnType.ToString();
				var parametersJoined = method.Parameters == null ? "void" :
					string.Join(", ", method.Parameters.Select(x =>
					{
						if (ShouldChangeParameterToInt(x.Type))
						{
							return $"int {x.Name}";
						}

						return $"{x.Type} {x.Name}";
					}));
				var modifiersJoined = method.Modifiers == null ? "" :
					string.Join(" ", method.Modifiers) + " ";
				var methodDefinition = method.Type == AcsMethodType.function ?
					$"function {methodReturnType} {method.Name}({parametersJoined}) {{}}" :
					$"script \"{method.Name}\"({parametersJoined}) {modifiersJoined}{{}}";
				
				if (method.Summary != null)
				{
					_ = stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{Environment.NewLine}// {method.Summary}");
				}
				
				_ = stringBuilder.AppendLine(methodDefinition);
			}
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			InsertPublicMethods(includedFile, stringBuilder);
		}
	}
}
