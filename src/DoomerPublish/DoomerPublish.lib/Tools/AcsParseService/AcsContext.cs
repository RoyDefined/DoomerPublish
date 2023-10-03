using System.Collections.ObjectModel;

namespace DoomerPublish.Tools;

/// <summary>
/// Represents the type of method being defined.
/// </summary>
public enum AcsMethodType
{
	function, script
}

/// <summary>
/// Represents a parameter type.
/// </summary>
// TODO: Remove fixed since it's not a valid type.
public enum AcsMethodParameterType
{
	@void, @int, @bool, str, @fixed, special
}

/// <summary>
/// Represents a `#libdefine`.
/// </summary>
public sealed class AcsLibdefine
{
	public required string Key { get; init; }
	public required string Value { get; init; }
}

/// <summary>
/// Represents a function or script definition.
/// </summary>
public sealed class AcsMethod
{
	public required bool IsPublic { get; init; }
	public required AcsMethodType Type { get; init; }
	public required AcsMethodParameterType ReturnType { get; init; }
	public required string Definition { get; init; }
	public required string? Summary { get; init; }
	public required string Name { get; init; }

	// TODO: turn to enum
	public required List<string>? Modifiers { get; init; }

	public required List<AcsMethodParameter>? Parameters { get; set; }
}

/// <summary>
/// Represents a parameter in a function or script.
/// </summary>
public sealed class AcsMethodParameter
{
	public required AcsMethodParameterType Type { get; init; }
	public required string Name { get; init; }
}

/// <summary>
/// Represents a todo item.
/// </summary>
public sealed class TodoItem
{
	public required string Value { get; init; }
	public required int Line { get; init; }
}

/// <summary>
/// Represents the contents of an ACS file.
/// </summary>
public sealed class AcsFile
{
	public required string Name { get; init; }
	public required string AbsoluteFolderPath { get; init; }
	public required string Content { get; init; }
	public required List<AcsLibdefine>? LibDefines { get; set; }
	public required List<AcsLibdefine>? EnumLibdefines { get; set; }
	public required List<AcsFile>? IncludedFiles { get; set; }
	public required List<AcsMethod>? Methods { get; set; }
	public required List<TodoItem>? Todos { get; set; }
	public string? Library { get; set; }
}
