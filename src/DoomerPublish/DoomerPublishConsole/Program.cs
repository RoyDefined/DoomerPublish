using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoomerPublishConsole;
using DoomerPublish;
using System.Text;
using System.Globalization;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using Serilog.Core;
using Serilog;

#pragma warning disable CA1852

IConfiguration configuration;
IServiceProvider serviceProvider;

if (!CommandOptions.TryParse(args, out var commandOptions))
{
	return;
}

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

	var services = new ServiceCollection();
	_ = services.AddLogging(builder => builder.AddSerilog(Log.Logger));
	_ = services.AddDoomerPublish();

	serviceProvider = services.BuildServiceProvider();
}
catch (Exception ex)
{
	if (Log.Logger is Logger seriLogger)
	{
		seriLogger.Error(ex, "Error during initial builder setup.");
	}
	else
	{
		Console.Error.WriteLine($"Error during initial builder setup.\n{ex.Message}\n{ex.StackTrace}");
	}
	return;
}

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

PublisherResult publisherResult;
try
{
	logger.LogDebug("Application has started.");

	var analyzerService = serviceProvider.GetService<IPublisherService>() ??
		throw new InvalidOperationException($"Service not found: {nameof(IPublisherService)}");

	publisherResult = await analyzerService.DoPublishAsync(commandOptions, CancellationToken.None);
}
catch (Exception ex)
{
	logger.LogError(ex, "Error during application lifetime.");
	return;
}

// Display an error message if the publisher failed.
if (publisherResult != null && publisherResult.Success)
{
	logger.LogDebug("Application has finished succesfully.");
	return;
}

logger.LogWarning("Error during tool invokation.");
if (publisherResult == null)
{
	logger.LogWarning("Tool returned failure, error unknown.");
	return;
}

var lastTask = publisherResult.Context.FinishedTasks.LastOrDefault();
if (lastTask != null)
{
	logger.LogWarning("Last finished task: {LastTask}.", lastTask.Name);
}

if (publisherResult.Context.RunningTask != null)
{
	logger.LogWarning("Running task: {RunningTask}.", publisherResult.Context.RunningTask.Name);
}

// Try to get the exception.
// The inner exception is the root issue.
var exception = publisherResult.Exception;
if (exception != null)
{
	logger.LogError(exception, "Application ended with exception.");
}
