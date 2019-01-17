function Write-Color([String[]]$Text, [ConsoleColor[]]$Color = "White", [int]$StartTab = 0, [int] $LinesBefore = 0,[int] $LinesAfter = 0, [string] $LogFile = "", $TimeFormat = "yyyy-MM-dd HH:mm:ss") {
    $DefaultColor = $Color[0]
    if ($LinesBefore -ne 0) {  for ($i = 0; $i -lt $LinesBefore; $i++) { Write-Host "`n" -NoNewline } } # Add empty line before
    if ($StartTab -ne 0) {  for ($i = 0; $i -lt $StartTab; $i++) { Write-Host "`t" -NoNewLine } }  # Add TABS before text
    if ($Color.Count -ge $Text.Count) {
        for ($i = 0; $i -lt $Text.Length; $i++) { Write-Host $Text[$i] -ForegroundColor $Color[$i] -NoNewLine } 
    } else {
        for ($i = 0; $i -lt $Color.Length ; $i++) { Write-Host $Text[$i] -ForegroundColor $Color[$i] -NoNewLine }
        for ($i = $Color.Length; $i -lt $Text.Length; $i++) { Write-Host $Text[$i] -ForegroundColor $DefaultColor -NoNewLine }
    }
    Write-Host
    if ($LinesAfter -ne 0) {  for ($i = 0; $i -lt $LinesAfter; $i++) { Write-Host "`n" } }  # Add empty line after
    if ($LogFile -ne "") {
        $TextToFile = ""
        for ($i = 0; $i -lt $Text.Length; $i++) {
            $TextToFile += $Text[$i]
        }
        Write-Output "[$([datetime]::Now.ToString($TimeFormat))]$TextToFile" | Out-File $LogFile -Encoding unicode -Append
    }
}

$Path="IRO.sln";
$IsRelease="";
$Configuration="";

function WriteOperationResultByExitCode($text, $exitCode)
{    
  if($exitCode -eq 0)
  {
    $text=$text+" Successful";	
    Write-Color -Text $text -Color Green;	
  }
  else
  {
    $text=$text+" Failed";
    Write-Color -Text $text -Color Red;
  }
  $exitCodeInfo="Exit code is: "+ $exitCode;  
  Write-Host $exitCodeInfo;
  Write-Host "";
}

function ReadBool($hint)
{
  $hint=$hint+" (y/n) ";
  $answer = Read-Host $hint
  if($answer -eq 'y'){
    return 1;
  }
  return 0;
}

function CleanSolution($path)
{  
  dotnet clean $path;
  Write-Output $lastexitcode;
  $exitCode=$lastexitcode;
  WriteOperationResultByExitCode "Solution/Project clean status: "  $exitCode;
  return $exitCode;
}

function RestoreNugets($path)
{
  dotnet restore $path; 
  $exitCode=$lastexitcode;
  WriteOperationResultByExitCode "Nuget restore status: "  $exitCode;
  return $exitCode;
}

function AskConfiguration()
{
  $isRelease = ReadBool "Please enter 'y' if you wan't to build in release mode: "; 
  $global:IsRelease=$isRelease;  
  if($isRelease)
  {
    $global:Configuration="Release";
  }
  else
  {
    $global:Configuration="Debug";
  }
}

function DotnetBuild($path)
{     
  dotnet build $path --configuration $global:Configuration
  $exitCode=$lastexitcode;
  Write-Host "Builded in " + $global:Configuration + " mode.";
  WriteOperationResultByExitCode "Dotnet build status: " $exitCode;
  return $exitCode;
}

$CleanExitStatus=CleanSolution $Path;
pause;

$NugetRestoreExitStatus=RestoreNugets $Path;
pause;

AskConfiguration;
$BuildExitStatus=DotnetBuild $path ;
Write-Host '--------- BUILD SCRIPT FINISHED ---------';
WriteOperationResultByExitCode "Solution/Project clean status: " $CleanExitStatus;
WriteOperationResultByExitCode "Nuget restore status: " $NugetRestoreExitStatus;
WriteOperationResultByExitCode "Dotnet build status: " $BuildExitStatus;
$BuildScriptStatus= $CleanExitStatus+$NugetRestoreExitStatus+$BuildExitStatus;
WriteOperationResultByExitCode "All script operations status: " $BuildExitStatus;
pause;
exit $BuildScriptStatus;
pause;