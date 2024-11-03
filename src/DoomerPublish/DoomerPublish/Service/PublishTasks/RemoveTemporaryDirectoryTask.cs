using Microsoft.Extensions.Logging;

namespace DoomerPublishConsole.PublishTasks;

/// <summary>
/// This task removes the temporary directory that is created.
/// </summary>
internal sealed class RemoveTemporaryDirectoryTask(
	ILogger<RemoveTemporaryDirectoryTask> logger)
	: IPublishTask
{
	private readonly ILogger _logger = logger;

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// The user did not create a temporary directory.
		if (!context.Configuration.CreateTemporaryProject)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		// The project path will have been modified to be the temporary directory.
		// This small check ensures it's not the same as the initial input directory.
		// Should never happen, but better safe than sorry.
		if (string.Equals(context.Configuration.InputProjectDir, projectContext.ProjectPath, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Expected the configured input directory to differ from the current input directory.");
		}

		this._logger.LogInformation("Deleting temporary folder {FolderPath}.", projectContext.ProjectPath);
		Directory.Delete(projectContext.ProjectPath, true);

		return Task.CompletedTask;
	}
}
