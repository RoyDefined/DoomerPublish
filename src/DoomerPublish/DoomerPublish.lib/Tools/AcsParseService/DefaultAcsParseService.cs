using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools;

internal sealed class DefaultAcsParseService : IAcsParseService
{
	private readonly ILogger _logger;

	public DefaultAcsParseService(
		ILogger<DefaultAcsParseService> logger)
	{
		this._logger = logger;
	}

	public void ParseFile(AcsFile acsFile)
	{
		throw new NotImplementedException();
	}
}
