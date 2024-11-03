using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoomerPublishConsole;
using System.Text;
using System.Globalization;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using Serilog.Core;
using Serilog;

#pragma warning disable CA1852

ServiceCollection? services = null;
IConfiguration? configuration = null;

if (!CommandOptions.TryParse(args, out var commandOptions))
{
	return;
}

#if DEBUG
// This allows Serilog to log any errors it occured during initialization.
SelfLog.Enable(Console.Error.WriteLine);
#endif

try
{
	// Set up configuration
	var configurationBuilder = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", false, false)
#if DEBUG
		.AddJsonFile($"appsettings.Development.json", true, false)
#endif
		;

	configuration = configurationBuilder.Build();

	// Use this only for development purposes, or when there is no access to injected services. Use the injected ILogger for anything else.
	Log.Logger = new LoggerConfiguration()
		.ReadFrom.Configuration(configuration)
		.CreateLogger();

	services = new ServiceCollection();
	_ = services.AddLogging(builder => builder.AddSerilog(Log.Logger));
	_ = services.AddPublisher();
}
catch (Exception ex)
{
	var stringbuilder = new StringBuilder()
		.AppendLine(CultureInfo.InvariantCulture, $"Error during initial builder setup.")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.Message}")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.StackTrace}");
	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
	return;
}

IServiceProvider? serviceProvider = null;

try
{
	serviceProvider = services.BuildServiceProvider();
}
catch (Exception ex)
{
	var stringbuilder = new StringBuilder()
		.AppendLine(CultureInfo.InvariantCulture, $"Error during application startup.")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.Message}")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.StackTrace}");
	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
	return;
}

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

PublisherResult? publisherResult = null;
try
{
	logger.LogDebug("Application has started.");

	var analyzerService = serviceProvider.GetService<IPublisherService>() ??
		throw new InvalidOperationException($"Service not found: {nameof(IPublisherService)}");

	publisherResult = await analyzerService.DoPublishAsync(commandOptions, CancellationToken.None);
}
catch (Exception ex)
{
	var stringbuilder = new StringBuilder()
		.AppendLine("Error during application lifetime.")
		.AppendLine(ex.Message)
		.AppendLine(ex.StackTrace);
	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
	return;
}

// Display an error message if the publisher failed.
if (publisherResult?.Success == false)
{
	var stringbuilder = new StringBuilder()
		.AppendLine("Error during tool invokation.");

	if (publisherResult != null)
	{
		var lastTask = publisherResult.Context.FinishedTasks.LastOrDefault();
		if (lastTask != null)
		{
			_ = stringbuilder.AppendLine("Last finished task: " + lastTask.Name);
		}
		if (publisherResult.Context.RunningTask != null)
		{
			_ = stringbuilder.AppendLine("Running task: " + publisherResult.Context.RunningTask.Name);
		}

		// Try to get the exception.
		// The inner exception is the root issue.
		var exception = publisherResult.Exception;
		if (exception != null)
		{
			_ = stringbuilder.AppendLine(CultureInfo.InvariantCulture, $"Exception: {exception.Message}");
			_ = stringbuilder.AppendLine(exception.StackTrace);

			var innerException = exception.InnerException;
			var stringPrefix = "\t";
			while (innerException != null)
			{
				_ = stringbuilder.AppendLine(CultureInfo.InvariantCulture, $"{stringPrefix}Inner Exception: {innerException.Message}");
				_ = stringbuilder.AppendLine(CultureInfo.InvariantCulture, $"{stringPrefix}{innerException.StackTrace}");

				innerException = innerException.InnerException;
				stringPrefix += "\t";
			}
		}
	}
	else
	{
		_ = stringbuilder.AppendLine("Tool returned failure, error unknown.");
	}

	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
}

logger.LogDebug("Application has finished.");
