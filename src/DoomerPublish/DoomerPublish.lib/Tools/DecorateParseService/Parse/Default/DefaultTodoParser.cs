using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultTodoParser : IDecorateParser
{
	private readonly ILogger _logger;

	public DefaultTodoParser(
		ILogger<DefaultTodoParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(DecorateFile decorateFile, CancellationToken _)
	{
		throw new NotImplementedException();
	}
}
