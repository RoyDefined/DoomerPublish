using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultDecorateParseService : IDecorateParseService
{
	private readonly ILogger _logger;
	private readonly IServiceProvider _serviceProvider;

	public DefaultDecorateParseService(
		ILogger<DefaultDecorateParseService> logger,
		IServiceProvider serviceProvider)
	{
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	public Task ParseFileAsync(DecorateFile acsFile, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
