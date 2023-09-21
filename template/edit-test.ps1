. "$PSScriptRoot\scripts\edit-project.ps1"

### This script will start up Ultimate DoomBuilder and load up map "TEST".

# The relative url of the location of the map wad file that you want to load.
$wad = "TestProjectMaps/test.wad";

# The map that you want to load from that wad file.
$map = "TEST";

# This is the configuration to use in UDB for the map.
$configuration = "zandronum_DoomUDMF.cfg";
	
try
{
	editProject -wad $wad -map $map -configuration $configuration;
}
catch
{
	$message = $_.Exception.Message;
	Write-Host -ForegroundColor Red "Editing map failed: $message"
	Read-Host -Prompt "Press any key to exit"
	Exit
}