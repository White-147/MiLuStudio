param(
    [string]$ProjectRoot = "D:\code\MiLuStudio"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$serviceRoot = Join-Path $ProjectRoot ".tmp\dev-services"
$stateFile = Join-Path $serviceRoot "services.json"

if (-not (Test-Path -LiteralPath $stateFile)) {
    Write-Host "No MiLuStudio local service state file found: $stateFile"
    exit 0
}

$state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
$buildOutput = [string]$state.buildOutput

function Test-CommandLineContains {
    param(
        [string]$CommandLine,
        [string]$Needle
    )

    return $CommandLine -and
        $Needle -and
        $CommandLine.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

foreach ($service in $state.services) {
    $processId = [int]$service.pid
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if (-not $process) {
        continue
    }

    $commandLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $processId" -ErrorAction SilentlyContinue).CommandLine
    $expectedCommand = [string]$service.expectedCommand
    $isExpected = (Test-CommandLineContains -CommandLine $commandLine -Needle $expectedCommand) -and
        (Test-CommandLineContains -CommandLine $commandLine -Needle $buildOutput)

    if (-not $isExpected) {
        Write-Warning "Skip PID $processId because it no longer matches the recorded MiLuStudio service command."
        continue
    }

    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    Write-Host "Stopped $($service.name) PID $processId."
}

Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
Write-Host "MiLuStudio local service state cleared."
