# A dotNet Core Cli tool for Power Platform

This cli tool is part of [Shazam](https://github.com/roshangautam/shazam) framework

## User Guide

### Using the tool in a Shazam project

[Shazam](https://github.com/roshangautam/shazam) projects come with this cli tool installed. Follow the instruction in shazam to learn how to use shazam-cli

### Using the tool in other projects

```dotnetcli
 dotnet tool install shazam-cli --version 0.0.1-alpha
```

### Settings

Make sure you have settings.dev.json in the root of your solution. A sample can be found in [Shazam](https://github.com/roshangautam/shazam)

## Developers Guide

### Prerequisites

- Windows 10 Operating Systems
- Install
  - [.net core 3.1.*](https://dotnet.microsoft.com/download/dotnet-framework)
  - [Git for windows](https://git-scm.com/download/win)
  - [Latest MSBuild tools](https://visualstudio.microsoft.com/downloads/?q=build+tools)
  - Optionally you can install [Visual Studio 2019](https://visualstudio.microsoft.com/downloads) with all these components selected
- [optional] [Add MSBuild to your path environment variable](https://stackoverflow.com/questions/6319274/how-do-i-run-msbuild-from-the-command-line-using-windows-sdk-7-1)
- [optional] Install [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7)
- [optional] Install [Windows Terminal](https://github.com/microsoft/terminal)

Fire up a powershell terminal and clone this repo and navigate to the project folder. Run the following command to build

```dotnetcli
dotnet build
```
