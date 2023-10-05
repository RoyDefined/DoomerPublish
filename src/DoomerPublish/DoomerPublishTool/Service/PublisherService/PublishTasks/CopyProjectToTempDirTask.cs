using DoomerPublish.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task copies over the project into a temporary directory, and uses that directory for concurrent tasks.<br/>
/// This ensures content is not deleted in the original project.
/// </summary>
internal sealed class CopyProjectToTempDirTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
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

		this._logger.LogDebug("Start copy '{FolderPath}' to temporary directory.", projectContext.ProjectPath);

		DirectoryUtils.CopyDirectoryContents(projectContext.ProjectPath, output);

		// The project path must be changed.
		projectContext.ProjectPath = output;
		this._logger.LogInformation("Copied project over to {TempDirLocation}.", output);

		return Task.CompletedTask;
	}
}
