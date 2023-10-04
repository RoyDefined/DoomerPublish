using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

public interface IAcsService
{
	/// <summary>
	/// Collects all root acs files from the project's ACS source folder with the given path.
	/// </summary>
	/// <param name="projectFolderPath">The project folder path to search.</param>
	/// <returns>En enumerator of all file paths.</returns>
	IEnumerable<string> GetRootAcsFilesFromSource(string projectFolderPath);
}
