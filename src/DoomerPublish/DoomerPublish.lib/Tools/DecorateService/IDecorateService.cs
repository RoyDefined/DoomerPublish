namespace DoomerPublish.Tools.Decorate;

/// <summary>
/// Represents a class that implements common DECORATE methods.
/// </summary>
public interface IDecorateService
{
	/// <summary>
	/// Collects all root DECORATE files from the project folder with the given path.
	/// </summary>
	/// <param name="projectFolderPath">The project folder path to search.</param>
	/// <returns>En enumerator of all file paths.</returns>
	IEnumerable<string> GetRootDecorateFiles(string projectFolderPath);
}
