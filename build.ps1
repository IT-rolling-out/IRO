[CmdletBinding()]
Param(
    #[switch]$CustomParam,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$NotSilent
)

if($NotSilent -eq 1){
  $Silent=0;
  $NotSilent=1;
}
else{  
  $Silent=1;
  $NotSilent=0;
}
Write-Host "Is silent: " $Silent;

###########################################################################
# BASE FUNCTIONS
###########################################################################

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

function SPause{
 if($NotSilent -eq 1){
   pause
 }
}

function CustomWrite([String[]]$Text, [ConsoleColor[]]$Color = "White"){
  $Text=">>>>> "+$Text;
  Write-Color -Text $Text -Color $Color;
}

function WriteOperationResultByExitCode($text, $exitCode)
{    
  if($exitCode)
  {
    $text=$text+" Failed";
    CustomWrite -Text $text -Color Red;
  }
  else
  {    
	$text=$text+" Successful";	
    CustomWrite -Text $text -Color Green;	
  }
  Write-Host "";
}

function ReadBool($hint)
{
  $hint=$hint+" y/n (y) ";
  if($Silent -eq 1)
  {
    Write-Host $hint;
    return 1;
  }
  $answer = Read-Host $hint
  if($answer -eq 'n'){
    return 0;
  }
  return 1;
}

###########################################################################
# CONFIGURATION
###########################################################################

$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

function AskConfiguration()
{
  $global:IsRelease = ReadBool "Please enter 'y' if you wan't to build in release mode"; 
  $global:IsRelease=$global:IsRelease;  
  if($global:IsRelease)
  {
    $global:Configuration="Release";
  }
  else
  {
    $global:Configuration="Debug";
  }
}

function DotnetBuildInfo()
{     
  # dotnet build $path --configuration $global:Configuration
  $exitCode=$lastexitcode;
  Write-Host "Builded in " + $global:Configuration + " mode.";
  WriteOperationResultByExitCode "Dotnet build status: " $exitCode;
  return $exitCode;
}

###########################################################################
# EXECUTION
###########################################################################

# $global:Path="$PSScriptRoot\IRO.sln";

$global:IsRelease="";
$global:Configuration="";
$global:BuildExitCode="";

# Remove old nupkg.
$WantRemoveNupkgFromSrc = ReadBool "Want to remove all .nupkg files from '.\src' before build? ";
if($WantRemoveNupkgFromSrc){
  & "$PSScriptRoot\scripts\delete_nupkgs.cmd" "$PSScriptRoot\src" $Silent
  CustomWrite "Removed .nupkg files." Green;
  SPause;  
}

# Build
AskConfiguration;

Get-ChildItem "$PSScriptRoot" -Recurse -Filter "*.sln" | Foreach-Object {    
  $SlnPath=$_.FullName;
  Write-Host "Building solution: " $SlnPath;
  dotnet restore $SlnPath /clp:ErrorsOnly
  dotnet build $SlnPath --configuration $global:Configuration /clp:ErrorsOnly
  $global:BuildExitCode=$lastexitcode;
  WriteOperationResultByExitCode "Solution build status: " $lastexitcode
  SPause;
}

# Tests
$WantExUnitTests = ReadBool "Want execute unit tests? "
if($WantExUnitTests){
  $testRes=0;
  Get-ChildItem "$PSScriptRoot" -Recurse -Filter "*UnitTest*.csproj" | 
  Foreach-Object {    
    dotnet test $_.FullName --configuration $global:Configuration --verbosity  m
	if($lastexitcode){
	  $testRes=1;
	}
  }
  WriteOperationResultByExitCode "Tests execution status: " $testRes;
  SPause;
}

# Copy nugets to output/nuget
$WantClearOutputNuget = ReadBool "Want clear '.\output\nupkg' and fill with new builded packages? "
if($WantClearOutputNuget){
  if(Test-Path -Path "$PSScriptRoot\output\nuget"){
    rd "$PSScriptRoot\output\nuget" -recurse;  
	CustomWrite "Removed old." Green	
  }    
  & "$PSScriptRoot\scripts\copy_nupkgs.cmd" "$PSScriptRoot\src\" "$PSScriptRoot\output\nuget\" $global:IsRelease $Silent
  CustomWrite "Copied." Green
}
SPause
exit $global:BuildExitCode;