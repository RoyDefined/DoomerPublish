using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

internal sealed class DefaultAcsParseService : IAcsParseService
{
	private readonly ILogger _logger;
	private readonly IServiceProvider _serviceProvider;

	private readonly Type _libraryParseTask = typeof(DefaultLibraryParser);
	private readonly List<Type> _parseTasks = new()
	{
		typeof(DefaultMethodParser),
		typeof(DefaultLibdefineParser),
		typeof(DefaultEnumParser),
		typeof(DefaultTodoParser),
		typeof(DefaultIncludeParser),
	};

	public DefaultAcsParseService(
		ILogger<DefaultAcsParseService> logger,
		IServiceProvider serviceProvider)
	{
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	public async Task ParseFileAsync(AcsFile acsFile, CancellationToken cancellationToken)
	{
		await this.ParseFileRecursiveAsync(acsFile, true, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task ParseFileRecursiveAsync(AcsFile acsFile, bool isLibrary, CancellationToken cancellationToken)
	{
		// If this is the first iteration, we are parsing the main library file.
		// This one also has the library name.
		var tasks = this._parseTasks;

		if (isLibrary)
		{
			tasks = new(tasks)
			{
				this._libraryParseTask,
			};
		}

		await this.ParseFileWithTasksAsync(acsFile, tasks, cancellationToken)
			.ConfigureAwait(false);

		if (acsFile.IncludedFiles == null)
		{
			return;
		}

		// Recursively do the same for the included files.
		foreach(var includedFile in acsFile.IncludedFiles)
		{
			await this.ParseFileRecursiveAsync(includedFile, false, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public async Task ParseFileWithTasksAsync(AcsFile acsFile, List<Type> parseTasks, CancellationToken cancellationToken)
	{
		foreach (var task in parseTasks)
		{
			var taskInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, task) as IAcsParser ??
				throw new InvalidOperationException($"Task does not implement {nameof(IAcsParser)}: {task.Name}");

			this._logger.LogInformation("Starting next parse task: {TaskName}", task.Name);

			try
			{
				await taskInstance.ParseAsync(acsFile, cancellationToken)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new Exception($"Parse task failed at task {task.Name}", ex);
			}
		}
	}
}
