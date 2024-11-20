. "$PSScriptRoot\scripts\publish-project.ps1"

### This script will publish your project to work for the production environment.
### By default, this script will compile the core project, generate a todo list and decorate summary in the logs folder just like the development environment.
### In addition, this script will also strip your ACS source, pack your decorate code and remove any unused directories.
### Finally, this script will pack your projects into a pk3 for distribution.
### Since files are modified, the project will be copied to a temporary folder so the original project is not changed.
### The configuration used can be modified, but if you want to go further and add or remove options, it is a better idea to look at `publish-template.ps1`.

# Set this to $false if you want to be warned of forward references.
# Note ACSUtils has these, and enabling this means that you will have a lot of existing warnings.
$noWarnForwardReferences = $true;

# The relative path of the location of the core project containing the ACS files that are made for GDCC.
$gdccProjectPath = "TestProjectGdccAcc";

# The relative path of the location of the core project containing the ACS files that are made for BCC.
$bccProjectPath = "TestProjectBcc";

# The relative path of the location of the core project containing the ACS files that are made for ACC.
$accProjectPath = "TestProjectAcc";

# The relative path of the location of the maps project containing the map files.
$mapsProjectPath = "TestProjectMaps";

# The relative path of the location of the maps project containing the main resources.
$resourcesProjectPath = "TestProjectResources";

# The relative path of the folder to put all log files into.
# This is also used for the todo list and decorate summary.
$logsPath = "logs";

# The engine to compile for.
$engine = "zandronum";

# The relative path of the folder to place the final build of the projects.
$buildPath = "build";

try
{
	invokePublishTool -projectPath $accProjectPath -packToOutput $buildPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "acc" -engine $engine -tempProject $true -publicAcs $true -removeAcs $true -removeUnrelated $true -packDecorate $true -removeEmptyDirectories $true -removeEmptyLogFiles $true;
	invokePublishTool -projectPath $bccProjectPath -packToOutput $buildPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "bcc" -engine $engine -tempProject $true -publicAcs $true -removeAcs $true -removeUnrelated $true -packDecorate $true -removeEmptyDirectories $true -removeEmptyLogFiles $true;
	invokePublishTool -projectPath $gdccProjectPath -packToOutput $buildPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "gdcc" -engine $engine -noWarnForwardReferences $noWarnForwardReferences -tempProject $true -publicAcs $true -removeAcs $true -removeUnrelated $true -packDecorate $true -removeEmptyDirectories $true -removeEmptyLogFiles $true;
	
	invokePublishTool -projectPath $mapsProjectPath -packToOutput $buildPath -logOutput $logsPath -tempProject $true -removeUnrelated $true -removeEmptyDirectories $true -removeEmptyLogFiles $true;
	invokePublishTool -projectPath $resourcesProjectPath -packToOutput $buildPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -tempProject $true -removeUnrelated $true -packDecorate $true -removeEmptyDirectories $true -removeEmptyLogFiles $true;
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Tool invokation failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}