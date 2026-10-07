# PCCC: How the File-Addressed Families Read and Write on the Wire

How a request reaches a data file on MicroLogix, SLC 500, and PLC-5 over EtherNet/IP. This document
is client-agnostic. It describes the wire, not how any specific library drives it.

What the data-file bytes mean once they arrive is in
[PCCC data-file types](pccc-data-file-types.md). The counterpart for the tag-addressed families is
[tag services](tag-services.md).

## The PCCC tunnel

These controllers do not speak the CIP [tag services](tag-services.md). Their application layer is
PCCC (Programmable Controller Communication Commands), the same command set used over DF1 serial
links. Over EtherNet/IP, a PCCC command is tunneled inside an ordinary CIP explicit message *(std,
see [the message-router format](../cip/networking-overview.md#cip-message-router-requestreply-format))*.

The message is sent to the PCCC object, class `0x67` in the vendor range, instance `1` (path
`20 67 24 01`), using the Execute PCCC service `0x4B`, the first code of the object-class-specific
range. The service data carries a requestor ID header followed by the PCCC command bytes: a CMD/FNC
pair such as CMD `0x0F` / FNC `0xA2` for "protected typed logical read", or FNC `0xAA`/`0xAB` for a
write. These address a data file by file number, element, and sub-element (`N7:0`, `T4:0.PRE`, and
so on).

Controllers without native Ethernet (older PLC-5, SLC 5/03·5/04, MicroLogix 1000/1200/1500) reach
EtherNet/IP through a bridge, either a 1756-ENxT + 1756-DHRIO ControlLogix gateway or a 1761-NET-ENI
serial converter, and the CIP route path hops through the bridge to the target node. See
[Reaching a legacy controller through a bridge](../../controllers/chassis-and-route-paths.md#reaching-a-legacy-controller-through-a-bridge).

## References

### Rockwell publications

- Rockwell Automation, *DF1 Protocol and Command Set Reference Manual* (1770-6.5.16), on PCCC commands
  and the data-file model:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1770-rm516_-en-p.pdf>

### Reference implementations

- pycomm3, `SLCDriver`, PCCC file access and per-file-type parsing:
  <https://github.com/ottowayi/pycomm3>

### Related in-tree docs

- [`pccc-data-file-types.md`](pccc-data-file-types.md), what the data files hold
- [`../cip/networking-overview.md`](../cip/networking-overview.md), the standard wire stack and
  message format the tunnel rides on
- [`chassis-and-route-paths.md`](../../controllers/chassis-and-route-paths.md), how a request
  reaches a controller behind a bridge
