using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultIncludeParser : IDecorateParser
{
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[\w\d\/\\\.]+)""", RegexOptions.IgnoreCase);

	public DefaultIncludeParser(
		ILogger<DefaultIncludeParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public async Task ParseAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		var includedFilesMatchCollection = this._includeRegex.Matches(decorateFile.Content);

		var fileGroupCollection = includedFilesMatchCollection.Select(x =>
			x.Groups.GetValueOrDefault("file") ??
				throw new InvalidOperationException($"Error when parsing included file {x.Name}"));

		var includedFiles = fileGroupCollection
			.Select(x => x.Value);

		// Convert to AcsFile.
		decorateFile.IncludedFiles = new();
		foreach (var includedFile in includedFiles)
		{
			var filePath = Path.Join(decorateFile.AbsoluteFolderPath, includedFile);
			var includedDecorateFile = await DecorateFile.FromPathAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			decorateFile.IncludedFiles.Add(includedDecorateFile);
		}
	}
}
