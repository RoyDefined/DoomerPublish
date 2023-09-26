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
}
