using DoomerPublish.Tools.Common;

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
public sealed class DecorateFile : IFileContext
{
	public required string Name { get; init; }
	public required string AbsoluteFolderPath { get; init; }
	public required string Content { get; init; }
	public List<DecorateActor>? Actors { get; set; }
	public List<TodoItem>? Todos { get; set; }
	public List<DecorateFile>? IncludedFiles { get; set; }

	public IEnumerable<IFileContext>? IncludedFileContexts => this.IncludedFiles?.Cast<IFileContext>();

	internal static async Task<DecorateFile> FromPathAsync(string filePath, CancellationToken cancellationToken)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"DECORATE file was not found: {filePath}.");
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
