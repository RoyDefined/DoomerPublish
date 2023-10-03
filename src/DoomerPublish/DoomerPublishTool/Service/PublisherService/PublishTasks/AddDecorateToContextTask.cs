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
/// This task collects all decorate content from the projects.
/// </summary>
internal sealed class AddDecorateToContextTask : IPublishTask
{
	/// <summary>
	/// Regex to find a decorate file.
	/// </summary>
	private readonly Regex _decorateRegex = new(@"decorate(\.[\w\d]+)?", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find an actor in the decorate file. Also returns a group for the actor name, what it inherited from, and the doomednum.
	/// </summary>
	private readonly Regex _actorRegex = new(@"^actor (?<actorName>[\w\d]+)(\s+: (?<inheritedFrom>[\w\d]+))?( (?<doomedNum>\d*))?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

	/// <summary>
	/// Regex to find a todo item.
	/// </summary>
	private readonly Regex _todoItemRegex = new(@"\s*\/\/\s*@todo:?\s*(?<todo>(.*))", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[\w\d\/\\\.]+)""", RegexOptions.IgnoreCase);

	private readonly ILogger _logger;

	public AddDecorateToContextTask(
		ILogger<AddDecorateToContextTask> logger)
	{
		this._logger = logger;
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
		// Get root decorate files using regex.
		var decorateFiles = Directory.GetFiles(project.ProjectPath)
			.Where(x => this._decorateRegex.IsMatch(x))
			.ToArray();

		// No decorate files.
		if (!decorateFiles.Any())
		{
			this._logger.LogInformation("Project has no root decorate files.", project.ProjectName);
			return;
		}

		await foreach(var decorateFile in this.CollectDecorateAsync(decorateFiles, project.ProjectPath, stoppingToken))
		{
			project.MainDecorateFiles ??= new();
			project.MainDecorateFiles.Add(decorateFile);
			this._logger.LogDebug("Added decorate file {DecorateFileName}", decorateFile.Name);
		}
	}

	private async IAsyncEnumerable<DecorateFile> CollectDecorateAsync(string[] decoratePaths, string baseProjectPath, [EnumeratorCancellation] CancellationToken stoppingToken)
	{
		foreach (var decoratePath in decoratePaths)
		{
			var decorateFile = await this.RecursiveCollectDecorateAsync(decoratePath, baseProjectPath, stoppingToken);
			yield return decorateFile;
		}
	}

	private async Task<DecorateFile> RecursiveCollectDecorateAsync(string decoratePath, string baseProjectPath, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Collecting {FilePath}", decoratePath);
		var content = await File.ReadAllTextAsync(decoratePath, cancellationToken);
		
		var actors = this.GetActors(content);
		var todos = this.GetTodos(content);

		var includedFilesPaths = this.GetIncludedFiles(content)
			.Select(x => Path.Join(baseProjectPath, x));

		var includedFiles = new List<DecorateFile>();
		foreach (var path in includedFilesPaths)
		{
			includedFiles.Add(await this.RecursiveCollectDecorateAsync(path, baseProjectPath, cancellationToken));
		}

		var decorateFileContext = new DecorateFile()
		{
			Name = Path.GetFileName(decoratePath),
			AbsoluteFolderPath = Path.GetDirectoryName(decoratePath)!,
			Content = content,
			IncludedFiles = includedFiles,
			Actors = actors,
			Todos = todos,
		};

		return decorateFileContext;
	}

	private List<DecorateActor> GetActors(string content)
	{
		var actorMatchCollection = this._actorRegex.Matches(content);

		return actorMatchCollection.Select(x =>
		{
			var actorNameGroup = x.Groups.GetValueOrDefault("actorName") ??
				throw new InvalidOperationException($"Expected actor name for {x.Name}");

			var inheritedFromGroup = x.Groups.GetValueOrDefault("inheritedFrom");
			var doomedNumGroup = x.Groups.GetValueOrDefault("doomedNum");

			int? doomedNum = doomedNumGroup != null && int.TryParse(doomedNumGroup.Value, out var doomedNumParsed) ? doomedNumParsed : null;

			return new DecorateActor()
			{
				Definition = x.Value,
				Name = actorNameGroup.Value,
				InheritedFrom = inheritedFromGroup?.Value,
				Doomednum = doomedNum
			};
		}).ToList();
	}

	private List<TodoItem> GetTodos(string content)
	{
		static int LineFromPos(string input, int indexPosition)
		{
			int lineNumber = 1;
			for (int i = 0; i < indexPosition; i++)
			{
				if (input[i] == '\n')
				{
					lineNumber++;
				};
			}
			return lineNumber;
		}

		var todoMatchCollection = this._todoItemRegex.Matches(content);

		return todoMatchCollection.Select(x =>
		{
			var todoGroup = x.Groups.GetValueOrDefault("todo") ??
				throw new InvalidOperationException($"Expected todo content for {x.Name}");

			return new TodoItem()
			{
				Value = todoGroup.Value,
				Line = LineFromPos(content, todoGroup.Index),
			};
		}).ToList();
	}

	private string[] GetIncludedFiles(string content)
	{
		var includedFilesMatchCollection = this._includeRegex.Matches(content);

		var fileGroupCollection = includedFilesMatchCollection.Select(x =>
			x.Groups.GetValueOrDefault("file") ??
				throw new InvalidOperationException($"Error when parsing included file {x.Name}"));

		var includedFiles = fileGroupCollection
			.Select(x => x.Value)
			.ToArray();

		return includedFiles;
	}
}
