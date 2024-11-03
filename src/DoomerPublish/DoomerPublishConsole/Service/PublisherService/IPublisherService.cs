namespace DoomerPublishConsole;

/// <summary>
/// Represents a class that implements publishing a project.
/// </summary>
internal interface IPublisherService
{
	/// <summary>
	/// Publishes a project.
	/// </summary>
	/// <param name="configuration">The configuration that has been set and must be used during publishing.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the process.</param>
	/// <returns>An awaitable task that returns the result of the process.</returns>
	Task<PublisherResult> DoPublishAsync(PublisherConfiguration configuration, CancellationToken cancellationToken);
}