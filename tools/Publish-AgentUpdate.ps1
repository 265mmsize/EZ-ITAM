<#
.SYNOPSIS
  에이전트 업데이트 패키지를 만들어 서버의 링 폴더에 배치합니다.
.EXAMPLE
  .\tools\Publish-AgentUpdate.ps1 -Version 1.0.1 -Ring Pilot
  파일럿 링에 먼저 올린 뒤, 문제가 없으면 -Ring Stable 로 같은 버전을 다시 배치합니다.
#>
param(
    [Parameter(Mandatory)] [string]$Version,
    [ValidateSet('Pilot', 'Stable')] [string]$Ring = 'Pilot',
    [string]$UpdatesRoot = (Join-Path $PSScriptRoot '..\src\EzItam.Api\updates')
)
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$pub = Join-Path $root "artifacts\agent-$Version"

dotnet publish (Join-Path $root 'src\EzItam.Agent\EzItam.Agent.csproj') -c Release -r win-x64 --self-contained false -p:Version=$Version -o $pub
if ($LASTEXITCODE -ne 0) { throw 'publish failed' }

# 운영 설정(appsettings*.json)은 패키지에서 뺍니다. 설치된 장치의 서버 주소와 키를 덮어쓰지 않기 위해서입니다.
Remove-Item (Join-Path $pub 'appsettings*.json') -ErrorAction SilentlyContinue

$dest = Join-Path $UpdatesRoot $Ring
New-Item -ItemType Directory -Force $dest | Out-Null
$zipName = "EzItam.Agent-$Version.zip"
$zipPath = Join-Path $dest $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath }
Compress-Archive -Path (Join-Path $pub '*') -DestinationPath $zipPath

$sha = (Get-FileHash $zipPath -Algorithm SHA256).Hash
[ordered]@{ Version = $Version; Sha256 = $sha; FileName = $zipName } |
    ConvertTo-Json | Set-Content (Join-Path $dest 'manifest.json') -Encoding UTF8
Write-Host "배치 완료: $dest ($Version, SHA256 $sha)"
