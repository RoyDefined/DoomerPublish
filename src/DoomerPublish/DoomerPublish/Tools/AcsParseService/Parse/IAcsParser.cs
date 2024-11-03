namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents a class that implements a parser to parse an <see cref="AcsFile"/>.
/// </summary>
internal interface IAcsParser
{
	/// <summary>
	/// Asynchronously parses the given <paramref name="acsFile"/> using the implemented parses method.
	/// </summary>
	/// <param name="acsFile">The acs file containing relevant context for the parser.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An awaitable task. The parser will update the <paramref name="acsFile"/> with more content after finishing.</returns>
	Task ParseAsync(AcsFile acsFile, CancellationToken cancellationToken);
}
