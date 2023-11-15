using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the default parser to parse a function or method.
/// </summary>
internal sealed class DefaultMethodParser : IAcsParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find functions, filtering public and non public, summary, name, actual definition and return type.
	/// </summary>
	private readonly Regex _functionRegex = new(@"(\/\/\s*@(?<public>(public))\s*)?(\/\/\s*@summary\s*(?<summary>(.*)))?\s*(?<definition>(function (?<returnType>([a-zA-Z]+)) (?<functionName>([\w\d]*)))\s*\((?<parameters>([\w\d,= ]*))\)\s*{)", RegexOptions.IgnoreCase);

	/// <summary>
	/// Regex to find scripts, filtering public and non public, summary, name, and actual definition.
	/// </summary>
	private readonly Regex _scriptRegex = new(@"(\/\/\s*@(?<public>(public))\s*)?(\/\/\s*@summary\s*(?<summary>(.*)))?\s*(?<definition>(script\s*""(?<scriptName>(.*))"")\s*(\((?<parameters>(.*))\))?\s*(?<modifiers>(.*))\s*{)", RegexOptions.IgnoreCase);

	public DefaultMethodParser(
		ILogger<DefaultMethodParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var functionMatchCollection = this._functionRegex.Matches(acsFile.Content);
		var scriptMatchCollection = this._scriptRegex.Matches(acsFile.Content);

		var functions = functionMatchCollection.Select(this.ParseFunctionMatch);
		var scripts = scriptMatchCollection.Select(this.ParseScriptMatch);

		acsFile.Methods = functions.Concat(scripts).ToList();
		var count = acsFile.Methods.Count;

		if (count > 0) {
			this._logger.LogDebug("Found {Count} method(s).", count);
		}
		return Task.CompletedTask;
	}

	private AcsMethod ParseScriptMatch(Match match)
	{
		try
		{
			var isPublicGroup = match.Groups.GetValueOrDefault("public");
			var isPublic = isPublicGroup?.Success ?? false;
			var definitionGroup = match.Groups.GetValueOrDefault("definition");
			if (definitionGroup == null || !definitionGroup.Success)
			{
				throw new InvalidOperationException($"Expected a method definition for \"{match.Value}\"");
			}

			var summaryGroup = match.Groups.GetValueOrDefault("summary");

			var summary = summaryGroup?.Value?.Trim();
			if (string.IsNullOrEmpty(summary))
			{
				summary = null;
			}

			var nameGroup = match.Groups.GetValueOrDefault("scriptName");
			if (nameGroup == null || !nameGroup.Success)
			{
				throw new InvalidOperationException($"Expected a script name for \"{match.Value}\"");
			}

			var parametersGroup = match.Groups.GetValueOrDefault("parameters");
			var parametersGroupValue = parametersGroup == null || !parametersGroup.Success ?
				null :
				parametersGroup.Value;
			var parameters = string.IsNullOrEmpty(parametersGroupValue) ?
				null :
				ParseMethodParameters(parametersGroupValue.Trim());

			var modifiersGroup = match.Groups.GetValueOrDefault("modifiers");
			var modifiers = modifiersGroup == null || !modifiersGroup.Success ?
				new List<string>() :
				modifiersGroup.Value
					.Split(" ")
					.Select(x => x.Trim())
					.ToList();

			return new AcsMethod()
			{
				IsPublic = isPublic,
				Type = AcsMethodType.script,
				Name = nameGroup.Value,
				Definition = definitionGroup.Value,
				Summary = summary,
				ReturnType = AcsMethodParameterType.@void,
				Modifiers = modifiers,
				Parameters = parameters,
			};
		}
		catch (Exception)
		{
			this._logger.LogError("Parsing failed for script match \"{Match}\"", match.Value);
			throw;
		}
	}

	private AcsMethod ParseFunctionMatch(Match match)
	{
		try
		{
			var isPublicGroup = match.Groups.GetValueOrDefault("public");
			var isPublic = isPublicGroup?.Success ?? false;
			var definitionGroup = match.Groups.GetValueOrDefault("definition");
			if (definitionGroup == null || !definitionGroup.Success)
			{
				throw new InvalidOperationException($"Expected a function definition for \"{match.Value}\"");
			}

			var summaryGroup = match.Groups.GetValueOrDefault("summary");

			var summary = summaryGroup?.Value?.Trim();
			if (string.IsNullOrEmpty(summary))
			{
				summary = null;
			}

			var nameGroup = match.Groups.GetValueOrDefault("functionName");
			if (nameGroup == null || !nameGroup.Success)
			{
				throw new InvalidOperationException($"Expected a function name for \"{match.Value}\"");
			}

			var returnTypeGroup = match.Groups.GetValueOrDefault("returnType");
			if (returnTypeGroup == null || !returnTypeGroup.Success)
			{
				throw new InvalidOperationException($"Expected a return type for \"{match.Value}\"");
			}

			var returnType = MethodParameterTypeFromString(returnTypeGroup!.Value);
			var parametersGroup = match.Groups.GetValueOrDefault("parameters");
			var parametersGroupValue = parametersGroup == null || !parametersGroup.Success ?
				null :
				parametersGroup.Value;
			var parameters = string.IsNullOrEmpty(parametersGroupValue) ?
				null :
				ParseMethodParameters(parametersGroupValue.Trim());

			return new AcsMethod()
			{
				IsPublic = isPublic,
				Type = AcsMethodType.function,
				Name = nameGroup.Value,
				Definition = definitionGroup.Value,
				Summary = summary,
				ReturnType = returnType,
				Modifiers = null,
				Parameters = parameters,
			};
		}
		catch (Exception)
		{
			this._logger.LogError("Parsing failed for function match \"{Match}\"", match.Value);
			throw;
		}
	}

	private static AcsMethodParameterType MethodParameterTypeFromString(string type)
	{
		return type.ToUpperInvariant() switch
		{
			"VOID" => AcsMethodParameterType.@void,
			"INT" => AcsMethodParameterType.@int,
			"STR" => AcsMethodParameterType.str,
			"BOOL" => AcsMethodParameterType.@bool,
			"FIXED" => AcsMethodParameterType.@fixed,
			"RAW" => AcsMethodParameterType.raw,
			_ => AcsMethodParameterType.special,
		};
	}

	private static List<AcsMethodParameter>? ParseMethodParameters(string parametersJoined)
	{
		// Ignore "void"
		if (parametersJoined.ToUpperInvariant() == "VOID")
		{
			return null;
		}

		// First split at the comma, and then just split at the space.
		// Then it's a matter of determing the type.
		var parameters = parametersJoined.Split(",")
			.Select(x => x.Trim());

		return parameters.Select(x =>
		{
			var parts = x.Split(" ");

			// A value larger than two is possible in BCC for optional parameters.
			// The third "part" must be the assignment.
			if (parts.Length == 4)
			{
				if (parts[3] != "=")
				{
					throw new InvalidOperationException($"Expected optional parameter for \"{x}\" but got {parts[3]}.");
				}

				parts = parts[..1];
			}

			if (parts.Length != 2)
			{
				throw new InvalidOperationException($"Expected two parts for parameter \"{x}\" but got {parts.Length}.");
			}

			return new AcsMethodParameter()
			{
				Type = MethodParameterTypeFromString(parts.ElementAt(0)),
				Name = parts.ElementAt(1),
			};
		}).ToList();
	}
}
