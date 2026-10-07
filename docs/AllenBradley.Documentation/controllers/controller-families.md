# Controller Families

Which controllers Rockwell sells under the Allen-Bradley brand, how they are shaped, which tool
programs them, and which protocol mechanism reaches their data. This document is client-agnostic. It
describes Rockwell hardware, not how any specific library addresses it.

Two splits run through the list. The first is addressing. A controller either names its data by
symbolic tag or by numbered data file, and that decides which port serves it. The second, inside the
Logix line, is the [generation](logix-generations.md), which decides which data types a controller
can declare at all.

## Two form factors, not four generations

Allen-Bradley controllers come in two physical shapes, and the split is by product line, not by era.
Both shapes have been sold in parallel since the 1990s, and both are current today.

The chassis-based shape is a kit. You buy a metal frame, a power supply, a controller module, and
communication and I/O modules, then assemble them. PLC-5 (1771 chassis), the modular SLC 500 (1746),
and every generation of ControlLogix from the 1997 5550 to today's 5580 work this way. The controller
is one module among several, which is why "a ControlLogix" colloquially means the assembled chassis
rather than the controller in it. A 5550 through 5570 has no network port of its own and reaches
Ethernet only through an ENxT module in another slot. A 5580 has one on the controller, so a modern
chassis needs no communication module, but it still needs the frame, the supply, and a slot.

The DIN-rail shape has no chassis and no backplane connector. Modules clip together on the rail and
reach each other over a local bus, and the Ethernet port is on the controller itself. MicroLogix,
Micro800, and the whole CompactLogix line (1769, 5370, 5380, 5480) are built this way. A Siemens
engineer will recognise the shape from an S7-1200 or S7-1500 CPU.

How much else is integrated varies within that shape, and the CompactLogix line is not uniform. A
5370, 5380 or 5480 takes its 24V DC on the controller, so controller, power input and Ethernet
are one unit. A 1769 does not. It needs a separate 1769-PA2/PB2 supply on the rail, and the
controller must sit within four modules of it. That is the shape of the
[L32E on the test bench](../test-bench/test-device-setup.md). What the whole line does share is the
absence of a communications bus. No CompactLogix can be given a second Ethernet port by adding a
module, because there is no slot to add one to.

What the shape means for the connection configuration, meaning chassis, slot, backplane, and the
route path that names them, is in [chassis and route paths](chassis-and-route-paths.md).

## The lines

| Line | Form factor | Addressing | Programmed with | Notes |
|------|-------------|------------|-----------------|-------|
| **ControlLogix** 1756 (5550, 5555, 5560, 5570, 5580) | Chassis | Symbolic tags | Studio 5000 Logix Designer | The high-end line. GuardLogix is its safety variant |
| **CompactLogix** (1769, 5370, 5380, 5480) | DIN-rail unit | Symbolic tags | Studio 5000 Logix Designer | Compact GuardLogix is the safety variant. The 5480 adds a Windows 10 IoT compute module |
| **SoftLogix 5800** | PC, virtual chassis | Symbolic tags | Studio 5000 Logix Designer | Software controller presenting a virtual backplane |
| **Micro800** (810-870) | DIN-rail unit | Symbolic tags, restricted | Connected Components Workbench | No program scope, no AOIs, tag listing only from firmware v10 |
| **MicroLogix** (1000, 1100, 1200, 1400, 1500) | DIN-rail unit | PCCC data files | RSLogix 500 / RSLogix Micro | Legacy |
| **SLC 500** | Chassis or fixed | PCCC data files | RSLogix 500 | Legacy |
| **PLC-5** | Chassis | PCCC data files | RSLogix 5 | Legacy |

The addressing column is the split that matters most outside this document. It decides how you reach
a value, which application service carries the request, and which data types exist at all. Each half
has its own data-type reference:
[symbolic tags](../protocol/allen-bradley-extension/symbolic-tag-data-types.md) for the top four
lines, [PCCC data files](../protocol/allen-bradley-extension/pccc-data-file-types.md) for the bottom
three.

The first three lines together are Logix. They share one programming tool, one tag model and one set
of CIP services. Inside Logix the four-digit numbers in the table (5570, 5580, 5370, 5380) carry a
second split, by [generation](logix-generations.md), which decides the type vocabulary.

GuardLogix and Compact GuardLogix are safety variants of their base line rather than separate
families. Standard tags behave identically over CIP, but the safety task's tags cannot be written
from outside. FlexLogix and DriveLogix were 2000s-era Logix variants, long discontinued.

The table also leaves out the oldest file-era processors. The PLC-2 has octal word addresses
(`010/07`) and no data files at all. The PLC-3 and the PLC-5/250 have data files but are rare.

Studio 5000 Logix Emulate belongs on this list for a different reason. It presents a virtual chassis
over CIP and answers symbolic reads and `@tags` listings like real hardware, so it is a route to
integration testing without a physical controller.

## Which service carries the request

| Access mechanism | Logix | Micro800 | MicroLogix | SLC 500 | PLC-5 |
|------------------|-------|----------|------------|---------|-------|
| [CIP symbolic tag services](../protocol/allen-bradley-extension/tag-services.md) (`0x4C` read, `0x4D` write, and their fragmented variants) | native | native | ❌ | ❌ | ❌ |
| [PCCC over EtherNet/IP](../protocol/allen-bradley-extension/pccc.md#the-pccc-tunnel) (PCCC object, class `0x67`, service `0x4B`) | ❌ | ❌ | native | native (5/05) | native (`/xxE`) |
| Reached through a bridge when there is no native Ethernet | n/a | n/a | 1000, 1200, 1500 | 5/01-5/04 | non-`E` models |

Micro800 speaks the same tag services as Logix, but not all of them. It has no Symbol Instance
Addressing and no Multiple Service Packet, so a client that batches requests falls back to one
service per packet. Value encoding is little-endian in CIP and PCCC alike. What changes across the
table is the addressing model and the service, never the byte order.

A Logix controller can answer PCCC as well, when its project maps PLC/SLC messages onto Logix arrays
so that an address such as `N7:0` reaches a tag. That mapping is a compatibility option for old HMIs
and devices, and it does not make the controller file-addressed.

CIP Security (ODVA Vol. 8, see
[the networking overview](../protocol/cip/networking-overview.md#security-considerations)) is
supported on the 5580 and 5380 with recent firmware, and on older Logix retrofitted with a
1756-EN4TR module. PLC-5, SLC 500, MicroLogix and Micro800 do not support it.

## References

### Rockwell publications

- Rockwell Automation, *ControlLogix System User Manual* (1756-UM001), on chassis, slots, and module
  placement:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/um/1756-um001_-en-p.pdf>
- Rockwell Automation, *Logix 5000 Controllers Data Access* (1756-PM020):
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
