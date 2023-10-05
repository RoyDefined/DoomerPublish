using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the default include parser to parse file inclusions.
/// </summary>
internal sealed class DefaultIncludeParser : IAcsParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[^\s]+.(acs|bcs))""", RegexOptions.IgnoreCase);

	public DefaultIncludeParser(
		ILogger<DefaultIncludeParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public async Task ParseAsync(AcsFile acsFile, CancellationToken cancellationToken)
	{
		var includedFilesMatchCollection = this._includeRegex.Matches(acsFile.Content);

		var fileGroupCollection = includedFilesMatchCollection.Select(x =>
			x.Groups.GetValueOrDefault("file") ??
				throw new InvalidOperationException($"Error when parsing included file {x.Name}"));

		var includedFiles = fileGroupCollection
			.Select(x => x.Value)

			// Do not include zcommon.
			.Where(x => !x.StartsWith("zcommon", StringComparison.OrdinalIgnoreCase));

		// Convert to AcsFile.
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
}
