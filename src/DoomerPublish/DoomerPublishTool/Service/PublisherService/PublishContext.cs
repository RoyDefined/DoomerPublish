using DoomerPublish.Tools;

namespace DoomerPublish;

/// <summary>
/// Represents an actor found in a decorate file.
/// </summary>
internal sealed class DecorateActor
{
	public required string Definition { get; init; }
	public required string Name { get; init; }
	public required string? InheritedFrom { get; init; }
	public required int? Doomednum { get; init; }
}

/// <summary>
/// Represents the contents of a DECORATE file.
/// </summary>
internal sealed class DecorateFile
{
	public required string Name { get; init; }
	public required string AbsoluteFolderPath { get; init; }
	public required string Content { get; init; }
	public required List<DecorateActor>? Actors { get; set; }
	public required List<TodoItem>? Todos { get; set; }
	public required List<DecorateFile>? IncludedFiles { get; set; }
}

internal sealed class ProjectContext
{
	public required string ProjectPath { get; set; }
	public required string ProjectName { get; init; }

	public string? AcsSourcePath { get; set; }
	public List<string>? AcsSourceStrayFiles { get; set; }

	/// <summary>
	/// The root ACS files found in the acs_source folders.
	/// </summary>
	public List<AcsFile>? MainAcsLibraryFiles { get; set; }

	/// <summary>
	/// The root DECORATE files found in the project.
	/// </summary>
	public List<DecorateFile>? MainDecorateFiles { get; set; }
}

/// <summary>
/// Represents the context used in the publish pipeline
/// </summary>
internal sealed class PublishContext
{
	/// <summary>
	/// Represents the configuration used.
	/// </summary>
	public required PublisherConfiguration Configuration { get; init; }

	/// <summary>
	/// Represents a list of finished tasks.
	/// </summary>
	public required List<Type> FinishedTasks { get; init; }

	/// <summary>
	/// Represents the current running task.
	/// </summary>
	public Type? RunningTask { get; set; }

	/// <summary>
	/// The project's context.
	/// </summary>
	public ProjectContext? ProjectContext { get; set; }
}
