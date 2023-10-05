﻿using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using DoomerPublish.Tools.Acs;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task collects all relevant data from the ACS files found in the ACS source and puts it in the main context.
/// </summary>
internal sealed class AddAcsSourceToContextTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
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
		var rootAcsFiles = this._acsService.GetRootAcsFilesFromSource(project.ProjectPath)
			.ToArray();

		// Project has no root ACS files.
		if (!rootAcsFiles.Any())
		{
			return;
		}

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
