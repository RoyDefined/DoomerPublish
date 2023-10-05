using Microsoft.Extensions.Logging;
using System.Threading;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removes the ACS source from all the projects.
/// </summary>
internal sealed class CompileTask : IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	public CompileTask(
		ILogger<CompileTask> logger)
	{
		this._logger = logger;
	}

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested) {
			return;
		}

		var compilerType = context.Configuration.CompileWithParsed;

		// User does not want to compile.
		if (compilerType == CompileType.Unknown) {
			return;
		}

		// Make sure we have atleast one compiler to compile with, and ensure the root folder is known.
		if (compilerType != CompileType.Unknown &&
			string.IsNullOrEmpty(context.Configuration.CompilersRootDirectory))
		{
			throw new InvalidOperationException("Compilation requested, but the compiler root directory is not set.");
		}

		// Compile based on the compile type.
		Task? task = null;
		switch(compilerType)
		{
			case CompileType.Acc:
				task = AccCompile.CompileAsync(this._logger, context, stoppingToken);
				break;

			case CompileType.Bcc:
				task = BccCompile.CompileAsync(this._logger, context, stoppingToken);
				break;

			case CompileType.GdccAcc:
			case CompileType.GdccC:
				task = GdccCompile.CompileAsync(this._logger, context, compilerType, stoppingToken);
				break;
		}

		try
		{
			if (task == null)
			{
				throw new InvalidOperationException("Expected a compilation task.");
			}

			await task;
		}
		catch (Exception ex)
		{
			this._logger.LogError(ex, "Exception during compilation.");
			throw;
		}
	}
}
