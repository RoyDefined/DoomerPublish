using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// This task removes the ACS source from all the projects.
/// </summary>
internal sealed class CompileTask(
	ILogger<CompileTask> logger)
	: IPublishTask
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	public async Task RunAsync(PublishContext context, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		var compilerType = context.Configuration.CompileWithParsed;

		// User does not want to compile.
		if (compilerType == CompileType.Unknown)
		{
			return;
		}

		// Make sure we have atleast one compiler to compile with, and ensure the root folder is known.
		if (compilerType != CompileType.Unknown &&
			string.IsNullOrEmpty(context.Configuration.CompilersRootDirectory))
		{
			throw new InvalidOperationException("Compilation requested, but the compiler root directory is not set.");
		}

		// Compile based on the compile type.
		var task = compilerType switch
		{
			CompileType.Acc => AccCompile.CompileAsync(this._logger, context, stoppingToken),
			CompileType.Bcc => BccCompile.CompileAsync(this._logger, context, stoppingToken),
			CompileType.GdccAcc or CompileType.GdccC => GdccCompile.CompileAsync(this._logger, context, compilerType, stoppingToken),
			CompileType.Unknown => throw new NotImplementedException(),
			_ => throw new UnreachableException(),
		};
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
