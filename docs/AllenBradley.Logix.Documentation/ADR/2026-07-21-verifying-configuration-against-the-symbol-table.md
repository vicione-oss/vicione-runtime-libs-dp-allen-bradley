# Verifying Configuration Against the Controller Symbol Table

## Context and Problem Statement

This decision covers where the controller's tag metadata comes from and how configured data points are
diffed against it. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The DataPort lifecycle runs a **verification step after connecting**. Each configured data point is
compared against the tag metadata the controller itself reports, meaning data type, dimensions and size,
and every mismatch is reported. A misconfigured tag is caught once, at connect, instead of surfacing as a
silent bad read on every poll. The sibling Siemens S7 addon does exactly this, diffing each configured
data point against the type declaration its symbol table reports. The Logix addon needs the same check,
over CIP.

Two facts frame where that metadata can come from.

libplctag has no metadata API and no connect call. The controller's **symbol table**, its directory of
every tag with name, type and dimensions, is reachable only through pseudo-tags. `@tags` lists the
controller-scoped tags, `Program:<name>.@tags` lists one program's tags, and `@udt/<id>` describes one
user-defined type (UDT, a structure the PLC programmer defines). Reading such a pseudo-tag makes libplctag
browse the controller's Symbol and Template objects and return raw bytes carrying type codes, array
dimensions, element sizes and names, without reading any tag's *value*. That is the value-free metadata
source the [decode ADR](2026-07-16-decoding-tag-bytes-into-typed-values.md) left open for its type-code
check.

The connectivity spike already proves the browse works, but the wrong way. The spike is the throwaway code
in the integration-test project that first proved the protocol against the real controller. It binds to
the typed-mapper API that is being removed upstream
([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)). It proves the protocol and
nothing beyond that, and it cannot be tested without a device.

## Considered Options

- **Option 1: Browse the symbol table once, decode the raw bytes with a pure decoder, diff locally**
- **Option 2: Keep the spike's typed-mapper listing** and build verification on top of it
- **Option 3: Read each configured tag's value and inspect the handle's metadata** after the read

## Decision Outcome

Chosen: **Option 1**, which browses the symbol table once through the existing tag-access seam, decodes
the raw bytes with a pure decoder, and diffs in memory. It reuses the one-member-per-operation boundary
[Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) established. It keeps
the two hard parts, the byte layout and the diff, unit-testable without a device. And it does not depend on the mapper
API being removed upstream.

The browse rides the tag-access seam. A symbol listing is a read of a specially named tag and nothing
more, so it goes through the same seam every data-point read does, and nothing above the libplctag adapter
touches the sealed wrapper type. The listing handles are transient. They are read once and disposed
immediately, under the same explicit-disposal rule that prevents the shutdown crash (see [tag disposal and
shutdown](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)). libplctag has no
load-everything call, so the browse costs one read per scope, the controller plus one per program.

Decoding is a pure function over bytes. The listing is decoded from a span rather than from the wrapper's
typed getters, which is what makes it testable against synthetic buffers and independent of the removed
mapper API. The 16-bit symbol-type field on each entry is unpacked into the three things it holds, a
structure marker, the array rank, and either the atomic type code or the id of the template that defines
the structure's members. What comes out is one tag definition per tag, the controller's own description of
it.

A structure comes out of the listing as a string, because that is all the listing can say. An entry names
the *id* of the template, never its members, so a `STRING_8` and a `TIMER` are indistinguishable here.
Every structure therefore decodes with a capacity read back from the declared element size, and a 12-byte
`TIMER` decodes as a string of capacity 8. That is exactly the accuracy a comparison against a declared
size already had. Telling the two apart needs the template itself (`@udt/<id>`), which arrives with
structured data-point support.

The definitions are a case-insensitive name lookup. Logix tag names are case-insensitive, so the lookup is
too. Program tags are keyed by their qualified name (`Program:Main.Count`), the form a configured tag
address takes. Nested programs are not walked in this cut.

The client owns the one browse, and verification resolves through it. Verification asks the client to pair
each configured data point with what the table reports for its tag. A tag the controller does not have
comes back paired with nothing, because an absent tag is itself something to report. Those pairs are the
very tags the poll will read from rather than a second lookup of them, so the schema that was verified is
the schema the poll uses. The load is idempotent and unconditional, which makes verification double as the
schema warm-up instead of an extra device round trip, and makes resolution correct whatever order it is
called in. The client is also the only owner the framework can construct against, since a client is what
the dataport base hands over when it asks for a verifier.

Verification implements the framework's contract directly. It reports the framework's own misconfiguration
and mismatch types instead of result types of ours behind an adapter. That is the shape the sibling S7
addon has over its client, with nothing in between.

The diff reads the configured data point against the declaration, and it is the only place that comparison
runs. Reads and
writes do not repeat it ([decode ADR](2026-07-16-decoding-tag-bytes-into-typed-values.md)). The metadata
cannot change while the connection lives, so a poll-time gate would re-reach the verdict connect already
reached, and it would report a configuration error as a bad value or a failed write rather than as the
misconfiguration it is. Verification only renders the kind the comparison reports. This is the one thing
not copied from S7, which accumulates independent predicates and can report several mismatches for one
data point. Reporting several at once is reachable here too, but the comparison short-circuits on purpose,
since an atomic type code compared against an array's is meaningless, so no two kinds can currently both
be true. It pays once arrays land and element count becomes independently checkable.

