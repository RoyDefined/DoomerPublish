using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoomerPublish.Tools.Acs;

internal sealed class DefaultTodoParser : IAcsParser
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

		var todoMatchCollection = this._todoItemRegex.Matches(acsFile.Content);

		var todoItems = todoMatchCollection.Select(x =>
		{
			var todoGroup = x.Groups.GetValueOrDefault("todo") ??
				throw new InvalidOperationException($"Expected todo content for {x.Name}");

			return new TodoItem()
			{
				Value = todoGroup.Value,
				Line = LineFromPos(acsFile.Content, todoGroup.Index),
			};
		}).ToList();

		acsFile.Todos = todoItems;
		return Task.CompletedTask;
	}
}
