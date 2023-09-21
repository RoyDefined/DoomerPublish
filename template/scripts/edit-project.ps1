. "$PSScriptRoot\common.ps1"

function editProject()
{
    Param(
		# This is the main wad file that UDB expects to initially load, containing the map.
        [parameter(Mandatory=$true)][string] $wad,
	
		# This is the map to load by default.
        [parameter(Mandatory=$true)][string] $map,
		
		# This is the map to load by default.
        [parameter(Mandatory=$true)][string] $configuration)
	
	try
	{
		# The absolute url of the root of the project.
		$rootPath = (Get-Item .).FullName;
		
		# The absolute url of the location of the scripts folder.
		$scriptsPath = Join-Path -Path $rootPath -ChildPath "scripts";
		
		# The absolute url of the location of the source folder.
		$baseSourcePath = Join-Path -Path $rootPath -ChildPath "src";
		
		# The absolute url of the location of the main wad file.
		$wad = Join-Path -Path $baseSourcePath -ChildPath $wad;
		
		# Test the wad path.
		if (!(Test-Path -Path $wad -PathType Leaf))
		{
			throw "Wad file could not be found at `"$wad`". Please provide a valid path.";
		}
		
		# Determine the projects to open in UDB.
		$joinedProjectDirResources = @()
		foreach($folder in Get-ChildItem -Path $baseSourcePath -Directory)
		{
			$projectDirPath = Join-Path -Path $baseSourcePath -ChildPath $folder;
			$projectDirResource = "-resource dir `"$projectDirPath`""
			$joinedProjectDirResources += $projectDirResource
		}

		#Write-Host "Project dir resources: $joinedProjectDirResources";
		#Read-Host -Prompt "Press any key to continue"

		# Determine the location of UDB and DOOM2.wad
		$udbExe = getFileFromShortcut -rootFolder $scriptsPath -shortcutFolder "UDB.lnk" -fileName "Builder.exe";
		$doom2file = getFileFromShortcut -rootFolder $scriptsPath -shortcutFolder "IWads.lnk" -fileName "DOOM2.wad";

		# Detemine wad and arguments to load.
		$doom2wadParameter = "-resource wad `"$doom2file`""

		$argumentList = "`"$wad`" -map $map -cfg `"$configuration`" $doom2wadParameter $joinedProjectDirResources";
		#Write-Host "Final argumentList: $argumentList";
		#Read-Host -Prompt "Press any key to continue"

		# Load UDB with the arguments.
		Start-Process -FilePath $udbExe -ArgumentList $argumentList;
	}
	catch
	{
		# Only log the line, the main script should catch and handle the rest.
		$line = $_.InvocationInfo.ScriptLineNumber
		Write-Host -ForegroundColor Red "Execution failed at line $line."
		throw;
	}
}