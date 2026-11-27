<#
.SYNOPSIS
    Idempotent local development seeder for AquaAPI + ESP32/Wokwi.

.DESCRIPTION
    Seeds a demo user, controller (default Wokwi MAC 24:0A:C4:00:01:10), ecosystem,
    6 sensors matching diagram.json, 4 relays (CH1..CH4), 2 automation rules,
    1 lighting schedule, historical telemetry for charts, and a FirmwareService hardware profile.
    Generates gitignored firmware/AquaSmart_ESP32/src/LocalDevSecrets.h so ESP32/Wokwi
    connects to your current LAN IP automatically after rebuild.

.EXAMPLE
    .\scripts\seed-local-dev.ps1 -BuildFirmware

.EXAMPLE
    .\scripts\seed-local-dev.ps1 -LanHost "host.wokwi.internal" -BuildFirmware

.EXAMPLE
    .\scripts\seed-local-dev.ps1 -MacAddress "30:AE:A4:07:0B:44" -WifiSsid "MyWiFi" -WifiPass "secret" -BuildFirmware
#>
[CmdletBinding()]
param(
    [string]$GatewayUrl = "http://localhost:5055",
    [string]$LanHost = "",
    [string]$MacAddress = "24:0A:C4:00:01:10",
    [string]$Email = "dev@aquasmart.local",
    [string]$Password = "DevPassword123!",
    [string]$PhoneNumber = "+375291112233",
    [string]$WifiSsid = "Wokwi-GUEST",
    [string]$WifiPass = "",
    [int]$TelemetryHours = 6,
    [switch]$SkipTelemetry,
    [switch]$RecreateController,
    [switch]$NoFirmwareSecrets,
    [switch]$BuildFirmware
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$SeedStatePath = Join-Path $PSScriptRoot ".dev-seed.json"
$LocalSecretsPath = Join-Path $RepoRoot "firmware\AquaSmart_ESP32\src\LocalDevSecrets.h"

function Write-Step([string]$Message) {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Cyan
}

function Write-Ok([string]$Message) {
    Write-Host "  [OK] $Message" -ForegroundColor Green
}

function Write-Warn([string]$Message) {
    Write-Host "  [WARN] $Message" -ForegroundColor Yellow
}

function Resolve-LanHost([string]$ExplicitHost) {
    if (-not [string]::IsNullOrWhiteSpace($ExplicitHost)) {
        return $ExplicitHost.Trim()
    }

    $candidates = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object {
            $_.IPAddress -notlike '127.*' -and
            $_.IPAddress -notlike '169.254.*' -and
            $_.InterfaceAlias -notlike '*vEthernet*' -and
            $_.InterfaceAlias -notlike '*WSL*' -and
            $_.InterfaceAlias -notlike '*Docker*' -and
            $_.InterfaceAlias -notlike '*VPN*' -and
            $_.InterfaceAlias -notlike '*Radmin*' -and
            $_.InterfaceAlias -notlike '*ZeroTier*' -and
            $_.InterfaceAlias -notlike '*Tailscale*'
        })

    $lan192 = $candidates | Where-Object { $_.IPAddress -like '192.168.*' } | Select-Object -First 1
    if ($lan192) { return $lan192.IPAddress }

    $lanPrivate = $candidates | Where-Object { $_.IPAddress -like '10.*' -or $_.IPAddress -like '172.*' } | Select-Object -First 1
    if ($lanPrivate) { return $lanPrivate.IPAddress }

    return "host.wokwi.internal"
}

function Normalize-Mac([string]$RawMac) {
    $hex = ($RawMac -replace '[^0-9A-Fa-f]', '').ToUpperInvariant()
    if ($hex.Length -ne 12) {
        throw "Invalid MAC address '$RawMac' (expected 12 hex characters)."
    }
    return "{0}:{1}:{2}:{3}:{4}:{5}" -f `
        $hex.Substring(0, 2), $hex.Substring(2, 2), $hex.Substring(4, 2), `
        $hex.Substring(6, 2), $hex.Substring(8, 2), $hex.Substring(10, 2)
}

