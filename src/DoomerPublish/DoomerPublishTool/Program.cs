using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoomerPublish;
using System.Text;
using System.Globalization;
using CommandLine;
using CommandLine.Text;

#if DEBUG
using Serilog.Debugging;
using Serilog.Core;
using Serilog;
#else
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
#endif

#pragma warning disable CA1852

CommandOptions? commandOptions = null;
ServiceCollection? services = null;
IConfiguration? configuration = null;

// First, try to parse the arguments.
try
{
	var result = Parser.Default.ParseArguments<CommandOptions>(args);
	if (result is NotParsed<CommandOptions>)
	{
		// Help was requested.
		if (result.Errors.Count() == 1 && result.Errors.Single().Tag == ErrorType.HelpRequestedError)
		{
			return;
		}

		var builder = SentenceBuilder.Create();
		var error = HelpText.RenderParsingErrorsText(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
		throw new ArgumentException(error);
	}

	commandOptions = result.Value;
}
catch (Exception ex)
{
	var stringbuilder = new StringBuilder()
		.AppendLine(CultureInfo.InvariantCulture, $"Error during command parsing.")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.Message}")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.StackTrace}");
	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
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
		.SetBasePath(Common.ExecutableDirectory)
		.AddJsonFile("appsettings.json", false, false)
#if DEBUG
		.AddJsonFile($"appsettings.Development.json", true, false)
#endif
		;

	configuration = configurationBuilder.Build();

#if DEBUG
	// Use this only for development purposes, or when there is no access to injected services. Use the injected ILogger for anything else.
	Log.Logger = new LoggerConfiguration()
		.ReadFrom.Configuration(configuration)
		.CreateLogger();
#endif

	services = new ServiceCollection();

	// Logging
#if DEBUG
	_ = services.AddLogging(builder => builder.AddSerilog(Log.Logger));
#else
	_ = services.AddLogging((configure) =>
		configure.AddSimpleConsole((configureConsole) =>
			configureConsole.ColorBehavior = LoggerColorBehavior.Disabled));
#endif

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

PublisherResult? publisherResult = null;
try
{
	await Console.Out.WriteLineAsync("Application has started.");

	var analyzerService = serviceProvider.GetService<IPublisherService>() ??
		throw new InvalidOperationException($"Service not found: {nameof(IPublisherService)}");

	publisherResult = await analyzerService.DoPublishAsync(commandOptions, CancellationToken.None);
}
catch (Exception ex)
{
	var stringbuilder = new StringBuilder()
		.AppendLine(CultureInfo.InvariantCulture, $"Error during application lifetime.")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.Message}")
		.AppendLine(CultureInfo.InvariantCulture, $"{ex.StackTrace}");
	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
	return;
}

// Display an error message if the publisher failed.
if (publisherResult?.Success == false)
{
	var stringbuilder = new StringBuilder()
		.AppendLine(CultureInfo.InvariantCulture, $"Error during publisher tool invokation.");

	if (publisherResult != null)
	{
		var lastTask = publisherResult.Context.FinishedTasks.LastOrDefault();
		if (lastTask != null)
		{
			_ = stringbuilder.AppendLine(CultureInfo.InvariantCulture, $"Last finished task: {lastTask.Name}");
		}
		if (publisherResult.Context.RunningTask != null)
		{
			_ = stringbuilder.AppendLine(CultureInfo.InvariantCulture, $"Running task: {publisherResult.Context.RunningTask.Name}");
		}
	}

	await Console.Error.WriteLineAsync(stringbuilder, CancellationToken.None);
}

await Console.Out.WriteLineAsync("Application has finished.");
