#!/usr/bin/env python3
"""
Automated end-to-end acceptance test runner for AquaAPI MQTT migration & ESP32 integration.
Covers scenarios E2E-1..E2E-9 from docs/TEST_PLAN_MQTT_AND_ESP32.md against a running docker compose stack.

Requirements:
    pip install paho-mqtt requests

Usage:
    python scripts/e2e_mqtt_acceptance.py
"""

import hashlib
import json
import os
import subprocess
import sys
import time
import uuid
from datetime import datetime, timezone
from pathlib import Path

import paho.mqtt.client as mqtt
import requests

GATEWAY_URL = os.environ.get("GATEWAY_URL", "http://localhost:5055")
MQTT_HOST = os.environ.get("MQTT_HOST", "localhost")
MQTT_PORT = int(os.environ.get("MQTT_PORT", "1883"))
POSTGRES_CONTAINER = os.environ.get("POSTGRES_CONTAINER", "aqua-postgres")
DEVICE_API_CONTAINER = os.environ.get("DEVICE_API_CONTAINER", "device-api")
DEFAULT_FIRMWARE_BIN = Path(__file__).resolve().parent.parent / "firmware" / "AquaSmart_ESP32" / ".pio" / "build" / "esp32dev" / "firmware.bin"


def log(msg: str) -> None:
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {msg}", flush=True)


def psql(db: str, sql: str) -> str:
    cmd = [
        "docker", "exec", POSTGRES_CONTAINER,
        "psql", "-U", "postgres", "-d", db, "-t", "-A", "-c", sql,
    ]
    res = subprocess.run(cmd, capture_output=True, text=True, check=True)
    return res.stdout.strip()


def assert_eq(actual, expected, ctx: str) -> None:
    if actual != expected:
        raise AssertionError(f"{ctx}: expected {expected!r}, got {actual!r}")


def assert_true(cond: bool, ctx: str) -> None:
    if not cond:
        raise AssertionError(f"Assertion failed: {ctx}")


def utc_now_iso() -> str:
    return datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")


def make_mqtt_client(
    client_id: str,
    username: str,
    password: str,
    keepalive: int = 30,
    lwt_topic: str | None = None,
    lwt_payload: str | None = None,
):
    c = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=client_id, protocol=mqtt.MQTTv311)
    c.username_pw_set(username, password)
    if lwt_topic and lwt_payload:
        c.will_set(lwt_topic, lwt_payload, qos=1, retain=True)
    c.connected_event = False
    c.connect_rc = None
    c.disconnected_event = False
    c.messages = []

    def on_connect(client, userdata, flags, reason_code, properties):
        client.connect_rc = reason_code
        if reason_code == 0:
            client.connected_event = True

    def on_disconnect(client, userdata, disconnect_flags, reason_code, properties):
        client.disconnected_event = True

    def on_message(client, userdata, msg):
        client.messages.append((msg.topic, msg.payload.decode("utf-8", errors="replace")))

    c.on_connect = on_connect
    c.on_disconnect = on_disconnect
    c.on_message = on_message
    return c


def wait_for(predicate, timeout_s: float = 15.0, interval_s: float = 0.25, desc: str = "condition"):
    deadline = time.time() + timeout_s
    while time.time() < deadline:
        val = predicate()
        if val:
            return val
        time.sleep(interval_s)
    raise TimeoutError(f"Timed out waiting for {desc} after {timeout_s}s")


def pop_topic_message(client, topic: str, timeout_s: float = 15.0):
    def finder():
        for idx, (t, p) in enumerate(client.messages):
            if t == topic:
                client.messages.pop(idx)
                return json.loads(p)
        return None
    return wait_for(finder, timeout_s=timeout_s, desc=f"MQTT message on {topic}")