function Invoke-ApiJson {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Url,
        [hashtable]$Headers = @{},
        [object]$Body = $null,
        [int]$TimeoutSec = 15
    )

    $params = @{
        Method      = $Method
        Uri         = $Url
        Headers     = $Headers
        TimeoutSec  = $TimeoutSec
        ErrorAction = "Stop"
    }
    if ($null -ne $Body) {
        $params.ContentType = "application/json"
        $params.Body = ($Body | ConvertTo-Json -Depth 10 -Compress)
    }
    for ($try = 1; $try -le 4; $try++) {
        try {
            return Invoke-RestMethod @params
        } catch {
            $status = Get-HttpStatusCode $_
            if ($status -eq 409 -and $try -lt 4) {
                Start-Sleep -Milliseconds (300 * $try)
                continue
            }
            throw
        }
    }
}

function Get-HttpStatusCode($ErrorRecord) {
    try {
        if ($ErrorRecord.Exception.Response -and $ErrorRecord.Exception.Response.StatusCode) {
            return [int]$ErrorRecord.Exception.Response.StatusCode
        }
    } catch {}
    return 0
}

function As-List($Value) {
    if ($null -eq $Value) {
        return ,@()
    }
    return ,@($Value | Where-Object { $null -ne $_ })
}

function Load-CachedState {
    if (Test-Path $SeedStatePath) {
        try {
            return (Get-Content -Raw -Path $SeedStatePath | ConvertFrom-Json)
        } catch {
            return $null
        }
    }
    return $null
}

# 0. Pre-flight: resolve MAC & LAN IP
$CanonicalMac = Normalize-Mac $MacAddress
$CompactMac = $CanonicalMac.Replace(":", "")
$ResolvedLanHost = Resolve-LanHost $LanHost
$Cached = Load-CachedState

Write-Step "AquaSmart Local Dev Seeder"
Write-Host "  Gateway:  $GatewayUrl"
Write-Host "  LAN Host: $ResolvedLanHost (API :5055 / MQTT :1883)"
Write-Host "  ESP MAC:  $CanonicalMac"
Write-Host "  DemoUser: $Email"

# 1. Register or login demo user
Write-Step "1/8 Authenticating demo user ($Email)..."
$accessToken = $null
try {
    $reg = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/identity/v1/auth/register" -Body @{
        email       = $Email
        phoneNumber = $PhoneNumber
        name        = "AquaSmart Developer"
        password    = $Password
        timeZone    = "UTC"
    }
    $accessToken = $reg.accessToken
    Write-Ok "Registered new demo user: $Email"
} catch {
    $code = Get-HttpStatusCode $_
    if ($code -eq 400 -or $code -eq 409) {
        $login = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/identity/v1/auth/login" -Body @{
            email    = $Email
            password = $Password
        }
        $accessToken = $login.accessToken
        Write-Ok "Logged in existing demo user: $Email"
    } else {
        throw
    }
}

$JwtHeaders = @{ Authorization = "Bearer $accessToken" }

# 2. Controller (find or create for $CanonicalMac)
Write-Step "2/8 Ensuring controller for MAC $CanonicalMac..."
$controllers = As-List (Invoke-ApiJson -Method Get -Url "$GatewayUrl/api/device/v1/controllers?take=50" -Headers $JwtHeaders)
$existingCtrl = $controllers | Where-Object {
    $_.macAddress -and ((Normalize-Mac $_.macAddress) -eq $CanonicalMac)
} | Select-Object -First 1

if ($existingCtrl -and $RecreateController) {
    Write-Warn "RecreateController requested: deleting controller $($existingCtrl.id)..."
    Invoke-ApiJson -Method Delete -Url "$GatewayUrl/api/device/v1/controllers/$($existingCtrl.id)" -Headers $JwtHeaders | Out-Null
    # Evict stale FusionCache Redis key so the re-created controller with the same MAC authenticates immediately
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker exec aqua-redis redis-cli DEL "v0:controller:mac:$CanonicalMac" "v0:controller:id:$($existingCtrl.id)" 2>$null | Out-Null
    }
    $existingCtrl = $null
}

$ControllerId = $null
$DeviceToken = $null
$IsNewController = $false

