using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DecorateService : IDecorateService
{
	/// <summary>
	/// Regex to find a decorate file. This accepts "decorate", but also "decorate.*" with any prefix since this is a valid file.
	/// </summary>
	private readonly Regex _decorateRegex = new(@"decorate(\.[\w\d]+)?", RegexOptions.IgnoreCase);

	/// <inheritdoc />
	public IEnumerable<string> GetRootDecorateFiles(string projectFolderPath)
	{
		return Directory.EnumerateFiles(projectFolderPath, "*.*", SearchOption.TopDirectoryOnly)
			.Where(x => this._decorateRegex.IsMatch(x) &&

				// Do not include "decorate.g.txt" as this is a generated file.
				!x.Equals("decorate.g.acs", StringComparison.OrdinalIgnoreCase));
	}
}