The check's scope tracks the data-point model. The model carries scalars only today, so the diff asks
whether the tag exists, whether it is an array, whether its shape is the one the configured type is stored
in (elementary or structure), and then either its atomic type or its declared string capacity. Element
count and UDT field layout drop into the same diff once the model grows those shapes. The tag definition
already carries the fields for them.

It reports rather than throws, which is the base class's contract. It hands back the failures, and the
base class logs them and aborts the connect. A browse that will not answer stays an exception, because
connect browses before it verifies, and a table that could not be read has already failed the connect.

The metadata the type check in the [decode ADR](2026-07-16-decoding-tag-bytes-into-typed-values.md)
compares against is this browse, decoded from raw bytes, and this step is where that check runs.

### Consequences

Two things follow. The first is an obligation on the client lifecycle that is **not met today**. The
browse disposes each transient handle the moment it has read it, and no polling handle is built until the
first tag is asked for after verification, so the shared session's handle count does drop to zero in
between. At zero handles libplctag tears the session down and has to register it again (see [the shared
session](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)), which costs a second session
registration and Forward Open and gives back part of the warm-up this step is supposed to double as.
Holding one handle across the gap, or building the polling handles before the listing handles are
released, would close it.

The second is that this step is load-bearing for the poll. Reads and writes decode and encode for the type
the data point declares, without asking again what the tag is. A mismatch this step fails to catch is
therefore one nothing downstream reports as a mismatch. The decode reads whatever bytes arrived as the
configured type, and a reply too short to hold one comes back as a bad value for that data point, which is
what a device fault looks like too.

### Enforcement

The unit suites hold each part in place without hardware. There are decoder tests against synthetic
`@tags` buffers, lookup tests for case-insensitive and program-scoped names, and browse tests against a
fake tag-access seam that also assert every transient handle is disposed. Verification is covered per
mismatch kind, plus one test over the framework seam that a misconfigured point comes back with its reason
attached. A device-tier integration test against the CompactLogix L32E (the test controller) asserts the
whole declaration the controller reports for a program `STRING`, which pins the real `@tags` byte layout
the synthetic buffers imitate, and the declared element size above all, since the capacity is read back
from it. That suite is written and has not yet been run against the device.

## Pros and Cons of the Options

### Option 1: Browse the symbol table once, decode with a pure decoder, diff locally (chosen)

#### Pros

The check reads no tag values, and it doubles as the connection warm-up. Reading `@tags` performs the same
session registration and Forward Open (the CIP connection handshake) the polling handles need anyway, so
verification is the warm-up rather than a cost on top of it. The decode, the lookup and the diff are pure
and tested without hardware, the decode against synthetic listing buffers and the diff against hand-built
definitions, which mirrors how the decode ADR tests converters against byte arrays. Because the modelled
type values *are* the CIP type codes, "controller type differs from expected type" is a direct comparison,
and the type table is shared with the decode work rather than duplicated. And it does not depend on the
mapper API being removed upstream.

#### Cons

libplctag has no load-everything call, so the browse is *N* reads, one for the controller plus one per
program. The S7 addon gets its whole symbol tree from a single call of its client library (AGLink). The
`@udt` responses must also be decoded by hand once structures are modelled, and the wrapper's own string
accessor currently hides their string layout. Program browsing goes one level deep in this version, with
no programs nested inside programs.

### Option 2: Keep the spike's typed-mapper listing (rejected)

#### Cons

It binds verification to the sealed wrapper type through the mapper API upstream is removing. That sealed
type cannot be mocked from our assembly, which is the exact trap the
[Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) exists to avoid
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)). The spike listing stays in
the test project as a protocol proof, not as production input.

### Option 3: Read each configured tag's value and inspect the handle's metadata (rejected)

#### Cons

It reads the tag's value, which defeats the point of a value-free check. The wrapper also exposes only
element size and element count on a handle, not the CIP type code and not the UDT layout, so it cannot
detect a wrong data type at all, only a wrong size. That is strictly less information for more I/O.

## More Information

Open questions:

- Nested programs and UDT recursion. This version browses controller tags plus one level of program tags.
  Programs nested inside programs, and the field-level `@udt/<id>` layout, arrive with structured
  data-point support.
- The `@udt` string layout that the wrapper's string accessor currently hides, needed once structures are
  modelled and compared field by field.

References:

- Wire format:
  [Symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)
  (the Logix symbol-type bitfield, the `@tags` entry layout)
- libplctag behaviour:
  [the shared session](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md) ·
  [tag disposal and shutdown](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)
- Related: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
  (whose type-code metadata source this closes)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)
  (removal of the typed-mapper API) ·
  [libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)
  (why the wrapper type cannot be mocked, and the maintainer's advice to wrap it)
- S7 precedent: its configuration verifier over its client, and its symbol load at connect
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
