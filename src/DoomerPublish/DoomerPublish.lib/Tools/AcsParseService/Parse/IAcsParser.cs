using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

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
