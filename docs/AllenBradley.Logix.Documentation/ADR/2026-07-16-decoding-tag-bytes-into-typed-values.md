# Decoding Tag Bytes into Typed Values

## Context and Problem Statement

This decision covers how a tag's raw bytes become a data point's .NET value, and how the type a data
point is configured as is held against the type the controller declares. The code sits in the client's
type-conversion folder, `src/AllenBradley.Logix/Client/TypeConversion/`. Its only callers are the read
and write batches and the configuration verifier. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

Batch read and write must be **type-safe** in two senses, and both must hold.

The first is compile time. The pieces that handle one data point (the batch entry, the byte decoder, and
the .NET value it produces) are tied together by generic type parameters rather than by `object` casts
scattered through the code.

The second is run time. A Logix tag is addressed by name, and the controller owns its data type, not our
configuration. A configured data point can simply disagree with the controller. It might be configured as
a 32-bit integer and actually be a float.

The open design choice is what the converters consume. They could read the wrapper's typed getters
(`GetInt32`, `GetString`, …), or they could decode the raw bytes themselves. One fact weighs on that
choice. The wrapper's typed-mapper API (`Tag<M,T>`), is being removed
upstream ([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)), currently only deprecated. The library's
direction is the base `Tag` plus raw buffers, with all marshalling owned by the caller.

## Considered Options

- **Option 1: A converter registry that decodes raw bytes**, guarded by a runtime CIP type-code
  check (the pattern the sibling S7 dataport uses)
- **Option 2: Converters built on the wrapper's typed getters** such as `GetInt32`, `GetString`, …
- **Option 3: A hybrid**, raw bytes only where needed and typed getters elsewhere (how the S7 dataport
  ended up)

## Decision Outcome

Chosen: **Option 1, a converter registry that decodes raw bytes**, guarded by a CIP type-code check. Raw
bytes keep the codecs testable against a byte array and independent of the wrapper surface that is being
removed. The type-code check supplies the run-time half of "type-safe", and it is paid once per connection
rather than once per read.

A converter is one type's codec. It decodes a raw span into the data point's value, and encodes a value
back into a buffer. What the tag is expected to be — its CIP type, its shape, a character capacity or an
element count for the types that have one — is the data point's to state, not the converter's.

- A converter is looked up by the data point's concrete type, and registration derives that key from the
  converter's own type parameter rather than taking it from the caller. Filing a converter under a data
  point it does not handle is therefore a compile error, not a run-time surprise. A lookup miss throws,
  naming the type and pointing at the registry, so a data-point type added without a converter fails
  immediately.
- The registry's boundary is a non-generic interface, so one loop handles a batch of mixed types. A typed
  base class performs the single cast back to the concrete data point, and a failure there can only mean
  the registry routed the wrong converter, which is what its message says. Subclasses work entirely in
  their own data-point and .NET types and never see `object`. Decoding builds the value through the data
  point, so the point owns its value record instead of a converter inventing one. Encoding demands the
  point's own typed value, which is what stops a failed read, carrying no payload, from being written back
  to the controller.
- The expectation is stated as data on the data point, in the same fields the controller's declaration
  reports its side in. One comparison then holds the whole rule for every type at once, shape first, then
  type, then the capacity or the element count the shape configures. A converter states none of it: the
  data type it decodes is the data point's, and a converter that repeated it could only agree or drift.
  The Studio 5000 spelling a data point carries decides nothing. It only names the type in a
  misconfiguration message and in a rejected write.
