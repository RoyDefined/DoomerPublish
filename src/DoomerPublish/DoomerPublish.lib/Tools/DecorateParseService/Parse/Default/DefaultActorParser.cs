using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Decorate;

/// <summary>
/// Represents the default parser to parse an actor definition.
/// </summary>
internal sealed class DefaultActorParser : IDecorateParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find an actor in the decorate file. Also returns a group for the actor name, what it inherited from, and the doomednum.
	/// </summary>
	private readonly Regex _actorRegex = new(@"^actor (?<actorName>[\w\d]+)(\s+: (?<inheritedFrom>[\w\d]+))?( (?<doomedNum>\d*))?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

	public DefaultActorParser(
		ILogger<DefaultActorParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(DecorateFile decorateFile, CancellationToken _)
	{
		var actorMatchCollection = this._actorRegex.Matches(decorateFile.Content);

		var actors = actorMatchCollection.Select(x =>
		{
			var actorNameGroup = x.Groups.GetValueOrDefault("actorName") ??
				throw new InvalidOperationException($"Expected actor name for {x.Name}");

			var inheritedFromGroup = x.Groups.GetValueOrDefault("inheritedFrom");
			var doomedNumGroup = x.Groups.GetValueOrDefault("doomedNum");

			int? doomedNum = doomedNumGroup != null && int.TryParse(doomedNumGroup.Value, out var doomedNumParsed) ? doomedNumParsed : null;

			return new DecorateActor()
			{
				Definition = x.Value,
				Name = actorNameGroup.Value,
				InheritedFrom = inheritedFromGroup?.Value,
				Doomednum = doomedNum
			};
		}).ToList();

		decorateFile.Actors = actors;

		var count = decorateFile.Actors.Count;
		if (count > 0)
		{
			this._logger.LogDebug("Found {Count} actor(s).", count);
		}
		
		return Task.CompletedTask;
	}
}
