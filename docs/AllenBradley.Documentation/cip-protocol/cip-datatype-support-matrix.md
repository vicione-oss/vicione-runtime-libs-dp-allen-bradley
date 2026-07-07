# Native data-type support across Allen-Bradley families

This document describes which data types are **natively** available per Allen-Bradley
controller family — i.e. which types each controller's programming environment exposes as
first-class tag types. This is **client-agnostic** — it describes the controllers, not how any
specific library handles them.

> **Native support, typed access.** Unlike raw-memory protocols (Siemens S7comm PUT/GET reads
> arbitrary byte ranges), CIP tag access is **typed and symbolic** — you read and write whole
> named tags of a declared type, and legacy PCCC access is **file-typed** (an `N` file is
> integer, an `F` file is float). There is no equivalent of parking an arbitrary byte layout at
> an arbitrary address. So the marks below reflect what each controller's programming tool lets
> you *declare*, which is also what appears on the wire.

For the wire format, type codes, and .NET equivalents of each type, see the
[CIP Data Types Reference](cip-datatypes-reference.md). This document is the companion that
answers *"which types exist on which controller"*.

---

## 1. Engineering software per family

Each family has its own programming tool and addressing model.

| Family                                                     | Tool                                            | Addressing model            | Native protocol                    |
|------------------------------------------------------------|-------------------------------------------------|-----------------------------|------------------------------------|
| **Logix5000** — ControlLogix, CompactLogix, GuardLogix, SoftLogix | Studio 5000 Logix Designer (formerly RSLogix 5000) | **Symbolic tags** (UDTs, AOIs) | **CIP / EtherNet/IP** native      |
| **Micro800** — Micro810/820/830/850/870                    | Connected Components Workbench (CCW)             | **Symbolic tags** (no AOIs; UDFBs) | **CIP / EtherNet/IP** (symbolic) |
| **MicroLogix** — 1000/1100/1200/1400/1500                  | RSLogix 500 / RSLogix Micro                     | **File-based** (`N7:0`)     | **PCCC** over EtherNet/IP          |
| **SLC-500** — SLC 5/01…5/05                                | RSLogix 500                                      | **File-based**              | **PCCC** (native Ethernet on 5/05) |
| **PLC-5** — PLC-5/xx, PLC-5/xxE                            | RSLogix 5                                        | **File-based**              | **PCCC** (native Ethernet on `/xxE`) |

