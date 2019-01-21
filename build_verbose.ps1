$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent;
& "$PSScriptRoot\build" -NotSilent 1