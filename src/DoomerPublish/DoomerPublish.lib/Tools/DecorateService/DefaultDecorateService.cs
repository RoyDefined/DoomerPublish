using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultDecorateService : IDecorateService
{
	private readonly ILogger _logger;

	public DefaultDecorateService(
		ILogger<DefaultDecorateService> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public IEnumerable<string> GetRootDecorateFiles(string projectFolderPath)
	{
		throw new NotImplementedException();
	}
}
