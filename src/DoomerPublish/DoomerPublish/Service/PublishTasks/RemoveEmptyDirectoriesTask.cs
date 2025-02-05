using DoomerPublish.Utils;
using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removed all empty directories. Useful for the actor folder if you pack your decorate code for example.
/// </summary>
internal sealed class RemoveEmptyDirectoriesTask(
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

		// User does not want to remove empty directories.
		if (!context.Configuration.RemoveEmptyDirectories)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		DirectoryUtils.RemoveEmptyDirectories(projectContext.ProjectPath);
		this._logger.LogInformation("Removed empty directories.");

		return Task.CompletedTask;
	}
}
