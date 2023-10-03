using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools;

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
	public AcsSourceFilesResult GetAcsSourceFiles(string projectRootPath)
	{
		this._logger.LogDebug("Start search for '{AcsSourceFolderName}' and stray files in project '{ProjectRootPath}'", AcsSourceFolderName, projectRootPath);
		var acsSourceFolderPath = this.GetAcsSourceFolderPath(projectRootPath);
		var strayAcsFiles = this.GetStrayAcsSourceFiles(projectRootPath, acsSourceFolderPath)
			.ToList();

		return new()
		{
			AcsSourceFolderPath = acsSourceFolderPath,
			StrayAcsFilePaths = strayAcsFiles,
		};
	}

	/// <inheritdoc />
	public IEnumerable<string> GetRootAcsFilesFromSource(string sourceFolderPath)
	{
		return Directory.EnumerateFiles(sourceFolderPath, "*.*", SearchOption.TopDirectoryOnly)
			.Where(x => this._acsFileRegex.IsMatch(x) &&
				!x.EndsWith(".g.acs", StringComparison.OrdinalIgnoreCase) &&
				!x.EndsWith(".g.bcs", StringComparison.OrdinalIgnoreCase));
	}

	private string? GetAcsSourceFolderPath(string projectRootPath)
	{
		return Directory.GetDirectories(projectRootPath, AcsSourceFolderName, SearchOption.AllDirectories)
			.SingleOrDefault();
	}

	private IEnumerable<string> GetStrayAcsSourceFiles(string projectRootPath, string? acsSourceFolderPath)
	{
		if (!Path.Exists(projectRootPath))
		{
			throw new InvalidOperationException($"ACS source folder path not found: {projectRootPath}");
		}

		return Directory.EnumerateFiles(projectRootPath, "*.*", SearchOption.AllDirectories)
			.Where(x => this._acsFileRegex.IsMatch(x) &&
				!x.EndsWith(".g.acs", StringComparison.OrdinalIgnoreCase) &&
				!x.EndsWith(".g.bcs", StringComparison.OrdinalIgnoreCase) &&
				!DirectoryUtils.IsDirectoryInPath(acsSourceFolderPath, x));
	}
}
