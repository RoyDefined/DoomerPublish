using DoomerPublish.Tools.Common;

namespace DoomerPublish.Tools.Acs;

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
/// Represents the contents of an ACS file.
/// </summary>
public sealed class AcsFile : IFileContext
{
	public required string Name { get; init; }
	public required string AbsoluteFolderPath { get; init; }
	public required string Content { get; init; }
	public List<AcsLibdefine>? LibDefines { get; set; }
	public List<AcsLibdefine>? EnumLibdefines { get; set; }
	public List<AcsFile>? IncludedFiles { get; set; }
	public List<AcsMethod>? Methods { get; set; }
	public List<TodoItem>? Todos { get; set; }
	public string? Library { get; set; }

	public IEnumerable<IFileContext>? IncludedFileContexts => this.IncludedFiles?.Cast<IFileContext>();

	internal static async Task<AcsFile> FromPathAsync(string filePath, CancellationToken cancellationToken)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"ACS file was not found: {filePath}.");
		}

		var directoryPath = Path.GetDirectoryName(filePath) ??
			throw new InvalidOperationException($"Expected an absolute folder path with '{filePath}'");

		return new()
		{
			Name = Path.GetFileName(filePath),
			AbsoluteFolderPath = directoryPath,
			Content = await File.ReadAllTextAsync(filePath, cancellationToken)
				.ConfigureAwait(false),
		};
	}
}
