# This is used to traverse the shortcut files.
$shell = New-Object -ComObject WScript.Shell;

function getFileFromShortcut()
{
    Param(
        [parameter(Mandatory=$true)][string] $rootFolder,
        [parameter(Mandatory=$true)][string] $shortcutFolder,
        [parameter(Mandatory=$true)][string] $fileName)

    $shortcutPath = Join-Path -Path $rootFolder -ChildPath "/$shortcutFolder"
    #Write-Host "Shortcut path for ${shortcutFolder}: $shortcutPath";

	if (!(Test-Path -Path $shortcutPath -PathType Leaf))
	{
		throw "${shortcutFolder} shortcut folder was not found at `"$shortcutPath`". Please create a shortcut file to use.";
	}
	
    $folderPath = $shell.CreateShortcut($shortcutPath).TargetPath
    $filePath = Join-Path -Path $folderPath -ChildPath "/$fileName"
    #Write-Host "File path: $filePath";

	if (!(Test-Path -Path $filePath -PathType Leaf))
	{
		throw "$fileName was not found at `"$filePath`". Please create a shortcut to the file folder to use, and point it to the folder containing $fileName.";
	}

    return $filePath;
}