using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools;

public interface IAcsService
{
	/// <summary>
	/// Collects the ACS source from the project.
	/// </summary>
	/// <param name="projectRootPath">The project root path to search in.</param>
	/// <returns>An <see cref="AcsSourceFilesResult"/> that represents the resulting file paths.</returns>
	AcsSourceFilesResult GetAcsSourceFiles(string projectRootPath);
}
