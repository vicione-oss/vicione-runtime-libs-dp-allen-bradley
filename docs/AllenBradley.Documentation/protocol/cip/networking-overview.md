# CIP / EtherNet/IP Networking Overview

Background on the networking stack that EtherNet/IP devices share, as ODVA specifies it. This
document is client-agnostic and vendor-agnostic. It describes the standard protocol on the wire, not
how any specific library implements it and not what Rockwell adds on top. The Rockwell additions
(tag objects, the PCCC tunnel, chassis routing conventions) are in
[`../allen-bradley-extension/`](../allen-bradley-extension/).

> **Naming.** In "EtherNet/IP", the IP stands for *Industrial Protocol*, not Internet Protocol.
> EtherNet/IP is the Ethernet adaptation of CIP, the Common Industrial Protocol. CIP is the
> media-independent application layer, and EtherNet/IP is CIP carried over standard TCP/IP and
> UDP/IP. Both are open standards administered by [ODVA](https://www.odva.org/).

## Protocol landscape

CIP is the application layer. EtherNet/IP is one of several adaptations that carry it. The others
are DeviceNet (CIP over CAN), ControlNet (CIP over a dedicated token network), and CompoNet. This
document covers only the EtherNet/IP adaptation.

Everything an EtherNet/IP device exchanges goes through the same encapsulation layer on TCP port
44818, whatever the application payload is. A vendor may carry an older protocol inside a CIP
service, as Rockwell does for its legacy controllers, but that is an extension and not part of the
standard. Which Allen-Bradley line uses which payload is in
[Controller families](../../controllers/controller-families.md#which-service-carries-the-request).

### CIP vs. proprietary protocols

Unlike reverse-engineered or proprietary PLC protocols such as Siemens S7comm and S7comm Plus, CIP
and EtherNet/IP are published, vendor-neutral standards. The specification is the *CIP Networks
Library* (Volume 1 = CIP common, Volume 2 = the EtherNet/IP adaptation, Volume 8 = CIP Security).
It is available from ODVA, and several independent open-source stacks implement it (OpENer,
pycomm3, libplctag, cpppo, EEIP.NET). A license from ODVA is required to ship a conformant product,
but the wire format is documented rather than secret.

## Explicit vs. implicit messaging

CIP defines two messaging models. Data-acquisition clients almost always use the first:

| Model                    | Transport      | Purpose                                                                  |
|--------------------------|----------------|--------------------------------------------------------------------------|
| **Explicit messaging**   | TCP 44818      | Request/response access to named objects and tags (read/write, diagnostics, discovery). Non-time-critical. |
| **Implicit (I/O) messaging** | UDP 2222   | Cyclic producer/consumer I/O data at a fixed **RPI** (Requested Packet Interval). Time-critical. |

Reading and writing tags for data acquisition is explicit messaging over TCP 44818. Implicit
messaging is how a controller exchanges real-time I/O with its distributed I/O modules and other
controllers. The connection that governs it is still opened via explicit messaging, with a Forward
Open described below, but the cyclic data itself flows over UDP 2222. The rest of this document
focuses on explicit messaging.

## Ports and transports

| Port      | Transport | Purpose                                                                     |
|-----------|-----------|-----------------------------------------------------------------------------|
| **44818** | **TCP**   | Explicit messaging: encapsulation session, unconnected (UCMM) and connected (Class 3) requests. Reliable, routable. |
| **2222**  | **UDP**   | Implicit (Class 0/1) cyclic I/O data.                                        |
| **44818** | **UDP**   | Broadcast device **discovery** (`ListIdentity` / `ListServices`).            |
| **2221**  | TCP/UDP   | CIP Security secure transport (TLS/DTLS), when enabled; see [Security](#security-considerations). |

## The EtherNet/IP wire stack

Siemens S7comm frames its payload with the legacy TPKT/COTP OSI layers. EtherNet/IP does not. It
rides directly on standard TCP with a single framing layer of its own, the encapsulation header,
which carries a CIP message:

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

All fields are little-endian.

### Encapsulation header (24 bytes, fixed)

Every EtherNet/IP message, over TCP or UDP, begins with this 24-byte header:

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
| ListIdentity        | `0x0063` | TCP or UDP     | Device discovery; returns Identity object info          |
| ListInterfaces      | `0x0064` | TCP or UDP     | List non-CIP communication interfaces (optional)        |
| **RegisterSession** | `0x0065` | TCP            | Open a session; obtain the Session Handle               |
| UnRegisterSession   | `0x0066` | TCP            | Close the session and free resources                    |
| **SendRRData**      | `0x006F` | TCP            | Send an **unconnected** (UCMM) request; expects a reply |
| **SendUnitData**    | `0x0070` | TCP            | Send **connected** (Class 3) data                       |

`SendRRData` ("send request/reply data") and `SendUnitData` are TCP-only. `ListIdentity` and
`ListServices` are the commands typically broadcast over UDP 44818 to discover devices.

### Session registration

Before any CIP request, the client registers a session:

1. The client sends RegisterSession (`0x0065`) with 4 bytes of command data: Protocol Version =
   `1` (u16) and Options Flags = `0` (u16). The header's Session Handle is 0.
2. The target replies with the same command. The header now carries a nonzero Session Handle (u32).
3. The client places that handle in the header of every subsequent message.
4. UnRegisterSession (`0x0066`), or a TCP close, ends the session.

### Common Packet Format (CPF)

`SendRRData` and `SendUnitData` carry their payload in a Common Packet Format list, which is an
item count followed by an address item and a data item.

```text
Item Count               u16   (usually 2)
Item 1: Type ID (u16), Length (u16), Data[]     ← Address item
Item 2: Type ID (u16), Length (u16), Data[]     ← Data item
```

| Type ID  | Name                    | Category | Notes                                                        |
|----------|-------------------------|----------|--------------------------------------------------------------|
| `0x0000` | Null Address Item       | Address  | Length 0, "no address", used for unconnected (UCMM) requests |
| `0x00A1` | Connected Address Item  | Address  | Length 4, carries the Connection ID (u32) for connected messaging |
| `0x00B1` | Connected Data Item     | Data     | Class 3 payload: 2-byte sequence count + CIP request/reply   |
| `0x00B2` | Unconnected Data Item   | Data     | UCMM: carries the CIP message-router request/reply directly  |
| `0x8002` | Sequenced Address Item  | Address  | Connection ID + sequence number, used on UDP I/O (Class 0/1) |
| `0x000C` | CIP Identity Item       | Data     | Payload of a `ListIdentity` reply                            |

Two usage patterns matter for tag access. Unconnected (UCMM) via `SendRRData` uses a Null Address
(`0x0000`) as item 1 and Unconnected Data (`0x00B2`) as item 2. Connected (Class 3) via
`SendUnitData` uses a Connected Address (`0x00A1`) with the Connection ID as item 1 and Connected
Data (`0x00B1`) with a sequence count as item 2.

## The CIP object model

CIP is object-oriented. A device is a collection of objects, and you act on them with services.
Every addressable thing is a:

```text
Class  →  Instance  →  Attribute        acted on by a Service
```

A class is a kind of object (Identity, Message Router, Connection Manager, and so on). An instance
is a specific object of that class, where instance 0 is the class itself and instance 1 or higher
is an actual object. An attribute is a data field of an instance. A service is an operation, such as
reading or writing an attribute.

Class IDs are partitioned between ODVA and the vendors:

| Class ID range | Defined by                                                        |
|----------------|-------------------------------------------------------------------|
| `0x00`-`0x63`  | CIP itself: the same object on every conformant device             |
| `0x64`-`0xC7`  | The vendor: meaning depends on who built the device                |
| `0xF0`-`0x2FF` | CIP again: the network-specific objects (TCP/IP Interface, …)      |

### Standard object classes

| Class    | Object                | Role                                                            |
|----------|-----------------------|-----------------------------------------------------------------|
| `0x01`   | Identity              | Vendor ID, device type, product code, revision, serial, name (required) |
| `0x02`   | Message Router        | Routes explicit messages to target objects (required)           |
| `0x04`   | Assembly              | Groups I/O data for implicit messaging                          |
| `0x06`   | Connection Manager    | Forward Open / Forward Close / Unconnected Send (required)      |
| `0xF5`   | TCP/IP Interface      | IP configuration                                                |
| `0xF6`   | Ethernet Link         | Per-port link status and counters                               |

> **Note:** the Message Router is class `0x02`. Some third-party summaries mislabel it `0x03`,
> which is a DeviceNet-specific code.

The Rockwell classes a tag client meets (Symbol `0x6B`, Template `0x6C`, PCCC `0x67`) sit in the
vendor range and are documented with the
[extension](../allen-bradley-extension/tag-browsing.md).

### Services

A service is an operation invoked on an object, comparable to a method call or an RPC opcode. The
word comes from OSI terminology, where a layer offers "services" through request/response
primitives. UDS, CANopen SDO, MMS / IEC 61850 and OPC UA use it the same way. It does not mean a
long-running process. Do not confuse it with the encapsulation command `ListServices` above, which
lists encapsulation-layer capabilities, not object services.

Service codes are partitioned like class IDs:

| Service code range | Meaning                                                        |
|--------------------|----------------------------------------------------------------|
| `0x00`-`0x31`      | Common services: the same meaning on every object              |
| `0x32`-`0x4A`      | Vendor-specific                                                |
| `0x4B`-`0x63`      | Object-class-specific: meaning depends on the target class     |

The common services a client uses most:

| Code   | Name                       | Purpose                                            |
|--------|----------------------------|----------------------------------------------------|
| `0x01` | Get_Attributes_All         | Dump every attribute of an instance; layout must be known |
| `0x03` | Get_Attribute_List         | Read several attributes of one instance in one request |
| `0x0E` | Get_Attribute_Single       | Read one attribute                                 |
| `0x10` | Set_Attribute_Single       | Write one attribute                                |
| `0x11` | Find_Next_Object_Instance  | Instance IDs of a class; optional and rare         |

A reply carries the request's service code with bit 7 set, so `0x0E` is answered by `0x8E`. The
request and reply layouts are in the
[message-router format](#cip-message-router-requestreply-format) below.

### EPATH — how a request addresses an object

A CIP request identifies its target with an EPATH, a packed sequence of segments. Each logical
segment is one type byte plus its value. The type byte is `[segment type][logical type][format]`,
and logical segments use the base `0x20`:

| Segment          | 8-bit form | 16-bit form | Followed by            |
|------------------|------------|-------------|------------------------|
| Class ID         | `0x20`     | `0x21`      | class number           |
| Instance ID      | `0x24`     | `0x25`      | instance number        |
| Member / element | `0x28`     | `0x29`      | member or array index  |
| Attribute ID     | `0x30`     | `0x31`      | attribute number       |

As an example, the Identity object's attribute 7 (product name) is `20 01 24 01 30 07`, which
reads as Class `0x01`, Instance `1`, Attribute `7`.

The 16-bit form carries a pad byte before the value: `21 00 6B 00` is class `0x006B`.

CIP also defines the ANSI Extended Symbol Segment (`0x91`): the byte `0x91`, a length (character
count), the ASCII name, and a `0x00` pad byte if the length is odd. So the name `MyTag` becomes
`91 05 4D 79 54 61 67 00`. CIP defines only the encoding. What a name resolves to is up to the
vendor, and Rockwell resolves it to a tag; see
[the extension](../allen-bradley-extension/tag-services.md).

### Discovery in standard CIP

Standard CIP has no online browse. A device is meant to describe itself offline, through its EDS
file (Electronic Data Sheet), which the client selects from the Identity object. What exists online
is limited:

| Mechanism                                                              | Purpose                          | Note                                  |
|------------------------------------------------------------------------|----------------------------------|---------------------------------------|
| Identity object (`0x01`)                                               | Vendor, product code, revision   | Used to pick the EDS file             |
| Message Router (`0x02`), attribute 1 `Object_list`                     | Implemented class IDs            | Optional, often missing               |
| Class attributes (instance 0): 2 `Max Instance`, 3 `Number of Instances` | Instance count per class       |                                       |
| `Find_Next_Object_Instance` (`0x11`)                                   | Instance IDs of a class          | Optional, rare                        |
| `Get_Attributes_All` (`0x01`)                                          | Dump attributes                  | Layout must be known in advance       |
| Encapsulation `ListIdentity`, `ListServices`                           | Find devices on the network      | Not the objects inside a device       |

Browsing the tags of a controller by name is therefore a vendor extension. Rockwell's is the
Symbol object and its `Get Instance Attribute List` service, documented with the
[extension](../allen-bradley-extension/tag-browsing.md).

## Connection setup sequence

A client establishes explicit messaging in two or three phases:

```text
Client                                           Target device
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
      │─── explicit requests ───────────────────────────►│
      │◄── replies ──────────────────────────────────────│
```

Phase 1 is TCP. Connect to port 44818. Clients typically disable Nagle (`NoDelay`) to keep the
small request messages prompt.

Phase 2 is RegisterSession. Obtain the Session Handle used in every later header.

Phase 3 is CIP messaging. From here the client can send requests in two ways.

### Unconnected (UCMM) messaging

A one-shot request/reply with no prior connection setup, sent with `SendRRData` (Null Address +
Unconnected Data items). When the request must be routed through a chassis backplane to a
processor, the CIP payload is wrapped in an Unconnected Send (`0x52`) service to the Connection
Manager, which carries the route path. UCMM is simplest and fine for occasional requests such as
discovery or a single read. It has higher per-message overhead, though, and the controller limits
how many concurrent UCMM requests it will service.

### Connected (Class 3 explicit) messaging

For repeated tag access, the client first opens a CIP connection with a Forward Open, then sends
every request with `SendUnitData` (Connected Address + Connected Data items), keyed by the
Connection ID and a sequence count. This gives lower per-message overhead, guaranteed controller
resources, and connection-timeout monitoring. Vendors recommend connected messaging for sustained
data access.

### Connection Manager, Forward Open, and route paths

The Connection Manager object (class `0x06`, instance `1`; path `20 06 24 01`) owns the connection
lifecycle:

| Service            | Code   | Purpose                                                             |
|--------------------|--------|---------------------------------------------------------------------|
| Forward Open       | `0x54` | Open a connection (connection size ≤ 511 bytes)                     |
| Large Forward Open | `0x5B` | Open a connection with 32-bit parameters (size > 511 bytes; the ceiling is the device's) |
| Forward Close      | `0x4E` | Tear down a connection                                              |
| Unconnected Send   | `0x52` | Wrap and route an embedded request through the backplane / network |

> **Service codes are object-dependent.** All four codes above sit in the object-class-specific
> range (`0x4B`-`0x63`, see [Services](#services)), so the same number means something else on
> another class. On the Connection Manager, `0x52` = Unconnected Send and `0x4E` = Forward Close.
> On Rockwell's Symbol object, `0x52` = Read Tag Fragmented and `0x4E` = Read-Modify-Write. Always
> resolve a service code against its target object class.

The module that terminates the EtherNet/IP session is not necessarily the controller. In a modular
chassis the Ethernet module and the controller may sit in different slots, so a request carries a
route path telling each device it reaches how to forward it onward. Each hop is a port segment: a
port number naming the network to leave by, followed by a link address on that network. `01 00`
means port 1 (the backplane), link address 0 (slot 0), which is the familiar "1,0". Multi-hop routes
chain port segments, for example out an Ethernet port, across a backplane, to another module. Each
device consumes the hop that names it and forwards the remainder.

This is the `Path = "1,0"` seen in client configuration. What the port numbers and link addresses
mean physically on Rockwell hardware, and which path each controller family conventionally takes,
is in
[Controller families, chassis, and route paths](../../controllers/controller-families.md).

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

Path Size is measured in 16-bit words. The reply service byte is the request service OR `0x80`,
the reply bit, so a reply to Get_Attribute_Single (`0x0E`) is `0x8E`.

### General status codes

The one-byte General Status signals success or the class of failure (CIP Vol. 1, Appendix B). The
most common:

| Code   | Meaning                                                            |
|--------|--------------------------------------------------------------------|
| `0x00` | Success                                                            |
| `0x04` | Path segment error (malformed EPATH)                              |
| `0x05` | Path destination unknown (the class / instance does not exist)    |
| `0x06` | Partial transfer: only part of the data was returned (see below)  |
| `0x08` | Service not supported                                             |
| `0x13` | Not enough data                                                   |
| `0x14` | Attribute not supported (object exists, attribute does not)       |
| `0x15` | Too much data                                                     |
| `0x1E` | Embedded service error                                            |

> **`0x05` vs. `0x14`.** `0x05` means the target class/instance itself is missing, for example a
> tag name that does not exist. `0x14` means the object exists but the requested attribute does
> not. And `0x06` is not an error for large reads. It means "more data remains", which is how a
> client learns to continue with a fragmented read or a paged listing.

## Security considerations

Classic EtherNet/IP has no authentication and no encryption. Any device on the network can read and
write controller data. Like classic Modbus/TCP and S7comm, it predates modern security practice.

CIP Security (ODVA *CIP Networks Library* Volume 8) is an optional extension. It adds a secure
transport, TLS (RFC 5246) for TCP explicit messaging and DTLS (RFC 6347) for UDP implicit I/O, on
port 2221. It adds device authentication through X.509 certificates or pre-shared keys. And it adds
message integrity (HMAC) with optional confidentiality (AES); a NULL-cipher mode allows
authentication only, which is still packet-capture-decodable.

Adoption is still recent and optional. Most deployed EtherNet/IP networks run unauthenticated.
Which Allen-Bradley lines support it is in
[Controller families](../../controllers/controller-families.md#the-lines).

Mitigations for the classic, unsecured case mirror those for any legacy PLC protocol: segment the
automation network from general IT, restrict access to ports 44818 / 2222 with firewall rules, and
enable CIP Security where the controllers and tooling support it.

## References

### ODVA specifications

- ODVA, *The CIP Networks Library*: Volume 1 (CIP common: object model, services App. A, status
  codes App. B, data types App. C), Volume 2 (EtherNet/IP adaptation: encapsulation and CPF),
  Volume 8 (CIP Security). See <https://www.odva.org/technology-standards/key-technologies/common-industrial-protocol-cip/>
- ODVA, *Common Industrial Protocol (CIP) and the Family of CIP Networks* (PUB00123R1):
  <https://www.odva.org/wp-content/uploads/2020/06/PUB00123R1_Common-Industrial_Protocol_and_Family_of_CIP_Networks.pdf>
- ODVA, *CIP Security* overview: <https://www.odva.org/technology-standards/distinct-cip-services/cip-security/>

### Reference implementations and tooling

- Wireshark ENIP/CIP dissector (`packet-enip.c` / `packet-cip.c`), encapsulation commands, CPF
  items: <https://github.com/wireshark/wireshark/tree/master/epan/dissectors>
- OpENer (EIPStackGroup), an open-source EtherNet/IP stack:
  <https://github.com/EIPStackGroup/OpENer>
- pycomm3, Python CIP driver and *CIP Reference* (encapsulation commands, services, class codes):
  <https://pycomm3.readthedocs.io/en/latest/cip_reference.html>
- Real Time Automation, *EtherNet/IP: A Technical Introduction*:
  <https://www.rtautomation.com/technologies/ethernetip/>

### Related in-tree docs

- [`data-types.md`](data-types.md): CIP type codes and wire formats
- [`../allen-bradley-extension/`](../allen-bradley-extension/): what Rockwell builds on this,
  the Symbol and Template objects, the tag services, the PCCC tunnel, and chassis routing
