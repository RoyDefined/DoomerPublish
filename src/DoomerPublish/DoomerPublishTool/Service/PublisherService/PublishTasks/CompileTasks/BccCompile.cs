using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DoomerPublish.PublishTasks;

internal sealed class BccCompile : Compiler
{
	/// <summary>
	/// Represents the target folder to find the compiler in.
	/// </summary>
	public const string FolderName = "bcc";

	/// <summary>
	/// A regex that will match the expected executable name of the compiler. This exists because there are multiple possible names for this compiler.
	/// </summary>
	public static readonly Regex executableRegex = new(@"^(zt-)?bcc.exe$", RegexOptions.IgnoreCase);

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

		var executableFolder = Path.Join(context.Configuration.CompilersRootDirectory, FolderName);
		if (!Directory.Exists(executableFolder))
		{
			throw new InvalidOperationException($"BCC executable folder not found at path {executableFolder}");
		}

		// BCC can be either bcc or zt-bcc depending on if you have the maintained version.
		var executableName = Directory.GetFiles(executableFolder)
			.Select(x => Path.GetFileName(x))
			.FirstOrDefault(executableRegex.IsMatch) ??
			throw new InvalidOperationException($"BCC executable not found in folder {executableFolder}");

		var logOutput = context.Configuration.LogOutputFolder;
		if (string.IsNullOrEmpty(logOutput))
		{
			throw new InvalidOperationException("Log output is not specified.");
		}

		// No need to verify it exists. This was created by finding the actual file.
		var executable = Path.Join(executableFolder, executableName);

		var commmonFile = Path.Join(Path.GetFullPath(executableFolder), "lib");
		if (!Directory.Exists(commmonFile))
		{
			throw new InvalidOperationException($"zcommon.bcs folder not found at {commmonFile}.");
		}

		await BccCompileAsync(context.Configuration, projectContext, executable, commmonFile, logOutput, logger, cancellationToken);
	}

	public static async Task BccCompileAsync(PublisherConfiguration publisherConfiguration, ProjectContext projectContext, string executable, string commonFile, string logOutputDirectory, ILogger logger, CancellationToken stoppingToken)
	{
		if (projectContext.MainAcsLibraryFiles == null || !projectContext.MainAcsLibraryFiles.Any())
		{
			logger.LogInformation("Project has no ACS library files.");
			return;
		}

		logger.LogInformation("Compiling ACS using BCC compiler ({CompilerExecutable}).", executable);

		foreach (var libraryFile in projectContext.MainAcsLibraryFiles)
		{
			var fileName = Path.GetFileNameWithoutExtension(libraryFile.Name);

			var arguments = new List<string>();
			var input = Path.Join(projectContext.ProjectPath, Compiler.InputFolder, libraryFile.Name);
			var output = Path.Join(projectContext.ProjectPath, Compiler.OutputFolder, fileName + ".o");

			var stdOutLogFile = Path.Join(logOutputDirectory, $"compileresult_{projectContext.ProjectName}_{fileName}.txt");
			var stdErrLogFile = Path.Join(logOutputDirectory, $"compileresult_error_{projectContext.ProjectName}_{fileName}.txt");

			_ = commonFile;
			//arguments.Add("-acc-err");
			//arguments.Add("-acc-stats");
			//arguments.Add("-i " + commonFile);

			var defines = publisherConfiguration.BCCParsedDefines;
			if (!string.IsNullOrEmpty(defines))
			{
				arguments.Add(defines);
			}

			arguments.Add(input);
			arguments.Add(output);

			await CompileAsync(stdOutLogFile, stdErrLogFile, executable, arguments, stoppingToken);
		}

		logger.LogDebug("Finished compiling ACS using BCC compiler ({CompilerExecutable}).", executable);
	}
}
