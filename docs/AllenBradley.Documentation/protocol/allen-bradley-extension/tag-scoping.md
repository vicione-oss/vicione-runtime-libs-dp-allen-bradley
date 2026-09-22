# Tag Scoping — Controller Scope and Program Scope

Where a Logix tag lives, what that adds to its address, and which tags have no choice about it.
**Client-agnostic** — this describes the controller and the wire, not how any specific library or
port handles them.

Everything here is a **Rockwell extension** of CIP, specified in *Logix 5000 Controllers Data Access*
(1756-PM020), not by ODVA. What the types of those tags are, and the Symbol and Template objects that
list and describe them, is in [Symbolic Tag Data Types](symbolic-tag-data-types.md), which this
document does not repeat.

**Who has scopes:** ControlLogix, CompactLogix, GuardLogix and SoftLogix. Micro800 has no program
scope at all — see [§3 of the type document](symbolic-tag-data-types.md#3-what-micro800-exposes).

---

## 1. A name is the address

A Logix controller has no address space to point at. There is no `DB100.DBW20` equivalent, because
the memory layout is not exposed on the wire and does not survive a download. A tag *name* is the
address, and the controller resolves it against its own symbol table on every request.

Names are not one flat namespace, though. Every tag lives in one of two scopes, and only one of them
contributes a segment to the address.

## 2. The two scopes

**Controller scope** is a single global namespace. A tag in it is addressed by its bare name:
`Count`, `Motor_Speed`, `Tank.Level`. Every routine in every program can reference it, and so can
every external client.

**Program scope** is per-program, and it prefixes the address with the program that owns it:

```text
Program:MainProgram.Count
```

That is the entire hierarchy on the wire. Programs do not nest for addressing purposes, and routines
have no tags of their own. Add-On Instruction locals are not reachable from outside at all — only the
AOI's instance tag is, and that tag sits in one of the two scopes like anything else.

Within the controller, program scope really is private: no other program can reference a program tag,
and there is no import and no cross-program alias. It is not a boundary on the wire, however. An
external CIP client reads and writes program tags exactly as it does controller tags, given the
prefix.

### Shadowing

When a program tag and a controller tag share a name, the program tag wins inside that program. The
shadowing is silent, and it catches people who read `Count` in ladder logic and assume it is the
global they configured.

## 3. Some tags cannot be program-scoped

The firmware requires controller scope for:

- **Produced and consumed tags** — the controller-to-controller sharing mechanism
- **Module tags** — the I/O structures Studio 5000 creates when you add a module, and anything
  aliased to one
- **Motion axis and motion group tags**

In practice this is why most of what an external client wants is controller-scoped already.

On GuardLogix, safety tags are controller-scoped as well, and a standard external client can read but
never write them.

## 4. Each scope is listed separately

Scope is not only an addressing prefix; it splits the tag listing too. The Symbol object holds one
instance per controller-scoped tag, and a program's tags are reached by prefixing the request path
with the symbolic segment `Program:<name>`. The programs themselves are discovered from the
controller listing: its entries whose name begins `Program:` are programs, not tags. So a full
listing is one pass for controller scope and one further pass per program. The services, the
attributes and the loop that does this are in
[§9 of the type document](symbolic-tag-data-types.md#9-discovering-what-a-controller-has).

Neither listing descends into a structure. A member such as `Program:MainProgram.Counter.PRE` is
readable and appears in no listing; a client reaches it through the tag's template instead.

> **Not confirmed.** Studio 5000 v32 and later allow a program to be nested inside another program.
> Whether the resulting tags address as `Program:Parent.Child.Tag` has not been confirmed against
> hardware.
