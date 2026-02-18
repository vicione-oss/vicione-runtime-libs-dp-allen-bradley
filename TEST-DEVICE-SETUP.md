# Test Device Setup

This document describes how to connect to the Allen-Bradley CompactLogix L32E used for E2E testing.

## Prerequisites

- **ifm Link Manager** installed on your machine (see step 6 below)
- Certificate and password stored in KeePass under **`ifmCLT_CR3170_Cert.lmc`**

## 1. Connect via GateManager

1. Open [https://gm01-na.ifm.com/](https://gm01-na.ifm.com/)
2. Click **Link Manager** and select **Certificate** as the login method
3. Click **Choose File** and upload `ifmCLT_CR3170_Cert.lmc` (see KeePass)
4. Enter the password from KeePass
5. Navigate to **US → ifm Demo → CR3170_CC2F**
6. Click **CR3170_CC2F** and then **ConnectAll**
   - If Link Manager is not installed, use the **Install LinkManager** button on this page
7. Verify that all subnets under **CR3170_CC2F** show a **green lightning bolt** icon
   - This confirms the `192.168.0.x` subnet is routed through the Link Manager tunnel

![GateManager portal showing CR3170_CC2F with active subnet connections](gatemanager.png)

## 2. PLC Device

| Property | Value |
|----------|-------|
| Model | Allen-Bradley CompactLogix L32E |
| IP Address | `192.168.0.100` |
| Protocol | EtherNet/IP (`ab_eip`) |
| Backplane path | `1,0` |

> **Note:** `192.168.0.102` (labeled `AB_CompactLogix`) is a separate device and is not configured at this time.

Once the Link Manager tunnel is active, the PLC should be reachable from your machine:

```
ping 192.168.0.100
```

## 3. Available Tags

### Program tags — `Program:MainProgram`

These are the user-defined tags accessible for testing. The full tag name is `Program:MainProgram.<name>`.

| Tag | Notes |
|-----|-------|
| `strValue1` | String — used as the default `CIP_TAG_NAME` in the write/read tests |
| `strValue2` | String |
| `strVarString` | String |
| `IOLM` | IO-Link master data |
| `IOLM_PDI` | IO-Link Process Data In |
| `IOLM_PDO` | IO-Link Process Data Out |
| `Blink` | Boolean output |
| `Outon` | Boolean output |
| `Counter` | Counter |

### Controller tags — IO-Link master (`AL1x2x_IOLink`)

| Tag | Length | Notes |
|-----|--------|-------|
| `AL1x2x_IOLink:I` | 452 bytes | Input data from IO-Link master |
| `AL1x2x_IOLink:O` | 304 bytes | Output data to IO-Link master |
| `AL1x2x_IOLink:C` | 108 bytes | IO-Link master configuration |

## 4. Running the E2E Tests

The E2E tests connect to the PLC using environment variables with the defaults below.
Override them if your setup differs.

| Variable | Default | Description |
|----------|---------|-------------|
| `CIP_GATEWAY` | `192.168.0.100` | PLC IP address |
| `CIP_PATH` | `1,0` | Backplane routing path |
| `CIP_TAG_NAME` | `Program:MainProgram.strValue1` | Tag used for write/read round-trip |

Run only E2E tests:

```
dotnet test --filter "Category=E2E"
```

Exclude E2E tests (e.g. in CI without device access):

```
dotnet test --filter "Category!=E2E"
```
