# Allen-Bradley

The language of Allen-Bradley programmable controllers and the Common Industrial Protocol they
speak. These are Rockwell's and ODVA's words, not ours: where the two authorities disagree, or
where a word means one thing in the engineering tool and another on the wire, the entry says so.

Two halves: **the tag model** — what a controller holds and how it is named — and **reaching one**
— the hardware a request crosses and the protocol it crosses it on.

## Language

### The controller

**Controller**:
The programmable controller itself — the thing that executes logic and holds tags. Rockwell's word
for every Logix product.
_Avoid_: PLC (generic), CPU (a module, not the machine), processor (Rockwell's word for the
PLC-5 / SLC-500 generation, so it belongs to the Legacy world)

**Logix**:
The controller generation that addresses data by tag — ControlLogix, CompactLogix, GuardLogix,
SoftLogix, and the discontinued FlexLogix and DriveLogix. Rockwell also writes Logix 5000, and uses
it for the whole line rather than any one product.

**Legacy**:
The controller families that store data in data files and are addressed through PCCC: PLC-5, SLC
500 and MicroLogix. Our word, not Rockwell's, and the name of the port that serves them. The PLC-2,
PLC-3 and PLC-5/250 are file-era too, but outside it.
_Avoid_: using it for Micro800, which is symbolic; using it for any controller merely because it is
old

**Micro800**:
The small controller line from 810 to 870, programmed in Connected Components Workbench. It reads
by tag name like Logix but is a separate, simpler platform: browsing and structures do not follow
the Logix model. Neither Logix nor Legacy.

**Controller family**:
Which line a controller belongs to: ControlLogix, CompactLogix, Micro800. It says how the hardware
is shaped and what it supports, not which part number was bought.
_Avoid_: controller type (reads as a catalog number), series (Rockwell's word for a hardware
revision)

**Controller generation**:
How far along the Logix line a controller is — 5X70 and earlier, or 5X80 — which is what says which
data types it has. It cuts across the family: a ControlLogix 5580 and a CompactLogix 5380 are both
5X80.
_Avoid_: series (a hardware revision within one catalog number), firmware revision (finer, and
changes without the controller changing)

**Controller kind**:
A family and a generation together, which is what one device node in the manifest stands for and all a
port ever needs to know about the controller it was configured against. A CompactLogix 5380 and a
CompactLogix 5480 are the same kind.
_Avoid_: controller type (reads as a catalog number), model

**Catalog number**:
The part number of one specific controller — `1756-L71`, `1769-L32E`. Narrower than a family and a
generation together, and never a substitute for either.

**Studio 5000 Logix Designer**:
The engineering tool a Logix project is authored in, and the authority for how tags are declared
and spelled.
_Avoid_: RSLogix 5000 (its name before v21 — still what many engineers say, but the two are the
same tool)

**Project**:
Everything one controller runs, authored offline as a single `.ACD` file and downloaded whole. One
project per controller: a download replaces what was there, so two cannot be combined in a
controller, only merged offline through `.L5X` export and import.
_Avoid_: solution, workspace, application (Micro800's word for its own project, so it belongs to the
Legacy world)

**Task**:
The scheduling unit of a project — continuous, periodic or event — holding an ordered list of the
programs it runs. It holds no tags, and its name is part of no address.
_Avoid_: thread, cycle, OB (the S7 word)

**Program**:
A container of routines with a tag scope of its own, scheduled by exactly one task. A controller runs
many; they do not nest for addressing purposes.
_Avoid_: task (what schedules a program — and the level above it in Studio 5000's tree, which is
where the two get confused)

**Routine**:
One body of executable code inside a program, in ladder, structured text, function block or SFC. It
declares no tags: a program's tags are shared by all of its routines.

### Controller hardware

**Chassis**:
The frame a modular controller's parts mount into, with a fixed number of positions — 4, 7, 10, 13
or 17. A chassis on its own is metal and a bus: nothing in it is a controller by default.
_Avoid_: rack (the PLC-5 word, and the one everyone says colloquially — Rockwell's own literature
says chassis)

**Slot**:
One numbered position in a chassis, counted from zero, left to right. The power supply hangs
outside the numbering; everything else occupies exactly one slot and is addressed by it.

