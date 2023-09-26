using DoomerPublish.Tools;
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
	private readonly ILogger _logger;
	private readonly AcsSourceFileCollectService _acsSourceFileCollectService;

	public AddAcsSourcePathsToContextTask(
		ILogger<AddAcsSourcePathsToContextTask> logger,
		AcsSourceFileCollectService acsSourceFileCollectService)
	{
		this._logger = logger;
		this._acsSourceFileCollectService = acsSourceFileCollectService;
	}

	public Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		var result = this._acsSourceFileCollectService.GetAcsSourceFiles(projectContext.ProjectPath);

		projectContext.AcsSourcePath = result.AcsSourceFolderPath;
		projectContext.AcsSourceStrayFiles = result.StrayAcsFilePaths;

		return Task.CompletedTask;
	}
}
