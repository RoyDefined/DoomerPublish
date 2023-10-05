using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Common;

/// <summary>
/// Represents the default TODO parser to parse todo items that exist in a given file.
/// </summary>
internal sealed class DefaultTodoParser : IAcsParser, IDecorateParser
{
	/// <inheritdoc cref="ILogger" />
	private readonly ILogger _logger;

	/// <summary>
	/// Regex to find a todo item.
	/// </summary>
	private readonly Regex _todoItemRegex = new(@"\s*\/\/\s*@todo:?\s*(?<todo>(.*))", RegexOptions.IgnoreCase);

	public DefaultTodoParser(
		ILogger<DefaultTodoParser> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public Task ParseAsync(AcsFile acsFile, CancellationToken _)
	{
		var todoItems = this.ParseTodoContent(acsFile.Content);
		acsFile.Todos = todoItems.ToList();

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task ParseAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		var todoItems = this.ParseTodoContent(decorateFile.Content);
		decorateFile.Todos = todoItems.ToList();

		return Task.CompletedTask;
	}

	/// <summary>
	/// Parses the todo items that can be found in the given <paramref name="content"/>.
	/// </summary>
	/// <param name="content">The content to parse.</param>
	/// <returns>An enumerable containing the parsed todo items.</returns>
	private IEnumerable<TodoItem> ParseTodoContent(string content)
	{
		// Submethod to determine the actual line a todo was on, by navigating the input and checking for new lines.
		static int LineFromPos(string input, int indexPosition)
		{
			int lineNumber = 1;
			for (int i = 0; i < indexPosition; i++)
			{
				// TODO: Improve this.
				if (input[i] == '\n')
				{
					lineNumber++;
				};
			}
			return lineNumber;
		}

		var todoMatchCollection = this._todoItemRegex.Matches(content);

		var todoItems = todoMatchCollection.Select(x =>
		{
			var todoGroup = x.Groups.GetValueOrDefault("todo") ??
				throw new InvalidOperationException($"Expected todo content for {x.Name}");

			return new TodoItem()
			{
				Value = todoGroup.Value,
				Line = LineFromPos(content, todoGroup.Index),
			};
		});

		return todoItems;
	}
}
