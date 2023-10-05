using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;

namespace DoomerPublish.Tools.Common;

/// <summary>
/// Represents the default include parser to parse file inclusions.
/// </summary>
internal sealed class DefaultIncludeParser : IAcsParser, IDecorateParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[\w\d\/\\ .]+)""", RegexOptions.IgnoreCase);

	public DefaultIncludeParser(
		ILogger<DefaultIncludeParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public async Task ParseAsync(AcsFile acsFile, CancellationToken cancellationToken)
	{
		var includedFiles = this.ParseIncludeContent(acsFile.Content)

			// Do not include zcommon.
			.Where(x => !x.StartsWith("zcommon", StringComparison.OrdinalIgnoreCase));

		acsFile.IncludedFiles = new();
		foreach(var includedFile in includedFiles)
		{
			var filePath = Path.Join(acsFile.AbsoluteFolderPath, includedFile);
			var includedAcsFile = await AcsFile.FromPathAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			this._logger.LogDebug("Found included file '{IncludedFilePath}'.", filePath);
			acsFile.IncludedFiles.Add(includedAcsFile);
		}
	}

	/// <inheritdoc />
	public async Task ParseAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		var includedFiles = this.ParseIncludeContent(decorateFile.Content);

		decorateFile.IncludedFiles = new();
		foreach (var includedFile in includedFiles)
		{
			var filePath = Path.Join(decorateFile.AbsoluteFolderPath, includedFile);
			var includedDecorateFile = await DecorateFile.FromPathAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			this._logger.LogDebug("Found included file '{IncludedFilePath}'.", filePath);
			decorateFile.IncludedFiles.Add(includedDecorateFile);
		}
	}

	/// <summary>
	/// Parses the included files that can be found in the given <paramref name="content"/>.
	/// </summary>
	/// <param name="content">The content to parse.</param>
	/// <returns>An enumerable containing the included files.</returns>
	private IEnumerable<string> ParseIncludeContent(string content)
	{
		var includedFilesMatchCollection = this._includeRegex.Matches(content);

		var fileGroupCollection = includedFilesMatchCollection.Select(x =>
			x.Groups.GetValueOrDefault("file") ??
				throw new InvalidOperationException($"Error when parsing included file {x.Name}"));

		var includedFiles = fileGroupCollection
			.Select(x => x.Value);

		return includedFiles;
	}
}