if ($existingCtrl) {
    $ControllerId = [string]$existingCtrl.id
    if ($Cached -and $Cached.controllerId -eq $ControllerId -and -not [string]::IsNullOrWhiteSpace($Cached.deviceToken)) {
        $DeviceToken = [string]$Cached.deviceToken
        Write-Ok "Reusing existing controller $ControllerId (token loaded from scripts/.dev-seed.json)"
    } else {
        throw "Controller $ControllerId already exists for MAC $CanonicalMac, but plaintext deviceToken is not in scripts/.dev-seed.json. Re-run with -RecreateController to issue a new token."
    }
} else {
    $createdCtrl = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/device/v1/controllers" -Headers $JwtHeaders -Body @{
        macAddress = $CanonicalMac
        name       = "AquaSmart Wokwi ESP32"
        isOnline   = $false
    }
    $ControllerId = [string]$createdCtrl.controllerId
    $DeviceToken = [string]$createdCtrl.deviceToken
    $IsNewController = $true
    Write-Ok "Created controller $ControllerId (MAC $CanonicalMac)"
    # Persist token immediately so re-runs never lose it if a later step is interrupted
    [ordered]@{
        seededAtUtc  = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        gatewayUrl   = $GatewayUrl
        lanHost      = $ResolvedLanHost
        email        = $Email
        password     = $Password
        macAddress   = $CanonicalMac
        controllerId = $ControllerId
        deviceToken  = $DeviceToken
    } | ConvertTo-Json -Depth 4 | Set-Content -Path $SeedStatePath -Encoding UTF8
}

$DevHeaders = @{
    "X-Device-Token" = $DeviceToken
    "X-Mac-Address"  = $CanonicalMac
}

# 3. Ecosystem in ControlService (create BEFORE sensors/relays so RabbitMQ syncs them cleanly)
Write-Step "3/8 Ensuring ecosystem in ControlService..."
$ecosystems = As-List (Invoke-ApiJson -Method Get -Url "$GatewayUrl/api/control/v1/ecosystems?controllerId=$ControllerId&take=20" -Headers $JwtHeaders)
$existingEco = $ecosystems | Select-Object -First 1
if ($existingEco) {
    $EcosystemId = [string]$existingEco.id
    Write-Ok "Reusing ecosystem $EcosystemId ($($existingEco.name))"
} else {
    $EcosystemId = [string](Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/control/v1/ecosystems" -Headers $JwtHeaders -Body @{
        type         = 1
        name         = "Main Aquarium"
        volume       = 120.0
        controllerId = $ControllerId
    })
    Write-Ok "Created ecosystem $EcosystemId (Main Aquarium, 120L)"
}

# 4. Sensors (6 sensors matching diagram.json) & 4 Relays (CH1..CH4, CH5..CH8 left free)
Write-Step "4/8 Ensuring 6 sensors and 4 relays (CH1..CH4)..."
$existingSensors = @()
$existingRelays = @()
if (-not $IsNewController) {
    $runtimeCfg = Invoke-ApiJson -Method Get -Url "$GatewayUrl/api/device/v1/controllers/me/config" -Headers $DevHeaders
    $existingSensors = As-List $runtimeCfg.sensors
    $existingRelays = As-List $runtimeCfg.relays
}

$desiredSensors = @(
    @{ name = "Water Temp";    connectionProtocol = 4; connectionAddress = "28000000000001AA"; type = 1; key = "water_temperature" },
    @{ name = "Surface Temp";  connectionProtocol = 4; connectionAddress = "28000000000002BB"; type = 1; key = "surface_temperature" },
    @{ name = "Air Temp";      connectionProtocol = 3; connectionAddress = "0x38";             type = 1; key = "air_temperature" },
    @{ name = "Air Humidity";  connectionProtocol = 3; connectionAddress = "0x39";             type = 3; key = "air_humidity" },
    @{ name = "Soil Moisture"; connectionProtocol = 3; connectionAddress = "0x36";             type = 3; key = "soil_moisture" },
    @{ name = "Water Level";   connectionProtocol = 1; connectionAddress = "34";               type = 1; key = "water_level" }
)

