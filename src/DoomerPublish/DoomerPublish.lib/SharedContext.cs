namespace DoomerPublish.Tools;

/// <summary>
/// Represents a todo item.
/// </summary>
public sealed class TodoItem
{
	public required string Value { get; init; }
	public required int Line { get; init; }
}
