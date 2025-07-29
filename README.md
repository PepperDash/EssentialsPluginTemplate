![PepperDash Essentials Pluign Logo](/images/essentials-plugin-blue.png)

# Essentials Plugin Template (c) 2025

## License

Provided under MIT license

## Overview

Fork this repo when creating a new plugin for Essentials. For more information about plugins, refer to the Essentials Wiki [Plugins](https://pepperdash.github.io/Essentials/docs/Plugins.html) article.

This repo contains example classes for the three main categories of devices:
* `MakeModelDevice`: Used for most third party devices which require communication over a streaming mechanism such as a Com port, TCP/SSh/UDP socket, CEC, etc
* `MakeModelLogicDevice`:  Used for devices that contain logic, but don't require any communication with third parties outside the program
* `MakeModelCrestronDevice`:  Used for devices that represent a piece of Crestron hardware

There are matching factory classes for each of the three categories of devices.  The `MakeModelConfigObject` should be used as a template and modified for any of the categories of device.  Same goes for the `MakeModeleBridgeJoinMap`.

This also illustrates how a plugin can contain multiple devices.

## Cloning Instructions

After forking this repository into your own GitHub space, you can create a new repository using this one as the template.  Then you must install the necessary dependencies as indicated below.

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

Dependencies will be automatically installed when

### Instructions for Renaming Solution and Files

See the Task List in Visual Studio for a guide on how to start using the template.  There is extensive inline documentation and examples as well.

For renaming instructions in particular, see the XML `remarks` tags on class definitions

## Build Instructions (PepperDash Internal) 

## Generating Nuget Package

A nuget package is automatically generated when the plugin is build. To modify the name and other details of the package, edit the following properties in the .csproj file:

1. `PackageId` - This is the name that will be used to pull the package from Nuget once it's published
2. `PackgeProjectUrl` - This should match the URL for the plugin repo
3. `AssemblyTitle` - This is the dll file name that is will show on a processor when the plugin is loaded