namespace DoomerPublish.Tools.Common;

/// <summary>
/// Represents a class that defines a file context in a project.
/// </summary>
public interface IFileContext
{
	string Name { get; }
	string AbsoluteFolderPath { get; }
	string Content { get; }

	// Specifies if this file should be stripped from the eventual output.
	bool StripFromOutput { get; set; }

	List<TodoItem>? Todos { get; set; }
	IEnumerable<IFileContext>? IncludedFileContexts { get; }
}
