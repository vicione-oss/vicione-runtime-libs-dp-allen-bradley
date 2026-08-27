# Test Device Setup

This document describes how to connect to the Allen-Bradley CompactLogix L32E used for integration testing.

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

```sh
ping 192.168.0.100
```

## 3. Available Tags

### Program tags — `Program:MainProgram`

These are the user-defined tags accessible for testing. The full tag name is `Program:MainProgram.<name>`.

| Tag | Notes |
|-----|-------|
| `strValue1` | String — **written by the test suite** (see below) |
| `strValue2` | String |
| `strVarString` | String |
| `IOLM` | IO-Link master data |
| `IOLM_PDI` | IO-Link Process Data In |
| `IOLM_PDO` | IO-Link Process Data Out |
| `Blink` | Boolean output |
| `Outon` | Boolean output |
| `Counter` | Counter — `Counter.PRE` is the only plain DINT the device exposes |

> **`strValue1` is written, not just read.** `LogixClientStringRoundtripTests` writes it once per
> theory case and reads it back. That is what the tag is for, so there is no save-and-restore; do not
> point any suite at a tag whose value someone depends on.

### Wire-format facts for `Program:MainProgram.strValue1`

The `STRING` converter's byte offsets hang off these. Two of the three were settled from libplctag's
documented Logix string layout rather than from the device, because the tunnel was down when the
converter was written; `StringWireFormatProbeTests` prints all three so they can be confirmed against
the real controller and this table completed. **Delete that probe once they are.**

| Fact | Value | Confirmed against the device? |
|------|-------|-------------------------------|
| Where `Tag.GetBuffer()` starts | At `.LEN`. libplctag strips the `A0 02 HH HH` abbreviated-structure prefix into its own type-info store and re-attaches it on write. Its Logix string defaults say the same: count word 4 bytes at offset 0, capacity 82, 2 pad bytes, 88 total | No — from libplctag |
| `ElementLength` in the `@tags` listing | Assumed **86** — the `.LEN` + `.DATA[82]` members without the alignment pad. May be 88 (padded) | **No — open** |
| The tag's string type | Assumed the built-in `STRING`, `.DATA[82]` | No — from libplctag's default |

The 86 assumption is load-bearing now, where it used to be tolerated. `TagsDecoder` subtracts the
4-byte `.LEN` from the listing's element length to get a `StringMaxLength`, so 86 decodes as the
82-character capacity `strValue1` is configured with and verification passes. If the controller
reports 88 instead, the same tag decodes as a capacity of 84, `LogixTypeComparison` reports
`StringCapacity`, and the connect aborts before a single `STRING` is polled — a padded size cannot be
read back as a capacity, because 88 is equally consistent with `.DATA[82]` and `.DATA[84]`.

Fixing that means reading the template (`@udt/<id>`) for the honest `.DATA : SINT[n]`, which is the
step ADR-003 already defers to structured data-point support. Run `StringWireFormatProbeTests` before
that becomes necessary.

### Controller tags — IO-Link master (`AL1x2x_IOLink`)

| Tag | Length | Notes |
|-----|--------|-------|
| `AL1x2x_IOLink:I` | 452 bytes | Input data from IO-Link master |
| `AL1x2x_IOLink:O` | 304 bytes | Output data to IO-Link master |
| `AL1x2x_IOLink:C` | 108 bytes | IO-Link master configuration |

## 4. Running the Integration Tests

The integration tests live in `tests/AllenBradley.Logix.Tests/Integration/` and connect to the PLC
using environment variables with the defaults below. Override them if your setup differs.

| Variable | Default | Description |
|----------|---------|-------------|
| `CIP_GATEWAY` | `192.168.0.100` | PLC IP address |
| `CIP_PATH` | `1,0` | Backplane routing path |
| `CIP_TAG_NAME` | `Program:MainProgram.strValue1` | Tag used by the raw-libplctag spike round trip |

Only the connection settings are environment variables. Which tags the client-stack suites target is
a fact about this controller, so those names are constants in `Integration/LogixTagAddresses.cs`.

Run only the integration tests (needs this device reachable):

```sh
dotnet test -p:test-suite=integration
```

Run only the hardware-free tests (this is the default, and what CI runs):

```sh
dotnet test
```

> The classic VSTest `--filter "Category=..."` syntax does **not** work in this repo. Tests run on
> Microsoft Testing Platform with xUnit v3, and suites are selected through the `test-suite` MSBuild
> property. See [AGENTS.md](../../../AGENTS.md).
