using CommandLine;
using CommandLine.Text;
using DoomerPublish;
using System.Diagnostics.CodeAnalysis;

namespace DoomerPublishConsole;

internal sealed class CommandOptions : PublisherConfiguration
{
	internal static bool TryParse(IEnumerable<string> args, [NotNullWhen(true)] out CommandOptions? options)
	{
		using var parser = new Parser(with => with.HelpWriter = null);
		var result = parser.ParseArguments<CommandOptions>(args);
		if (result is NotParsed<CommandOptions>)
		{
			// Help was requested.
			if (result.Errors.Count() == 1 && result.Errors.Single().Tag == ErrorType.HelpRequestedError)
			{
				var helpText = HelpText.AutoBuild(result, x => x, x => x);
				helpText.Heading = "";
				helpText.Copyright = "";
				Console.WriteLine(helpText);

				options = null;
				return false;
			}

			var builder = SentenceBuilder.Create();
			var error = HelpText.RenderParsingErrorsText(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
			Console.Error.WriteLine(error);

			options = null;
			return false;
		}

		options = result.Value;
		return true;
	}
}
