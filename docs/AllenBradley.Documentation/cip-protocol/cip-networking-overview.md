# CIP / EtherNet/IP Networking Overview

Background on the networking stack that Allen-Bradley PLCs use over Ethernet. **This document
is client-agnostic** — it describes the protocol on the wire, not how any specific library
implements it.

> **Naming.** In "EtherNet/IP", the **IP stands for *Industrial Protocol***, not Internet
> Protocol. EtherNet/IP is the Ethernet adaptation of **CIP** (the Common Industrial Protocol).
> CIP is the media-independent application layer; EtherNet/IP is CIP carried over standard
> TCP/IP and UDP/IP. Both are open standards administered by [ODVA](https://www.odva.org/).

## Protocol landscape

Allen-Bradley controllers speak one of two application-layer protocols over Ethernet,
depending on generation:

| Protocol           | Era     | Auth / Crypto                            | Specification                        | Used by                                      |
|--------------------|---------|------------------------------------------|--------------------------------------|----------------------------------------------|
| **CIP** (EtherNet/IP) | ~2001+ | None by default (CIP Security is opt-in) | Open standard (ODVA CIP Networks Library) | Logix (ControlLogix/CompactLogix/GuardLogix/SoftLogix), Micro800 |
| **PCCC** (over EtherNet/IP) | ~1990s | None | Rockwell DF1/PCCC command set (pub. 1770-6.5.16), tunneled in CIP | PLC-5, SLC-500, MicroLogix (legacy, file-based) |

Both reach the controller through the **same EtherNet/IP encapsulation layer** on TCP port
44818. The difference is the application payload: modern controllers use native CIP tag
services; legacy controllers use PCCC commands wrapped in a CIP "Execute PCCC" service (see
[Legacy PCCC tunneling](#legacy-pccc-tunneling)).

CIP is also carried over other physical layers by ODVA's sibling networks — **DeviceNet** (CIP
over CAN), **ControlNet** (CIP over a dedicated token network), and **CompoNet**. This document
covers only the **EtherNet/IP** adaptation.

### CIP vs. proprietary protocols

Unlike reverse-engineered or proprietary PLC protocols (Siemens S7comm, S7comm Plus), CIP and
EtherNet/IP are **published, vendor-neutral standards**. The specification — the *CIP Networks
Library* (Volume 1 = CIP common, Volume 2 = the EtherNet/IP adaptation, Volume 8 = CIP
Security) — is available from ODVA, and multiple independent open-source stacks implement it
(OpENer, pycomm3, libplctag, cpppo, EEIP.NET). A license from ODVA is required to *ship* a
conformant product, but the wire format is documented rather than secret.

## Explicit vs. implicit messaging

CIP defines two messaging models. Data-acquisition clients almost always use the first:

| Model                    | Transport      | Purpose                                                                  |
|--------------------------|----------------|--------------------------------------------------------------------------|
| **Explicit messaging**   | TCP 44818      | Request/response access to named objects and tags (read/write, diagnostics, discovery). Non-time-critical. |
| **Implicit (I/O) messaging** | UDP 2222   | Cyclic producer/consumer I/O data at a fixed **RPI** (Requested Packet Interval). Time-critical. |

Reading and writing tags for data acquisition is **explicit messaging over TCP 44818**.
Implicit messaging is how a controller exchanges real-time I/O with its distributed I/O modules
and other controllers; the connection that governs it is still *opened* via explicit messaging
(a Forward Open — see below), but the cyclic data itself flows over UDP 2222. The rest of this
document focuses on explicit messaging.

## Ports and transports

| Port      | Transport | Purpose                                                                     |
|-----------|-----------|-----------------------------------------------------------------------------|
| **44818** | **TCP**   | Explicit messaging: encapsulation session, unconnected (UCMM) and connected (Class 3) requests. Reliable, routable. |
| **2222**  | **UDP**   | Implicit (Class 0/1) cyclic I/O data.                                        |
| **44818** | **UDP**   | Broadcast device **discovery** (`ListIdentity` / `ListServices`).            |
| **2221**  | TCP/UDP   | CIP Security secure transport (TLS/DTLS), when enabled — see [Security](#security-considerations). |

## The EtherNet/IP wire stack

Unlike Siemens S7comm (which frames its payload with the legacy TPKT/COTP OSI layers),
EtherNet/IP rides directly on standard TCP with a single, simple framing layer of its own — the
**encapsulation** header — that carries a CIP message:

```text
┌──────────────────────────────────────────────────────────┐
│                    TCP  (port 44818)                     │
│  ┌────────────────────────────────────────────────────┐  │
│  │        EtherNet/IP Encapsulation (24-byte hdr)     │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │        Common Packet Format (CPF)            │  │  │
│  │  │  ┌────────────────────────────────────────┐  │  │  │
│  │  │  │      CIP Message Router request        │  │  │  │
│  │  │  │   (service + EPATH + service data)     │  │  │  │
│  │  │  └────────────────────────────────────────┘  │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

All fields are **little-endian**.

### Encapsulation header (24 bytes, fixed)

Every EtherNet/IP message — over TCP or UDP — begins with this 24-byte header:

```text
 0       2       4               8              12                      20      24
┌───────┬───────┬───────────────┬───────────────┬───────────────────────┬───────┐
│Command│Length │ Session Handle│    Status     │    Sender Context     │Options│
│ (u16) │ (u16) │    (u32)      │    (u32)      │      (8 bytes)        │ (u32) │
└───────┴───────┴───────────────┴───────────────┴───────────────────────┴───────┘
```

| Offset | Field           | Size | Meaning                                                              |
|--------|-----------------|------|----------------------------------------------------------------------|
| 0      | Command         | 2    | Encapsulation command code (see table below)                         |
| 2      | Length          | 2    | Byte count of the command-specific data that *follows* the 24-byte header |
| 4      | Session Handle  | 4    | Assigned by the target in the `RegisterSession` reply; echoed on every subsequent message |
| 8      | Status          | 4    | `0x00000000` = success (requests send 0; replies carry encapsulation status) |
| 12     | Sender Context  | 8    | Opaque; the target echoes it verbatim so the originator can match replies |
| 20     | Options         | 4    | Reserved; shall be 0                                                  |

### Encapsulation commands

| Command             | Code     | Transport      | Use                                                     |
|---------------------|----------|----------------|---------------------------------------------------------|
| NOP                 | `0x0000` | TCP            | Keep-alive / no operation; no reply                     |
| ListServices        | `0x0004` | TCP or UDP     | Enumerate the encapsulation services a device offers    |
| ListIdentity        | `0x0063` | TCP or UDP     | Device discovery — returns Identity object info         |
| ListInterfaces      | `0x0064` | TCP or UDP     | List non-CIP communication interfaces (optional)        |
| **RegisterSession** | `0x0065` | TCP            | Open a session; obtain the Session Handle               |
| UnRegisterSession   | `0x0066` | TCP            | Close the session and free resources                    |
| **SendRRData**      | `0x006F` | TCP            | Send an **unconnected** (UCMM) request; expects a reply |
| **SendUnitData**    | `0x0070` | TCP            | Send **connected** (Class 3) data                       |

`SendRRData` ("send request/reply data") and `SendUnitData` are TCP-only. `ListIdentity` and
`ListServices` are the commands typically broadcast over UDP 44818 to discover devices.

### Session registration

Before any CIP request, the client registers a session:

1. Client sends **RegisterSession (`0x0065`)** with 4 bytes of command data: **Protocol
   Version** = `1` (u16) and **Options Flags** = `0` (u16). The header's Session Handle is 0.
2. The target replies with the same command; the header now carries a nonzero **Session
   Handle** (u32).
3. The client places that handle in the header of **every** subsequent message.
4. **UnRegisterSession (`0x0066`)** (or a TCP close) ends the session.

### Common Packet Format (CPF)

`SendRRData` and `SendUnitData` carry their payload in a **Common Packet Format** list: an item
count followed by an *address item* and a *data item*.

```text
Item Count               u16   (usually 2)
Item 1: Type ID (u16), Length (u16), Data[]     ← Address item
Item 2: Type ID (u16), Length (u16), Data[]     ← Data item
```

| Type ID  | Name                    | Category | Notes                                                        |
|----------|-------------------------|----------|--------------------------------------------------------------|
| `0x0000` | Null Address Item       | Address  | Length 0 — "no address", used for unconnected (UCMM) requests |
| `0x00A1` | Connected Address Item  | Address  | Length 4 — carries the Connection ID (u32) for connected messaging |
| `0x00B1` | Connected Data Item     | Data     | Class 3 payload: 2-byte sequence count + CIP request/reply   |
| `0x00B2` | Unconnected Data Item   | Data     | UCMM: carries the CIP message-router request/reply directly  |
| `0x8002` | Sequenced Address Item  | Address  | Connection ID + sequence number, used on UDP I/O (Class 0/1) |
| `0x000C` | CIP Identity Item       | Data     | Payload of a `ListIdentity` reply                            |

Two usage patterns matter for tag access:

- **Unconnected (UCMM)** via `SendRRData`: Item 1 = Null Address (`0x0000`), Item 2 =
  Unconnected Data (`0x00B2`).
- **Connected (Class 3)** via `SendUnitData`: Item 1 = Connected Address (`0x00A1`) with the
  Connection ID, Item 2 = Connected Data (`0x00B1`) with a sequence count.

## The CIP object model

CIP is **object-oriented**. A device is a collection of **objects**; you act on them with
**services**. Every addressable thing is a:

```text
Class  →  Instance  →  Attribute        acted on by a Service
```

- **Class** — a kind of object (Identity, Message Router, a tag Symbol, …).
- **Instance** — a specific object of that class (instance 0 = the class itself; instance ≥ 1 =
  an actual object).
- **Attribute** — a data field of an instance.
- **Service** — an operation (read attribute, write attribute, read tag, …).

### Standard object classes

| Class    | Object                | Role                                                            |
|----------|-----------------------|-----------------------------------------------------------------|
| `0x01`   | Identity              | Vendor ID, device type, product code, revision, serial, name (required) |
| `0x02`   | Message Router        | Routes explicit messages to target objects (required)           |
| `0x04`   | Assembly              | Groups I/O data for implicit messaging                          |
| `0x06`   | Connection Manager    | Forward Open / Forward Close / Unconnected Send (required)      |
| `0x6B`   | Symbol Object         | Logix **tags** (tag name in attribute 1) — Rockwell vendor-specific |
| `0x6C`   | Template Object       | Logix **UDT / structure** layout definitions — Rockwell vendor-specific |
| `0x67`   | PCCC Object           | Tunnels legacy PCCC commands (see below) — Rockwell vendor-specific |
| `0xF5`   | TCP/IP Interface      | IP configuration                                                |
| `0xF6`   | Ethernet Link         | Per-port link status and counters                               |

> **Note:** the Message Router is class `0x02`. Some third-party summaries mislabel it `0x03` —
> that code is DeviceNet-specific.

### EPATH — how a request addresses an object

A CIP request identifies its target with an **EPATH**: a packed sequence of **segments**. Each
logical segment is one type byte plus its value. The type byte is
`[segment type][logical type][format]`; logical segments use the base `0x20`:

| Segment          | 8-bit form | 16-bit form | Followed by            |
|------------------|------------|-------------|------------------------|
| Class ID         | `0x20`     | `0x21`      | class number           |
| Instance ID      | `0x24`     | `0x25`      | instance number        |
| Member / element | `0x28`     | `0x29`      | member or array index  |
| Attribute ID     | `0x30`     | `0x31`      | attribute number       |

Example — Identity object, attribute 7 (product name): `20 01 24 01 30 07` = Class `0x01`,
Instance `1`, Attribute `7`.

For **named tags**, Logix controllers use the **ANSI Extended Symbol Segment** (`0x91`): the
byte `0x91`, a length (character count), the ASCII name, and a `0x00` pad byte if the length is
odd. So the tag `MyTag` becomes `91 05 4D 79 54 61 67 00`. Members (`Motor.Speed`) chain
symbol segments; array elements (`Arr[5]`) append a member/element segment (`28 05`).

## Connection setup sequence

A client establishes explicit messaging in two or three phases:

```text
Client                                           Controller (ControlLogix / CompactLogix / …)
      │                                                  │
      │─── TCP SYN ─────────────────────────────────────►│
      │◄── TCP SYN-ACK ──────────────────────────────────│
      │─── TCP ACK ─────────────────────────────────────►│
      │                                                  │
      │  Phase 1: TCP connected (port 44818)             │
      │                                                  │
      │─── RegisterSession (0x0065) ────────────────────►│
      │◄── RegisterSession reply (Session Handle) ───────│
      │                                                  │
      │  Phase 2: encapsulation session registered       │
      │                                                  │
      │  ── (optional) Forward Open (0x54) ─────────────►│   ← for connected (Class 3) messaging
      │◄── Forward Open reply (Connection IDs) ──────────│
      │                                                  │
      │  Phase 3: CIP connection established              │
      │                                                  │
      │─── Read/Write Tag requests ─────────────────────►│
      │◄── replies ──────────────────────────────────────│
```

**Phase 1 — TCP.** Connect to port 44818. Clients typically disable Nagle (`NoDelay`) to keep
the small request messages prompt.

**Phase 2 — RegisterSession.** Obtain the Session Handle used in every later header.

**Phase 3 — CIP messaging.** From here the client can send requests two ways:

### Unconnected (UCMM) messaging

A one-shot request/reply with no prior connection setup, sent with `SendRRData` (Null Address +
Unconnected Data items). When the request must be **routed** through a chassis backplane to a
processor, the CIP payload is wrapped in an **Unconnected Send (`0x52`)** service to the
Connection Manager, which carries the route path. UCMM is simplest and fine for occasional
requests (discovery, a single read), but has higher per-message overhead and the controller
limits how many concurrent UCMM requests it will service.

### Connected (Class 3 explicit) messaging

For repeated tag access, the client first opens a **CIP connection** with a **Forward Open**,
then sends every request with `SendUnitData` (Connected Address + Connected Data items), keyed
by the Connection ID and a sequence count. Lower per-message overhead, guaranteed controller
resources, and connection-timeout monitoring. Rockwell recommends connected messaging for
sustained data access.

### Connection Manager, Forward Open, and route paths

The **Connection Manager** object (class `0x06`, instance `1`; path `20 06 24 01`) owns
connection lifecycle:

| Service            | Code   | Purpose                                                             |
|--------------------|--------|---------------------------------------------------------------------|
| Forward Open       | `0x54` | Open a connection (connection size ≤ 511 bytes)                     |
| Large Forward Open | `0x5B` | Open a connection with 32-bit parameters (size > 511 bytes, up to ~4000 on Logix) |
| Forward Close      | `0x4E` | Tear down a connection                                              |
| Unconnected Send   | `0x52` | Wrap and route an embedded request through the backplane / network |

> **Service codes are object-dependent.** The same numeric code means different things on
> different object classes. On the **Connection Manager**, `0x52` = Unconnected Send and `0x4E`
> = Forward Close; on the **Symbol (tag) object**, `0x52` = Read Tag Fragmented and `0x4E` =
> Read-Modify-Write. Always resolve a service code against its target object class.

**Route path (backplane routing).** The module that terminates the EtherNet/IP session is not
necessarily the controller — in a ControlLogix chassis the Ethernet module and the controller sit
in different slots — so a request carries a **route path** telling each device it reaches how to
forward it onward. Each hop is a **port segment**: a port number naming the network to leave by,
followed by a link address on that network.

- `01 00` = **port 1 (backplane), link address 0 (slot 0)** — the familiar "**1,0**".
- Multi-hop routes chain port segments (out an Ethernet port, across a backplane, to another
  module). Each device consumes the hop that names it and forwards the remainder.

This is exactly the `Path = "1,0"` seen in libplctag-style client configuration. What the port
numbers and link addresses mean physically, and which path each controller family conventionally
takes, is in
[Controller families, chassis, and route paths](controller-families-and-routing.md).

## CIP message-router request/reply format

Inside the CPF data item, a CIP request has this shape:

```text
Request:
┌─────────┬───────────────┬──────────────────────┬──────────────┐
│ Service │ Path Size     │ Request Path (EPATH)  │ Request Data │
│ (1 byte)│ (words, 1 B)  │  (Path Size × 2 B)    │  (variable)  │
└─────────┴───────────────┴──────────────────────┴──────────────┘

Reply:
┌───────────────┬──────────┬────────────────┬─────────────────────┬────────────┐
│ Reply Service │ Reserved │ General Status │ Ext. Status Size    │ Reply Data │
│ (svc | 0x80)  │  (0x00)  │   (1 byte)     │ (words, 1 B) + ext. │ (variable) │
└───────────────┴──────────┴────────────────┴─────────────────────┴────────────┘
```

- **Path Size** is measured in **16-bit words**.
- The **reply service** byte is the request service **OR `0x80`** (the reply bit). A reply to
  Read Tag (`0x4C`) is `0xCC`.

### General status codes

The one-byte **General Status** signals success or the class of failure (CIP Vol. 1,
Appendix B). The most common:

| Code   | Meaning                                                            |
|--------|--------------------------------------------------------------------|
| `0x00` | Success                                                            |
| `0x04` | Path segment error (malformed EPATH)                              |
| `0x05` | Path destination unknown (the class / instance does not exist)    |
| `0x06` | Partial transfer — only part of the data was returned (see below) |
| `0x08` | Service not supported                                             |
| `0x13` | Not enough data                                                   |
| `0x14` | Attribute not supported (object exists, attribute does not)       |
| `0x15` | Too much data                                                     |
| `0x1E` | Embedded service error                                            |

> **`0x05` vs. `0x14`.** `0x05` means the target class/instance itself is missing (e.g. a tag
> name that does not exist); `0x14` means the object exists but the requested attribute does
> not. And **`0x06` is not an error** for large reads — it means "more data remains", which is
> how a client learns to continue with a fragmented read.

## Legacy PCCC tunneling

PLC-5, SLC-500, and MicroLogix controllers do not speak native CIP tag services. Their
application layer is **PCCC** (Programmable Controller Communication Commands) — the same
command set used over DF1 serial links. Over EtherNet/IP, a PCCC command is tunneled inside a
CIP explicit message:

- Sent to the **PCCC Object** — class **`0x67`** (Rockwell vendor-specific), instance `1`
  (path `20 67 24 01`).
- Using the **Execute PCCC** service — **`0x4B`**.
- The service data carries a *requestor ID* header followed by the PCCC command bytes
  (a **CMD**/**FNC** pair, e.g. CMD `0x0F` / FNC `0xA2` = "protected typed logical read", FNC
  `0xAA`/`0xAB` = write), which address a data file by **file number, element, sub-element**
  (`N7:0`, `T4:0.PRE`, …).

Controllers without native Ethernet (older PLC-5, SLC 5/03·5/04, MicroLogix 1000/1200/1500)
reach EtherNet/IP through a bridge — a 1756-ENxT + 1756-DHRIO ControlLogix gateway, or a
1761-NET-ENI serial converter — and the CIP route path hops through the bridge to the target
node.

## Security considerations

Classic EtherNet/IP has **no authentication and no encryption** — any device on the network can
read and write controller data. Like classic Modbus/TCP and S7comm, it predates modern security
practice.

**CIP Security** (ODVA *CIP Networks Library* Volume 8) is an optional extension that adds:

- **Secure transport** — **TLS** (RFC 5246) for TCP explicit messaging and **DTLS** (RFC 6347)
  for UDP implicit I/O, on port **2221**.
- **Device authentication** — X.509 certificates or pre-shared keys.
- **Message integrity** (HMAC) and **optional confidentiality** (AES); a NULL-cipher mode
  allows authentication-only (still packet-capture-decodable).

CIP Security is supported on newer Logix controllers (e.g. ControlLogix/CompactLogix 5580/5380
with recent firmware, or older Logix retrofitted with a 1756-EN4TR module). Legacy PLC-5 /
SLC-500 / MicroLogix / Micro800 controllers do **not** support it. Adoption is still recent and
optional — most deployed EtherNet/IP networks run unauthenticated.

Mitigations for the classic (unsecured) case mirror those for any legacy PLC protocol:

- **Network segmentation** — isolate the automation network from general IT.
- **Firewall rules** — restrict access to ports 44818 / 2222.
- **CIP Security** — enable it where the controllers and tooling support it.

## References

### ODVA specifications

- ODVA — *The CIP Networks Library*: Volume 1 (CIP common — object model, services App. A,
  status codes App. B, data types App. C), Volume 2 (EtherNet/IP adaptation — encapsulation and
  CPF), Volume 8 (CIP Security). See <https://www.odva.org/technology-standards/key-technologies/common-industrial-protocol-cip/>
- ODVA — *Common Industrial Protocol (CIP) and the Family of CIP Networks* (PUB00123R1):
  <https://www.odva.org/wp-content/uploads/2020/06/PUB00123R1_Common-Industrial_Protocol_and_Family_of_CIP_Networks.pdf>
- ODVA — *CIP Security* overview: <https://www.odva.org/technology-standards/distinct-cip-services/cip-security/>

### Rockwell publications

- Rockwell Automation — *Logix 5000 Controllers Data Access* (1756-PM020) — Symbol object
  `0x6B`, Template object `0x6C`, Read/Write Tag services:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation — *DF1 Protocol and Command Set Reference Manual* (1770-6.5.16) — PCCC
  command set:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1770-rm516_-en-p.pdf>

### Reference implementations and tooling

- Wireshark ENIP/CIP dissector (`packet-enip.c` / `packet-cip.c`) — encapsulation commands, CPF
  items: <https://github.com/wireshark/wireshark/tree/master/epan/dissectors>
- OpENer (EIPStackGroup) — an open-source EtherNet/IP stack:
  <https://github.com/EIPStackGroup/OpENer>
- pycomm3 — Python CIP driver and *CIP Reference* (services, class codes, status codes):
  <https://docs.pycomm3.dev/en/latest/cip_reference.html>
- Real Time Automation — *EtherNet/IP: A Technical Introduction*:
  <https://www.rtautomation.com/technologies/ethernetip/>

### Related in-tree docs

- [`cip-datatypes-reference.md`](cip-datatypes-reference.md) — CIP type codes and wire formats
- [`symbolic-tag-data-types.md`](symbolic-tag-data-types.md) — the types the tag-addressed families
  expose, and the symbol table that names them
- [`pccc-data-file-types.md`](pccc-data-file-types.md) — the data-file types of the legacy families
