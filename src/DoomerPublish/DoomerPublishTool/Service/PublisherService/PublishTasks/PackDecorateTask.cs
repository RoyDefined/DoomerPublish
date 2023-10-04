using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task packs all the decorate code into a single file, and removes the packed files from the project.
/// </summary>
internal sealed class PackDecorateTask : IPublishTask
{
	/// <summary>
	/// Header message to put as the header for the generated files.
	/// </summary>
	private readonly string _fileHeader = """
		// ----------------------------------------
		// THIS FILE IS AUTO-GENERATED.
		// Any modifications may be overwritten.
		// ----------------------------------------

		// This file has been automatically generated to maintain data/code consistency.
		// Manual changes could be lost during the next regeneration. It is advised not to modify this file directly.

		// Purpose:
		// This file serves as the main decorate file of the project, containing all the decorate code that could be found.

		// Usage Guidelines:
		// - DO NOT manually edit this file unless you fully understand its purpose and structure.
		// - If changes are necessary, request modification of the source code and regenerate the file.
		// - Reach out to the developer for assistance with any concerns.
		""";

	/// <summary>
	/// Regex to find any sort of comment or empty line.
	/// </summary>
	private readonly Regex _commentWhitelineRegex = new(@"^(\s*\/\/|\s*\/\*[\w\d\(\)\"",.\s]*\*\/|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

	/// <summary>
	/// Regex to determine an included file.
	/// </summary>
	private readonly Regex _includeRegex = new(@"#include ""(?<file>[\w\d\/\\\.]+)""", RegexOptions.IgnoreCase);

	private readonly ILogger _logger;

	public PackDecorateTask(
		ILogger<PackDecorateTask> logger)
	{
		this._logger = logger;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		// User does not want the decorate files packed.
		if (!context.Configuration.PackDecorate)
		{
			return;
		}

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.PackDecorateForProjectAsync(projectContext, stoppingToken);
	}

	private async Task PackDecorateForProjectAsync(ProjectContext projectContext, CancellationToken stoppingToken)
	{
		var decorateFiles = projectContext.MainDecorateFiles;

		// This project has no decorate files.
		if (decorateFiles == null)
		{
			this._logger.LogInformation("Project has no known decorate files.");
			return;
		}

		var stringBuilder = new StringBuilder();
		foreach (var decorateFile in decorateFiles)
		{
			this.RecursiveProcessDecorateFile(decorateFile, stringBuilder);
		}

		// Stringbuilder has no content.
		if (stringBuilder.Length == 0)
		{
			this._logger.LogInformation("Project has no relevant code in the decorate files.");
			return;
		}

		_ = stringBuilder.Insert(0, this._fileHeader + Environment.NewLine + Environment.NewLine);

		var outputFile = Path.Join(projectContext.ProjectPath, "decorate.g.txt");

		await File.WriteAllTextAsync(outputFile, stringBuilder.ToString(), stoppingToken);
		this._logger.LogInformation("Created decorate file for {ProjectName}: {DecorateFile}", projectContext.ProjectName, outputFile);
	}

	private void RecursiveProcessDecorateFile(DecorateFile file, StringBuilder stringBuilder)
	{
		var lines = file.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		var filteredLines = this.FilterEmptyCommentedOrIncludingLines(lines)
			.Select(x => x.Trim());

		_ = stringBuilder
			.AppendJoin(Environment.NewLine, filteredLines)
			.AppendLine();

		// Delete the file
		var filePath = Path.Join(file.AbsoluteFolderPath, file.Name);
		this._logger.LogDebug("Deleting packed decorate file: {DecorateFile}", filePath);
		File.Delete(filePath);

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			this.RecursiveProcessDecorateFile(includedFile, stringBuilder);
		}
	}

	private IEnumerable<string> FilterEmptyCommentedOrIncludingLines(string[] lines)
	{
		return lines.Where(x => !this._commentWhitelineRegex.IsMatch(x) && !this._includeRegex.IsMatch(x));
	}
}
