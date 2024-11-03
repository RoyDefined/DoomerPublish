using DoomerPublish.Utils;
using Microsoft.Extensions.Logging;

namespace DoomerPublishConsole.PublishTasks;

/// <summary>
/// This task removed all empty log files from the configured log folder.
/// </summary>
internal sealed class RemoveEmptyLogfilesTask(
	ILogger<RemoveEmptyDirectoriesTask> logger)
	: IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to remove empty log files, or no log folder was configured.
		if (!context.Configuration.RemoveEmptyLogFiles || string.IsNullOrEmpty(context.Configuration.LogOutputFolder))
		{
			return Task.CompletedTask;
		}

		var files = Directory.GetFiles(context.Configuration.LogOutputFolder);

		var deletedFileCount = 0;
		foreach (var file in files)
		{
			var fileInfo = new FileInfo(file);

			if (fileInfo.Length != 0)
			{
				continue;
			}

			fileInfo.Delete();
			deletedFileCount++;
		}

		if (deletedFileCount > 0)
		{
			this._logger.LogInformation("Removed {RemovedFileCount} empty log file(s).", deletedFileCount);
		}

		return Task.CompletedTask;
	}
}
