# Chassis, Slot, Backplane, and the Route Path

What sits behind an Allen-Bradley controller's Ethernet port, and why that produces the `1,0` you
type into a client configuration. **This document is client-agnostic.** It describes Rockwell
hardware and the CIP route path on the wire, not how any specific library expresses them.

The route path is the one piece of connection configuration that cannot be guessed from an IP
address, because it describes hardware the network cannot see. To get it right you have to know what
sits behind the Ethernet port. Which controllers are chassis-based and which are DIN-rail units is
in [controller families](controller-families.md).

## Chassis, slot, and backplane

Three terms, all from the 1756 ControlLogix form factor.

A **chassis** is the metal frame that mounts in the cabinet, colloquially a *rack*, though every
Rockwell publication says chassis and so does this repo. It comes in fixed sizes of 4, 7, 10, 13, or
17 positions. A **slot** is one numbered position in it, counted from zero, left to right. The
**backplane** is the passive circuit board along the back of the chassis, carrying power rails plus a
parallel bus that every slot taps into.

```text
┌────┬─────┬──────┬─────┬─────┬─────┐
│ PS │  0  │  1   │  2  │  3  │  4  │   ← slot numbers
│    │ CPU │ EN2T │ DI  │ DO  │ --- │   ← modules
└────┴─────┴──────┴─────┴─────┴─────┘
     └────── backplane bus ──────────┘
```

The power supply hangs on the left and takes no slot number. Everything else occupies exactly one
slot and is addressed by it. Nothing in the chassis is a controller by default. An empty 1756 chassis
is metal and a bus, with no CPU, no power, and no firmware.

That also means one chassis can hold several controllers, each in its own slot with its own tags,
reachable through whichever Ethernet module happens to be in the chassis with them.

A CompactLogix has none of this. Its modules clip together on a DIN rail with no chassis and no slot
numbering for the controller, so the firmware presents a **virtual backplane** with itself at slot 0.
The routing model below then applies unchanged.

## The backplane is a CIP network

CIP does not treat the backplane as an implementation detail of a chassis. It treats it as a network
like any other, where modules are nodes and the slot number is a node address. That is the whole
trick behind the route path.

A hop is a pair of numbers, **port and address**. The port number identifies which network the
message leaves by:

| Port | Network |
|------|---------|
| `1`  | The backplane |
| `2`  | The module's own network port (Ethernet on an ENxT, ControlNet on a CNB, DH+ on a DHRIO) |

The address that follows is a slot number when the hop crosses a backplane, or an IP address when it
leaves over an Ethernet port.

So `1,0` reads as "leave by port 1 onto the backplane, to node 0", meaning the controller in slot 0.
On the wire this is a port segment, `01 00`. The
[encoding is in the networking overview](../protocol/cip/networking-overview.md#connection-manager-forward-open-and-route-paths).

The gateway address and the route path answer two different questions. **The gateway picks the module
that terminates the EtherNet/IP session**, and the path says how that module forwards the request
onward. The module's own slot never appears in the path, because you already selected it by IP. A
1756 chassis with an EN2T in slot 0 and the controller in slot 3 is `1,3`, not `1,0,1,3`.

## Conventional paths per family

| Family | Conventional path | Why |
|--------|-------------------|-----|
| CompactLogix (1769, 5370, 5380, 5480) | `1,0` | Ethernet is on the controller, whose virtual backplane position is 0 |
| ControlLogix 1756, via an ENxT module | `1,<cpu slot>` | The slot is chosen by whoever built the chassis |
| ControlLogix 5580, via its onboard port | `1,<cpu slot>` | Still routes across the backplane, back to itself |
| SoftLogix 5800 | `1,<virtual slot>` | Virtual chassis. The controller is usually placed at slot 0 |
| Micro800 | *(none)* | No backplane, and no route path in the configuration at all |

Slot 0 is a convention on ControlLogix, not a rule, and it breaks in exactly the places you would
expect: redundant controller pairs, multi-controller chassis, and retrofits where slot 0 was already
occupied. Treat `1,0` as a default to pre-fill, never as a constant to hard-code.

## Multi-hop routes

Because every hop has the same shape, the chain has no fixed length. A request can cross a backplane,
leave over Ethernet, and cross another backplane before it reaches a controller:

```text
1,3,2,192.168.1.20,1,0
│ │ │ │             └─┴ backplane → slot 0 (the controller)
│ │ └─┴ out the Ethernet port of that module → 192.168.1.20
└─┴ backplane → slot 3 (a second Ethernet module)
```

Each device consumes the hop that names it and forwards the shortened remainder. This is what a
Siemens rack/slot pair generalises to. S7 pins two numbers because the connection reaches one CPU in
one rack, whereas a CIP request may cross ControlNet or DH+ segments on its way. (Rack and CPU are
Siemens' words. On this side of the fence they are chassis and controller.)

## Reaching a legacy controller through a bridge

PLC-5, SLC 5/03 and 5/04, and the older MicroLogix families have no native EtherNet/IP interface.
They are reached through a ControlLogix gateway (an ENxT plus a 1756-DHRIO or 1756-CNB) or a
1761-NET-ENI serial converter, and the route path hops through the bridge to the target node. Over
DH+, the last hop names a channel and a node rather than a slot.

Once the request arrives, the addressing is PCCC data files rather than tags. That layer is covered
in [the PCCC tunnel](../protocol/allen-bradley-extension/pccc.md#the-pccc-tunnel).

## References

### Rockwell publications

- Rockwell Automation, *ControlLogix System User Manual* (1756-UM001), on chassis, slots, and module
  placement:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/um/1756-um001_-en-p.pdf>
- Rockwell Automation, *EtherNet/IP Network Devices User Manual* (ENET-UM006), on bridging and
  routing through communication modules:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/um/enet-um006_-en-p.pdf>

### Reference implementations

- libplctag, on the `path` attribute and its per-PLC-type conventions:
  <https://github.com/libplctag/libplctag/wiki/API#tag-string-attributes>
- pycomm3, *Getting Started*, on CIP paths and slot numbers:
  <https://pycomm3.readthedocs.io/en/latest/getting_started.html>