**Logix vs. Micro800 tags:** both address data by symbolic name over CIP, but Micro800 uses
**UDFBs** (user-defined function blocks) instead of **AOIs** (Add-On Instructions), and its CIP
tag access has limitations (see [§2](#2-per-family-notes)).

**Legacy families:** PLC-5, SLC-500, and MicroLogix use **file-based** addressing over **PCCC**,
tunneled inside EtherNet/IP (see the [networking overview](cip-networking-overview.md#legacy-pccc-tunneling)).
Not every model has native Ethernet — see [§2](#2-per-family-notes).

---

## 2. Per-family notes

### Logix (ControlLogix / CompactLogix / GuardLogix / SoftLogix)

Logix5000 controllers expose data as **symbolic tags** of atomic types, plus **structures**
(UDTs, AOIs, and the predefined `STRING`, `TIMER`, `COUNTER`, `CONTROL`, motion `AXIS_*`, `MSG`,
…). Two type-availability generations matter:

- **Classic Logix** (ControlLogix 5550/5560/5570, CompactLogix 5370 and earlier): atomic types
  `BOOL`, `SINT`, `INT`, `DINT`, `LINT`, `REAL`. No unsigned integers, no `LREAL`.
- **5x80 controllers** (CompactLogix 5380, ControlLogix 5580, 5480) added the **extended data
  types**: the unsigned integers `USINT`/`UINT`/`UDINT`/`ULINT` and `LREAL`. These require a
  recent Studio 5000 / controller firmware — the tool is the arbiter for a given catalog and
  revision.

`BYTE`/`WORD`/`DWORD`/`LWORD` are **not** declarable Logix tag types (Logix uses `SINT`/`INT`/
`DINT`/`LINT` with a hex/binary display style); they still appear on the wire as structure
members. Wall-clock time is a `LINT`, not a CIP `DATE`/`TIME` elementary type.

### Micro800

Micro800 (CCW) supports the **IEC 61131-3 elementary type set** — the widest atomic vocabulary
of the families here, including the bit-string types `BYTE`/`WORD`/`DWORD`/`LWORD` and `TIME`/
`DATE`. Caveats:

- **No AOIs** — the equivalent is a **UDFB**. UDTs are supported.
- **Timers/counters are IEC function-block instances** (`TON`, `TOF`, `CTU`, …), not the Logix
  `TIMER`/`COUNTER` predefined structures.
- **CIP access limits:** Micro800 uses only **symbolic** addressing (no firmware-v21 Symbol
  Instance Addressing), does **not** support the Multiple Service Packet service, and early
  firmware could not browse tags at all (tag listing via the Symbol object arrived around
  firmware v10). It tolerates large single packets but relies on fragmented services for them.
- Embedded Ethernet is present on **Micro820/850/870** (and version-dependent variants); the
  Micro810/830 are serial/USB.

### MicroLogix / SLC-500 / PLC-5 (legacy, file-based)

These controllers have **no symbolic tags and no user-defined structures**. Data lives in
numbered **data files** whose *type letter* fixes the element type:

| File letter | Type              | Maps to        |
|-------------|-------------------|----------------|
| `N`         | Integer (16-bit)  | `INT`          |
| `L`         | Long (32-bit)     | `DINT` — MicroLogix 1400 / newer SLC only; **not on PLC-5** |
| `F`         | Float (32-bit)    | `REAL`         |
| `B`         | Binary / bit      | `BOOL` (bit-addressed 16-bit words) |
| `T`/`C`/`R` | Timer/Counter/Control | predefined 3-word structures |
| `ST`        | String (82 chars) | `STRING`       |
| `A`         | ASCII             | `CHAR`/`SINT`  |

There is **no 8-bit, no unsigned, no 64-bit, and no `LREAL`** type. PLC-5 in particular has **no
32-bit integer** — move data as 16-bit `INT`. Timer/counter status bits live in the element's
control word (Timer: `EN`/`TT`/`DN`; Counter: `CU`/`CD`/`DN`/`OV`/`UN`/`UA`). The full
file-addressing grammar is in the
[legacy addressing doc](../../../dataport-definition/Allen-Bradley%20Legacy%20Adressierung.md).

**Native Ethernet** is model-specific: MicroLogix **1100/1400**, SLC **5/05**, and PLC-5 **`/xxE`**
have it; the rest reach EtherNet/IP through a bridge (1756-ENxT + 1756-DHRIO ControlLogix
gateway, or a 1761-NET-ENI serial converter), with the CIP route path hopping through the bridge.

---

## 3. Native type availability matrix

These marks reflect **native** support — what each family's programming tool exposes as a
first-class tag type (or, for legacy, a data-file type).

### Legend

| Symbol | Meaning                                                                                     |
|--------|---------------------------------------------------------------------------------------------|
| ✅      | Native type — fully supported.                                                              |
| ✅*️     | Native, but with a caveat (generation-dependent, or a different mechanism) — see [§2](#2-per-family-notes). |
| ❌      | Not a native type on this family.                                                           |

### Matrix

| Type                           | Logix (5x70 & earlier) | Logix (5x80) | Micro800 | MicroLogix | SLC-500 | PLC-5 |
|--------------------------------|------------------------|--------------|----------|------------|---------|-------|
| **Bool**                       |                        |              |          |            |         |       |
| `BOOL`                         | ✅                      | ✅            | ✅        | ✅ (`B`)    | ✅ (`B`) | ✅ (`B`) |
| **Bit strings**                |                        |              |          |            |         |       |
| `BYTE`                         | ❌                      | ❌            | ✅        | ❌          | ❌       | ❌     |
| `WORD`                         | ❌                      | ❌            | ✅        | ❌          | ❌       | ❌     |
| `DWORD`                        | ❌                      | ❌            | ✅        | ❌          | ❌       | ❌     |
| `LWORD`                        | ❌                      | ❌            | ✅*️      | ❌          | ❌       | ❌     |
| **Signed / unsigned integers** |                        |              |          |            |         |       |
| `SINT`                         | ✅                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| `USINT`                        | ❌                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| `INT`                          | ✅                      | ✅            | ✅        | ✅ (`N`)    | ✅ (`N`) | ✅ (`N`) |
| `UINT`                         | ❌                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| `DINT`                         | ✅                      | ✅            | ✅        | ✅*️ (`L`)  | ✅*️ (`L`) | ❌   |
| `UDINT`                        | ❌                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| `LINT`                         | ✅                      | ✅            | ✅*️      | ❌          | ❌       | ❌     |
| `ULINT`                        | ❌                      | ✅            | ✅*️      | ❌          | ❌       | ❌     |
| **Floating point**             |                        |              |          |            |         |       |
| `REAL`                         | ✅                      | ✅            | ✅        | ✅ (`F`)    | ✅*️ (`F`) | ✅ (`F`) |
| `LREAL`                        | ❌                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| **Character / string**         |                        |              |          |            |         |       |
| `STRING`                       | ✅*️ (struct)           | ✅*️ (struct) | ✅        | ✅ (`ST`)   | ✅ (`ST`) | ✅ (`ST`) |
| **Aggregate / structured**     |                        |              |          |            |         |       |
| UDT / `STRUCT`                 | ✅                      | ✅            | ✅        | ❌          | ❌       | ❌     |
| `ARRAY`                        | ✅ (≤3 dims)            | ✅ (≤3 dims)  | ✅        | ✅*️ (per file) | ✅*️ | ✅*️  |
| `TIMER`/`COUNTER`/`CONTROL`    | ✅ (struct)             | ✅ (struct)   | ✅*️ (IEC FBs) | ✅ (`T`/`C`/`R`) | ✅ | ✅ |
| AOI (Add-On Instruction)       | ✅                      | ✅            | ✅*️ (UDFB) | ❌         | ❌       | ❌     |

**Notes on the caveats (✅*️):**

- **Logix `STRING`** is a predefined **structure** (`.LEN : DINT` + `.DATA : SINT[82]`), not the
  elementary CIP `STRING` — see the [datatypes reference](cip-datatypes-reference.md#the-logix-string-structure-not-elementary-0xd0).
- **SLC `REAL` (`F`)** requires SLC 5/03 OS301+, 5/04, or 5/05.
- **`DINT` on legacy** is the 32-bit `L` (Long) file — MicroLogix 1400 and newer SLC only; PLC-5
  has no 32-bit integer.
- **Micro800 64-bit and bit-string types** (`LWORD`/`LINT`/`ULINT`) are firmware/model-dependent.
- **Micro800 timers/counters** are IEC function-block instances; **AOIs** are UDFBs.
- **Legacy arrays** are the sequence of elements within a data file, not a declared array type.

---

## 4. Protocol layer (orthogonal to data types)

Which application protocol reaches the controller, and how tag/file access is expressed:

| Protocol                          | Logix   | Micro800 | MicroLogix | SLC-500        | PLC-5          |
|-----------------------------------|---------|----------|------------|----------------|----------------|
| CIP symbolic tag services (`0x4C`/`0x4D`/…) | native | native   | ❌          | ❌              | ❌              |
| PCCC over EtherNet/IP (class `0x67`, service `0x4B`) | ❌ | ❌ | native | native (5/05)  | native (`/xxE`) |
| Reached via a bridge when no native Ethernet | — | — | 1000/1200/1500 | 5/01–5/04 | non-`E` models |

Value encoding is **little-endian** in both CIP and PCCC payloads. What varies per family is the
addressing model (symbolic tag vs. file/element) and the messaging service, not the byte order.

---

## 5. References

### Rockwell publications

- Rockwell Automation — *Logix 5000 Controllers Data Access* (1756-PM020) — Logix tag types,
  the `STRING`/`TIMER` structures:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation — *DF1 Protocol and Command Set Reference Manual* (1770-6.5.16) — PCCC and
  the legacy data-file model:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1770-rm516_-en-p.pdf>
- Rockwell Automation — *SLC 500 Instruction Set Reference Manual* (1747-RM001) — file types and
  Timer/Counter/Control layouts:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1747-rm001_-en-p.pdf>
- Rockwell Automation — *Micro800 Programmable Controllers* user manuals (IEC types, UDFBs, CIP
  symbolic access): <https://literature.rockwellautomation.com/idc/groups/literature/documents/qs/2080-qs002_-en-e.pdf>

### Reference implementations

- pycomm3 — `LogixDriver` (tag/type parsing) and `SLCDriver` (PCCC file access):
  <https://github.com/ottowayi/pycomm3>
- libplctag — Logix and legacy PLC support:
  <https://github.com/libplctag/libplctag>

### Related in-tree docs

- [`cip-datatypes-reference.md`](cip-datatypes-reference.md) — per-type wire format, type codes,
  ranges, .NET equivalents
- [`cip-networking-overview.md`](cip-networking-overview.md) — wire stack, object model, tag
  services, PCCC tunneling
- [`../../../dataport-definition/Allen-Bradley Logix Adressierung.md`](../../../dataport-definition/Allen-Bradley%20Logix%20Adressierung.md) — Logix symbolic addressing grammar
- [`../../../dataport-definition/Allen-Bradley Legacy Adressierung.md`](../../../dataport-definition/Allen-Bradley%20Legacy%20Adressierung.md) — legacy file-based addressing grammar
