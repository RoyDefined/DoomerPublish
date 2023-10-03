using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

internal sealed class DefaultLibdefineParser : IAcsParser
{
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to `#libdefine` entries.
	/// </summary>
	private readonly Regex _libdefineRegex = new(@"#libdefine (?<key>[^\s]+) (?<value>.+)", RegexOptions.IgnoreCase);

	public DefaultLibdefineParser(
		ILogger<DefaultLibdefineParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var libdefineMatchCollection = this._libdefineRegex.Matches(acsFile.Content);

		var libdefinesParsed = libdefineMatchCollection.Select(x =>
			new AcsLibdefine()
			{
				Key = x.Groups.GetValueOrDefault("key")?.Value ??
					throw new InvalidOperationException($"Expected a key of libdefine for {x.Name}"),
				Value = x.Groups.GetValueOrDefault("value")?.Value ??
					throw new InvalidOperationException($"Expected a value of libdefine for {x.Name}"),
			});

		acsFile.LibDefines = libdefinesParsed.ToList();
		return Task.CompletedTask;
	}
}
