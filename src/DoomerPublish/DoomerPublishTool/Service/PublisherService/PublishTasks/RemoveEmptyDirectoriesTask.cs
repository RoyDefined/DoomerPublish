using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removed all empty directories. Useful for the actor folder if you pack your decorate code for example.
/// </summary>
internal sealed class RemoveEmptyDirectoriesTask : IPublishTask
{
	private readonly ILogger _logger;

	public RemoveEmptyDirectoriesTask(
		ILogger<RemoveEmptyDirectoriesTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to remove empty directories.
		if (!context.Configuration.RemoveEmptyDirectories) {
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		RemoveEmptyDirectories(projectContext.ProjectPath);
		this._logger.LogInformation("Removed empty directories.", projectContext.ProjectName);

		return Task.CompletedTask;
	}

	private static void RemoveEmptyDirectories(string folder)
	{
		foreach (var directory in Directory.GetDirectories(folder))
		{
			RemoveEmptyDirectories(directory);
			if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
			{
				Directory.Delete(directory, false);
			}
		}
	}
}
