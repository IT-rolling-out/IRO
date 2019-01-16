@echo off
cd ../
set /p NugetPath=<scripts/localData/local_nuget_path.txt
echo Path of yor local nuget dir: '%NugetPath%'
echo Warning! 
echo Don`t continue if it`s empty. 
echo You must create file 'local_nuget_path.txt' and paste path to nuget there.
pause
echo Found files:
FORFILES /M "*.nupkg" /S /C "cmd /c echo @file"
pause
FORFILES /M "*.nupkg" /S /C "cmd /c xcopy @file %NugetPath%  /y /i /s "
echo Copied!
call delete_nupkgs.cmd
pause