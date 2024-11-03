using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Acs;

/// <summary>
/// Represents the parser to parse a file namespace.
/// </summary>
internal sealed class NamespaceParser(
	ILogger<NamespaceParser> logger)
	: IAcsParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger = logger;

	/// <summary>
	/// Regex to find functions, filtering public and non public, summary, name, actual definition and return type.
	/// </summary>
	private readonly Regex _namespaceRegex = new(@"(?<isStrict>(strict ))?namespace(\s*(?<name>([\w])+))?\s*{", RegexOptions.IgnoreCase);

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var namespaceMatch = this._namespaceRegex.Match(acsFile.Content);
		if (namespaceMatch == null || !namespaceMatch.Success)
		{
			this._logger.LogDebug("No namespace found.");
			return Task.CompletedTask;
		}

		var isStrictGroup = namespaceMatch.Groups.GetValueOrDefault("isStrict");
		var isStrict = isStrictGroup?.Success ?? false;

		var namespaceNameGroup = namespaceMatch.Groups.GetValueOrDefault("name");
		if (namespaceNameGroup == null || !namespaceNameGroup.Success)
		{
			namespaceNameGroup = null;
		}

		var namespaceName = namespaceNameGroup?.Value;
		this._logger.LogDebug("Found namespace. Name: \"{Name}\", is strict: {Strict}.", namespaceName ?? "N/A", isStrict ? "true" : "false");

		acsFile.Namespace = namespaceName;
		acsFile.IsStrictNamespace = isStrict;
		return Task.CompletedTask;
	}
}
