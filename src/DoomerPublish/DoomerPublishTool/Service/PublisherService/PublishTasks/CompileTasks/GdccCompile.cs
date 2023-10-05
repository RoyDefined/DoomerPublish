using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DoomerPublish.PublishTasks;

internal sealed class GdccCompile : Compiler
{
	/// <summary>
	/// Represents the target folder to find the compiler in.
	/// </summary>
	public const string FolderName = "gdcc";

	/// <inheritdoc />
	public static async Task CompileAsync(ILogger logger, PublishContext context, CompileType compileType, CancellationToken cancellationToken)
	{
		var compilerPath = context.Configuration.CompilersRootDirectory ??
			throw new InvalidOperationException("Expected the compiler root directory.");

		if (!Directory.Exists(compilerPath))
		{
			throw new InvalidOperationException($"Compiler root folder not found at path {compilerPath}");
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var compilerExecutable = compileType switch
		{
			CompileType.GdccAcc => "gdcc-acc.exe",
			CompileType.GdccC => "gdcc-cc.exe",
			_ => throw new InvalidOperationException($"Invalid compile type for GDCC compilation: {compileType}"),
		};

		var executable = Path.Join(context.Configuration.CompilersRootDirectory, FolderName, compilerExecutable);
		if (!File.Exists(executable))
		{
			throw new InvalidOperationException($"{compilerExecutable} not found at path {executable}");
		}

		var logOutput = context.Configuration.LogOutputFolder;
		if (string.IsNullOrEmpty(logOutput))
		{
			throw new InvalidOperationException("Log output is not specified.");
		}

		await GdccCompileAsync(context.Configuration, projectContext, executable, logOutput, logger, cancellationToken);
	}

	public static async Task GdccCompileAsync(PublisherConfiguration publisherConfiguration, ProjectContext projectContext, string executable, string logOutputDirectory, ILogger logger, CancellationToken stoppingToken)
	{
		if (projectContext.MainAcsLibraryFiles == null || !projectContext.MainAcsLibraryFiles.Any())
		{
			logger.LogInformation("Project has no ACS library files.");
			return;
		}

		logger.LogInformation("Compiling ACS using GDCC compiler ({CompilerExecutable}).", executable);

		foreach (var libraryFile in projectContext.MainAcsLibraryFiles)
		{
			var fileName = Path.GetFileNameWithoutExtension(libraryFile.Name);

			var arguments = new List<string>();
			var input = Path.Join(projectContext.ProjectPath, CompileTask.InputFolder, fileName + ".acs");
			var output = Path.Join(projectContext.ProjectPath, CompileTask.OutputFolder, fileName + ".o");

			var stdOutLogFile = Path.Join(logOutputDirectory, $"compileresult_{projectContext.ProjectName}_{fileName}.txt");
			var stdErrLogFile = Path.Join(logOutputDirectory, $"compileresult_error_{projectContext.ProjectName}_{fileName}.txt");

			arguments.Add($"\"{input}\"");
			arguments.Add($"-o \"{output}\"");
			if (publisherConfiguration.NoWarnForwardReferences)
			{
				arguments.Add("--no-warn-forward-reference");
			}

			// If the engine option is defined, add it.
			// Otherwise, log a warning specifying that the engine option should be provided.
			var engine = publisherConfiguration.EngineTypeParsed;
			if (engine != EngineType.Unknown)
			{
				arguments.Add("--target-engine " + engine);
			}

			var defines = publisherConfiguration.GDCCParsedDefines;
			if (!string.IsNullOrEmpty(defines))
			{
				arguments.Add(defines);
			}

			await CompileAsync(stdOutLogFile, stdErrLogFile, executable, arguments, stoppingToken);
		}

		logger.LogDebug("Finished compiling ACS using GDCC compiler ({CompilerExecutable}).", executable);
	}
}
