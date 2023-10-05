namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents a class that implements common ACS methods.
/// </summary>
public interface IAcsService
{
	/// <summary>
	/// Collects all root acs files from the project's ACS source folder with the given path.
	/// </summary>
	/// <param name="projectFolderPath">The project folder path to search.</param>
	/// <returns>En enumerator of all file paths.</returns>
	IEnumerable<string> GetRootAcsFilesFromSource(string projectFolderPath);
}
