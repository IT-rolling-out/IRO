#$path="Common\IRO.CmdLine\IRO.CmdLine.csproj";
$path="IRO_main.sln";
dotnet restore $path;
echo "Nuget packages restored.";
$nugetRestoreStatus=$lastexitcode;
pause;
dotnet clean $path;
$cleanStatus=$lastexitcode;
echo "Cleaned.";
pause;
$IsRelease = Read-Host "Please enter 'y' if you wan't to build in release mode: "
if($IsRelease -eq 'y'){
dotnet build $path --configuration Release
echo 'Builded in Release mode.';
}
else{
dotnet build $path --configuration Debug
echo 'Builded in Debug mode.';
}
$buildStatus=$lastexitcode;
echo '';
Write-Host 'Nuget restore exit code: '$nugetRestoreStatus;
Write-Host 'Project cleaning exit code: '$nugetRestoreStatus;
Write-Host 'Build exit code: '$buildStatus;
pause;