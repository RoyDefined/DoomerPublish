using DoomerPublish.Tools;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task generates a todo file for the project.
/// </summary>
internal sealed class GenerateTodoListTask : IPublishTask
{
	/// <summary>
	/// The file header that will be put in each file.
	/// </summary>
	private readonly string _fileHeader = """
		// ----------------------------------------
		// THIS FILE IS AUTO-GENERATED.
		// Any modifications may be overwritten.
		// ----------------------------------------

		// This file has been automatically generated to maintain consistency.
		// Manual changes could be lost during the next regeneration. It is advised not to modify this file directly.

		// Purpose:
		// This file serves as a collection of all todo items currently in the ACS source.
		// This file is generated automatically to ensure it is up to date with the actual todo items found in the code.

		// Usage Guidelines:
		// - DO NOT manually edit this file unless you fully understand its purpose and structure.
		// - If changes are necessary, request modification of the source code and regenerate the file.
		// - If the todo item is not specific to the ACS source, advice making a seperate todo file.
		// - Reach out to the developer for assistance with any concerns.
		""";

	private readonly ILogger _logger;

	public GenerateTodoListTask(
		ILogger<GenerateTodoListTask> logger)
	{
		this._logger = logger;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		var todoOutput = context.Configuration.GenerateTodoListAtDirectory;
		if (string.IsNullOrEmpty(todoOutput)) {
			return;
		}

		todoOutput = Path.GetFullPath(todoOutput);

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.GenerateTodoListForProjectAsync(projectContext, todoOutput, stoppingToken);
	}

	private async Task GenerateTodoListForProjectAsync(ProjectContext projectContext, string todoOutput, CancellationToken stoppingToken)
	{
		var baseLibraryFilesInContext = projectContext.MainAcsLibraryFiles;

		// This project has no files.
		if (baseLibraryFilesInContext == null)
		{
			this._logger.LogDebug("Project has no ACS library files.");
			return;
		}

		var stringBuilder = new StringBuilder();

		foreach (var file in baseLibraryFilesInContext)
		{
			InsertTodos(file, stringBuilder);
		}

		// StringBuilder has no public content available.
		if (stringBuilder.Length == 0)
		{
			return;
		}

		_ = stringBuilder.Insert(0, this._fileHeader + Environment.NewLine + Environment.NewLine);

		if (!Directory.Exists(todoOutput))
		{
			_ = Directory.CreateDirectory(todoOutput);
		}

		var outputFile = Path.Combine(todoOutput, $"todo_{projectContext.ProjectName}.g.txt");
		if (File.Exists(outputFile))
		{
			File.Delete(outputFile);
		}

		// Save the file.
		await File.WriteAllTextAsync(outputFile, stringBuilder.ToString(), stoppingToken);
		this._logger.LogInformation("Created todo list {Todo}", outputFile);
	}

	private static void InsertTodos(AcsFile file, StringBuilder stringBuilder)
	{
		// The file has todo items.
		if (file.Todos != null && file.Todos.Any())
		{
			var filePath = Path.Join(file.AbsoluteFolderPath, file.Name);
			foreach (var todo in file.Todos)
			{
				_ = stringBuilder
					.AppendLine(CultureInfo.InvariantCulture, $"// {todo.Value.Trim()}")
					.AppendLine(CultureInfo.InvariantCulture, $"// At \"{filePath}:{todo.Line}\"")
					.AppendLine();
			}
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			InsertTodos(includedFile, stringBuilder);
		}
	}
}
