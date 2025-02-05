using System.Diagnostics;

namespace DoomerPublish.PublishTasks;

/// <summary>
/// Represents the base for every compiler.
/// </summary>
internal abstract class Compiler
{
	public const string InputFolder = "acs_source";
	public const string OutputFolder = "acs";

	/// <summary>
	/// Calls the target compiler with the given arguments.
	/// </summary>
	/// <param name="logOutput">The absolute folder path to log any textual output in.</param>
	/// <param name="errorLogOutput">The absolute folder path to log any textual output in that represents errors..</param>
	/// <param name="executable">The absolute path to the executable to call.</param>
	/// <param name="arguments">The arguments to pass into the executable.</param>
	/// <param name="cancellationToken">A cancellation token that can cancel the current operation.</param>
	/// <returns>An awaitable task.</returns>
	protected static async Task CompileAsync(string logOutput, string errorLogOutput, string executable, List<string> arguments, CancellationToken cancellationToken)
	{
		using var process = new Process();

		process.StartInfo.FileName = executable;
		process.StartInfo.Arguments = string.Join(" ", arguments);
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		_ = process.Start();

		await process.WaitForExitAsync(cancellationToken);

		await File.WriteAllTextAsync(logOutput,
			await process.StandardOutput.ReadToEndAsync(cancellationToken),
			cancellationToken);

		await File.WriteAllTextAsync(errorLogOutput,
			await process.StandardError.ReadToEndAsync(cancellationToken),
			cancellationToken);

		process.Close();
	}
}
