. "$PSScriptRoot\common.ps1"

function playProject()
{
    Param(
        [parameter(Mandatory=$true)][string] $map,
		[parameter(Mandatory=$false)][string] $logOutput,
		[parameter(Mandatory=$false)][string] $gameMode,
		[parameter(Mandatory=$false)][string[]] $extraParameters)
	
	try
	{
		# The absolute url of the root of the project.
		$rootPath = (Get-Item .).FullName;
		
		# The absolute url of the location of the scripts folder.
		$scriptsPath = Join-Path -Path $rootPath -ChildPath "scripts";
		
		# The absolute url of the location of the source folder.
		$baseSourcePath = Join-Path -Path $rootPath -ChildPath "src";
			
		# Determine the projects to run.
		$joinedProjectDirResources = @()
		foreach($folder in Get-ChildItem -Path $baseSourcePath -Directory)
		{
			$projectDirPath = Join-Path -Path $baseSourcePath -ChildPath $folder;
			$projectDirResource = "-file `"$projectDirPath`""
			$joinedProjectDirResources += $projectDirResource
		}

		# Determine the location of the engine and DOOM2.wad
		$engineExecutable = getFileFromShortcut -rootFolder $scriptsPath -shortcutFolder "Engine.lnk" -fileName "Zandronum.exe";
		$doom2file = getFileFromShortcut -rootFolder $scriptsPath -shortcutFolder "IWads.lnk" -fileName "DOOM2.wad";

		# Detemine wad and arguments to load.
		$doom2wadParameter = "-iwad `"$doom2file`""
		
		# Set log file outputs.
		if (!$logOutput) {
			$logOutput = "logs";
		}
		$logOutput = Join-Path -Path $rootPath -ChildPath (Join-Path -Path $logOutput -ChildPath "log");
		
		if ($gameMode) {
			$gameMode = "+$gameMode true"
		}
		
		# Join the extra parameters that were passed.
		$joinedParameters = @()
		foreach ($string in $extraParameters) {
			$joinedParameters += " $string";
		}

		$argumentList = "$doom2wadParameter $joinedProjectDirResources -stdout $gameMode +logfile $logOutput +map $map $joinedParameters";
		
		#Write-Host "Final argumentList: $argumentList";
		#Read-Host -Prompt "Press any key to continue"

		$null = New-Item -ItemType Directory -Force -Path (Split-Path -Path $logOutput);

		# Load UDB with the arguments.
		Start-Process -FilePath $engineExecutable -ArgumentList $argumentList;
	}
	catch
	{
		# Only log the line, the main script should catch and handle the rest.
		$line = $_.InvocationInfo.ScriptLineNumber
		Write-Host -ForegroundColor Red "Execution failed at line $line."
		throw;
	}
}