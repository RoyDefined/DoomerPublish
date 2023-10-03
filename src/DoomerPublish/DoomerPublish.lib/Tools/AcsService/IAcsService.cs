using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

public interface IAcsService
{
	/// <summary>
	/// Collects the ACS source from the project.
	/// </summary>
	/// <param name="projectRootPath">The project root path to search in.</param>
	/// <returns>An <see cref="AcsSourceFilesResult"/> that represents the resulting file paths.</returns>
	AcsSourceFilesResult GetAcsSourceFiles(string projectRootPath);

	/// <summary>
	/// Collects all root acs files from the source folder with the given path.
	/// </summary>
	/// <param name="sourceFolderPath">The source folder path to search.</param>
	/// <returns>En enumerator of all file paths.</returns>
	IEnumerable<string> GetRootAcsFilesFromSource(string sourceFolderPath);
}
