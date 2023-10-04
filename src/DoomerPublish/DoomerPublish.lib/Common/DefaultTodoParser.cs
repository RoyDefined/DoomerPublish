using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools.Common;

internal sealed class DefaultTodoParser : IAcsParser, IDecorateParser
{
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

	public Task ParseAsync(DecorateFile decorateFile, CancellationToken cancellationToken)
	{
		var todoItems = this.ParseTodoContent(decorateFile.Content);
		decorateFile.Todos = todoItems.ToList();

		return Task.CompletedTask;
	}

	private IEnumerable<TodoItem> ParseTodoContent(string content)
	{
		static int LineFromPos(string input, int indexPosition)
		{
			int lineNumber = 1;
			for (int i = 0; i < indexPosition; i++)
			{
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
