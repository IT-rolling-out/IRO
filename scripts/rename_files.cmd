@echo off
echo Use it only for renaming folders in solution if needed. 
pause
Setlocal EnableDelayedExpansion
set filesMatchStr=*MvcPart*
set origStr=MvcPart
set replaceStr=Mvc
echo Files to be renamed:
FORFILES /M "%filesMatchStr%" /S /C "powershell /c if ('@ISDIR' -eq 'TRUE'){ exit;} $fileFrom='@path'; $fileTo='@path'.replace('%origStr%', '%replaceStr%'); Write-Host $fileFrom' --> '$fileTo "
echo Press enter to rename files.
pause
FORFILES /M "%filesMatchStr%" /S /C "powershell /c if ('@ISDIR' -eq 'TRUE'){ exit;} $fileFrom='@path'; $fileTo='@file'.replace('%origStr%', '%replaceStr%'); Write-Host $fileFrom' --> '$fileTo; Rename-Item -Path $fileFrom -NewName $fileTo "
echo Files renamed.

echo Dirs to be renamed:
FORFILES /M "%filesMatchStr%" /S /C "powershell /c if ('@ISDIR' -eq 'FALSE'){ exit;} $fileFrom='@path'; $fileTo='@path'.replace('%origStr%', '%replaceStr%'); Write-Host $fileFrom' --> '$fileTo "
echo Press enter to rename dirs. It will throw exceptions, but rename, seems it is normal.
pause
FORFILES /M "%filesMatchStr%" /S /C "powershell /c if ('@ISDIR' -eq 'FALSE'){ exit;}  $fileFrom='@path'; $fileTo='@path'.replace('%origStr%', '%replaceStr%'); Write-Host $fileFrom' --> '$fileTo; Rename-Item -Path $fileFrom -NewName $fileTo "
echo Dirs renamed.
pause