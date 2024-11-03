using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;

namespace DoomerPublishConsole;

/// <summary>
/// Represents the main project context containing all collected data.
/// </summary>
internal sealed class ProjectContext
{
	public required string ProjectPath { get; set; }
	public required string ProjectName { get; init; }

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
