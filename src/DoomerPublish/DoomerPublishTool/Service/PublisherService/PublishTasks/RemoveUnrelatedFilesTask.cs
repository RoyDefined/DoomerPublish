using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removes all unrelated files from the project, which has no use and only serves as bloat or a trigger for warnings.
/// </summary>
internal sealed class RemoveUnrelatedFilesTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	private readonly List<Regex> _filesToIgnore = new()
	{
		new(@"^\.gitignore$", RegexOptions.IgnoreCase),
		new(@"^[a-z]+\.dbs?", RegexOptions.IgnoreCase),
		new(@"^[a-z]+\.wad\.backup[1-9]?", RegexOptions.IgnoreCase),
	};

	public RemoveUnrelatedFilesTask(
		ILogger<RemoveUnrelatedFilesTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to remove the unrelated files.
		if (!context.Configuration.RemoveUnrelatedFiles)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var files = Directory.GetFiles(projectContext.ProjectPath, "*", SearchOption.AllDirectories);

		// Iterate through each file path
		foreach (var filePath in files)
		{
			var file = Path.GetFileName(filePath);
			if (this._filesToIgnore.Any(x => x.IsMatch(file)))
			{
				File.Delete(filePath);
				this._logger.LogInformation("Deleted unrelated file {File}.", file);
			}
		}

		return Task.CompletedTask;
	}
}
