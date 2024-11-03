using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace DoomerPublishConsole.PublishTasks;

/// <summary>
/// This task generates a summary of all decorate actors used.
/// </summary>
internal sealed class GenerateDecorateSummaryTask(
	ILogger<GenerateDecorateSummaryTask> logger)
	: IPublishTask
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
		// This file serves as a summary of all actors that exist in the project, summarizing all actor names, their inheritance and doomednum if available.
		// This file is generated automatically to ensure accuracy and avoid mistakes with non-existing actors.

		// Usage Guidelines:
		// - DO NOT manually edit this file unless you fully understand its purpose and structure.
		// - If changes are necessary, request modification of the source code and regenerate the file.
		// - Reach out to the developer for assistance with any concerns.
		""";

	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		// User does not want a decorate summary.
		var summaryOutput = context.Configuration.GenerateDecorateSummaryAtDirectory;
		if (string.IsNullOrEmpty(summaryOutput))
		{
			return;
		}
		summaryOutput = Path.GetFullPath(summaryOutput);

		var projectContext = context.ProjectContext ??
			throw new InvalidOperationException("Expected a project context.");

		await this.GenerateDecorateSummaryForProjectAsync(projectContext, summaryOutput, stoppingToken);
	}

	private async Task GenerateDecorateSummaryForProjectAsync(ProjectContext projectContext, string summaryOutput, CancellationToken stoppingToken)
	{
		var decorateFiles = projectContext.MainDecorateFiles;

		// This project has no files.
		if (decorateFiles == null)
		{
			this._logger.LogInformation("Project has no decorate files.");
			return;
		}

		var stringBuilder = new StringBuilder();
		var doomedNums = new List<int>();
		foreach (var file in decorateFiles)
		{
			GetDoomedNums(file, doomedNums);
		}
		InsertDecorateDoomedNums(doomedNums, stringBuilder);

		foreach (var file in decorateFiles)
		{
			InsertDecorateSummary(file, stringBuilder);
		}

		// StringBuilder has no public content available.
		if (stringBuilder.Length == 0)
		{
			return;
		}

		_ = stringBuilder.Insert(0, this._fileHeader + Environment.NewLine + Environment.NewLine);

		if (!Directory.Exists(summaryOutput))
		{
			_ = Directory.CreateDirectory(summaryOutput);
		}

		var outputFile = Path.Combine(summaryOutput, $"decorate-summary_{projectContext.ProjectName}.g.txt");
		if (File.Exists(outputFile))
		{
			this._logger.LogDebug("Deleting existing decorate summary: {OutputFile}.", outputFile);
			File.Delete(outputFile);
		}

		// Save the file.
		await File.WriteAllTextAsync(outputFile, stringBuilder.ToString(), stoppingToken);
		this._logger.LogInformation("Created decorate summary: {OutputFile}.", outputFile);
	}

	private static void GetDoomedNums(DecorateFile file, List<int> doomedNums)
	{
		if (file.Actors != null && file.Actors.Count != 0)
		{
			doomedNums.AddRange(
				file.Actors
					.Select(x => x.Doomednum)
					.Where(x => x != null && x.HasValue)
					.Select(x => x!.Value));
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			GetDoomedNums(includedFile, doomedNums);
		}
	}

	private static void InsertDecorateDoomedNums(List<int> doomedNums, StringBuilder stringBuilder)
	{
		if (doomedNums.Count == 0)
		{
			return;
		}

		var orderedDoomedNums = doomedNums.Order();

		_ = stringBuilder
			.AppendLine("// This is a full list of all known Doomednums.");

		foreach (var doomedNum in orderedDoomedNums)
		{
			_ = stringBuilder
				.Append("// ")
				.Append(doomedNum + Environment.NewLine);
		}

		_ = stringBuilder
			.AppendLine();
	}

	private static void InsertDecorateSummary(DecorateFile file, StringBuilder stringBuilder)
	{
		// The file has actors.
		if (file.Actors != null && file.Actors.Count != 0)
		{
			foreach (var actor in file.Actors)
			{
				var definitionBuilder = new StringBuilder()
					.Append("// ")
					.Append(actor.Name);

				if (!string.IsNullOrEmpty(actor.InheritedFrom))
				{
					_ = definitionBuilder.Append(CultureInfo.InvariantCulture, $": {actor.InheritedFrom}");
				}
				if (actor.Doomednum != null)
				{
					_ = definitionBuilder
						.Append(' ')
						.Append(actor.Doomednum);
				}

				_ = stringBuilder.AppendLine(definitionBuilder.ToString());
			}
		}

		// Do the same for all its included files.
		if (file.IncludedFiles == null)
		{
			return;
		}

		foreach (var includedFile in file.IncludedFiles)
		{
			InsertDecorateSummary(includedFile, stringBuilder);
		}
	}
}
