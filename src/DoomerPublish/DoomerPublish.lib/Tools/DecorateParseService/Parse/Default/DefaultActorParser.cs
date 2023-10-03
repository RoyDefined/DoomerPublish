using Microsoft.Extensions.Logging;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultActorParser : IDecorateParser
{
	private readonly ILogger _logger;

	public DefaultActorParser(
		ILogger<DefaultActorParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(DecorateFile decorateFile, CancellationToken _)
	{
		throw new NotImplementedException();
	}
}