$sensorMap = @{}
foreach ($s in $desiredSensors) {
    $found = $existingSensors | Where-Object { $_.name -eq $s.name } | Select-Object -First 1
    if ($found) {
        $sensorMap[$s.key] = [string]$found.sensorId
        Write-Ok "Sensor exists: $($s.name) ($($found.sensorId))"
    } else {
        $res = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/device/v1/sensors" -Headers $JwtHeaders -Body @{
            controllerId       = $ControllerId
            name               = $s.name
            connectionProtocol = $s.connectionProtocol
            connectionAddress  = $s.connectionAddress
            type               = $s.type
        }
        $sensorMap[$s.key] = [string]$res.id
        Write-Ok "Created sensor: $($s.name) ($($res.id))"
    }
}

$desiredRelays = @(
    @{ name = "Filter"; connectionAddress = "16"; purpose = 3; key = "filter" },
    @{ name = "Heater"; connectionAddress = "17"; purpose = 4; key = "heater" },
    @{ name = "Light";  connectionAddress = "18"; purpose = 2; key = "light"  },
    @{ name = "Pump";   connectionAddress = "19"; purpose = 3; key = "pump"   }
)

$relayMap = @{}
foreach ($r in $desiredRelays) {
    $found = $existingRelays | Where-Object { $_.name -eq $r.name } | Select-Object -First 1
    if ($found) {
        $relayMap[$r.key] = [string]$found.relayId
        Write-Ok "Relay exists: $($r.name) CH/$($r.connectionAddress) ($($found.relayId))"
    } else {
        $res = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/device/v1/relays" -Headers $JwtHeaders -Body @{
            controllerId       = $ControllerId
            name               = $r.name
            connectionProtocol = 1
            connectionAddress  = $r.connectionAddress
            isNormallyOpen     = $true
            purpose            = $r.purpose
            isActive           = $false
            isManual           = $false
        }
        $relayMap[$r.key] = [string]$res.id
        Write-Ok "Created relay: $($r.name) GPIO $($r.connectionAddress) ($($res.id))"
    }
}

# 5. Automation Rules & Lighting Schedule in ControlService (retry briefly until RabbitMQ sync lands)
Write-Step "5/8 Ensuring automation rules (Heater) and schedule (Light)..."
$existingRules = As-List (Invoke-ApiJson -Method Get -Url "$GatewayUrl/api/control/v1/automation-rules?ecosystemId=$EcosystemId&take=50" -Headers $JwtHeaders)

$desiredRules = @(
    @{
        name       = "Heat ON (< 24.0 C)"
        relayId    = $relayMap["heater"]
        action     = 1  # SwitchOn
        conditions = @(@{ sensorId = $sensorMap["water_temperature"]; condition = 3; threshold = 24.0; hysteresis = 0.5 })
    },
    @{
        name       = "Heat OFF (> 27.0 C)"
        relayId    = $relayMap["heater"]
        action     = 2  # SwitchOff
        conditions = @(@{ sensorId = $sensorMap["water_temperature"]; condition = 2; threshold = 27.0; hysteresis = 0.5 })
    }
)

foreach ($rule in $desiredRules) {
    $foundRule = $existingRules | Where-Object { $_.name -eq $rule.name } | Select-Object -First 1
    if ($foundRule) {
        Write-Ok "Rule exists: $($rule.name)"
        continue
    }

    $created = $false
    for ($attempt = 1; $attempt -le 10; $attempt++) {
        try {
            Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/control/v1/automation-rules" -Headers $JwtHeaders -Body @{
                ecosystemId = $EcosystemId
                relayId     = $rule.relayId
                name        = $rule.name
                operator    = 1
                action      = $rule.action
                isActive    = $true
                conditions  = $rule.conditions
            } | Out-Null
            Write-Ok "Created rule: $($rule.name)"
            $created = $true
            break
        } catch {
            if ($attempt -lt 10) {
                Start-Sleep -Seconds 1
            } else {
                throw
            }
        }
    }
}

