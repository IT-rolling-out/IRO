echo Press enter if you want to delete .nupkg files from subfolders (recommendet) or close console.
pause
FORFILES /M "*.nupkg" /S /C "cmd /c del @file"
echo Deleted.
pause