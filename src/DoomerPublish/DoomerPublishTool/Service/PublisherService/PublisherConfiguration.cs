using CommandLine;

namespace DoomerPublish;

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations

/// <summary>
/// Represents the compiler to use during publishing.
/// </summary>
public enum CompileType
{
	Unknown,
	Acc,
	Bcc,
	GdccAcc,
	GdccC
};

/// <summary>
/// Represents the engine to target during publishing.
/// </summary>
public enum EngineType
{
	Unknown,
	Zdoom,
	Zandronum,
	Eternity
};

/// <summary>
/// Represents all configurable configuration for a publish process.
/// </summary>
public class PublisherConfiguration
{
	[Value(index: 0, Required = true, HelpText = "Absolute path to the project directory.")]
	public required string InputProjectDir { get; init; }

	[Option(longName: "logOutput", Required = true, HelpText = "Specify absolute path of directory to put log files.")]
	public string? LogOutputFolder { get; init; }

	[Option(longName: "packToOutput", Required = false, HelpText = "Specify absolute path to directory to pack the projects and copy them here.")]
	public string? PackToOutputDir { get; init; }

	// File generation

	[Option(longName: "todoAt", Required = false, HelpText = "Specify absolute path of directory to generate a todo list.")]
	public string? GenerateTodoListAtDirectory { get; init; }

	[Option(longName: "decorateSummaryAt", Required = false, HelpText = "Specify absolute path of directory to generate a decorate summary.")]
	public string? GenerateDecorateSummaryAtDirectory { get; init; }

	// Compilation options

	[Option(longName: "compilerRoot", Required = false, HelpText = "Specify absolute path of directory containing the compiler folders.")]
	public string? CompilersRootDirectory { get; init; }

	[Option(longName: "compileWith", Required = false, HelpText = "Specify if the project should be compiled (possible values: acc, bcc, gdccacc). Requires option \"compilerRoot\".")]
	public string? CompileWith { get; init; }

	[Option(longName: "defines", Required = false, HelpText = "Preprocessor defines for the compiler. Unused if the compiler does not support it.")]
	public string? CompileDefines { get; init; }

	[Option(longName: "engine", Required = false, HelpText = "Specify the engine to support the tool in compilation (possible values: zdoom, zandronum, eternity). This is currently only relevant for GDCC compilation.")]
	public string? ForEngine { get; init; }

	[Option(longName: "noWarnForwardReferences", Required = false, HelpText = "Specify if the compiler should not warn of forward references. Only used with the GDCC compilers.")]
	public bool NoWarnForwardReferences { get; init; }

	// Task options

	[Option(longName: "tempProject", Required = false, HelpText = "Specify to create a temporary project.")]
	public bool CreateTemporaryProject { get; init; }

	[Option(longName: "publicAcs", Required = false, HelpText = "Specify to create a public acs source.")]
	public bool GeneratePublicAcsSource { get; init; }

	[Option(longName: "removeAcs", Required = false, HelpText = "Specify to remove the acs source.")]
	public bool RemoveAcsSource { get; init; }

	[Option(longName: "removeUnrelated", Required = false, HelpText = "Specify to remove unrelated files.")]
	public bool RemoveUnrelatedFiles { get; init; }

	[Option(longName: "packDecorate", Required = false, HelpText = "Specify to pack decorate into one file.")]
	public bool PackDecorate { get; init; }

	[Option(longName: "removeEmpty", Required = false, HelpText = "Specify to remove empty directories.")]
	public bool RemoveEmptyDirectories { get; init; }

	// Property getters

	public CompileType CompileWithParsed => this.CompileWith?.ToUpperInvariant() switch
	{
		"ACC" => CompileType.Acc,
		"BCC" => CompileType.Bcc,
		"GDCCACC" => CompileType.GdccAcc,
		"GDCCC" => throw new NotImplementedException("Compiling using GDCC-CC is not yet supported."),
		_ => CompileType.Unknown,
	};

	public EngineType EngineTypeParsed => this.ForEngine?.ToUpperInvariant() switch
	{
		"ZDOOM" => EngineType.Zdoom,
		"ZANDRONUM" => EngineType.Zandronum,
		"ETERNITY" => EngineType.Eternity,
		_ => EngineType.Unknown,
	};

	public string? GDCCParsedDefines => this.CompileDefines is string compileDefines ?
		string.Join(' ', compileDefines.SplitArgs().Select(x => $"--define {x}")) :
		null;

	public string? BCCParsedDefines => this.CompileDefines is string compileDefines ?
		string.Join(' ', compileDefines.SplitArgs().Select(x => $"-D {x}")) :
		null;
}