$existingSchedules = As-List (Invoke-ApiJson -Method Get -Url "$GatewayUrl/api/control/v1/schedules?ecosystemId=$EcosystemId" -Headers $JwtHeaders)
$lightSchedule = $existingSchedules | Where-Object { $_.relayId -eq $relayMap["light"] } | Select-Object -First 1
if ($lightSchedule) {
    Write-Ok "Schedule exists for Light ($($lightSchedule.cronExpression))"
} else {
    for ($attempt = 1; $attempt -le 8; $attempt++) {
        try {
            Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/control/v1/schedules" -Headers $JwtHeaders -Body @{
                ecosystemId    = $EcosystemId
                relayId        = $relayMap["light"]
                cronExpression = "0 8 * * *"
                durationMin    = 600
                isFadeMode     = $false
                isEnabled      = $true
            } | Out-Null
            Write-Ok "Created schedule for Light (0 8 * * *, 600 min)"
            break
        } catch {
            if ($attempt -lt 8) { Start-Sleep -Seconds 1 } else { throw }
        }
    }
}

# 6. Historical telemetry (for frontend charts & TelemetryService)
if (-not $SkipTelemetry) {
    Write-Step "6/8 Seeding ${TelemetryHours}h historical telemetry (15-min intervals)..."
    $nowUtc = [DateTime]::UtcNow
    # Anchor timestamps to 15-min UTC slots so re-runs within the same slot deduplicate cleanly.
    $anchorMin = [Math]::Floor($nowUtc.Minute / 15) * 15
    $anchorUtc = [DateTime]::new($nowUtc.Year, $nowUtc.Month, $nowUtc.Day, $nowUtc.Hour, $anchorMin, 0, [DateTimeKind]::Utc)
    $steps = [Math]::Max(4, $TelemetryHours * 4)

    $allItems = New-Object System.Collections.Generic.List[object]
    for ($i = $steps; $i -ge 1; $i--) {
        $ts = $anchorUtc.AddMinutes(-15 * $i)
        $tsIso = $ts.ToString("yyyy-MM-ddTHH:mm:ssZ")
        $slotTag = $ts.ToString("yyyyMMddHHmm")
        $phase = ($steps - $i) * 0.35

        $readings = @(
            @{ key = "water_temperature";   idx = 0; val = [Math]::Round(25.2 + 0.8 * [Math]::Sin($phase), 2) },
            @{ key = "surface_temperature"; idx = 1; val = [Math]::Round(23.1 + 0.6 * [Math]::Cos($phase), 2) },
            @{ key = "air_temperature";     idx = 2; val = [Math]::Round(24.5 + 1.0 * [Math]::Sin($phase * 0.7), 2) },
            @{ key = "air_humidity";        idx = 3; val = [Math]::Round(56.0 + 4.5 * [Math]::Cos($phase * 0.6), 1) },
            @{ key = "soil_moisture";       idx = 4; val = [Math]::Round(62.0 - 0.15 * ($steps - $i), 1) },
            @{ key = "water_level";         idx = 5; val = 1.0 }
        )

        foreach ($rd in $readings) {
            $allItems.Add(@{
                sensorId          = $sensorMap[$rd.key]
                value             = $rd.val
                externalMessageId = "SEED-$CompactMac-$slotTag-$($rd.idx)"
                recordedAt        = $tsIso
            })
        }
    }

    $sent = 0
    for ($offset = 0; $offset -lt $allItems.Count; $offset += 50) {
        $end = [Math]::Min($offset + 49, $allItems.Count - 1)
        $chunk = @($allItems.GetRange($offset, ($end - $offset + 1)))
        Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/telemetry/v1/data" -Headers $DevHeaders -Body @{
            macAddress = $CanonicalMac
            items      = $chunk
        } | Out-Null
        $sent += $chunk.Count
    }
    Write-Ok "Ingested $sent historical telemetry records across 6 sensors"
} else {
    Write-Step "6/8 Skipping historical telemetry (-SkipTelemetry)."
}

