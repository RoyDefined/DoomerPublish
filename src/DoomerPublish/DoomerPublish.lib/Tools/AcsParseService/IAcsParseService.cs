namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents a class that implements parsing an <see cref="AcsFile"/>.
/// </summary>
public interface IAcsParseService
{
	/// <summary>
	/// Asynchronously parses the given <paramref name="acsFile"/>'s content.
	/// </summary>
	/// <param name="acsFile">The <see cref="AcsFile"/> containing the relevant content to parse.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An awaitable task. The <paramref name="acsFile"/> will be filled with information when the task is completed.</returns>
	Task ParseFileAsync(AcsFile acsFile, CancellationToken cancellationToken);
}
