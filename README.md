# Essentials Plugin Template (c) 2020

## License

Provided under MIT license

## Overview

Fork this repo when creating a new plugin for Essentials. For more information about plugins, refer to the Essentials Wiki [Plugins](https://github.com/PepperDash/Essentials/wiki/Plugins) article.

This repo contains example classes for the three main categories of devices:
* `EssentialsPluginTemplateDevice`: Used for most third party devices which require communication over a streaming mechanism such as a Com port, TCP/SSh/UDP socket, CEC, etc
* `EssentialsPluginTemplateLogicDevice`:  Used for devices that contain logic, but don't require any communication with third parties outside the program
* `EssentialsPluginTemplateCrestronDevice`:  Used for devices that represent a piece of Crestron hardware

There are matching factory classes for each of the three categories of devices.  The `EssentialsPluginTemplateConfigObject` should be used as a template and modified for any of the categories of device.  Same goes for the `EssentialsPluginTemplateBridgeJoinMap`.

This also illustrates how a plugin can contain multiple devices.

## Cloning Instructions

After forking this repository into your own GitHub space, you can create a new repository using this one as the template.  Then you must install the necessary dependencies as indicated below.

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
Alternatively, you can simply run the `GetPackages.bat` file.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

### Installing Different versions of PepperDash Core

If you need a different version of PepperDash Core, use the command `nuget install .\packages.config -OutputDirectory .\packages -excludeVersion -Version {versionToGet}`. Omitting the `-Version` option will pull the version indicated in the packages.config file.

### Instructions for Renaming Solution and Files

See the Task List in Visual Studio for a guide on how to start using the template.  There is extensive inline documentation and examples as well.

For renaming instructions in particular, see the XML `remarks` tags on class definitions

## Build Instructions (PepperDash Internal) 

## Generating Nuget Package 

In the solution folder is a file named "PDT.EssentialsPluginTemplate.nuspec" 

1. Rename the file to match your plugin solution name 
2. Edit the file to include your project specifics including
    1. <id>PepperDash.Essentials.Plugin.MakeModel</id> Convention is to use the prefix "PepperDash.Essentials.Plugin" and include the MakeModel of the device. 
    2. <projectUrl>https://github.com/PepperDash/EssentialsPluginTemplate</projectUrl> Change to your url to the project repo

There is no longer a requirement to adjust workflow files for nuget generation for private and public repositories.  This is now handled automatically in the workflow.

__If you do not make these changes to the nuspec file, the project will not generate a nuget package__