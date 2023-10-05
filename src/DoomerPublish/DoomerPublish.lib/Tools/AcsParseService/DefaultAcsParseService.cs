using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DoomerPublish.Tools.Common;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the default service to handle the parsing of an ACS file.
/// </summary>
internal sealed class DefaultAcsParseService : IAcsParseService
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <inheritdoc cref="IServiceProvider" />
	private readonly IServiceProvider _serviceProvider;

	/// <summary>
	/// Represents the library parse task type. This is added seperately in case we parse a library file.
	/// </summary>
	private readonly Type _libraryParseTask = typeof(DefaultLibraryParser);

	/// <summary>
	/// Represents the list of parse tasks that must be invoked on the file.
	/// </summary>
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

	/// <inheritdoc />
	public async Task ParseFileAsync(AcsFile acsFile, CancellationToken cancellationToken)
	{
		await this.ParseFileRecursiveAsync(acsFile, true, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Recursively parses the given <paramref name="acsFile"/>. If <paramref name="isLibrary"/> is <see langword="true"/> the file is parsed as a library file.
	/// </summary>
	/// <param name="acsFile">The file to parse.</param>
	/// <param name="isLibrary">Determines if the file is a library file.</param>
	/// <param name="cancellationToken">A token to cancel the ongoing process.</param>
	/// <returns>An awaitable task.</returns>
	private async Task ParseFileRecursiveAsync(AcsFile acsFile, bool isLibrary, CancellationToken cancellationToken)
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

	/// <summary>
	/// Parses the given <paramref name="acsFile"/>. The <paramref name="parseTasks"/> determine the tasks to invoke.
	/// </summary>
	/// <param name="acsFile">The file to parse.</param>
	/// <param name="parseTasks">The tasks to invoke on the <paramref name="acsFile"/>.</param>
	/// <param name="cancellationToken">A token to cancel the ongoing process.</param>
	/// <returns>An awaitable task.</returns>
	private async Task ParseFileWithTasksAsync(AcsFile acsFile, List<Type> parseTasks, CancellationToken cancellationToken)
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
