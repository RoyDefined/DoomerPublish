namespace DoomerPublish;

/// <summary>
/// Represents a class that implements a publish task.
/// </summary>
internal interface IPublishTask
{
	/// <summary>
	/// Runs the publish task.
	/// </summary>
	/// <param name="context">The current context of the publish process.</param>
	/// <param name="stoppingToken">A token that can stop the running process.</param>
	/// <returns>An awaitable task.</returns>
	Task RunAsync(PublishContext context, CancellationToken stoppingToken);
}
