namespace DoomerPublish;

/// <summary>
/// Represents the result of a publish process.
/// </summary>
internal sealed class PublisherResult
{
	public required PublishContext Context { get; init; }
	public Exception? Exception { get; init; }

	public bool Success => this.Exception == null;
}