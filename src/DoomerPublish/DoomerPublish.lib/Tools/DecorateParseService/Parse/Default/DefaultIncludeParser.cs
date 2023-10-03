using Microsoft.Extensions.Logging;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultIncludeParser : IDecorateParser
{
	private readonly ILogger _logger;

	public DefaultIncludeParser(
		ILogger<DefaultIncludeParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(DecorateFile decorateFile, CancellationToken _)
	{
		throw new NotImplementedException();
	}
}