# 7. FirmwareService Hardware Profile
Write-Step "7/8 Ensuring FirmwareService hardware profile..."
$HardwareProfileId = $null
if ($Cached -and $Cached.PSObject.Properties['hardwareProfileId'] -and -not [string]::IsNullOrWhiteSpace($Cached.hardwareProfileId)) {
    $HardwareProfileId = [string]$Cached.hardwareProfileId
    Write-Ok "Reusing cached hardwareProfileId $HardwareProfileId"
} else {
    $hw = Invoke-ApiJson -Method Post -Url "$GatewayUrl/api/firmware/v1/hardware-profiles" -Headers $JwtHeaders -Body @{
        name          = "ESP32-DevKitC-v4"
        board_revision = "rev4"
    }
    $HardwareProfileId = [string]$hw.id
    Write-Ok "Created hardware profile $HardwareProfileId (ESP32-DevKitC-v4)"
}

# 8. Save local state JSON & firmware/AquaSmart_ESP32/src/LocalDevSecrets.h
Write-Step "8/8 Writing local seed state and firmware LocalDevSecrets.h..."
$seedState = [ordered]@{
    seededAtUtc       = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
    gatewayUrl        = $GatewayUrl
    lanHost           = $ResolvedLanHost
    email             = $Email
    password          = $Password
    macAddress        = $CanonicalMac
    controllerId      = $ControllerId
    deviceToken       = $DeviceToken
    ecosystemId       = $EcosystemId
    hardwareProfileId = $HardwareProfileId
    sensors           = $sensorMap
    relays            = $relayMap
}
$seedState | ConvertTo-Json -Depth 6 | Set-Content -Path $SeedStatePath -Encoding UTF8
Write-Ok "Saved state to scripts/.dev-seed.json (gitignored)"

if (-not $NoFirmwareSecrets) {
    $headerContent = @"
// Auto-generated by scripts/seed-local-dev.ps1 (gitignored — do not commit!)
#ifndef LOCAL_DEV_SECRETS_H
#define LOCAL_DEV_SECRETS_H

#define LOCAL_DEV_WIFI_SSID      "$WifiSsid"
#define LOCAL_DEV_WIFI_PASS      "$WifiPass"
#define LOCAL_DEV_API_URL        "http://${ResolvedLanHost}:5055"
#define LOCAL_DEV_MQTT_HOST      "$ResolvedLanHost"
#define LOCAL_DEV_CONTROLLER_ID  "$ControllerId"
#define LOCAL_DEV_DEVICE_TOKEN   "$DeviceToken"

#endif
"@
    Set-Content -Path $LocalSecretsPath -Value $headerContent -Encoding UTF8
    Write-Ok "Generated firmware/AquaSmart_ESP32/src/LocalDevSecrets.h (LAN=$ResolvedLanHost)"
}

if ($BuildFirmware) {
    Write-Step "Building ESP32 firmware with generated LocalDevSecrets.h..."
    $pio = Get-Command pio -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    if (-not $pio) {
        $fallbackPio = Join-Path $env:USERPROFILE ".platformio\penv\Scripts\pio.exe"
        if (Test-Path $fallbackPio) { $pio = $fallbackPio }
    }
    if (-not $pio) {
        throw "PlatformIO CLI (pio) not found. Build manually in VS Code PlatformIO."
    }
    & $pio run -d (Join-Path $RepoRoot "firmware\AquaSmart_ESP32") -e esp32dev
    if ($LASTEXITCODE -ne 0) {
        throw "PlatformIO build failed with exit code $LASTEXITCODE"
    }
    Write-Ok "Firmware built successfully!"
}

Write-Host "`n================ LOCAL DEV SEED COMPLETE ================" -ForegroundColor Green
Write-Host "  Demo Login:    $Email / $Password"
Write-Host "  LAN Target:    http://${ResolvedLanHost}:5055  |  mqtt://${ResolvedLanHost}:1883"
Write-Host "  ControllerId:  $ControllerId"
Write-Host "  MAC Address:   $CanonicalMac"
Write-Host "  EcosystemId:   $EcosystemId"
Write-Host "  Relays (4):    Filter (16), Heater (17), Light (18), Pump (19) | CH5..CH8 free"
Write-Host "  Sensors (6):   Water Temp, Surface Temp, Air Temp, Air Humidity, Soil Moisture, Water Level"
Write-Host "=========================================================`n" -ForegroundColor Green