def main() -> int:
    run_id = uuid.uuid4().hex[:8]
    email = f"e2e_{run_id}@aquasmart.local"
    phone_suffix = f"{int(run_id, 16) % 10_000_000:07d}"
    password = "StrongPassword123!"
    results: list[tuple[str, str]] = []

    log(f"=== SETUP: Register user {email} ===")
    reg_resp = requests.post(
        f"{GATEWAY_URL}/api/identity/v1/auth/register",
        json={
            "email": email,
            "phoneNumber": f"+37529{phone_suffix}",
            "name": "E2E Acceptance Runner",
            "password": password,
            "timeZone": "UTC",
        },
        timeout=10,
    )
    assert_eq(reg_resp.status_code, 200, "User register")
    jwt_token = reg_resp.json()["accessToken"]
    jwt_headers = {"Authorization": f"Bearer {jwt_token}"}

    # Verify Gateway security: /internal/* blocked even via swagger-docs path, swagger json accessible
    swag_block = requests.post(f"{GATEWAY_URL}/swagger-docs/device/internal/mqtt/auth", json={}, timeout=5)
    assert_eq(swag_block.status_code, 404, "Swagger route must NOT expose /internal/mqtt/auth")
    swag_get = requests.get(f"{GATEWAY_URL}/swagger-docs/device/swagger/v1/swagger.json", timeout=5)
    assert_eq(swag_get.status_code, 200, "GET /swagger-docs/device/swagger/v1/swagger.json")

    b = uuid.uuid4().bytes
    mac_a = f"A0:{b[0]:02X}:{b[1]:02X}:{b[2]:02X}:{b[3]:02X}:{b[4]:02X}"
    compact_a = mac_a.replace(":", "")
    mac_b = f"B0:{b[5]:02X}:{b[6]:02X}:{b[7]:02X}:{b[8]:02X}:{b[9]:02X}"
    compact_b = mac_b.replace(":", "")

    # Negative-cache prevention check: failed auth BEFORE registration must not block subsequent valid auth
    c_pre = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, "ak_not_registered_yet")
    c_pre.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_pre.loop_start()
    wait_for(lambda: c_pre.connect_rc is not None, timeout_s=5, desc="pre-reg MQTT rejection")
    assert_true(c_pre.connect_rc != 0, "Unregistered MAC must be denied")
    c_pre.loop_stop()
    c_pre.disconnect()

    ctrl_a_resp = requests.post(
        f"{GATEWAY_URL}/api/device/v1/controllers",
        headers=jwt_headers,
        json={"macAddress": mac_a, "name": f"TankA-{run_id}", "isOnline": False},
        timeout=10,
    )
    assert_eq(ctrl_a_resp.status_code, 201, "Create Controller A")
    ctrl_a_id = ctrl_a_resp.json()["controllerId"]
    token_a = ctrl_a_resp.json()["deviceToken"]

    ctrl_b_resp = requests.post(
        f"{GATEWAY_URL}/api/device/v1/controllers",
        headers=jwt_headers,
        json={"macAddress": mac_b, "name": f"TankB-{run_id}", "isOnline": False},
        timeout=10,
    )
    assert_eq(ctrl_b_resp.status_code, 201, "Create Controller B")
    ctrl_b_id = ctrl_b_resp.json()["controllerId"]
    token_b = ctrl_b_resp.json()["deviceToken"]
    log(f"Created Controller A={ctrl_a_id} ({mac_a}) & B={ctrl_b_id} ({mac_b})")

    eco_resp = requests.post(
        f"{GATEWAY_URL}/api/control/v1/ecosystems",
        headers=jwt_headers,
        json={"type": 1, "name": f"Aqua-{run_id}", "volume": 100.0, "controllerId": ctrl_a_id},
        timeout=10,
    )
    assert_eq(eco_resp.status_code, 201, "Create Ecosystem")
    eco_id = eco_resp.json()

    # ================= E2E-1: EMQX AuthN & ACL Isolation =================
    log("=== E2E-1: EMQX AuthN & ACL Isolation ===")
    c_bad_tok = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, "ak_invalid_secret")
    c_bad_tok.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_bad_tok.loop_start()
    wait_for(lambda: c_bad_tok.connect_rc is not None, timeout_s=5, desc="bad token rejection")
    assert_true(c_bad_tok.connect_rc != 0, "Invalid token must be denied by EMQX")
    c_bad_tok.loop_stop()
    c_bad_tok.disconnect()

    c_bad_cid = make_mqtt_client(f"aqua-esp32-{compact_b}", mac_a, token_a)
    c_bad_cid.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_bad_cid.loop_start()
    wait_for(lambda: c_bad_cid.connect_rc is not None, timeout_s=5, desc="bad ClientId rejection")
    assert_true(c_bad_cid.connect_rc != 0, "Mismatched ClientId must be denied by EMQX")
    c_bad_cid.loop_stop()
    c_bad_cid.disconnect()

    c_acl_pub = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, token_a)
    c_acl_pub.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_acl_pub.loop_start()
    wait_for(lambda: c_acl_pub.connected_event, timeout_s=5, desc="A connect for ACL publish test")
    c_acl_pub.publish(f"aquasmart/devices/{compact_b}/telemetry", "{}", qos=1)
    wait_for(lambda: c_acl_pub.disconnected_event, timeout_s=5, desc="EMQX disconnect on foreign publish")
    c_acl_pub.loop_stop()

    c_acl_sub_b = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, token_a)
    c_acl_sub_b.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_acl_sub_b.loop_start()
    wait_for(lambda: c_acl_sub_b.connected_event, timeout_s=5, desc="A connect for foreign subscribe test")
    c_acl_sub_b.subscribe(f"aquasmart/devices/{compact_b}/commands", qos=1)
    wait_for(lambda: c_acl_sub_b.disconnected_event, timeout_s=5, desc="EMQX disconnect on foreign subscribe")
    c_acl_sub_b.loop_stop()

    c_acl_sub = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, token_a)
    c_acl_sub.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_acl_sub.loop_start()
    wait_for(lambda: c_acl_sub.connected_event, timeout_s=5, desc="A connect for ACL wildcard test")
    c_acl_sub.subscribe("#", qos=1)
    wait_for(lambda: c_acl_sub.disconnected_event, timeout_s=5, desc="EMQX disconnect on wildcard subscribe")
    c_acl_sub.loop_stop()

    dev_headers_a = {"X-Device-Token": token_a, "X-Mac-Address": mac_a}
    dev_headers_b = {"X-Device-Token": token_b, "X-Mac-Address": mac_b}
    c_a = make_mqtt_client(
        f"aqua-esp32-{compact_a}",
        mac_a,
        token_a,
        keepalive=10,
        lwt_topic=f"aquasmart/devices/{compact_a}/status",
        lwt_payload=json.dumps({"status": "offline"}),
    )
    c_a.connect(MQTT_HOST, MQTT_PORT, keepalive=10)
    c_a.loop_start()
    wait_for(lambda: c_a.connected_event, timeout_s=5, desc="Valid Controller A MQTT connect")
    c_a.subscribe(f"aquasmart/devices/{compact_a}/commands", qos=1)
    c_a.subscribe(f"aquasmart/devices/{compact_a}/config/notify", qos=1)
    time.sleep(0.5)
    assert_true(not c_a.disconnected_event, "Controller A stays connected on allowed subscribes")
    results.append(("E2E-1", "PASS"))
    log("E2E-1 PASS")

    # ================= E2E-2: Bootstrap & config/notify =================
    log("=== E2E-2: Bootstrap & config/notify ===")
    cfg1 = requests.get(f"{GATEWAY_URL}/api/device/v1/controllers/me/config", headers=dev_headers_a, timeout=10)
    assert_eq(cfg1.status_code, 200, "GET /me/config without JWT")
    assert_eq(cfg1.json()["configVersion"], 1, "Initial configVersion == 1")

    sensor_resp = requests.post(
        f"{GATEWAY_URL}/api/device/v1/sensors",
        headers=jwt_headers,
        json={
            "controllerId": ctrl_a_id,
            "name": "Water Temp",
            "connectionProtocol": 4,
            "connectionAddress": "28FF4A1B2C3D4E5F",
            "type": 1,
        },
        timeout=10,
    )
    assert_eq(sensor_resp.status_code, 201, "Add sensor")
    sensor_id = sensor_resp.json()["id"]

    notify1 = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/config/notify", timeout_s=15)
    assert_true(notify1["version"] >= 2, f"Expected config/notify version >= 2, got {notify1}")

    relay_resp = requests.post(
        f"{GATEWAY_URL}/api/device/v1/relays",
        headers=jwt_headers,
        json={
            "controllerId": ctrl_a_id,
            "name": "Heater",
            "connectionProtocol": 1,
            "connectionAddress": "D5",
            "isNormallyOpen": True,
            "purpose": 4,
            "isActive": False,
            "isManual": False,
        },
        timeout=10,
    )
    assert_eq(relay_resp.status_code, 201, "Add relay")
    relay_id = relay_resp.json()["id"]

    notify2 = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/config/notify", timeout_s=15)
    assert_true(notify2["version"] >= 3, f"Expected config/notify version >= 3, got {notify2}")

    cfg2 = requests.get(f"{GATEWAY_URL}/api/device/v1/controllers/me/config", headers=dev_headers_a, timeout=10)
    assert_eq(cfg2.status_code, 200, "GET /me/config after mutations")
    assert_eq(cfg2.json()["configVersion"], notify2["version"], "configVersion matches config/notify")
    assert_eq(len(cfg2.json()["sensors"]), 1, "1 sensor in config")
    assert_eq(len(cfg2.json()["relays"]), 1, "1 relay in config")
    results.append(("E2E-2", "PASS"))
    log("E2E-2 PASS")

    # ================= E2E-7 (part 1): Online status & diagnostics =================
    log("=== E2E-7 (part 1): Publish status=online & verify diagnostics ===")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/status",
        json.dumps({
            "status": "online",
            "fwVersion": "1.0.0",
            "ip": "192.168.1.77",
            "rssi": -54,
            "uptimeSec": 123,
            "timestamp": utc_now_iso(),
        }),
        qos=1,
        retain=True,
    )
    wait_for(
        lambda: psql(
            "device_db",
            f"SELECT is_online || '|' || COALESCE(firmware_version,'') || '|' || COALESCE(ip_address,'') || '|' || COALESCE(rssi::text,'') FROM controllers WHERE id='{ctrl_a_id}';",
        ) == "true|1.0.0|192.168.1.77|-54",
        timeout_s=10,
        desc="device_db online diagnostics update",
    )

    # ================= E2E-3: Telemetry -> Rule (SwitchOn & SwitchOff) -> Command -> Ack =================
    log("=== E2E-3: Telemetry -> Rule (SwitchOn & SwitchOff) -> Command -> Ack ===")
    wait_for(lambda: psql("control_db", f"SELECT count(*) FROM relays WHERE id='{relay_id}';") == "1", timeout_s=15, desc="ControlService relay sync")
    wait_for(lambda: psql("control_db", f"SELECT count(*) FROM sensors WHERE id='{sensor_id}';") == "1", timeout_s=15, desc="ControlService sensor sync")

    r1 = requests.post(
        f"{GATEWAY_URL}/api/control/v1/automation-rules",
        headers=jwt_headers,
        json={
            "ecosystemId": eco_id,
            "relayId": relay_id,
            "name": "Heat On",
            "operator": 1,
            "action": 1,
            "isActive": True,
            "conditions": [{"sensorId": sensor_id, "condition": 3, "threshold": 24.0, "hysteresis": 0.5}],
        },
        timeout=10,
    )
    assert_eq(r1.status_code, 201, "Create SwitchOn rule")

    r2 = requests.post(
        f"{GATEWAY_URL}/api/control/v1/automation-rules",
        headers=jwt_headers,
        json={
            "ecosystemId": eco_id,
            "relayId": relay_id,
            "name": "Heat Off",
            "operator": 1,
            "action": 2,
            "isActive": True,
            "conditions": [{"sensorId": sensor_id, "condition": 2, "threshold": 27.0, "hysteresis": 0.5}],
        },
        timeout=10,
    )
    assert_eq(r2.status_code, 201, "Create SwitchOff rule")

    ext_dup = f"{compact_a}-1-1-water_temperature"
    now_iso = utc_now_iso()
    c_a.publish(
        f"aquasmart/devices/{compact_a}/telemetry",
        json.dumps({
            "macAddress": mac_a,
            "items": [
                {"sensorId": sensor_id, "value": 22.0, "externalMessageId": ext_dup, "recordedAt": now_iso},
                {"sensorId": sensor_id, "value": 22.0, "externalMessageId": ext_dup, "recordedAt": now_iso},
            ],
        }),
        qos=1,
    )
    wait_for(
        lambda: psql("telemetry_db", f"SELECT count(*) FROM telemetry_raw_data WHERE sensor_id='{sensor_id}';") == "1",
        timeout_s=15,
        desc="telemetry_raw_data deduplicated insert",
    )

    cmd_on = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)
    assert_eq(cmd_on["relayId"], relay_id, "SwitchOn command relayId")
    assert_eq(cmd_on["targetState"], True, "SwitchOn command targetState==True")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/commands/ack",
        json.dumps({"commandId": cmd_on["commandId"], "status": "completed", "errorMessage": None, "executedAt": utc_now_iso()}),
        qos=1,
    )
    wait_for(
        lambda: psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "t"
        and psql("control_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "t",
        timeout_s=15,
        desc="Relay is_active=true synced to device_db and control_db",
    )

    c_a.publish(
        f"aquasmart/devices/{compact_a}/telemetry",
        json.dumps({"macAddress": mac_a, "items": [{"sensorId": sensor_id, "value": 29.0, "externalMessageId": f"{compact_a}-1-2-water_temperature", "recordedAt": utc_now_iso()}]}),
        qos=1,
    )
    cmd_off = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)
    assert_eq(cmd_off["targetState"], False, "SwitchOff command targetState==False")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/commands/ack",
        json.dumps({"commandId": cmd_off["commandId"], "status": "completed", "errorMessage": None, "executedAt": utc_now_iso()}),
        qos=1,
    )
    wait_for(
        lambda: psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "f"
        and psql("control_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "f",
        timeout_s=15,
        desc="Relay is_active=false synced after SwitchOff",
    )
    time.sleep(2.0)
    extra_cmds = [p for t, p in c_a.messages if t == f"aquasmart/devices/{compact_a}/commands"]
    assert_eq(len(extra_cmds), 0, "No echo loop commands after SwitchOff ack")
    results.append(("E2E-3", "PASS"))
    log("E2E-3 PASS")

    # ================= E2E-4: App Manual Toggle (202 Accepted) & Ack/Failed =================
    log("=== E2E-4: App Manual Toggle (202 Accepted) ===")
    tog = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/toggle-state/{relay_id}", headers=jwt_headers, timeout=10)
    assert_eq(tog.status_code, 202, "toggle-state returns 202 Accepted")
    tog_body = tog.json()
    assert_eq(tog_body["targetState"], True, "Queued targetState == True")
    assert_eq(tog_body["status"], "Pending", "Queued status == Pending")
    assert_eq(psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';"), "f", "Relay.IsActive unchanged before ack")

    cmd_app = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)
    assert_eq(cmd_app["commandId"], tog_body["commandId"], "MQTT pushed commandId matches 202 response")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/commands/ack",
        json.dumps({"commandId": cmd_app["commandId"], "status": "completed", "errorMessage": None, "executedAt": utc_now_iso()}),
        qos=1,
    )
    wait_for(lambda: psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "t", timeout_s=10, desc="Relay is_active=true after app command ack")

    tog_fail = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/toggle-state/{relay_id}", headers=jwt_headers, timeout=10)
    assert_eq(tog_fail.status_code, 202, "toggle-state for failed ack test")
    cmd_fail = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)
    c_a.publish(
        f"aquasmart/devices/{compact_a}/commands/ack",
        json.dumps({"commandId": cmd_fail["commandId"], "status": "failed", "errorMessage": "Relay hardware fault", "executedAt": utc_now_iso()}),
        qos=1,
    )
    wait_for(lambda: psql("device_db", f"SELECT status FROM relay_command_queues WHERE id='{cmd_fail['commandId']}';") == "4", timeout_s=10, desc="RelayCommand marked Failed (4)")
    assert_eq(psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';"), "t", "Relay.IsActive unchanged on failed ack")
    results.append(("E2E-4", "PASS"))
    log("E2E-4 PASS")

    # ================= E2E-5: Manual Override Wins (Online & Offline->Online) =================
    log("=== E2E-5: Manual Override Wins ===")
    tog_ov = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/toggle-state/{relay_id}", headers=jwt_headers, timeout=10)
    assert_eq(tog_ov.status_code, 202, "Queue command before manual override")
    ov_cmd_id = tog_ov.json()["commandId"]
    _ = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)

    c_a.publish(
        f"aquasmart/devices/{compact_a}/relays/state",
        json.dumps({"macAddress": mac_a, "source": "manual", "recordedAt": utc_now_iso(), "relays": [{"relayId": relay_id, "state": True, "isManual": True}]}),
        qos=1,
    )
    wait_for(
        lambda: psql("device_db", f"SELECT status FROM relay_command_queues WHERE id='{ov_cmd_id}';") == "5"
        and psql("device_db", f"SELECT is_manual FROM relays WHERE id='{relay_id}';") == "t"
        and psql("control_db", f"SELECT is_manual FROM relays WHERE id='{relay_id}';") == "t",
        timeout_s=15,
        desc="Pending/Sent command Cancelled (5) and IsManual=true in both DBs",
    )

    c_a.publish(
        f"aquasmart/devices/{compact_a}/telemetry",
        json.dumps({"macAddress": mac_a, "items": [{"sensorId": sensor_id, "value": 29.5, "externalMessageId": f"{compact_a}-1-3-water_temperature", "recordedAt": utc_now_iso()}]}),
        qos=1,
    )
    time.sleep(2.5)
    assert_eq(len([p for t, p in c_a.messages if t == f"aquasmart/devices/{compact_a}/commands"]), 0, "No automation command while IsManual=true")

    c_a.publish(
        f"aquasmart/devices/{compact_a}/relays/state",
        json.dumps({"macAddress": mac_a, "source": "manual", "recordedAt": utc_now_iso(), "relays": [{"relayId": relay_id, "state": True, "isManual": False}]}),
        qos=1,
    )
    wait_for(lambda: psql("control_db", f"SELECT is_manual FROM relays WHERE id='{relay_id}';") == "f", timeout_s=15, desc="IsManual=false synced to ControlService")

    c_a.publish(
        f"aquasmart/devices/{compact_a}/telemetry",
        json.dumps({"macAddress": mac_a, "items": [{"sensorId": sensor_id, "value": 29.6, "externalMessageId": f"{compact_a}-1-4-water_temperature", "recordedAt": utc_now_iso()}]}),
        qos=1,
    )
    resumed_cmd = pop_topic_message(c_a, f"aquasmart/devices/{compact_a}/commands", timeout_s=15)
    assert_eq(resumed_cmd["targetState"], False, "Automation resumed after AUTO")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/commands/ack",
        json.dumps({"commandId": resumed_cmd["commandId"], "status": "completed", "errorMessage": None, "executedAt": utc_now_iso()}),
        qos=1,
    )
    wait_for(lambda: psql("device_db", f"SELECT is_active FROM relays WHERE id='{relay_id}';") == "f", timeout_s=10, desc="resumed SwitchOff acked")

    # E2E-5b: OFFLINE -> ONLINE priority manual override before status=online
    c_a.publish(f"aquasmart/devices/{compact_a}/status", json.dumps({"status": "offline"}), qos=1, retain=True)
    wait_for(lambda: psql("device_db", f"SELECT is_online FROM controllers WHERE id='{ctrl_a_id}';") == "f", timeout_s=10, desc="controller A marked offline for E2E-5b")

    tog_off = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/toggle-state/{relay_id}", headers=jwt_headers, timeout=10)
    assert_eq(tog_off.status_code, 202, "Queue command while offline")
    off_cmd_id = tog_off.json()["commandId"]
    c_a.messages.clear()

    c_a.publish(
        f"aquasmart/devices/{compact_a}/relays/state",
        json.dumps({"macAddress": mac_a, "source": "manual", "recordedAt": utc_now_iso(), "relays": [{"relayId": relay_id, "state": True, "isManual": True}]}),
        qos=1,
    )
    c_a.publish(
        f"aquasmart/devices/{compact_a}/status",
        json.dumps({"status": "online", "fwVersion": "1.0.0", "ip": "192.168.1.77", "rssi": -52, "uptimeSec": 200, "timestamp": utc_now_iso()}),
        qos=1,
        retain=True,
    )
    wait_for(lambda: psql("device_db", f"SELECT status FROM relay_command_queues WHERE id='{off_cmd_id}';") == "5", timeout_s=10, desc="Offline pending command cancelled on reconnect")
    time.sleep(2.2)
    redelivered = [p for t, p in c_a.messages if t == f"aquasmart/devices/{compact_a}/commands" and json.loads(p).get("commandId") == off_cmd_id]
    assert_eq(len(redelivered), 0, "Cancelled offline command must NOT be redelivered")

    c_a.publish(
        f"aquasmart/devices/{compact_a}/relays/state",
        json.dumps({"macAddress": mac_a, "source": "manual", "recordedAt": utc_now_iso(), "relays": [{"relayId": relay_id, "state": False, "isManual": False}]}),
        qos=1,
    )
    wait_for(lambda: psql("device_db", f"SELECT is_manual FROM relays WHERE id='{relay_id}';") == "f", timeout_s=10, desc="Restore AUTO before E2E-6")
    results.append(("E2E-5", "PASS"))
    log("E2E-5 PASS")

    # ================= E2E-6: Boot Reconciliation =================
    log("=== E2E-6: Boot Reconciliation ===")
    c_a.publish(
        f"aquasmart/devices/{compact_a}/relays/state",
        json.dumps({"macAddress": mac_a, "source": "boot", "recordedAt": utc_now_iso(), "relays": [{"relayId": relay_id, "state": True, "isManual": True}]}),
        qos=1,
    )
    wait_for(
        lambda: psql("device_db", f"SELECT is_active || '|' || is_manual FROM relays WHERE id='{relay_id}';") == "true|false"
        and psql("control_db", f"SELECT is_active || '|' || is_manual FROM relays WHERE id='{relay_id}';") == "true|false",
        timeout_s=15,
        desc="Boot reconciliation updates is_active without forcing is_manual=true",
    )
    results.append(("E2E-6", "PASS"))
    log("E2E-6 PASS")

    # ================= E2E-7 (part 2): LWT offline, grace period & anti-flap =================
    log("=== E2E-7 (part 2): LWT offline, grace period & anti-flap ===")
    c_a.loop_stop()
    sock = c_a.socket()
    if sock is not None:
        try:
            sock.close()
        except OSError:
            pass
    wait_for(
        lambda: psql("device_db", f"SELECT is_online || '|' || offline_alert_sent FROM controllers WHERE id='{ctrl_a_id}';") == "false|false",
        timeout_s=25,
        desc="EMQX LWT marks controller offline with offline_alert_sent=false",
    )
    # Reconnect within grace period -> online restored with zero premature alerts
    c_flap = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, token_a, keepalive=15)
    c_flap.connect(MQTT_HOST, MQTT_PORT, keepalive=15)
    c_flap.loop_start()
    wait_for(lambda: c_flap.connected_event, timeout_s=5, desc="Controller A anti-flap reconnect")
    c_flap.publish(
        f"aquasmart/devices/{compact_a}/status",
        json.dumps({"status": "online", "fwVersion": "1.0.0", "ip": "192.168.1.77", "rssi": -50, "uptimeSec": 260, "timestamp": utc_now_iso()}),
        qos=1,
        retain=True,
    )
    wait_for(
        lambda: psql("device_db", f"SELECT is_online || '|' || offline_alert_sent FROM controllers WHERE id='{ctrl_a_id}';") == "true|false",
        timeout_s=10,
        desc="Controller A back online within grace period",
    )
    c_flap.loop_stop()
    c_flap.disconnect()
    results.append(("E2E-7", "PASS"))
    log("E2E-7 PASS")

    # ================= E2E-8: HTTP Fallback during mqtt-broker outage =================
    log("=== E2E-8: HTTP Fallback when mqtt-broker is stopped ===")
    subprocess.run(["docker", "compose", "stop", "mqtt-broker"], check=True, capture_output=True)
    try:
        ext_fb = f"{compact_a}-2-1-water_temperature"
        fb_tel = requests.post(
            f"{GATEWAY_URL}/api/telemetry/v1/data",
            headers=dev_headers_a,
            json={"macAddress": mac_a, "items": [{"sensorId": sensor_id, "value": 25.5, "externalMessageId": ext_fb, "recordedAt": utc_now_iso()}]},
            timeout=10,
        )
        assert_eq(fb_tel.status_code, 202, "HTTP fallback telemetry 202")

        fb_ping = requests.post(f"{GATEWAY_URL}/api/device/v1/controllers/{ctrl_a_id}/ping", headers=dev_headers_a, timeout=10)
        assert_eq(fb_ping.status_code, 200, "HTTP fallback ping 200")
        assert_true("serverTimeUtc" in fb_ping.json(), "HTTP fallback ping returns serverTimeUtc")

        tog_fb = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/toggle-state/{relay_id}", headers=jwt_headers, timeout=10)
        assert_eq(tog_fb.status_code, 202, "Queue command during broker outage")
        fb_cmd_id = tog_fb.json()["commandId"]

        pend = requests.get(f"{GATEWAY_URL}/api/device/v1/commands/pending/{ctrl_a_id}", headers=dev_headers_a, timeout=10)
        assert_eq(pend.status_code, 200, "HTTP fallback pending commands 200")
        assert_true(any(c["id"] == fb_cmd_id for c in pend.json()), "Pending command returned over HTTP fallback")

        comp = requests.post(f"{GATEWAY_URL}/api/device/v1/commands/{fb_cmd_id}/complete", headers=dev_headers_a, timeout=10)
        assert_eq(comp.status_code, 204, "HTTP fallback complete 204")
    finally:
        subprocess.run(["docker", "compose", "start", "mqtt-broker"], check=True, capture_output=True)

    wait_for(
        lambda: "MQTT gateway worker connected" in subprocess.run(["docker", "logs", DEVICE_API_CONTAINER, "--tail", "15"], capture_output=True, text=True).stdout,
        timeout_s=25,
        desc="device-api MQTT worker reconnect after broker start",
    )

    c_a2 = make_mqtt_client(f"aqua-esp32-{compact_a}", mac_a, token_a, keepalive=20)
    c_a2.connect(MQTT_HOST, MQTT_PORT, keepalive=20)
    c_a2.loop_start()
    wait_for(lambda: c_a2.connected_event, timeout_s=10, desc="Controller A reconnect after broker restart")
    c_a2.publish(
        f"aquasmart/devices/{compact_a}/telemetry",
        json.dumps({"macAddress": mac_a, "items": [{"sensorId": sensor_id, "value": 25.5, "externalMessageId": ext_fb, "recordedAt": utc_now_iso()}]}),
        qos=1,
    )
    time.sleep(2.5)
    cnt_fb = psql("telemetry_db", f"SELECT count(*) FROM telemetry_raw_data WHERE external_message_id='{ext_fb}';")
    assert_eq(cnt_fb, "1", "No duplicate telemetry after HTTP fallback -> MQTT recovery")
    c_a2.loop_stop()
    c_a2.disconnect()
    results.append(("E2E-8", "PASS"))
    log("E2E-8 PASS")

    # ================= E2E-9: FirmwareService OTA Proxy, IDOR & Rollback/Failure =================
    log("=== E2E-9: OTA Proxy, IDOR & Rollback/Failure ===")
    hw_resp = requests.post(
        f"{GATEWAY_URL}/api/firmware/v1/hardware-profiles",
        headers=jwt_headers,
        json={"name": f"ESP32-WROOM-{run_id}", "board_revision": "rev1"},
        timeout=10,
    )
    assert_eq(hw_resp.status_code, 201, "Create hardware profile")
    hw_id = hw_resp.json()["id"]

    bin_path = Path(os.environ.get("FIRMWARE_BIN", str(DEFAULT_FIRMWARE_BIN)))
    if bin_path.is_file():
        fw_bytes = bin_path.read_bytes()
        log(f"Using compiled ESP32 binary for OTA E2E: {bin_path} ({len(fw_bytes)} bytes)")
    else:
        fw_bytes = (b"AQUASMART_ESP32_FIRMWARE_BIN_" + run_id.encode("utf-8")) * 128
    fw_sha = hashlib.sha256(fw_bytes).hexdigest()

    fw_resp = requests.post(
        f"{GATEWAY_URL}/api/firmware/v1/firmwares",
        headers=jwt_headers,
        data={"hardware_id": hw_id, "version": "1.2.3", "file_hash": fw_sha},
        files={"firmware_file": ("firmware.bin", fw_bytes, "application/octet-stream")},
        timeout=30,
    )
    assert_eq(fw_resp.status_code, 201, "Upload firmware release")
    fw_id = fw_resp.json()["id"]

    pub_resp = requests.post(f"{GATEWAY_URL}/api/firmware/v1/firmwares/{fw_id}/publish", headers=jwt_headers, timeout=10)
    assert_eq(pub_resp.status_code, 204, "Publish firmware")

    camp_resp = requests.post(
        f"{GATEWAY_URL}/api/firmware/v1/campaigns",
        headers=jwt_headers,
        json={"firmware_id": fw_id, "name": f"OTA-{run_id}"},
        timeout=10,
    )
    assert_eq(camp_resp.status_code, 201, "Create campaign")
    camp_id = camp_resp.json()["id"]

    tgt_resp = requests.post(
        f"{GATEWAY_URL}/api/firmware/v1/campaigns/{camp_id}/targets",
        headers=jwt_headers,
        json={"controller_ids": [ctrl_a_id]},
        timeout=10,
    )
    assert_eq(tgt_resp.status_code, 204, "Add Controller A target")

    start_resp = requests.post(f"{GATEWAY_URL}/api/firmware/v1/campaigns/{camp_id}/start", headers=jwt_headers, timeout=10)
    assert_eq(start_resp.status_code, 204, "Start campaign")

    assert_eq(requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/check?controller_id={ctrl_a_id}", timeout=10).status_code, 401, "OTA check without token -> 401")
    assert_eq(requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/check?controller_id={ctrl_a_id}", headers=dev_headers_b, timeout=10).status_code, 403, "OTA check IDOR -> 403")
    assert_eq(requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/download/{camp_id}?controller_id={ctrl_a_id}", headers=dev_headers_b, timeout=10).status_code, 403, "OTA download IDOR -> 403")
    assert_eq(
        requests.post(
            f"{GATEWAY_URL}/api/firmware/v1/ota/status?controller_id={ctrl_b_id}",
            headers=dev_headers_b,
            json={"campaign_id": camp_id, "controller_id": ctrl_a_id, "status": 4, "error_message": "spoof"},
            timeout=10,
        ).status_code,
        403,
        "OTA status query/body IDOR -> 403",
    )
    assert_eq(requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/download/{camp_id}?controller_id={ctrl_b_id}", headers=dev_headers_b, timeout=10).status_code, 403, "Non-target download -> 403")

    chk = requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/check?controller_id={ctrl_a_id}", headers=dev_headers_a, timeout=10)
    assert_eq(chk.status_code, 200, "OTA check 200")
    chk_json = chk.json()
    assert_true(chk_json["has_update"], "has_update==True")
    assert_eq(chk_json["campaign_id"], camp_id, "campaign_id matches")
    assert_eq(chk_json["file_hash"], fw_sha, "file_hash matches")

    assert_eq(requests.post(f"{GATEWAY_URL}/api/firmware/v1/ota/status", headers=dev_headers_a, json={"campaign_id": camp_id, "controller_id": ctrl_a_id, "status": 1, "error_message": ""}, timeout=10).status_code, 204, "OTA status 1 (Downloading)")
    dl = requests.get(f"{GATEWAY_URL}{chk_json['download_url']}", headers=dev_headers_a, timeout=30)
    assert_eq(dl.status_code, 200, "OTA download via ApiGateway :5055")
    assert_eq(dl.headers.get("Content-Type"), "application/octet-stream", "Content-Type octet-stream")
    assert_eq(len(dl.content), len(fw_bytes), "Downloaded byte size matches")
    assert_eq(hashlib.sha256(dl.content).hexdigest(), fw_sha, "Downloaded SHA-256 matches")

    assert_eq(requests.post(f"{GATEWAY_URL}/api/firmware/v1/ota/status", headers=dev_headers_a, json={"campaign_id": camp_id, "controller_id": ctrl_a_id, "status": 2, "error_message": ""}, timeout=10).status_code, 204, "OTA status 2 (Flashing)")
    assert_eq(requests.post(f"{GATEWAY_URL}/api/firmware/v1/ota/status", headers=dev_headers_a, json={"campaign_id": camp_id, "controller_id": ctrl_a_id, "status": 3, "error_message": ""}, timeout=10).status_code, 204, "OTA status 3 (Success)")

    # Second campaign: TargetFailed prevents endless download loop
    camp2_id = requests.post(f"{GATEWAY_URL}/api/firmware/v1/campaigns", headers=jwt_headers, json={"firmware_id": fw_id, "name": f"OTA2-{run_id}"}, timeout=10).json()["id"]
    requests.post(f"{GATEWAY_URL}/api/firmware/v1/campaigns/{camp2_id}/targets", headers=jwt_headers, json={"controller_ids": [ctrl_a_id, ctrl_b_id]}, timeout=10).raise_for_status()
    requests.post(f"{GATEWAY_URL}/api/firmware/v1/campaigns/{camp2_id}/start", headers=jwt_headers, timeout=10).raise_for_status()
    assert_eq(requests.post(f"{GATEWAY_URL}/api/firmware/v1/ota/status", headers=dev_headers_a, json={"campaign_id": camp2_id, "controller_id": ctrl_a_id, "status": 4, "error_message": "E012 SHA-256 mismatch"}, timeout=10).status_code, 204, "OTA status 4 (Failed)")
    chk_after_fail = requests.get(f"{GATEWAY_URL}/api/firmware/v1/ota/check?controller_id={ctrl_a_id}", headers=dev_headers_a, timeout=10)
    assert_eq(chk_after_fail.status_code, 200, "OTA check after TargetFailed")
    assert_eq(chk_after_fail.json()["has_update"], False, "Failed target must NOT re-enter download loop")
    results.append(("E2E-9", "PASS"))
    log("E2E-9 PASS")

    print("\n================ E2E ACCEPTANCE SUMMARY ================")
    for name, status in results:
        print(f"  {name}: {status}")
    print("========================================================")
    log("ALL E2E-1..E2E-9 PASSED SUCCESSFULLY!")
    return 0


if __name__ == "__main__":
    sys.exit(main())
