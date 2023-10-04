using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

internal sealed class DefaultAcsService : IAcsService
{
	public const string AcsSourceFolderName = "acs_source";

	/// <summary>
	/// Regex to find an ACS file.
	/// </summary>
	private readonly Regex _acsFileRegex = new(@".*\.(acs|bcs)$", RegexOptions.IgnoreCase);

	private readonly ILogger _logger;

	public DefaultAcsService(
		ILogger<DefaultAcsService> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public IEnumerable<string> GetRootAcsFilesFromSource(string sourceFolderPath)
	{
		var acsSourceFolderPath = Path.Join(sourceFolderPath, AcsSourceFolderName);

		// No source folder found.
		if (!Directory.Exists(acsSourceFolderPath))
		{
			this._logger.LogInformation("Project contains no {AcsSourceFolderName} at '{AcsSourceFolderPath}'.", AcsSourceFolderName, acsSourceFolderPath);
			return Enumerable.Empty<string>();
		}

		return Directory.EnumerateFiles(acsSourceFolderPath, "*.*", SearchOption.TopDirectoryOnly)
			.Where(x => this._acsFileRegex.IsMatch(x) &&
				!x.EndsWith(".g.acs", StringComparison.OrdinalIgnoreCase) &&
				!x.EndsWith(".g.bcs", StringComparison.OrdinalIgnoreCase));
	}
}
