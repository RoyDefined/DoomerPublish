using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// Represents a task that creates a project context which will be used by the tasks.
/// </summary>
internal sealed class AddProjectContextToMainContextTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public AddProjectContextToMainContextTask(
		ILogger<AddProjectContextToMainContextTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		var input = context.Configuration.InputProjectDir;
		if (!Directory.Exists(input))
		{
			throw new InvalidOperationException($"Input directory was not found: {input}");
		}

		context.ProjectContext = new ProjectContext()
		{
			ProjectPath = Path.GetFullPath(input),
			ProjectName = new DirectoryInfo(input).Name,
		};

		this._logger.LogInformation("Project found: {ProjectName}.", context.ProjectContext.ProjectName);
		return Task.CompletedTask;
	}
}
