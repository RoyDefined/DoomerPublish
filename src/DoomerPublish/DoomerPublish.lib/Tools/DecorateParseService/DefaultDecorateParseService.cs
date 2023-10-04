using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DoomerPublish.Tools.Decorate;
using DoomerPublish.Tools.Common;

namespace DoomerPublish.Tools.Decorate;

internal sealed class DefaultDecorateParseService : IDecorateParseService
{
	private readonly ILogger _logger;
	private readonly IServiceProvider _serviceProvider;

	private readonly List<Type> _parseTasks = new()
	{
		typeof(DefaultActorParser),
		typeof(DefaultTodoParser),
		typeof(DefaultIncludeParser),
	};

	public DefaultDecorateParseService(
		ILogger<DefaultDecorateParseService> logger,
		IServiceProvider serviceProvider)
	{
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	public async Task ParseFileAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		await this.ParseFileRecursiveAsync(decorateFile, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task ParseFileRecursiveAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
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

	public async Task ParseFileWithTasksAsync(DecorateFile decorateFile, List<Type> parseTasks, CancellationToken cancellationToken)
	{
		foreach (var task in parseTasks)
		{
			var taskInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, task) as IDecorateParser ??
				throw new InvalidOperationException($"Task does not implement {nameof(IDecorateParser)}: {task.Name}");

			this._logger.LogInformation("Starting next parse task: {TaskName}", task.Name);

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
