using DoomerPublishConsole.PublishTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DoomerPublishConsole;

/// <summary>
/// The default implementation of <see cref="IPublisherService"/>.
/// </summary>
internal sealed class DefaultPublisherService(
	ILogger<IPublisherService> logger,
	IServiceProvider serviceProvider)
	: IPublisherService
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger = logger;

	/// <inheritdoc cref="IServiceProvider"/>
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	/// <summary>
	/// This represents all the tasks in order that must be invoked.
	/// </summary>
	private readonly List<Type> _tasks =
	[
		typeof(AddProjectContextToMainContextTask),
		typeof(CopyProjectToTempDirTask),
		typeof(AddAcsSourceToContextTask),
		typeof(AddDecorateToContextTask),
		typeof(RemoveUnrelatedFilesTask),
		typeof(GeneratePublicAcsSourceTask),
		typeof(GenerateTodoListTask),
		typeof(GenerateDecorateSummaryTask),
		typeof(StripFilesTask),
		typeof(CompileTask),
		typeof(RemoveAcsSourceTask),
		typeof(PackDecorateTask),
		typeof(RemoveEmptyDirectoriesTask),
		typeof(PackToOutputTask),
		typeof(RemoveTemporaryDirectoryTask),
		typeof(RemoveEmptyLogfilesTask),
	];

	/// <inheritdoc/>
	public async Task<PublisherResult> DoPublishAsync(PublisherConfiguration configuration, CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Starting publish tool.");

		var context = new PublishContext()
		{
			Configuration = configuration,
			FinishedTasks = [],
		};

		foreach (var task in this._tasks)
		{
			var taskInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, task) as IPublishTask ??
				throw new InvalidOperationException($"Task does not implement {nameof(IPublishTask)}: {task.Name}");

			this._logger.LogInformation("Starting next task: {TaskName}", task.Name);
			context.RunningTask = task;

			try
			{
				await taskInstance.RunAsync(context, cancellationToken);
				context.FinishedTasks.Add(task);
				context.RunningTask = null;
			}
			catch (Exception ex)
			{
				return new()
				{
					Context = context,
					Exception = ex,
				};
			}
		}

		return new()
		{
			Context = context,
		};
	}
}