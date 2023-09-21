<!-- Anchor for the "back to top" links -->
<a id="readme-top"></a>

<!-- Project logo -->
<br />
<div align="center">
  <h1>Zandronum project template for DoomerPublish</h1>
</div>

This is a project template for Zandronum projects, which improves development by making it easier to edit, test and compile your projects. This template has been set up to work with DoomerPublish.

<!-- Table of contents -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#prerequisites">Prerequisites</a>
    </li>
    <li>
      <a href="#features">Features</a>
    </li>
	<li>
      <a href="#usage">Usage</a>
    </li>
  </ol>
</details>



## Prerequisites
This is a list of prerequisites that must be met in order to properly use this template.
- An installation of the latest Zandronum 3.2 release.
- An installation of Ultimate Doombuilder.
- A valid installation of the desired compiler inside `scripts/<compiler name>`.
	- ACC can be found [here](https://github.com/ZDoom/acc/releases).
	- BCC can be found [here](https://github.com/positively-charged/bcc/releases).
	- ZT-BCC can be found [here](https://github.com/zeta-group/zt-bcc/releases).
	- GDCC can be found [here](https://github.com/DavidPH/GDCC/tags). Windows builds found [here](https://www.dropbox.com/sh/5wae0ro7vuesud7/AADSyNu4S89Gc2RJc0PdS3qHa?dl=0)
- Ensure the `scripts` folder has the following:
	- A shortcut folder called `Engine` which points to your Zandronum installation.
	- A shortcut folder called `IWads` which points to your folder containing the IWADs.
	- A shortcut folder called `UDB` which points to your folder containing the Ultimate Doombuilder executable.
- Ensure the `scripts/DoomerPublish` folder has a valid installation of the tool. Installations can be found [here](https://github.com/RoyDefined/DoomerPublish/tags).

Not having one or more prerequisite causes the template to not work correctly.

## Features
- Four ready to use powershell scripts that allow you to compile your projects, test and edit individual maps. These scripts can be duplicated and by only modifying a few parameters you can support your own projects.
- An easy to use template script if you want more control, giving full explanation on the options available in the scripts.
- The ability to compile using five different compilers, depending on your use case.
- The ability to generate a public acs file. This is a file that contains dummy functions and scripts that can be included in the final mod. ACS does not care if you include the real file. If you include this file, you can use all functions that you want to use, and by providing the compiled ACS in the mod's project, Zandronum will work as normal.
	- To add a function to the generated file, put `// @public` above a function or script. You can optionally provide `// @summary <text>` and provide a helpful comment in the generated file.
	- Libdefines are automatically added.
	- GDCC and BCC enums are converted into libdefines.
- The ability to strip the ACS source from your project after it has been compiled.
- The ability to generate a todo list. The generated file will display the todo topic, and also the location of the todo item in your code.
	- The todo list uses todo comments from your ACS and Decorate files. To specify a todo item, put `// @todo <text>` or `// @todo: <text>` in your ACS or Decorate file.
- The ability to generate a Decorate summary. The generated summary will summarize all doomednums that exist in your project, in order. The generated summary will also display a list of all known actors in your project, including inherited actor and doomednum if it has one.
- The ability to pack your Decorate code. Packing your decorate code will pack everything that can be found and is included, and put it in a single generated file in the root of the project. This removes the file that was packed into the generated file.
- The ability to remove unneeded files from your projects, such as backup maps, or meta files.
- The ability to remove empty directories.

## Usage
- The template assumes that every folder inside `src/` is an individual project. Once you pack your projects into a pk3, these are the folders being compiled.
- By default, there are five files. One is a general resources folder, one is for maps, and three contain ACS code that can be compiled using different compilers.
- `publish-dev.ps1` is an example script that will compile your projects to work under a development environment. The following rules are applied:
	- Your project is compiled.
	- A pragma called "dev" is used in compilation when you compile with GDCC or BCC. Any ACS within blocks defining this is conditionally compiled with your project.
	- The tool generates a "public acs source".
	- A todo-list is generated in the logs folder.
	- A decorate summary is generated in the logs folder.

  Note that this script compiles the three ACS folders individually using the different compilers, as a test.
- `publish-prod.ps1` is an example script that will compile your projects to work under a development environment. The following rules are applied:
	- Your project is copied into a temporary folder to avoid losing data in the actual project.
	- Your project is compiled.
	- The tool generates a "public acs source".
	- The actual ACS source is deleted.
	- A todo-list is generated in the logs folder.
	- A decorate summary is generated in the logs folder.
	- Your decorate is packed and all the files that get packed are removed.
	- Empty directories are removed.
- `publish-help.ps1` This is a script that will output helpful details about the tool in the logs folder.
- `publish-template.ps1` This is a script template that has all the available options explained in detail, which is a great base to start your custom scripts with.
- `edit-test.ps1` is an example script that will open up the "TEST" map included with the base template project, inside Ultimate DoomBuilder.
- `play-test.ps1` is an example script that will open up the "TEST" map included with the base template project, inside Zandronum.
- `logs/` will provide the compiler output, todo items if requested, decorate summary if requested, and the play log from Zandronum. You should always check if the log ending in `stderr.log` contains any errors, and fix it.

<p align="right">(<a href="#readme-top">back to top</a>)</p>