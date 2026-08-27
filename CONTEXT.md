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
SoftLogix. Rockwell also writes Logix 5000, and uses it for the whole line rather than any one
product.

**Controller family**:
Which line a controller belongs to: ControlLogix, CompactLogix, Micro800. It says how the hardware
is shaped and what it supports, not which part number was bought.
_Avoid_: controller type (reads as a catalog number), series (Rockwell's word for a hardware
revision)

**Catalog number**:
The part number of one specific controller — `1756-L71`, `1769-L32E`. Narrower than a family and
never a substitute for it.

**Studio 5000 Logix Designer**:
The engineering tool a Logix project is authored in, and the authority for how tags are declared
and spelled.
_Avoid_: RSLogix 5000 (its name before v21 — still what many engineers say, but the two are the
same tool)

**Program**:
A container of routines with a tag scope of its own. A controller runs many; they do not nest for
addressing purposes.

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
The older Allen-Bradley protocol that file-addressed controllers speak, tunnelled inside CIP to
reach them. The Legacy world's protocol, named here so it is not mistaken for a CIP service.

### Tags

**Tag**:
A named area of a controller's memory holding a typed value. In Logix it is the _only_ way to
address data — there is no memory layout to point at, and none survives a download.
_Avoid_: variable, point, register, data point (the host's word — see Boundary below)

**Tag name**:
The identifier a tag is declared with, unique within its scope. Letters, digits and underscores, up
to 40 characters. The controller preserves the case it was declared with but resolves names
case-insensitively.

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

### Data types

**Data type**:
What a tag stores — either an atomic type or a structure. Declared per tag and reported by the
controller.

**Atomic data type**:
A data type holding a single value: `BOOL`, `SINT`, `INT`, `DINT`, `LINT`, `REAL`, `LREAL`.
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

**Type code**:
The one-byte CIP code identifying an atomic type on the wire — `0xC4` for `DINT`, `0xCA` for `REAL`.
Structures have no type code; they carry a template id instead.

## Boundary with the host

The addon translates between this language and `ViciOne.Suite.DataPort`'s. Host words are not ours
to redefine, and they must not be used for Allen-Bradley concepts:

- **Data point** — the host's configured unit of exchange. A data point _resolves to_ a tag; it is
  not a tag.
- **Node** — a position in the configuration tree.
- **Quality** — the host's verdict on a read value.

## Open

Terms we still need and Rockwell does not supply, recorded so nobody invents a third word for them:

- **The string that reaches a value.** `Motor.Speed` and `Arr[5]` are neither tag names nor tags.
  CIP encodes them as a chain of symbolic, member and element segments; Rockwell has no crisp
  project-level word. Candidates: tag address, tag path.
- **The collection of a controller's symbols.** Rockwell names the Symbol object and enumerates its
  instances per scope, but never names the set. Studio 5000 has two named collections — Controller
  Tags and Program Tags — not one. Candidates: symbol table, tag listing.
  Note `@tags` is libplctag's own token and has no standing here.
- **The device whose IP address identifies a controller.** It may be the controller or a bridge in
  front of it. Candidates: gateway (libplctag's word), EtherNet/IP interface.
