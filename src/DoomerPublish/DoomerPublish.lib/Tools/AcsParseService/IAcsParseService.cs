using Microsoft.Extensions.Logging;
using DoomerPublish.Utils;
using System.Text.RegularExpressions;

namespace DoomerPublish.Tools;

public interface IAcsParseService
{
	public void ParseFile(AcsFile acsFile);
}
