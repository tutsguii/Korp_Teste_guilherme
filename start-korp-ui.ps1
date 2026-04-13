$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$uiPath = Join-Path $repoRoot 'korp-notas-ui'

if (-not (Test-Path $uiPath)) {
    Write-Error "Pasta do Angular nao encontrada: $uiPath"
}

$npmCmd = Get-Command npm.cmd -ErrorAction SilentlyContinue
if (-not $npmCmd) {
    Write-Error 'npm.cmd nao encontrado. Instale o Node.js antes de iniciar o frontend.'
}

$packageLock = Join-Path $uiPath 'package-lock.json'
$nodeModules = Join-Path $uiPath 'node_modules'

if ((Test-Path $packageLock) -and -not (Test-Path $nodeModules)) {
    Write-Host 'Instalando dependencias do Angular...'
    & $npmCmd.Source install
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host 'Iniciando Angular em http://localhost:4200 ...'
Start-Process powershell -ArgumentList @(
    '-NoExit',
    '-ExecutionPolicy', 'Bypass',
    '-Command', "Set-Location '$uiPath'; npm.cmd start"
) -WorkingDirectory $uiPath

Write-Host 'Frontend iniciado em uma nova janela do PowerShell.'
