. "$PSScriptRoot\scripts\publish-project.ps1"

### This script will call the publish tool with the `--help` parameter.
### This will log the available parameters in a log file.

# The relative path of the folder to put all log files into.
$logsPath = "logs";

try
{
	invokePublishTool -help $true -logOutput $logsPath;
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Tool invokation failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}
