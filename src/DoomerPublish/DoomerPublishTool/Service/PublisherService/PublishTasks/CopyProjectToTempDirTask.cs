using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task copies over the project into a temporary directory, and uses that directory for concurrent tasks.<br/>
/// This ensures content is not deleted in the original project.
/// </summary>
internal sealed class CopyProjectToTempDirTask : IPublishTask
{
	private readonly ILogger _logger;

	public CopyProjectToTempDirTask(
		ILogger<CopyProjectToTempDirTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User requested not to make a temporary project.
		if (!context.Configuration.CreateTemporaryProject)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var output = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

		// Delete existing folder if it exists.
		if (Directory.Exists(output))
		{
			this._logger.LogDebug("Deleting existing temporary folder {FolderName}.", output);
			Directory.Delete(output, true);
		}
		
		// Create base folder.
		_ = Directory.CreateDirectory(output);

		this._logger.LogDebug("Start copy to temporary directory.", projectContext.ProjectName);

		this.CopyDirectoryContents(projectContext.ProjectPath, output);

		// The project path must be changed.
		projectContext.ProjectPath = output;
		this._logger.LogInformation("Copied project over to {TempDirLocation}.", output);

		return Task.CompletedTask;
	}

	// Recursively copies all the directories and their child directories into the output folder.
	// This will ensure all files persist, even if they are being locked by another operation.
	private void CopyDirectoryContents(string sourceFolder, string outputFolder)
	{
		if (!Directory.Exists(outputFolder))
		{
			_ = Directory.CreateDirectory(outputFolder);
		}

		this._logger.LogDebug("Copying files from folder {FolderPath}.", sourceFolder);

		// TODO: Look into a faster way of doing this. Especially bigger mods like Floppy Disk Mod just takes so long doing this.
		foreach (var filePath in Directory.GetFiles(sourceFolder))
		{
			var fileName = Path.GetFileName(filePath);
			var outputPath = Path.Combine(outputFolder, fileName);

			using var inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var outputFile = new FileStream(outputPath, FileMode.Create);
			var buffer = new byte[0x10000];
			int bytes;

			while ((bytes = inputFile.Read(buffer, 0, buffer.Length)) > 0)
			{
				outputFile.Write(buffer, 0, bytes);
			}
		}

		foreach (var directoryPath in Directory.GetDirectories(sourceFolder))
		{
			var outputSubFolder = Path.Combine(outputFolder, Path.GetFileName(directoryPath));
			_ = Directory.CreateDirectory(outputSubFolder);

			this.CopyDirectoryContents(directoryPath, outputSubFolder);
		}
	}
}
