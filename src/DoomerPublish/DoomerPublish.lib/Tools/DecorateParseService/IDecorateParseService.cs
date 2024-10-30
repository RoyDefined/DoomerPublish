namespace DoomerPublish.Tools.Decorate;

/// <summary>
/// Represents a class that implements parsing an <see cref="DecorateFile"/>.
/// </summary>
public interface IDecorateParseService
{
	/// <summary>
	/// Asynchronously parses the given <paramref name="decorateFile"/>'s content.
	/// </summary>
	/// <param name="decorateFile">The <see cref="DecorateFile"/> containing the relevant content to parse.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An awaitable task. The <paramref name="decorateFile"/> will be filled with information when the task is completed.</returns>
	Task ParseFileAsync(DecorateFile decorateFile, CancellationToken cancellationToken);
}
