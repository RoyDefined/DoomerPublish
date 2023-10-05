using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task packs the projects into a pk3 and into the given folder.
/// </summary>
internal sealed class PackToOutputTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public PackToOutputTask(
		ILogger<PackToOutputTask> logger)
	{
		this._logger = logger;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		// User does not want to pack the projects.
		var outputFolder = context.Configuration.PackToOutputDir;
		if (string.IsNullOrEmpty(outputFolder))
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var outputFileName = projectContext.ProjectName + ".pk3";
		var outputFile = Path.GetFullPath(Path.Join(outputFolder, outputFileName));

		if (!Directory.Exists(outputFolder))
		{
			this._logger.LogDebug("Creating output directory: {OutputDirectory}.", outputFolder);
			_ = Directory.CreateDirectory(outputFolder);
		}

		if (File.Exists(outputFile))
		{
			this._logger.LogDebug("Deleting existing packet project: {PacketProjectPath}.", outputFile);
			File.Delete(outputFile);
		}

		ZipFile.CreateFromDirectory(projectContext.ProjectPath, outputFile);
		this._logger.LogInformation("Project has been packed to: {PacketProjectPath}.", outputFile);

		return Task.CompletedTask;
	}
}
