using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomerPublish;

/// <summary>
/// Represents the type of method being defined.
/// </summary>
internal enum AcsMethodType
{
	function, script
}

/// <summary>
/// Represents a parameter type.
/// </summary>
// TODO: Remove fixed since it's not a valid type.
internal enum AcsMethodParameterType
{
	@void, @int, @bool, str, @fixed, special
}

/// <summary>
/// Represents a `#libdefine`.
/// </summary>
internal sealed class AcsLibdefine
{
	public required string Key { get; init; }
	public required string Value { get; init; }
}

/// <summary>
/// Represents a function or script definition.
/// </summary>
internal sealed class AcsMethod
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
internal sealed class AcsMethodParameter
{
	public required AcsMethodParameterType Type { get; init; }
	public required string Name { get; init; }
}

/// <summary>
/// Represents a todo item.
/// </summary>
internal sealed class TodoItem
{
	public required string Value { get; init; }
	public required int Line { get; init; }
}

/// <summary>
/// Represents the contents of an ACS file.
/// </summary>
internal sealed class AcsFile
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
