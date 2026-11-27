<#
.SYNOPSIS
    Runs all 5 ESP32 AquaCore Unity host test suites.
    Uses local `pio test -e native` when g++ is in PATH, otherwise compiles & runs offline in Docker (golang:1.25).
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$FwDir = Join-Path $RepoRoot "firmware\AquaSmart_ESP32"

$pio = Get-Command pio -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $pio) {
    $fallbackPio = Join-Path $env:USERPROFILE ".platformio\penv\Scripts\pio.exe"
    if (Test-Path $fallbackPio) { $pio = $fallbackPio }
}

if ((Get-Command g++ -ErrorAction SilentlyContinue) -and $pio) {
    Write-Host "[TEST] Running local PlatformIO native Unity tests..." -ForegroundColor Cyan
    & $pio test -d $FwDir -e native -v
    exit $LASTEXITCODE
}

Write-Host "[TEST] Local g++ not found in PATH — running all 5 Unity suites via Docker (golang:1.25)..." -ForegroundColor Cyan
if ($pio) {
    & $pio pkg install -d $FwDir -e native | Out-Null
}

$unitySrc = Join-Path $FwDir ".pio\libdeps\native\Unity\src\unity.c"
if (-not (Test-Path $unitySrc)) {
    throw "Native Unity library not cached at $unitySrc. Run 'pio pkg install -d firmware\AquaSmart_ESP32 -e native' first."
}

$bashScript = @'
set -e
gcc -c -I/src/.pio/libdeps/native/Unity/src -DUNITY_INCLUDE_DOUBLE -DUNITY_SUPPORT_64 /src/.pio/libdeps/native/Unity/src/unity.c -o /tmp/unity.o
for suite in test_ota_state_machine test_relay_manager test_telemetry_queue test_protocol_codecs test_ui_controller; do
  echo "=== $suite ==="
  g++ -std=gnu++17 -D UNIT_TEST -D UNITY_SUPPORT_64 \
    -I/src/lib/AquaCore/src \
    -I/src/.pio/libdeps/native/ArduinoJson/src \
    -I/src/.pio/libdeps/native/Unity/src \
    /src/test/$suite/test_main.cpp \
    /src/lib/AquaCore/src/*.cpp \
    /tmp/unity.o \
    -o /tmp/$suite
  /tmp/$suite
done
echo "=== ALL 5 UNITY SUITES PASSED ==="
'@

docker run --rm -v "${FwDir}:/src:ro" golang:1.25 bash -c $bashScript
exit $LASTEXITCODE
