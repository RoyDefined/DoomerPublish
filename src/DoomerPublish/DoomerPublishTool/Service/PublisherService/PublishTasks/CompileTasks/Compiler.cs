using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DoomerPublish.PublishTasks;

internal abstract class Compiler
{
	protected static async Task CompileAsync(string logOutput, string errorLogOutput, string executable, List<string> arguments, CancellationToken cancellationToken)
	{
		using var process = new Process();

		process.StartInfo.FileName = executable;
		process.StartInfo.Arguments = string.Join(" ", arguments);
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		_ = process.Start();

		await process.WaitForExitAsync(cancellationToken);
		await File.WriteAllTextAsync(logOutput, await process.StandardOutput.ReadToEndAsync(cancellationToken), cancellationToken);
		await File.WriteAllTextAsync(errorLogOutput, await process.StandardError.ReadToEndAsync(cancellationToken), cancellationToken);

		process.Close();
	}
}
