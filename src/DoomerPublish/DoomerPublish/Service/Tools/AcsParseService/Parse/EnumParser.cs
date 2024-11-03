using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the enum parser to parse enums.
/// </summary>
internal sealed class EnumParser(
	ILogger<EnumParser> logger)
	: IAcsParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	/// <summary>
	/// Regex to find enum definitions, which will return the inner enum content.
	/// </summary>
	private readonly Regex _enumContentRegex = new(@"(?<isPrivate>(private ))?enum \w*\s*(:\s*(?<enumType>(fixed|int|str)))?\s*{(?<enumContent>([\sa-z0-9_\-\/\""\<\>,.'`\(\)\=]*))};", RegexOptions.IgnoreCase);

	/// <summary>
	/// The possible line separators that can be used.
	/// </summary>
	private readonly string[] lineSeparators = ["\r\n", "\r", "\n"];

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var enumMatchCollection = this._enumContentRegex.Matches(acsFile.Content);
		acsFile.EnumLibdefines = [];

		foreach (var match in enumMatchCollection.ToList())
		{
			var isPrivate = match.Groups.GetValueOrDefault("isPrivate")?.Success ?? false;

			// Skip if private
			if (isPrivate)
			{
				continue;
			}

			// The enum is of type "integer" if nothing is specified.
			var enumType = match.Groups.GetValueOrDefault("enumType")?.Value;
			var isIntegerType = string.IsNullOrEmpty(enumType) || enumType.Equals("int", StringComparison.OrdinalIgnoreCase);

			var enumContent = match.Groups.GetValueOrDefault("enumContent")?.Value ??
				throw new InvalidOperationException($"Expected enum content for {match.Name}");

			var filteredEnumLines = this.FilterEnumContent(enumContent);
			var enumLibDefines = this.EnumerateFilteredEnumLines(filteredEnumLines, isIntegerType);
			acsFile.EnumLibdefines.AddRange(enumLibDefines);
		}

		var count = acsFile.EnumLibdefines.Count;
		if (count > 0)
		{
			this._logger.LogDebug("Found {Count} enum(s).", count);
		}
		return Task.CompletedTask;
	}

	private IEnumerable<string> FilterEnumContent(string content)
	{
		var lines = content.Split(this.lineSeparators, StringSplitOptions.None);
		var linesNoComment = lines.Where(y =>
		{
			var trimmedString = y.Trim();
			return !trimmedString.StartsWith("//", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(trimmedString);
		});
		var linesNoComma = linesNoComment.Select(y =>
		{
			if (y.EndsWith(','))
			{
				return y[..^1].Trim();
			}

			return y.Trim();
		});

		return linesNoComma;
	}

	private IEnumerable<AcsLibdefine> EnumerateFilteredEnumLines(IEnumerable<string> lines, bool isIntegerType)
	{
		// If parsing an enum that consists of integers, we increment the value if none is given.
		var valueIncrement = 0;
		foreach (var line in lines)
		{
			var parts = line.Split("=");
			if (parts.Length is not 1 and not 2)
			{
				throw new InvalidOperationException($"Failed for \"{line}\". Expected one or two parts for enum definition.");
			}

			var key = parts.ElementAt(0).Trim();
			var value = parts.ElementAtOrDefault(1)?.Trim();

			if (!isIntegerType)
			{
				// Any other type has an explicit value we use.
				if (value == null)
				{
					throw new InvalidOperationException($"Failed for \"{line}\". Non-integer enum should have a key and value.");
				}

				value = TrimCasting(value);

				yield return new()
				{
					Key = key,
					Value = value,
				};

				continue;
			}

			if (value == null)
			{
				yield return new()
				{
					Key = key,
					Value = valueIncrement++.ToString(CultureInfo.InvariantCulture),
				};

				continue;
			}

			// Attempt to parse the enum. This can fail if the integer has a bitshift.
			if (int.TryParse(value, CultureInfo.InvariantCulture, out valueIncrement))
			{
				yield return new()
				{
					Key = parts[0],
					Value = valueIncrement.ToString(CultureInfo.InvariantCulture),
				};
			}
			else
			{
				yield return new()
				{
					Key = key,
					Value = value,
				};
			}
		}
	}

	/// <summary>
	/// Performs trimming of explicit casting on enum values
	/// </summary>
	private static string TrimCasting(string value)
	{
		var comparer = StringComparison.OrdinalIgnoreCase;
		return value
			.Replace("(string)", string.Empty, comparer)
			.Replace("(int)", string.Empty, comparer)
			.Replace("(fixed)", string.Empty, comparer);
	}
}
