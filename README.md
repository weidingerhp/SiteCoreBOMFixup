## File Checker for SiteCore

this is a simple Program that iterates through a Directory (recursively) and searches for XML Files.
All XML Files are checked for their BOM (Byte Order Mark).
Since its considered bad behavior using UTF-8-BOMs on Windows they can be removed via the command-line Tool

### Application
There is a Problem happening every now and then with SiteCore XML Config Files. They can contain UTF-8-BOM.

### Building

Current Build Status:

[![Build Status](https://dev.azure.com/hans-peterweidinger/hans-peterweidinger/_apis/build/status/weidingerhp.SiteCoreBOMFixup?branchName=master)](https://dev.azure.com/hans-peterweidinger/hans-peterweidinger/_build/latest?definitionId=2&branchName=master)

#### Dev Builds
You need at least .Net Core 3.1 installed to build the whole application.
If you just stick with the CommandLine - Tool .Net Core 2.2 will also do (or any netstandard2.0 compliant Framework)

Build everything

```
dotnet restore
dotnet publish --framework netcoreapp3.1
```
You may then run
```
.\SitCoreFixConsole\bin\Debug\netcoreapp3.1\publish\SitCoreFixConsole.exe
```

If needed there is also a UI
```
.\SiteCoreFixup\bin\Debug\netcoreapp3.1\publish\SiteCoreFixup.exe
```

#### Release Builds

Build the CLI Tool as Self-Contained EXE (framework 3.1) into the Directory ``CLITool``

```
dotnet restore
dotnet publish SiteCoreFixConsole/SiteCoreFixConsole.csproj -o CLITool --configuration Release --framework netcoreapp3.1 --self-contained true --runtime win-x64
```

Build the GUI Tool as Self-Contained EXE (framework 3.1) into the Directory ``GUITool``
```
dotnet restore
dotnet publish SiteCoreFixup/SiteCoreFixup.csproj -o GUITool --configuration Release --framework netcoreapp3.1 --self-contained true --runtime win-x64
```

