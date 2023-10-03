using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

internal sealed class DefaultMethodParser : IAcsParser
{
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find methods, filtering public and non public, summary, name, actual definition and return type if it's a function.
	/// </summary>
	private readonly Regex _methodRegex = new(@"(\/\/\s*@(?<public>(public))\s*)?(\/\/\s*@summary\s*(?<summary>(.*)))?\s*((?<definition>((function (?<returnType>([a-zA-Z]+)) (?<functionName>(.*))|script\s*""(?<scriptName>(.*))"")\s*\((?<parameters>(.*))\)\s*(?<modifiers>(.*))\s*{)))", RegexOptions.IgnoreCase);

	public DefaultMethodParser(
		ILogger<DefaultMethodParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var methodMatchCollection = this._methodRegex.Matches(acsFile.Content);

		var acsMethods = methodMatchCollection.Select(x =>
		{
			var isPublicGroup = x.Groups.GetValueOrDefault("public");
			var isPublic = isPublicGroup?.Success ?? false;
			var definitionGroup = x.Groups.GetValueOrDefault("definition");
			if (definitionGroup == null || !definitionGroup.Success)
			{
				throw new InvalidOperationException($"Expected a method definition for {x.Name}");
			}

			var type = definitionGroup.Value.StartsWith("function", StringComparison.OrdinalIgnoreCase) ? AcsMethodType.function :
				definitionGroup.Value.StartsWith("script", StringComparison.OrdinalIgnoreCase) ? AcsMethodType.script :
				throw new InvalidOperationException($"Expected a valid method type for {x.Name}");
			var summaryGroup = x.Groups.GetValueOrDefault("summary");

			// TODO: This has to be done because the regex seems to pass newline characters.
			var summary = summaryGroup?.Value?.Trim();
			if (string.IsNullOrEmpty(summary))
			{
				summary = null;
			}

			var nameGroup = type == AcsMethodType.function ?
				x.Groups.GetValueOrDefault("functionName") :
				x.Groups.GetValueOrDefault("scriptName");
			if (nameGroup == null || !nameGroup.Success)
			{
				throw new InvalidOperationException($"Expected a method name for {x.Name}");
			}

			var returnTypeGroup = x.Groups.GetValueOrDefault("returnType");
			if ((returnTypeGroup == null || !returnTypeGroup.Success) && type == AcsMethodType.function)
			{
				throw new InvalidOperationException($"Expected a return type for function for {x.Name}");
			}

			var returnType = type == AcsMethodType.script ? AcsMethodParameterType.@void :
				MethodParameterTypeFromString(returnTypeGroup!.Value);

			var parametersGroup = x.Groups.GetValueOrDefault("parameters");
			var parameters = parametersGroup == null || !parametersGroup.Success ?
				null : ParseMethodParameters(parametersGroup.Value);

			var modifiersGroup = x.Groups.GetValueOrDefault("modifiers");
			var modifiers = modifiersGroup == null || !modifiersGroup.Success ?
				new List<string>() :
				modifiersGroup.Value
					.Split(" ")
					.Select(x => x.Trim())
					.ToList();

			return new AcsMethod()
			{
				IsPublic = isPublic,
				Type = type,
				Name = nameGroup.Value,
				Definition = definitionGroup.Value,
				Summary = summary,
				ReturnType = returnType,
				Modifiers = modifiers,
				Parameters = parameters,
			};
		});

		acsFile.Methods = acsMethods.ToList();
		return Task.CompletedTask;
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
			if (parts.Length != 2)
			{
				throw new InvalidOperationException("Expected two parts.");
			}

			return new AcsMethodParameter()
			{
				Type = MethodParameterTypeFromString(parts.First()),
				Name = parts.Last(),
			};
		}).ToList();
	}
}
