﻿using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using DoomerPublish.Tools.Acs;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all relevant data from the ACS files found in the ACS source and puts it in the main context.
/// </summary>
internal sealed class AddAcsSourceToContextTask(
	IAcsService acsService,
	IAcsParseService acsParseService)
	: IPublishTask
{
	private readonly IAcsService _acsService = acsService;
	private readonly IAcsParseService _acsParseService = acsParseService;

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.CollectLibraryCodeForProjectAsync(projectContext, stoppingToken);
	}

	private async Task CollectLibraryCodeForProjectAsync(ProjectContext project, CancellationToken stoppingToken)
	{
		var rootAcsFiles = this._acsService.GetRootAcsFilesFromSource(project.ProjectPath)
			.ToArray();

		// Project has no root ACS files.
		if (rootAcsFiles.Length == 0)
		{
			return;
		}

		// Iterate each root acs file, and add the full context to the project's context.
		await foreach (var libraryAcsFile in this.CollectLibraryCodeAsync(rootAcsFiles, stoppingToken))
		{
			project.MainAcsLibraryFiles ??= [];
			project.MainAcsLibraryFiles.Add(libraryAcsFile);
		}
	}

	private async IAsyncEnumerable<AcsFile> CollectLibraryCodeAsync(string[] acsLibraryPaths, [EnumeratorCancellation] CancellationToken stoppingToken)
	{
		// TODO: Look into doing this parallel.
		foreach (var acsLibraryPath in acsLibraryPaths)
		{
			var acsFile = await AcsFile.FromPathAsync(acsLibraryPath, stoppingToken);
			await this._acsParseService.ParseFileAsync(acsFile, stoppingToken);

			yield return acsFile;
		}
	}
}
