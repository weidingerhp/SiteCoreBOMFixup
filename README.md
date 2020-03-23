## File Checker for SiteCore

this is a simple Program that iterates through a Directory (recursively) and searches for XML Files.
All XML Files are checked for their BOM (Byte Order Mark).
Since its considered bad behavior using UTF-8-BOMs on Windows they can be removed via the command-line Tool

### Application
There is a Problem happening every now and then with SiteCore XML Config Files. They can contain UTF-8-BOM.

### Building
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

