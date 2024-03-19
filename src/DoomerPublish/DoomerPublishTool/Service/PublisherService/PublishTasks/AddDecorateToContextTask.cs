using DoomerPublish.Tools;
using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using DoomerPublish.Tools.Common;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all decorate content from the projects.
/// </summary>
internal sealed class AddDecorateToContextTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;
	private readonly IDecorateService _decorateService;
	private readonly IDecorateParseService _decorateParseService;

	public AddDecorateToContextTask(
		ILogger<AddDecorateToContextTask> logger,
		IDecorateService decorateService,
		IDecorateParseService decorateParseService)
	{
		this._logger = logger;
		this._decorateService = decorateService;
		this._decorateParseService = decorateParseService;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested) {
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.CollectDecorateForProjectAsync(projectContext, stoppingToken);
	}

	private async Task CollectDecorateForProjectAsync(ProjectContext project, CancellationToken stoppingToken)
	{
		var rootDecorateFiles = this._decorateService.GetRootDecorateFiles(project.ProjectPath)
			.ToArray();

		// No decorate files.
		if (rootDecorateFiles.Length == 0)
		{
			this._logger.LogInformation("Project {ProjectName} has no root decorate files.", project.ProjectName);
			return;
		}

		await foreach(var decorateFile in this.CollectDecorateAsync(rootDecorateFiles, stoppingToken))
		{
			project.MainDecorateFiles ??= new();
			project.MainDecorateFiles.Add(decorateFile);
			this._logger.LogDebug("Added decorate file {DecorateFileName}", decorateFile.Name);
		}
	}

	private async IAsyncEnumerable<DecorateFile> CollectDecorateAsync(string[] decoratePaths, [EnumeratorCancellation] CancellationToken stoppingToken)
	{
		// TODO: Look into doing this parallel.
		foreach (var decoratePath in decoratePaths)
		{
			var decorateFile = await DecorateFile.FromPathAsync(decoratePath, stoppingToken);
			await this._decorateParseService.ParseFileAsync(decorateFile, stoppingToken);

			yield return decorateFile;
		}
	}
}
