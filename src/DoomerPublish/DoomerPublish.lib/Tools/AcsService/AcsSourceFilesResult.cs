using System.Collections.ObjectModel;

namespace DoomerPublish.Tools.Acs;

public sealed class AcsSourceFilesResult
{
	public required string? AcsSourceFolderPath { get; init; }
	public required List<string> StrayAcsFilePaths { get; init; }
}
