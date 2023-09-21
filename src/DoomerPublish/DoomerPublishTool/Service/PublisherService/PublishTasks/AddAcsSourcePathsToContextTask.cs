using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all the ACS files and the acs source file that can be found in the projects.
/// </summary>
internal sealed class AddAcsSourcePathsToContextTask : IPublishTask
{
	/// <summary>
	/// Regex to find an ACS file.
	/// </summary>
	private readonly Regex _acsFileRegex = new(@".*\.(acs|bcs)$", RegexOptions.IgnoreCase);

	private readonly ILogger _logger;

	public AddAcsSourcePathsToContextTask(
		ILogger<AddAcsSourcePathsToContextTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		// Folders first, then files. This prevents extra work.
		// It should not be possible that there is more than one source file.
		projectContext.AcsSourcePath = Directory.GetDirectories(projectContext.ProjectPath, "acs_source", SearchOption.AllDirectories)
			.SingleOrDefault();

		// Only collect files not part of the ACS source folder, and not generated.
		projectContext.AcsSourceStrayFiles =
			Directory.EnumerateFiles(projectContext.ProjectPath, "*.*", SearchOption.AllDirectories)
				.Where(x => this._acsFileRegex.IsMatch(x) &&
					!x.EndsWith(".g.acs", StringComparison.OrdinalIgnoreCase) &&
					!x.EndsWith(".g.bcs", StringComparison.OrdinalIgnoreCase) &&
					!IsDirectoryInPath(projectContext.AcsSourcePath, x))
				.ToList();

		return Task.CompletedTask;
	}

	private static bool IsDirectoryInPath(string? directoryPath, string fullPath)
	{
		// No ACS source was found.
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
