using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomerPublish.Utils;

public static class DirectoryUtils
{
	public static bool IsDirectoryInPath(string? directoryPath, string fullPath)
	{
		if (directoryPath == null)
		{
			return false;
		}

		// Normalize the paths to ensure consistent formatting
		directoryPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar);
		fullPath = Path.GetFullPath(fullPath);

		return fullPath.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Recursively copies all the directories and their child directories into the output folder.
	/// This will ensure all files persist, even if they are being locked by another operation.
	/// </summary>
	/// <param name="sourceFolder">The source folder to copy the files from.</param>
	/// <param name="outputFolder">The target folder to copy the files into.</param>
	public static void CopyDirectoryContents(string sourceFolder, string outputFolder)
	{
		if (!Directory.Exists(outputFolder))
		{
			_ = Directory.CreateDirectory(outputFolder);
		}

		// TODO: Look into a faster way of doing this. Especially bigger mods like Floppy Disk Mod just takes so long doing this.
		foreach (var filePath in Directory.GetFiles(sourceFolder))
		{
			var fileName = Path.GetFileName(filePath);
			var outputPath = Path.Combine(outputFolder, fileName);

			using var inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var outputFile = new FileStream(outputPath, FileMode.Create);
			var buffer = new byte[0x10000];
			int bytes;

			while ((bytes = inputFile.Read(buffer, 0, buffer.Length)) > 0)
			{
				outputFile.Write(buffer, 0, bytes);
			}
		}

		foreach (var directoryPath in Directory.GetDirectories(sourceFolder))
		{
			var outputSubFolder = Path.Combine(outputFolder, Path.GetFileName(directoryPath));
			_ = Directory.CreateDirectory(outputSubFolder);

			CopyDirectoryContents(directoryPath, outputSubFolder);
		}
	}
}
