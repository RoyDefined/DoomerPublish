using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DoomerPublish.Tools.Common;
using DoomerPublish.Tools.Acs;
using DoomerPublish.Service.Tools.DecorateParseService.Parse;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DecorateParseService(
	ILogger<DecorateParseService> logger,
	IServiceProvider serviceProvider)
	: IDecorateParseService
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	/// <inheritdoc cref="IServiceProvider" />
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	/// <summary>
	/// Represents the list of parse tasks that must be invoked on the file.
	/// </summary>
	private readonly List<Type> _parseTasks =
	[
		typeof(ActorParser),
		typeof(TodoParser),
		typeof(IncludeParser),
	];

	/// <inheritdoc />
	public async Task ParseFileAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		await this.ParseFileRecursiveAsync(decorateFile, cancellationToken)
			.ConfigureAwait(false);
	}


	/// <summary>
	/// Recursively parses the given <paramref name="decorateFile"/>.
	/// </summary>
	/// <param name="decorateFile">The file to parse.</param>
	/// <param name="cancellationToken">A token to cancel the ongoing process.</param>
	/// <returns>An awaitable task.</returns>
	private async Task ParseFileRecursiveAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		await this.ParseFileWithTasksAsync(decorateFile, this._parseTasks, cancellationToken)
			.ConfigureAwait(false);

		if (decorateFile.IncludedFiles == null)
		{
			return;
		}

		// Recursively do the same for the included files.
		foreach (var includedFile in decorateFile.IncludedFiles)
		{
			await this.ParseFileRecursiveAsync(includedFile, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Parses the given <paramref name="decorateFile"/>. The <paramref name="parseTasks"/> determine the tasks to invoke.
	/// </summary>
	/// <param name="decorateFile">The file to parse.</param>
	/// <param name="parseTasks">The tasks to invoke on the <paramref name="decorateFile"/>.</param>
	/// <param name="cancellationToken">A token to cancel the ongoing process.</param>
	/// <returns>An awaitable task.</returns>
	private async Task ParseFileWithTasksAsync(DecorateFile decorateFile, List<Type> parseTasks, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Starting parsing file: {FilePath}", Path.Join(decorateFile.AbsoluteFolderPath, decorateFile.Name));

		foreach (var task in parseTasks)
		{
			var taskInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, task) as IDecorateParser ??
				throw new InvalidOperationException($"Task does not implement {nameof(IDecorateParser)}: {task.Name}");

			try
			{
				await taskInstance.ParseAsync(decorateFile, cancellationToken)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new Exception($"Parse task failed at task {task.Name}", ex);
			}
		}
	}
}
