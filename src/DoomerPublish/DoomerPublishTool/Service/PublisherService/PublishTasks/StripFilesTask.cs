using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task strips any files that end with the given strings from the project. This includes included files included in the parent file.
/// </summary>
internal sealed class StripFilesTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public StripFilesTask(
		ILogger<StripFilesTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to strip any files.
		if (context.Configuration.Stripfiles == null || !context.Configuration.Stripfiles.Any())
		{
			return Task.CompletedTask;
		}

#pragma warning disable IDE0059
		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");
#pragma warning restore IDE0059

		throw new NotImplementedException();
	}
}
