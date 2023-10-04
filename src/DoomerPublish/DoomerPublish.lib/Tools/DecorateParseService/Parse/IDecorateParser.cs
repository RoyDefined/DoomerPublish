namespace DoomerPublish.Tools.Decorate;

internal interface IDecorateParser
{
	/// <summary>
	/// Asynchronously parses the given <paramref name="decorateFile"/> using the implemented parses method.
	/// </summary>
	/// <param name="decorateFile">The DECORATE file containing relevant context for the parser.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An awaitable task. The parser will update the <paramref name="decorateFile"/> with more content after finishing.</returns>
	Task ParseAsync(DecorateFile decorateFile, CancellationToken cancellationToken);
}
