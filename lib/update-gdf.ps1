<#
    Haalt de nieuwste versie van GDF op van gdf.acits.nl en vervangt de map lib van dit project.
    Wordt gestart door update-gdf.bat. Je eigen code en je map Data blijven ongemoeid.

    De oude map lib wordt hernoemd naar lib-oud-<versie>, zodat je altijd terug kunt.
#>
param(
    [string]$ProjectMap = (Split-Path $PSScriptRoot -Parent),
    [string]$Server = 'https://gdf.acits.nl/',
    [switch]$Forceer   # ook downloaden als je al de nieuwste versie hebt
)

$ErrorActionPreference = 'Stop'
$libMap = Join-Path $ProjectMap 'lib'

function Regel($tekst, $kleur = 'Gray') { Write-Host $tekst -ForegroundColor $kleur }

Regel ""
Regel "GDF bijwerken" Cyan
Regel "-------------"

# Welke versie heb je nu?
$huidig = 'onbekend'
$versieBestand = Join-Path $libMap 'gdf-versie.txt'
if (Test-Path $versieBestand) { $huidig = (Get-Content $versieBestand -TotalCount 1).Trim() }
Regel "Deze map gebruikt GDF $huidig"

# Welke versie staat er online?
try {
    $versionsTxt = (Invoke-WebRequest -Uri ($Server + 'versions.txt') -UseBasicParsing -TimeoutSec 20).Content
}
catch {
    Regel "Kan de server niet bereiken ($Server). Heb je internet?" Red
    exit 1
}

$nieuwste = ($versionsTxt -split "`n" | Where-Object { $_ -like 'latestVersion:*' } | Select-Object -First 1)
$nieuwste = if ($nieuwste) { $nieuwste.Split(':')[1].Trim() } else { $null }
if (-not $nieuwste) { Regel "Kon de nieuwste versie niet bepalen." Red; exit 1 }

Regel "Nieuwste versie online: $nieuwste"
if ($huidig -eq $nieuwste -and -not $Forceer) {
    Regel ""
    Regel "Je bent al bij. Er is niets te doen." Green
    exit 0
}

# Downloaden
$zipUrl = $Server + "gdf-lib-$nieuwste.zip"
$tijdelijk = Join-Path ([IO.Path]::GetTempPath()) ("gdf-update-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $tijdelijk | Out-Null
$zip = Join-Path $tijdelijk 'gdf-lib.zip'

Regel ""
Regel "Downloaden van $zipUrl ..."
try {
    $voortgang = $ProgressPreference; $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest -Uri $zipUrl -OutFile $zip -UseBasicParsing -TimeoutSec 600
    $ProgressPreference = $voortgang
}
catch {
    Regel "Downloaden is mislukt: $($_.Exception.Message)" Red
    Remove-Item $tijdelijk -Recurse -Force -ErrorAction SilentlyContinue
    exit 1
}
Regel ("   {0} MB opgehaald" -f [Math]::Round((Get-Item $zip).Length / 1MB, 1))

# Uitpakken en controleren voor we iets vervangen
Regel "Uitpakken ..."
Expand-Archive -Path $zip -DestinationPath $tijdelijk -Force
$nieuweLib = Join-Path $tijdelijk 'lib'
if (-not (Test-Path (Join-Path $nieuweLib 'GDF.dll'))) {
    Regel "Het gedownloade bestand klopt niet (GDF.dll ontbreekt). Er is niets gewijzigd." Red
    Remove-Item $tijdelijk -Recurse -Force -ErrorAction SilentlyContinue
    exit 1
}

# De oude map lib bewaren
if (Test-Path $libMap) {
    $backup = Join-Path $ProjectMap "lib-oud-$huidig"
    $nummer = 2
    while (Test-Path $backup) { $backup = Join-Path $ProjectMap "lib-oud-$huidig($nummer)"; $nummer++ }
    Move-Item $libMap $backup
    Regel "De oude versie staat nu in $(Split-Path $backup -Leaf)"
}

Move-Item $nieuweLib $libMap
Remove-Item $tijdelijk -Recurse -Force -ErrorAction SilentlyContinue

Regel ""
Regel "Klaar. Dit project gebruikt nu GDF $nieuwste." Green
Regel "Open je project in Visual Studio en bouw het opnieuw (Ctrl+Shift+B)."
Regel "Werkt alles? Dan mag de map lib-oud-$huidig weg."
