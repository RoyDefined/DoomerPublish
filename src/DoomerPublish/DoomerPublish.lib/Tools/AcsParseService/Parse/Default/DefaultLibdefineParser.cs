using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the default parser to parse libdefines that exist in a file.
/// </summary>
internal sealed class DefaultLibdefineParser : IAcsParser
{
	/// <inheritdoc cref="ILogger" />
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

		var count = acsFile.LibDefines.Count;
		if (count > 0)
		{
			this._logger.LogDebug("Found {Count} libdefine(s).", count);
		}
		return Task.CompletedTask;
	}
}
