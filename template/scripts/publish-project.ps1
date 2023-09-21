function invokePublishTool()
{
    Param(
		[parameter(Mandatory=$false)][boolean] $help,
        [parameter(Mandatory=$false)][string] $projectPath,
        [parameter(Mandatory=$false)][string] $packToOutput,
		[parameter(Mandatory=$false)][string] $logOutput,
		[parameter(Mandatory=$false)][string] $todoAt,
		[parameter(Mandatory=$false)][string] $decorateSummaryAt,
		[parameter(Mandatory=$false)][string] $compileWith,
		[parameter(Mandatory=$false)][string] $defines,
		[parameter(Mandatory=$false)][string] $engine,
		[parameter(Mandatory=$false)][boolean] $noWarnForwardReferences,
        [parameter(Mandatory=$false)][boolean] $tempProject,
		[parameter(Mandatory=$false)][boolean] $publicAcs,
		[parameter(Mandatory=$false)][boolean] $removeAcs,
		[parameter(Mandatory=$false)][boolean] $removeUnrelated,
		[parameter(Mandatory=$false)][boolean] $packDecorate,
		[parameter(Mandatory=$false)][boolean] $removeEmpty)

	try
	{
		# The absolute url of the root of the project.
		$rootPath = (Get-Item .).FullName;
		
		# The absolute url of the location of the scripts folder.
		$scriptsPath = Join-Path -Path $rootPath -ChildPath "scripts";
		
		# The absolute url of the location of the source folder.
		$baseSourcePath = Join-Path -Path $rootPath -ChildPath "src";
		
		# The absolute url of the location of the publish tool executable.
		$executableFolder = Join-Path -Path $scriptsPath -ChildPath "DoomerPublish";
		$executablePath = Join-Path -Path $executableFolder -ChildPath "DoomerPublish.exe";
		
		# Make sure the executable can be found.
		# This is the only path beign tested apart from the project path. The publisher tests the rest.
		if (!(Test-Path -Path $executablePath -PathType Leaf))
		{
			throw "Publisher executable was not found at `"$executablePath`". Please ensure the publisher exists.";
		}
		
		# Set absolute url of the project path to publish if it exists.
		if ($projectPath) {
			$projectPath = Join-Path -Path $baseSourcePath -ChildPath $projectPath
		}
		
		# Set the absolute path of the log output directory if it exists.
		if ($logOutput) {
			$logOutput = Join-Path -Path $rootPath -ChildPath $logOutput
		}
		
		# Set the absolute path of the output directory if it exists.
		if ($packToOutput) {
			$packToOutput = Join-Path -Path $rootPath -ChildPath $packToOutput
		}
		
		# Set the absolute path of the todo list output directory if it exists.
		if ($todoAt) {
			$todoAt = Join-Path -Path $rootPath -ChildPath $todoAt
		}
		
		# Set the absolute path of the decorate summary output directory if it exists.
		if ($decorateSummaryAt) {
			$decorateSummaryAt = Join-Path -Path $rootPath -ChildPath $decorateSummaryAt
		}
		
		# Set the absolute path of the compiler root directory.
		$compilerRoot = Join-Path -Path $rootPath -ChildPath "scripts";
		
		# Get project name.
		# If the project path is not set, then just use a default.
		$projectName = "project";
		if ($projectPath) {
			$projectName = (Get-Item $projectPath).Basename;
		}
		
		$argumentList = @();
		if ($projectPath) 			  { $argumentList += "`"$projectPath`""; }
		if ($packToOutput) 			  { $argumentList += " --packToOutput `"$packToOutput`""; }
		if ($logOutput) 			  { $argumentList += " --logOutput `"$logOutput`""; }
		if ($todoAt) 				  { $argumentList += " --todoAt `"$todoAt`""; }
		if ($decorateSummaryAt) 	  { $argumentList += " --decorateSummaryAt `"$decorateSummaryAt`""; }
		
		# compilerRoot is passed regardless of if compileWith is passed.
		$argumentList += " --compilerRoot `"$compilerRoot`"";
		
		if ($compileWith) 			  { $argumentList += " --compileWith $compileWith"; }
		if ($defines) 				  { $argumentList += " --defines $defines"; }
		if ($engine) 				  { $argumentList += " --engine $engine"; }
		if ($noWarnForwardReferences) { $argumentList += " --noWarnForwardReferences"; }
		if ($tempProject) 			  { $argumentList += " --tempProject"; }
		if ($publicAcs) 			  { $argumentList += " --publicAcs"; }
		if ($removeAcs) 			  { $argumentList += " --removeAcs"; }
		if ($removeUnrelated) 		  { $argumentList += " --removeUnrelated"; }
		if ($packDecorate) 			  { $argumentList += " --packDecorate"; }
		if ($removeEmpty) 			  { $argumentList += " --removeEmpty"; }
		
		# If help was set, then instead the argument is simply the help call.
		if ($help) {
			$argumentList = "--help";
		}
		
		#Write-Host "Final argumentList: $argumentList";
		#Read-Host -Prompt "Press any key to continue"
		
		# Determine the location to put logfiles for the tool output.
		# This is just the $logOutput, but if that is not set, just make a new log folder in the tool's folder.
		$toolLogOutput = $logOutput;
		if (!$toolLogOutput) {
			$toolLogOutput = Join-Path -Path $executableFolder -ChildPath "Logs";
		}
		
		$standardOut = Join-Path -Path $toolLogOutput -ChildPath "$projectName.stdout.log";
		$standardError = Join-Path -Path $toolLogOutput -ChildPath "$projectName.stderr.log";
			
		# Make sure this folder exists, because the tool is not going to do that for us in time.
		$null = New-Item -ItemType Directory -Force -Path (Split-Path -Path $standardOut);
		$null = New-Item -ItemType Directory -Force -Path (Split-Path -Path $standardError);
			
		# Load the publisher with the arguments.
		Start-Process -FilePath $executablePath -ArgumentList $argumentList -RedirectStandardOutput "$standardOut" -RedirectStandardError "$standardError"
	}
	catch
	{
		# Only log the line, the main script should catch and handle the rest.
		$line = $_.InvocationInfo.ScriptLineNumber
		Write-Host -ForegroundColor Red "Execution failed at line $line."
		throw;
	}
}