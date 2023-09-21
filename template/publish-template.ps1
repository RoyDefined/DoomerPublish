. "$PSScriptRoot\scripts\publish-project.ps1"

### This is a base template which allows you to create your own configuration without puzzling around the existing publish scripts.
### If you don't want to use a parameter in your project, then do not pass it at the eventual call to `invokePublishTool`.

# Set this to $false if you want to be warned of forward references.
# Note ACSUtils has these, and enabling this means that you will have a lot of existing warnings.
$noWarnForwardReferences = $true;

# The relative path of the location of one of your projects.
# This parameter is required by the tool unless you specifically use the `--help` parameter.
$projectPath = "TestProjectGdccAcc";

# Specify the compiler to use here.
# possible values: acc, bcc, gdccacc, gdccc.
# Not passing this parameter will cause the tool to not compile your project's ACS.
$compileWith = "gdccacc";

# The relative path of the folder to put all log files into.
# This parameter is required by the tool unless you specifically use the `--help` parameter.
$logsPath = "logs";

# The relative path of the folder to put all todo lists into.
# Not passing this parameter will cause the tool to not generate a list of todo items.
$todoPath = "logs";

# The relative path of the folder to put all decorate summaries into.
# Not passing this parameter will cause the tool to not generate a decorate summary.
$decorateSummaryPath = "logs";

# Specify a list of defines here.
# Not passing this parameter will cause the tool to not compile with any defines.
$defines = "dev";

# The engine to compile for, to aid the tool in compilation.
# This is useful for GDCC since it improves compilation.
# Possible values: zdoom, zandronum, eternity.
$engine = "zandronum";

# If true, generate a temporary project to handle all tasks in.
$tempProject = $false;

# If true, generate a public ACS file.
$publicAcs = $false;

# If true, remove the known ACS files from the project.
$removeAcs = $false;

# If true, remove files that are not related to the project.
$removeUnrelated = $false;

# If true, pack all known decorate files into a single file.
$packDecorate = $false;

# If true, remove all empty directories.
$removeEmpty = $false;

# The relative path of the folder to place the final build of the projects.
# Not passing this parameter will cause the tool to not pack your project into a pk3.
$buildPath = "build";

try
{
	# Invoke the publish tool here using `invokePublishTool`, and pass the relevant arguments.
	# A single call of `invokePublishTool` will target a single project, so if you have multiple, you will call it multiple times.
	invokePublishTool -projectPath $projectPath -packToOutput $buildPath -logOutput $logsPath -todoAt $todoPath -decorateSummaryAt $decorateSummaryPath -compileWith $compileWith -defines $defines -engine $engine -noWarnForwardReferences $noWarnForwardReferences -tempProject $tempProject -publicAcs $publicAcs -removeAcs $removeAcs -removeUnrelated $removeUnrelated -packDecorate $packDecorate -removeEmpty $removeEmpty;
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Tool invokation failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}