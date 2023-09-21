using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removes the ACS source from the project.
/// </summary>
internal sealed class RemoveAcsSourceTask : IPublishTask
{
	private readonly ILogger _logger;

	public RemoveAcsSourceTask(
		ILogger<RemoveAcsSourceTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to remove the acs source.
		if (!context.Configuration.RemoveAcsSource)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		if (projectContext.AcsSourcePath != null)
		{
			this._logger.LogInformation("Deleting acs source: {AcsSourcePath}", projectContext.AcsSourcePath);
			Directory.Delete(projectContext.AcsSourcePath, true);
		}

		if (projectContext.AcsSourceStrayFiles != null)
		{
			foreach (var file in projectContext.AcsSourceStrayFiles)
			{
				this._logger.LogInformation("Deleting stray ACS file: {AcsFile}", file);
				File.Delete(file);
			}
		}

		return Task.CompletedTask;
	}
}
