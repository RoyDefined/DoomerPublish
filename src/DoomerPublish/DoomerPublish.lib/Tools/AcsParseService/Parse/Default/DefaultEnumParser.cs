using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools.Acs;

internal sealed class DefaultEnumParser : IAcsParser
{
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find enum definitions, which will return the inner enum content.
	/// </summary>
	private readonly Regex _enumContentRegex = new(@"enum .* \s*{(?<enumContent>([\sa-z\/,.`\(\)]*))};", RegexOptions.IgnoreCase);

	public DefaultEnumParser(
		ILogger<DefaultEnumParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var enumMatchCollection = this._enumContentRegex.Matches(acsFile.Content);

		var filteredLines = enumMatchCollection.Select(x =>
		{
			var enumContent = x.Groups.GetValueOrDefault("enumContent")?.Value ??
				throw new InvalidOperationException($"Expected enum content for {x.Name}");

			// Get individual lines, removing whitespace and comments.
			return enumContent
				.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
				.Where(y =>
				{
					var trimmedString = y.Trim();
					return !trimmedString.StartsWith("//", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(trimmedString);
				})
				.Select(y =>
				{
					if (y.EndsWith(",", StringComparison.OrdinalIgnoreCase))
					{
						return y[..^1].Trim();
					}

					return y.Trim();
				}).ToList();
		}).ToList();

		acsFile.EnumLibdefines = this.EnumerateFilteredEnumLines(filteredLines)
			.ToList();

		return Task.CompletedTask;
	}

	private IEnumerable<AcsLibdefine> EnumerateFilteredEnumLines(List<List<string>> lines)
	{
		// If the enum has an equality after it, then we must continue with that number.
		foreach (var filteredCollection in lines)
		{
			var value = 0;
			foreach (var line in filteredCollection)
			{
				var parts = line.Split("=");
				if (parts.Length is not 1 or 2)
				{
					throw new InvalidOperationException("Expected one or two parts for enum definition");
				}

				if (parts.Length == 2)
				{
					value = int.Parse(parts.Last(), CultureInfo.InvariantCulture);
				}

				yield return new()
				{
					Key = parts.First(),
					Value = value++.ToString(CultureInfo.InvariantCulture),
				};
			}
		}
	}
}
