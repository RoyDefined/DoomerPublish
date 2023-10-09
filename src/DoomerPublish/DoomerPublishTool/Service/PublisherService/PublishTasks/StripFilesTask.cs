using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Common;
using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task strips any files that end with the given strings from the project. This includes included files included in the parent file.
/// </summary>
internal sealed class StripFilesTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public StripFilesTask(
		ILogger<StripFilesTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to strip any files.
		if (context.Configuration.Stripfiles == null || !context.Configuration.Stripfiles.Any())
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		foreach(var stripFileExtension in context.Configuration.Stripfiles)
		{
			var files = projectContext.MainDecorateFiles?
				.Cast<IFileContext>()
				.Where(x => x.Name.EndsWith(stripFileExtension, StringComparison.OrdinalIgnoreCase));

			if (files == null || !files.Any()) {
				continue;
			}

			this.RemoveFiles(files);
		}

		return Task.CompletedTask;
	}

	private void RemoveFiles(IEnumerable<IFileContext> files)
	{
		foreach(var file in files)
		{
			var filePath = Path.Join(file.AbsoluteFolderPath, file.Name);
			if (!File.Exists(filePath)) {
				throw new FileNotFoundException($"The file to delete could not be found: {filePath}", file.Name);
			}

			this._logger.LogDebug("Deleting stripped file '{FilePath}'.", filePath);
			File.Delete(filePath);
			file.StripFromOutput = true;

			if (file.IncludedFileContexts != null)
			{
				this.RemoveFiles(file.IncludedFileContexts);
			}
		}
	}
}