**Module**:
Anything that occupies a slot — a controller, a communication module, an I/O module. One chassis
may hold several controllers, each with its own tags.

**Backplane**:
The passive bus along the back of a chassis. CIP does not treat it as an implementation detail of
the hardware: it is a network like any other, and a slot number is a node address on it.

**Virtual backplane**:
The same model presented by hardware that has no chassis — a CompactLogix on a DIN rail, or a
SoftLogix on a PC — so that routing works unchanged. The controller places itself at slot 0.

### Reaching a controller

**CIP**:
The Common Industrial Protocol, administered by ODVA: an object model and a service set, defined
independently of any wire.

**EtherNet/IP**:
CIP carried over Ethernet. The `IP` is **Industrial Protocol**, not Internet Protocol.

**Encapsulation**:
The wrapper that carries CIP over TCP and UDP — a fixed 24-byte header, of which the Session Handle
is the part every later message echoes.

**Session**:
One EtherNet/IP conversation over TCP 44818, opened with `RegisterSession` and identified by the
Session Handle the target returns. Every request travels inside one. Note that libplctag uses the
word for the pair — a session plus the CIP connection opened over it — so say whose sense is meant
when quoting the library.
_Avoid_: connection — a session is not a CIP connection, and the two are separately established

**CIP connection**:
A reservation of resources at both ends, opened with a `Forward Open` and consuming one of the
controller's finite connection slots. Requests sent over one are _connected_ messaging; requests
sent without one are _unconnected_.
_Avoid_: session, socket

**Forward Open**:
The Connection Manager service that opens a CIP connection, carrying the route path to the device
the connection terminates at.

**Originator** / **Target**:
ODVA's names for the two ends of a CIP conversation. A client is always the originator.
_Avoid_: master / slave, client / server (right idea, wrong vocabulary for CIP)

**Explicit messaging**:
Request and reply against a named object — reads, writes and tag enumeration. This is the whole of
what a data-collection client does.

**Implicit messaging**:
Cyclic I/O exchange over UDP 2222, scheduled by an RPI rather than requested. Not how a client
reads tags.
_Avoid_: using "I/O messaging" for explicit reads

**Message Router**:
The object every explicit request is addressed to, which dispatches it to the object the request
path names.

**Multiple Service Packet**:
One CIP request carrying several services, so many tag reads cross the network in a single
round-trip. Abbreviated MSP.

