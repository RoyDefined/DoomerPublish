using System.Diagnostics;
using System.Reflection;

namespace DoomerPublishConsole;

internal static class Common
{
	// The executable directory so that we have the directory of the appsettings and such.
	public static string ExecutableDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

	// This corresponds to the main executable name.
	public static string ExecutableName => Process.GetCurrentProcess().ProcessName;
}
