# Tag Services: How Logix Reads and Writes a Tag on the Wire

The CIP services Rockwell defines to read and write a named tag, how a tag is addressed in the
request, and what a reply carries ahead of the value. This document is client-agnostic. It describes
the wire, not how any specific library drives it.

CIP has no tag services of its own. Rockwell defines them in the object-class-specific code range
(*(std)*, see [services](../cip/networking-overview.md#services)), which is why `0x4C` below means
one thing on a tag and another on the Template object of [tag browsing](tag-browsing.md). What the
value bytes mean once they arrive is in [symbolic tag data types](symbolic-tag-data-types.md) and
the [CIP data types](../cip/data-types.md) reference.

## The services

| Service                 | Code   | Target                                                        |
|-------------------------|--------|---------------------------------------------------------------|
| Read Tag                | `0x4C` | A tag, by symbolic path or Symbol instance                    |
| Write Tag               | `0x4D` | A tag, by symbolic path or Symbol instance                    |
| Read Tag Fragmented     | `0x52` | A tag whose value exceeds one packet                          |
| Write Tag Fragmented    | `0x53` | Same, for writes                                              |
| Read-Modify-Write       | `0x4E` | Masked bit write into a tag                                   |
| Multiple Service Packet | `0x0A` | Several of the above in one request (Message Router, *(std)*) |

The Multiple Service Packet is what lets a client fetch many tags in one round trip. It is also the
mechanism libplctag's request packing rides on, described in
[the shared session](../../libplctag/the-shared-session.md). Micro800 supports neither it nor the
Symbol Instance Addressing below; see
[controller families](../../controllers/controller-families.md#which-service-carries-the-request).

## How a tag is addressed

A tag is addressed in one of two ways.

The symbolic path uses the ANSI Extended Symbol Segment `0x91` *(std, encoding in
[EPATH](../cip/networking-overview.md#epath--how-a-request-addresses-an-object))*. `MyTag` becomes
`91 05 4D 79 54 61 67 00`. A member such as `Motor.Speed` chains two symbol segments. An array
element such as `Arr[5]` appends a member/element segment (`28 05`). A program-scoped tag is
prefixed with the symbolic segment `Program:<name>`.

Symbol Instance Addressing uses class `0x6B` plus the instance id learned from the
[listing](tag-browsing.md#service-0x55-get-instance-attribute-list). It is shorter on the wire and
faster in the controller, and available from firmware v21. Micro800 does not support it.

## Request and reply data

The request data of a Read Tag is the element count, a UINT. The reply data starts with the tag's
type and then the value. For an atomic tag that is a 2-byte type code (`C4 00` for `DINT`) followed
by the value in the [reference](../cip/data-types.md) layout. For a structured tag it is the
abbreviated-structure marker and a handle, described next.

A Write Tag carries the same type prefix ahead of the value it sends, and the controller rejects a
write whose prefix does not match the tag's declared type.

### The structure reply

When a structured tag (a UDT, a `STRING`, a `TIMER`) is read, the reply data begins with the
abbreviated-structure marker `0xA0`, one of the constructed type codes in the
[reference](../cip/data-types.md#7-constructed-type-codes), followed by a 2-byte structure handle:

```text
A0 02        ← type marker: 0xA0 (ABBREV_STRUCT), 0x02 = a 2-byte handle follows
             (read as a little-endian u16 this is 0x02A0)
HH HH        ← 2-byte structure handle: a CRC of the template's type-encoding string
.. .. ..     ← packed member data (little-endian, with alignment pad bytes)
```

The handle identifies the template. A client matches it against the template definition, read from
the [Template object](tag-browsing.md#the-template-object-class-0x6c), class `0x6C`. The handle is
not unique across differently-ordered structs, so the template must be read to learn the actual
member layout. A worked example, the 88-byte `STRING` reply, is with
[the Logix `STRING` structure](symbolic-tag-data-types.md#the-logix-string-structure).

## References

### Rockwell publications

- Rockwell Automation, *Logix 5000 Controllers Data Access* (1756-PM020), on the tag services, the
  symbolic and instance addressing modes, and structure handles:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation, *Type Encoding of Logix Structures in CIP Data Table Read/Write*, on the
  `0xA0` abbreviated-structure marker:
  <https://www.rockwellautomation.com/content/dam/rockwell-automation/sites/downloads/pdf/TypeEncode_CIPRW.pdf>

### Reference implementations

- pycomm3, `LogixDriver`, the tag services end to end:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/logix_driver.py>
- libplctag, `eip_cip.c`, the read and write request builders:
  <https://github.com/libplctag/libplctag/blob/release/src/protocols/ab/eip_cip.c>
