@echo off
set OutDir=%1
set ProjectDir=%2
set Configuration=%3
set AbsoluteOutDir=%ProjectDir%%OutDir%
set ScriptPath=%~dp0
set NugetDirPath=%ScriptPath%..\output\nuget\
echo %ScriptPath%
echo Name: copy_to_output_nuget.cmd
echo Current time: %date% %time%
echo WARNING! Nuget package will be builded after execution of current file (if it launched from build events). So, you will always get previous version of package.
echo Will copy all .nupkg to output dir.
echo Build output path: '%AbsoluteOutDir%'
set IsRelease=1
IF "%Configuration%"=="Debug" (
    set IsRelease=0
)
copy_nupkgs %ProjectDir% %NugetDirPath% %IsRelease% 1
echo.