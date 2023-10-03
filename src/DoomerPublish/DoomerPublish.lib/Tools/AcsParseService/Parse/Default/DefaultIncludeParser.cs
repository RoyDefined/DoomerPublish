using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DoomerPublish.Tools.Acs;

internal sealed class DefaultIncludeParser : IAcsParser
{
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
			var filePath = Path.Join(Path.GetDirectoryName(acsFile.AbsoluteFolderPath), includedFile);
			var includedAcsFile = await AcsFile.FromPathAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			acsFile.IncludedFiles.Add(includedAcsFile);
		}
	}
}
