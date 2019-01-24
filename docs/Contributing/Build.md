# How to build?

Run `build.ps1` or `build_verbose.ps1` and wait, it will build all automatically.

To use it you must have installed .net core and powershell.

Below will be described all steps of build scripts, so you can do it manually if you want:

1. Search `build.ps1` in .\submodules and (if exists) execute them with same parameters.
1. Remove .\output\nuget.
1. Remove old .nupkg files from .\src directory, recursevly.
1. Call `dotnet build` for all .sln files in script folder.
1. Call all projects with "UnitTests" in name with `dotnet test`.
1. Copy new .nupkg files to .\output\nuget .

NOTE: How to config projects you can read in solution architecture section.