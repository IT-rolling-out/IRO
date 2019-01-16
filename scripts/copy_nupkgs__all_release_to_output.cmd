@echo off
set ScriptPath=%~dp0
set NugetDirPath=%ScriptPath%..\output\nuget\
call copy_nupkgs ..\src\ %NugetDirPath% 1 0
pause