**Connection endpoint**:
The device an EtherNet/IP session is opened against, identified by its IP address or host name. It
may be the controller itself or a bridge in front of it, which is why it names a connection rather
than a controller. Configured as `ConnectionEndpoint`, with the **TCP port** beside it.
_Avoid_: gateway (libplctag's word), IP address alone

**TCP port**:
Which socket on the connection endpoint the session is opened on — 44818, the port ODVA registered
for EtherNet/IP, unless a NAT rule or a tunnel moved it. A property of its own, `TcpPort`, joined to
the endpoint as `host:port` only where libplctag is handed the pair.
_Avoid_: port alone, which in this domain is a hop's port number

**Route path**:
How a request travels from the device that terminates the session to the controller, expressed as a
sequence of hops. It describes hardware the network cannot see, which is why no IP address implies
it.
_Avoid_: routing path, CIP path, connection path, path alone

**Hop**:
One step of a route path: a **port** and an **address**. `1,0` is a single hop.

**Port** (in a route path):
Which network the message leaves by — `1` is the backplane, `2` is the module's own network port.
Nothing to do with a TCP port; say "TCP port" whenever that is what is meant.

**Bridge**:
A module that forwards CIP from one network to another — a 1756-EN2T between Ethernet and a
backplane, a 1756-DHRIO between a backplane and DH+. Bridging is what makes a route path have more
than one hop.

**PCCC**:
Programmable Controller Communication Commands: the command set file-addressed controllers (PLC-5,
SLC 500, MicroLogix) use to read and write data files, carried over DF1 and DH+ before CIP existed.
Over EtherNet/IP it travels as the payload of the Execute PCCC service (`0x4B`) on the PCCC object
(`0x67`). It is not a CIP service itself; Logix uses the CIP tag services instead. See [the PCCC
tunnel](docs/AllenBradley.Documentation/protocol/allen-bradley-extension/pccc.md#the-pccc-tunnel).

### Tags

**Tag**:
A named area of a controller's memory holding a typed value. In Logix it is the _only_ way to
address data — there is no memory layout to point at, and none survives a download.
_Avoid_: variable, point, register, data point (the host's word — see Boundary below)

**Tag name**:
The identifier a tag is declared with, unique within its scope. Letters, digits and underscores, up
to 40 characters. The controller preserves the case it was declared with but resolves names
case-insensitively.

**Tag address**:
The string that reaches a value — `Count`, `Program:Main.Count`, `Arr[5]`. It is what a data point
resolves to, and what libplctag is handed unless the point has a handle address of its own. Rockwell
has no word for it; PM004 writes such references as `array_name[subscript]` and
`structure_tag.member` and calls the whole thing an operand. When the tag sits in controller scope
with no member or subscript, the address and the tag name are the same string.
_Avoid_: tag name (only the identifier), operand (Rockwell's word, but it names a slot in an
instruction rather than the string in it)

**Tag path**:
The parts a tag address is composed from, held apart: the program that scopes the tag, if any; the
tag's declared name; the members reached through, if any; and the subscript, if the point is one
element of an array. The first two say where the declaration is, the last two where inside it the
value is. What a data point carries, filled in by the tree walk, and what the tag address is rendered
from — the address the point is verified and named by, and the declared tag's address for the symbol
table.
_Avoid_: tag address (the rendered string), symbol path

**Member path**:
The members part of a tag path on its own — `Ramp` then `Target` for `Motor.Ramp.Target` — in the order
they are reached, from the tag inwards. A tag addressed whole has none, the way it has no subscript.
_Avoid_: member name (one member), dotted name (the rendered text)

**Container path**:
The containers the tree walk has passed through on its way down to a data point node, each as the
segment it contributes — a program, a UDT, an array container, a subscript. The walk appends what each
node is; which name is the tag and which are members is decided when the path is read back into a tag
path. Lives in the mapper only: no data point carries one.
_Avoid_: symbolic path (the CIP address segment in the protocol pages, which is not this), tag path (the four
parts, what the data point carries), node path

**Scope**:
Which part of a controller a tag is visible in — controller scope or program scope. Scope is part
of a tag's identity, so two tags may share a name if they sit in different scopes.

**Controller scope**:
The controller's single global namespace. Every program and every external client can reach a tag
in it.
_Avoid_: global scope

**Program scope**:
A namespace private to one program. No other program can reach into it, and there is no aliasing
across programs.
_Avoid_: local scope

**Local tag**:
A program-scope tag that only its own program can reach — the ordinary kind, and everything a
program held before parameters existed.
_Avoid_: private tag, local variable

**Program parameter**:
A program-scope tag declared with a direction — Input, Output, InOut or Public — so that data can be
connected to it from the controller scope or from another program. Available from Logix Designer
v24; it lives in the same namespace as a local tag and is addressed the same way.
_Avoid_: argument; routine parameter and Add-On Instruction parameter (both real Rockwell terms for
other mechanisms, so say which parameter is meant)

**Tag type**:
What a tag _does_, as declared: base, alias, produced or consumed. This is Rockwell's "Type" in the
tag-creation dialog and is **not** the tag's data type — the two are separate declarations.
_Avoid_: saying "type" alone for either this or the data type

**Base tag**:
Rockwell uses this for two things: a tag that stores its own data (the default tag type), and the
tag an alias points at. Say which one you mean.
_Avoid_: using it for the leading segment of an address — that is not a Rockwell sense of the word

**Alias tag**:
A tag that is a second name for another tag, or for one of its members or elements.

**Produced tag** / **Consumed tag**:
A tag one controller publishes for others to read, and the tag in another controller that receives
it. Both must be controller-scoped.

**Tag handle**:
libplctag's `Tag` object, one per PLC tag. _Handle_ is the general programming word for a token that
stands in for a resource somebody else owns: we never look inside it and cannot reach the tag
without it, we hand it back to the library whenever we want a read or a write, and we must dispose
it when we are done. It holds the tag's connection state and its byte buffer, so two operations on
the same handle are two operations on the same buffer — which is why access to one is serialized
(see [Operations, not
accessors](docs/AllenBradley.Logix.Documentation/ADR/2026-07-16-operations-not-accessors-over-libplctag.md)).
One `LogixTagAccess` wraps exactly one handle, so "a shared handle" and "a shared access" name the
same thing from the library's side and ours.
_Avoid_: Session Handle — that is the EtherNet/IP header field above, and unrelated
_Avoid_: saying "tag" for the handle — the tag is the controller's memory, the handle is the
library's object standing in for it

**Handle address**:
The address a data point's tag handle is created for. It is the tag address, except when the point
stands for a structure but reads only one of its members: a timer point has the tag address
`Delay1` and the handle address `Delay1.ACC`, and an element of a timer array has `Delays[3]` and
`Delays[3].ACC`. Verification and every message use the tag address; only libplctag sees the handle
address.
_Avoid_: tag address (what the user configured), member address

### Data types

**Data type**:
What a tag stores — either an atomic type or a structure. Declared per tag and reported by the
controller.

**Symbol table**:
The collection of a controller's symbols as the client holds it: every listed tag by address —
controller tags and program tags together — and every template a structured tag names, by id. Read
once at connect and asked by tag path; the lookup walks the templates for what the path reaches.
Rockwell names the Symbol object but never the set, and Studio 5000 shows it as two collections
(Controller Tags and Program Tags), so this is our name for the one the client keeps.
_Avoid_: tag definitions (its old class name — it holds tag definitions, but it is not one of them),
schema, tag listing (the per-scope `@tags` read the table is built from, not the table), `@tags`
(libplctag's token, no standing here)

**Declared type**:
What the controller declares the value at one address to be: its data type, a string's capacity, its
rank and its element count, at the address a tag path renders to. The symbol table hands one
back for whatever a path reaches — a tag, a member inside it, or one element of an array — and
verification compares a data point against it. A member has no declared type of its own in the
listing; the one it gets is read off its template and carries the tag's address with the member path
behind it.
_Avoid_: metadata (the client's old word for it), tag definition (the lookup's own walk node, below,
which nothing outside the symbol-table lookup sees)

**Tag definition**:
One node of the symbol-table walk: what the controller declares a value to be, and the template to
walk into when the value is a structure. A listed tag and a template member both hold one, so a
member and a tag of the same type read alike. It carries no address, because a member has none of
its own; the walk puts the address on when it hands the node out as a declared type.
_Avoid_: declared type (the addressed shape above, the one the outside sees), listed tag (the
listing's entry — an address and a tag definition — which a member never has)

**Atomic data type**:
A data type holding a single value: `BOOL`, `SINT`, `INT`, `DINT`, `LINT`, `USINT`, `UINT`, `UDINT`,
`ULINT`, `REAL`, `LREAL`.
_Avoid_: elementary (ODVA's word for the same set — permitted only when quoting the CIP
specification), primitive, scalar (see below — a different axis)

**Structure**:
A data type holding several named members. Four origins: predefined (`TIMER`, `COUNTER`, `STRING`),
module-defined, Add-On Instruction, and user-defined.

**User-defined data type (UDT)**:
A structure an engineer declares in the project. The universal shorthand is UDT.

**Add-On Instruction (AOI)**:
A reusable instruction with its own data type. Only its instance tag is reachable from outside; its
locals are not.

**Member**:
A named field of a structure, reached with a dot — `Timer1.PRE`. **A member is not a tag**: it has
no declaration of its own and never appears when a controller enumerates its tags.
_Avoid_: field, property, sub-tag

**Dimension**:
One axis of an array. A tag has at most three.

**Element**:
One entry in an array, reached with brackets — `Arr[5]`. **Not a tag**, for the same reason a member
is not.

**Subscript**:
The bracketed part that selects an element — `[5]`, or `[2,3]` across two dimensions. Rockwell's
word, from 1756-PM004. It is not part of the tag name: `Arr` is the name, `[5]` addresses into it.
_Avoid_: index (fine in prose, but the subscript is what the address carries)

**Scalar**:
A tag with no dimensions. Orthogonal to atomic/structure: a `DINT[10]` has an atomic data type and
is not scalar; a `TIMER` is scalar and is not atomic.

**`STRING`**:
The predefined structure Logix models text with: a `.LEN` member holding the _current_ character
count, and a `.DATA` member of `SINT`, 82 characters by default. A project may declare narrower or
wider string types.

**Maximum characters**:
The character capacity a string type is declared with — the size of its `.DATA`. Distinct from
`.LEN`, which is how many of them are currently in use.
_Avoid_: length, max length (in Logix, "length" is `.LEN`, the current value)

**`TIMER`**:
The predefined structure the `TON`, `TOF` and `RTO` instructions keep their state in: a status word
with the `.EN`, `.TT` and `.DN` bits, then the preset and the accumulated value. A negative preset or
accumulated value is a major fault as soon as an instruction runs the timer, and the controller stops.

**`COUNTER`**:
The predefined structure the `CTU` and `CTD` instructions keep their state in: a status word with the
`.CU`, `.CD`, `.DN`, `.OV` and `.UN` bits, then the preset and the accumulated value. Both may be
negative. A count that passes the end of the `DINT` range wraps and sets `.OV` or `.UN`.

**Preset**:
`.PRE`, the `DINT` a `TIMER` or a `COUNTER` counts towards: milliseconds for a timer, counts for a
counter. A timer's is never negative; a counter's may be.
_Avoid_: setpoint, limit

**Accumulated value**:
`.ACC`, the `DINT` a `TIMER` or a `COUNTER` has counted so far: milliseconds for a timer, counts for a
counter. Rockwell's word for the member in both structures. A timer's is never negative; a counter's
may be.
_Avoid_: current value

**Accumulated time**:
A timer's accumulated value, in milliseconds. The word wherever only a timer is meant, as in the timer
node and its tests; a counter's accumulated value is a count, not a time.
_Avoid_: elapsed time, accumulated time for a counter

### On the wire

**Symbol**:
A tag as CIP represents it — one instance of the Symbol object, class `0x6B`, carrying a Symbol Name
and a Symbol Type. Not a second concept: a symbol _is_ a tag, seen from the protocol.
_Avoid_: the RSLogix 500 sense — a mnemonic for a data-file address such as `N7:0` — which belongs
to the Legacy world and means something else entirely

**Symbol Name**:
Attribute 1 of a Symbol object: the tag's name.

**Symbol Type**:
Attribute 2 of a Symbol object: a 16-bit field saying whether the tag is a structure, its array
rank, and either its atomic type code or the id of its template.

**Template**:
The layout of a structure data type, held by the controller as an instance of the Template object,
class `0x6C`, and referenced by id from a structure's Symbol Type. It is what turns a structure from
a byte count into named members.

**Template id**:
The instance number of a template in the Template object — the low twelve bits of a structure's
Symbol Type, so `0` to `4095`. What a tag definition carries for a structured tag, and the key a
template is held under once read.
_Avoid_: UDT id (libplctag's word, and a predefined `STRING` has one too), type id

**Structure handle**:
The 16-bit value a read reply of a structured tag carries behind the abbreviated-structure marker: a
CRC of the template's type encoding, and Template attribute 1. It says which template a reply's bytes
follow, and it is not unique across differently ordered structures, so it is a check and not a key.
_Avoid_: template handle, tag handle (libplctag's `Tag` object — see above)

**Type code**:
The one-byte CIP code identifying an atomic type on the wire — `0xC4` for `DINT`, `0xCA` for `REAL`.
Structures have no type code; they carry a template id instead.

## Boundary with the host

The dataport translates between this language and `ViciOne.Suite.DataPort`'s. Host words are not ours
to redefine, and they must not be used for Allen-Bradley concepts:

- **Data point** — the host's configured unit of exchange. A data point _resolves to_ a tag; it is
  not a tag.
- **Node** — a position in the configuration tree.
- **Quality** — the host's verdict on a read value.

## Open

Terms we still need and Rockwell does not supply, recorded so nobody invents a third word for them.
None at the moment; the last one, the collection of a controller's symbols, became "Symbol table" above.
