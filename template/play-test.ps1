. "$PSScriptRoot\scripts\play-project.ps1"

### This script will start up Zandronum and load up map "TEST".

# The map to load.
$map = "test";

# The game mode that should be used for testing.
$gameMode = "coop";

# Extra parameters to pass into the query.
$extraParameters = "";

# The relative path of the folder to put all log files into.
$logsPath = "logs";

try
{
	playProject -map $map -logOutput $logsPath -gameMode $gameMode -extraParameters $extraParameters
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Playing project failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}