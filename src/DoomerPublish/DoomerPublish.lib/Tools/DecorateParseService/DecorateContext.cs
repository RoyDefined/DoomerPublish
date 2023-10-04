using DoomerPublish.Tools.Shared;

namespace DoomerPublish.Tools.Decorate;


/// <summary>
/// Represents an actor found in a decorate file.
/// </summary>
public sealed class DecorateActor
{
	public required string Definition { get; init; }
	public required string Name { get; init; }
	public required string? InheritedFrom { get; init; }
	public required int? Doomednum { get; init; }
}

/// <summary>
/// Represents the contents of a DECORATE file.
/// </summary>
public sealed class DecorateFile
{
	public required string Name { get; init; }
	public required string AbsoluteFolderPath { get; init; }
	public required string Content { get; init; }
	public required List<DecorateActor>? Actors { get; set; }
	public required List<TodoItem>? Todos { get; set; }
	public required List<DecorateFile>? IncludedFiles { get; set; }
}
