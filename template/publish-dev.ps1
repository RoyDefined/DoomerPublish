. "$PSScriptRoot\scripts\publish-project.ps1"

### This script will publish your project to work for the development environment.
### By default, this script will compile the core project, and generate a todo list and decorate summary in the logs folder.
### The configuration used can be modified, but if you want to go further and add or remove options, it is a better idea to look at `publish-template.ps1`.

### Note this script is an example. It compiles the three projects for each compiler (acc, bcc, gdcc) and also runs the tool through the resources.

# Set this to $false if you want to be warned of forward references.
# Note ACSUtils has these, and enabling this means that you will have a lot of existing warnings.
$noWarnForwardReferences = $true;

# The relative path of the location of the core project containing the ACS files that are made for GDCC.
$gdccProjectPath = "TestProjectGdccAcc";

# The relative path of the location of the core project containing the ACS files that are made for BCC.
$bccProjectPath = "TestProjectBcc";

# The relative path of the location of the core project containing the ACS files that are made for ACC.
$accProjectPath = "TestProjectAcc";

# The relative path of the location of the maps project containing the main resources.
$resourcesProjectPath = "TestProjectResources";

# The relative path of the folder to put all log files into.
# This is also used for the todo list and decorate summary.
$logsPath = "logs";

# The engine to compile for.
$engine = "zandronum";

try
{
	# Core projects with different compilable code.
	invokePublishTool -projectPath $accProjectPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "acc" -engine $engine -publicAcs $true -removeEmptyLogFiles $true;
	invokePublishTool -projectPath $bccProjectPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "bcc" -defines "dev" -engine $engine -publicAcs $true -removeEmptyLogFiles $true;
	invokePublishTool -projectPath $gdccProjectPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath -compileWith "gdccacc" -defines "dev" -engine $engine -noWarnForwardReferences $noWarnForwardReferences -publicAcs $true -removeEmptyLogFiles $true;
	
	# Resources
	invokePublishTool -projectPath $resourcesProjectPath -logOutput $logsPath -todoAt $logsPath -decorateSummaryAt $logsPath;
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Tool invokation failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}
