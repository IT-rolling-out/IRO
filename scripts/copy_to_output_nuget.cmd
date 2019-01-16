@echo off
set OutDir=%1
set ProjectDir=%2
set Configuration=%3
set AbsoluteOutDir=%ProjectDir%%OutDir%
set ScriptPath=%~dp0
set NugetDirPath=%ScriptPath%..\output\nuget\
echo.
echo ----- AFTER BUILDING SCRIPT STARTED ----- 
echo %ScriptPath%
echo Name: copy_to_output_nuget.cmd
echo Current time: %date% %time%
echo WARNING! Nuget package will be builded after execution of current file (if it launched from build events). So, you will always get previous version of package.
echo Will copy all .nupkg to output dir.
echo Build output path: '%AbsoluteOutDir%'
echo Found nuget files:
FORFILES /P %ProjectDir% /M "*.nupkg" /S /C "cmd /c echo @file"
set NotReplaceModifier=/XF /XN /XO
IF "%Configuration%"=="Debug" (
    echo "Copy without replacement (debug)."	
) ELSE (
    echo "Copy with replacement (release)."
	set NotReplaceModifier=/IS
)
FORFILES /P %ProjectDir% /M "*.nupkg" /S /C "cmd /c robocopy @path/.. %NugetDirPath% @file %NotReplaceModifier%"
echo Copied!
echo ----- FINISHED -----
echo.