- Every converter decodes a `ReadOnlySpan<byte>`. CIP transmits scalars little-endian and .NET is
  little-endian too, so a scalar is a direct `BinaryPrimitives` read with no byte swap. The structural
  cases are the Logix `STRING` and packed BOOL arrays. The `STRING` is a structure holding a `DINT` length
  (Logix's 32-bit integer) followed by 82 `SINT` bytes (8-bit integers) and padding. The buffer starts at
  that length, because libplctag strips the CIP abbreviated-structure marker into its own type-info store
  (see Consequences). The length is the controller's claim about its own character data, so the decode
  clamps it against the configured capacity and against the bytes actually in hand. The encode throws
  rather than truncating an over-long value, and it covers the whole of `.DATA` with zeros past the
  characters so a shorter value does not leave the previous one visible in Studio 5000.
- A converter states no tag width. It returns the bytes the value occupies, sized from its type or the
  point's configured capacity, and the padding the controller adds after a structure is not among them.
  A `STRING` and a `STRING_20` are one converter and two widths, and the controller owns which, so the
  bytes go to libplctag's handle as they are: the handle is the controller's width, takes a shorter
  payload from the start, and refuses a longer one before sending.
- The type check runs at connect, not on every read. What the data point says the tag is gets compared
  with the controller's declaration once, by
  [configuration verification](2026-07-21-verifying-configuration-against-the-symbol-table.md), which is
  the comparison's only caller, and a mismatch aborts the connect. Decoding then reads the type the data
  point was configured for. Repeating the comparison per read would re-reach a verdict already reached, on
  metadata that cannot change while the connection lives. It would also let a misconfiguration that
  verification somehow let through look like a device fault instead of the configuration error it is. A
  reply too short for its type is caught narrowly on the read path, and only so that the tag it happened
  to can be named among the batch's failures.
- Conversion lives in `Client/` and **never** in the domain core. The domain declares what a data point
  exchanges, with the .NET type as a type parameter on the data point itself rather than a discriminator
  beside it. The client owns how to produce that value from libplctag's bytes. The converters touch no
  libplctag type at all. Only the tag-access adapter does, and only `Client` may depend on that assembly
  (see Enforcement).

### Consequences

The decision carries the elementary scalars (`DINT`, `REAL`) and one structure, the Logix `STRING`. The
structure work turned on two questions. One is settled and the other is committed to but unconfirmed.

The marshalling mechanism is settled. Explicit `BinaryPrimitives` codecs, not
`StructLayout(Sequential, Pack = 4)` with `Marshal`. That is what keeps a converter a pure function over a
span, testable against a byte array.

The structure buffer's starting offset is taken from libplctag rather than from a capture. `GetBuffer`
returns only the member bytes. The library strips the two-byte `A0 02` abbreviated-structure marker and
the template handle into its own type-info store, and re-attaches them on write. Its documented Logix
string layout says the same thing, a count word of 4 bytes at offset 0, capacity 82, 2 pad bytes, 88
total. The `STRING` codec is built on that, and every `STRING` and UDT offset hangs off it. Confirming it
against the controller is what the wire-format probe and the device-tier round trip exist for, and neither
has been run (see [the test-device setup](../../AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md)).

A second unconfirmed fact lands on this decision from the other side. The symbol-table listing carries no
capacity of its own, so a `STRING`'s is read back from the declared element size by subtracting the 4-byte
length prefix. If the controller declares the padded 88 instead of the 86 member bytes assumed, the codec
and the declaration disagree by two characters, the comparison reports a capacity mismatch, and the
connect aborts before a single `STRING` is polled.

Packed BOOL arrays and UDTs are still unmodelled. A UDT needs its template (`@udt/<id>`) for a field-level
layout. Both drop into this same registry when they arrive.

### Enforcement

The registry's completeness is checked, but not exhaustively. A theory over every modelled data-point type
asserts that each one resolves a converter and that the point and its converter spell the type the same
way. That list is hand-maintained, so a type added without being added to it is a type nothing catches
until the registry throws at run time. A reflection-driven test over every data point would close the gap
and does not exist yet. What the miss itself does is pinned separately, against a data-point shape
declared deliberately without a converter.

Codec regressions fail without hardware. The `STRING` codec is pinned to hand-built byte arrays covering
the length prefix at offset 0, the clamps, the Latin-1 fallback and the zeroed tail, and it is driven
through the non-generic boundary so the routing cast and its message are under test too. The comparison's
ordering is pinned against real converters, which is the one thing the verifier's messages cannot show,
and the verifier itself is covered per mismatch kind. The client tests decode and encode a `DINT` and a
`STRING` through the batches. The `REAL` codec has no test of its own and is reached only through the
type-name theory.

The device tier pins the same layouts against the real controller. It is written but unrun, a round trip
of a program `STRING` through the production stack that also asserts the whole declaration the controller
reports for that tag.

Keeping conversion clear of the domain is a code-review rule. The `libplctag` package reference sits on
the whole project, and an architecture test that fails the build when anything outside `Client` touches it
is worth adding and does not exist yet.

## Pros and Cons of the Options

### Option 1: A converter registry that decodes raw bytes (chosen)

#### Pros

The hard part needs no hardware to test. Every converter decodes a raw byte span, so each one runs against
a byte array laid out the way the controller sends it. That byte-layout knowledge does not depend on
libplctag either. If the client library were ever swapped, the codecs would survive and only the
tag-access adapter would be rewritten (see [Operations, not
accessors](2026-07-16-operations-not-accessors-over-libplctag.md)). It also avoids the upstream removal of
the mapper API completely, because we never touch that API. And the S7 dataport's registry pattern ports over directly,
bringing the exhaustive dictionary and the single boundary cast with it.

#### Cons

Raw bytes are only half of "type-safe" on their own. The check needs a source for the controller's actual
type, and supplying it is a decision of its own. [Verifying configuration against the controller symbol
table](2026-07-21-verifying-configuration-against-the-symbol-table.md) reads that type from the symbol
table at connect and owns the comparison. Without that browse there is nothing for the configured type
to be checked against.

The byte layout also becomes ours to be right about. Nothing in a decode reports a wrong offset. It
reports a plausible wrong value. That is why the `STRING` offsets, taken from libplctag's documented
layout rather than from the wire, still want confirming against the controller. That includes whether the
bytes open with the two-byte `A0 02` marker CIP uses to flag an abbreviated structure, which would shift
every `STRING` and UDT offset. (A UDT is a user-defined type the PLC programmer declares.)

### Option 2: Converters on the wrapper's typed getters (rejected)

#### Cons

It ties the codecs to the API surface that upstream is removing, and it rules out testing against
captured buffers, because a typed getter cannot be pointed at a saved byte dump. The typed getters
are still fine for a quick throwaway experiment. They just cannot be the production converter input.

### Option 3: The hybrid, raw bytes where needed and typed getters elsewhere (rejected)

The sibling S7 dataport decodes raw bytes only for the cases where its client library (S7.Net) lays
the bytes out incorrectly, and uses the library's typed values everywhere else.

#### Cons

That split existed only to work around S7.Net's bugs. Nothing forces a split here, so going all-raw
is simpler. There is one uniform path, and it is uniformly testable.

## More Information

The controller's actual type comes from the symbol-table browse, and the comparison against it runs once
per connect. See [Verifying configuration against the controller symbol
table](2026-07-21-verifying-configuration-against-the-symbol-table.md). The decode does not repeat it.

This record names roles rather than types. The folder is the entry point, and the current names live in
the source and its comments. A rename should not oblige anyone to revisit a decision that has not changed.

- Wire formats:
  [CIP data types reference](../../AllenBradley.Documentation/protocol/cip/cip-datatypes-reference.md)
  (little-endian scalars) ·
  [Symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)
  (the Logix `STRING` structure, BOOL packing, the symbol-type bitfield)
- Related: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)
  (removal of the typed-mapper API)
- S7 precedent: its client's data-item conversion folder and its conversion-architecture ADR
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
