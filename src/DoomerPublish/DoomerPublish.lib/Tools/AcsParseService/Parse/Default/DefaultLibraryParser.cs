using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

internal sealed class DefaultLibraryParser : IAcsParser
{
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find the `#library` entry in the root <see cref="AcsFile"/>.
	/// </summary>
	private readonly Regex _libraryRegex = new(@"#library ""(?<library>[a-z]+)""", RegexOptions.IgnoreCase);

	public DefaultLibraryParser(
		ILogger<DefaultLibraryParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var libraryMatch = this._libraryRegex.Match(acsFile.Content);

		var libraryGroup = libraryMatch.Groups.GetValueOrDefault("library") ??
			throw new InvalidOperationException($"Expected a `#library` entry in library file '{acsFile.AbsoluteFolderPath}'.");

		acsFile.Library = libraryGroup.Value;
		this._logger.LogDebug("Library found: {Library}", acsFile.Library);

		return Task.CompletedTask;
	}
}
