using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Common;
using Microsoft.Extensions.Logging;

namespace DoomerPublishConsole.PublishTasks;

/// <summary>
/// This task strips any files that end with the given strings from the project. This includes included files included in the parent file.
/// </summary>
internal sealed class StripFilesTask(
	ILogger<StripFilesTask> logger)
	: IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	/// <summary>
	/// This is the list of folders that should be looked into and stripped based on the provided parameters.
	/// </summary>
	private readonly List<string> _stripFolders =
	[
		"graphics",
		"patches",
		"sprites",
		"textures",
	];

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

		this.StripFiles(projectContext, context.Configuration.Stripfiles);
		this.StripFolders(projectContext, context.Configuration.Stripfiles);
		return Task.CompletedTask;
	}

	private void StripFiles(ProjectContext projectContext, IEnumerable<string> fileSuffixes)
	{
		foreach (var stripFileExtension in fileSuffixes)
		{
			var files = projectContext.MainDecorateFiles?
				.Cast<IFileContext>()
				.Where(x => x.Name.EndsWith(stripFileExtension, StringComparison.OrdinalIgnoreCase));

			if (files == null || !files.Any())
			{
				continue;
			}

			this.RemoveFiles(files);
		}
	}

	private void RemoveFiles(IEnumerable<IFileContext> files)
	{
		foreach (var file in files)
		{
			var filePath = Path.Join(file.AbsoluteFolderPath, file.Name);
			if (!File.Exists(filePath))
			{
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

	private void StripFolders(ProjectContext projectContext, IEnumerable<string> fileSuffixes)
	{
		foreach (var folder in this._stripFolders)
		{
			// Find the target folder
			var folderPath = Path.Join(projectContext.ProjectPath, folder);
			if (!Directory.Exists(folderPath))
			{
				continue;
			}

			foreach (var stripFileExtension in fileSuffixes)
			{
				var stripFolderPath = Path.Join(folderPath, stripFileExtension);
				if (!Directory.Exists(stripFolderPath))
				{
					continue;
				}

				this._logger.LogDebug("Removing stripped folder {FolderPath}.", stripFolderPath);
				Directory.Delete(stripFolderPath, true);
			}
		}
	}
}
