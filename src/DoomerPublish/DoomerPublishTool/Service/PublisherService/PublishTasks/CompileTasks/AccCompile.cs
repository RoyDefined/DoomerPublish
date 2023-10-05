using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DoomerPublish.PublishTasks;

internal sealed class AccCompile : Compiler
{
	/// <summary>
	/// Represents the target folder to find the compiler in.
	/// </summary>
	public const string FolderName = "acc";

	/// <inheritdoc />
	public static async Task CompileAsync(ILogger logger, PublishContext context, CancellationToken cancellationToken)
	{
		var compilerPath = context.Configuration.CompilersRootDirectory ??
			throw new InvalidOperationException("Expected the compiler root directory.");

		if (!Directory.Exists(compilerPath))
		{
			throw new InvalidOperationException($"Compiler root folder not found at path {compilerPath}");
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var executable = Path.Join(context.Configuration.CompilersRootDirectory, FolderName, "acc.exe");
		if (!File.Exists(executable))
		{
			throw new InvalidOperationException($"ACC executable not found at path {executable}");
		}

		var logOutput = context.Configuration.LogOutputFolder;
		if (string.IsNullOrEmpty(logOutput))
		{
			throw new InvalidOperationException("Log output is not specified.");
		}

		logOutput = Path.GetFullPath(logOutput);
		await AccCompileAsync(projectContext, executable, logOutput, logger, cancellationToken);
	}

	public static async Task AccCompileAsync(ProjectContext projectContext, string executable, string logOutputDirectory, ILogger logger, CancellationToken stoppingToken)
	{
		if (projectContext.MainAcsLibraryFiles == null || !projectContext.MainAcsLibraryFiles.Any()) {
			logger.LogInformation("Project has no ACS library files.");
			return;
		}

		logger.LogInformation("Compiling ACS using ACC compiler ({CompilerExecutable}).", executable);

		foreach (var libraryFile in projectContext.MainAcsLibraryFiles)
		{
			var fileName = Path.GetFileNameWithoutExtension(libraryFile.Name);

			var arguments = new List<string>();
			var input = Path.Join(projectContext.ProjectPath, CompileTask.InputFolder, libraryFile.Name);
			var output = Path.Join(projectContext.ProjectPath, CompileTask.OutputFolder, fileName + ".o");

			//var accOutput = Path.Join(logOutputDirectory, $"compileresult_debug_{projectContext.ProjectName}_{fileName}.txt");
			var stdOutLogFile = Path.Join(logOutputDirectory, $"compileresult_{projectContext.ProjectName}_{fileName}.txt");
			var stdErrLogFile = Path.Join(logOutputDirectory, $"compileresult_error_{projectContext.ProjectName}_{fileName}.txt");

			//arguments.Add("-d" + accOutput);
			arguments.Add(input);
			arguments.Add(output);

			await CompileAsync(stdOutLogFile, stdErrLogFile, executable, arguments, stoppingToken);
		}

		logger.LogInformation("Finished compiling ACS using ACC compiler ({CompilerExecutable}).", executable);
	}
}
