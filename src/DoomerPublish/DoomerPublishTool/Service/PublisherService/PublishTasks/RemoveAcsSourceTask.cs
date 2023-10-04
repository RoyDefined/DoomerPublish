using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.Logging;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removes the ACS source from the project.
/// </summary>
internal sealed class RemoveAcsSourceTask : IPublishTask
{
	private readonly ILogger _logger;

	public RemoveAcsSourceTask(
		ILogger<RemoveAcsSourceTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to remove the acs source.
		if (!context.Configuration.RemoveAcsSource)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		if (projectContext.MainAcsLibraryFiles == null)
		{
			this._logger.LogInformation("Not removing main ACS library files. Project has none.");
			return Task.CompletedTask;
		}

		foreach (var includedFile in projectContext.MainAcsLibraryFiles)
		{
			this.DeleteAcsFileRecursive(includedFile);
		}

		return Task.CompletedTask;
	}

	public void DeleteAcsFileRecursive(AcsFile acsFile)
	{
		var filePath = Path.Join(acsFile.AbsoluteFolderPath, acsFile.Name);
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"File was not found at '{filePath}'.", acsFile.Name);
		}

		File.Delete(filePath);

		if (acsFile.IncludedFiles == null)
		{
			return;
		}

		// Recursively delete included files.
		foreach (var includedFile in acsFile.IncludedFiles)
		{
			this.DeleteAcsFileRecursive(includedFile);
		}
	}
}
