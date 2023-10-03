using DoomerPublish.Tools;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all relevant data from the ACS files found in the ACS source and puts it in the main context.
/// </summary>
internal sealed class AddAcsSourceToContextTask : IPublishTask
{
	private readonly ILogger _logger;
	private readonly IAcsService _acsService;
	private readonly IAcsParseService _acsParseService;

	public AddAcsSourceToContextTask(
		ILogger<AddAcsSourceToContextTask> logger,
		IAcsService acsService,
		IAcsParseService acsParseService)
	{
		this._logger = logger;
		this._acsService = acsService;
		this._acsParseService = acsParseService;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested) {
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.CollectLibraryCodeForProjectAsync(projectContext, stoppingToken);
	}

	private async Task CollectLibraryCodeForProjectAsync(ProjectContext project, CancellationToken stoppingToken)
	{
		// This project has no ACS source.
		if (project.AcsSourcePath == null) {
			this._logger.LogInformation("Project has no ACS source.");
			return;
		}

		var rootAcsFiles = this._acsService.GetRootAcsFilesFromSource(project.AcsSourcePath)
			.ToArray();

		// Iterate each root acs file, and add the full context to the project's context.
		await foreach (var libraryAcsFile in this.CollectLibraryCodeAsync(rootAcsFiles, stoppingToken))
		{
			project.MainAcsLibraryFiles ??= new();
			project.MainAcsLibraryFiles.Add(libraryAcsFile);
		}
	}

	private async IAsyncEnumerable<AcsFile> CollectLibraryCodeAsync(string[] acsLibraryPaths, [EnumeratorCancellation] CancellationToken stoppingToken)
	{
		// TODO: Look into doing this parallel.
		foreach(var acsLibraryPath in acsLibraryPaths)
		{
			var acsFile = await AcsFile.FromPathAsync(acsLibraryPath, stoppingToken);
			await this._acsParseService.ParseFileAsync(acsFile, stoppingToken);

			yield return acsFile;
		}
	}
}
