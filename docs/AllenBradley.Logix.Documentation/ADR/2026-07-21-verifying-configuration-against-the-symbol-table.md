# Verifying Configuration Against the Controller Symbol Table

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/Schema/` and
`src/AllenBradley.Logix/Verification/`. It spans the symbol-table browse (`LogixSchemaBrowser`,
`LogixSymbolListingDecoder`, `SymbolType`, `LogixControllerSchema`), the device-metadata records
(`LogixTypeDeclaration`, `LogixResolvedDataPoint`), and the client-owned verifier
(`LogixConfigurationVerifier`). It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The DataPort lifecycle runs a **verification step after connecting**. Each configured data point
is compared against the tag metadata the controller itself reports (data type, dimensions, size),
and every mismatch is reported. A misconfigured tag is then caught once, at connect, instead of
surfacing as a silent bad read on every poll. The sibling Siemens S7 addon does exactly this. Its
`DataPointConfigurationVerifier` diffs each configured data point against the type declaration
the controller's symbol table reports. The Logix addon needs the same check, over CIP.

Two facts frame where that metadata can come from:

- **libplctag has no metadata API and no connect call.** The controller's **symbol table**, its
  directory of every tag with name, type and dimensions, is reachable only through pseudo-tags.
  `@tags` lists the controller-scoped tags, `Program:<name>.@tags` lists one program's tags, and
  `@udt/<id>` describes one user-defined type (UDT, a structure the PLC programmer defines).
  Reading such a pseudo-tag makes libplctag browse the controller's Symbol and Template objects
  and return raw bytes carrying type codes, array dimensions, element sizes and names, without
  reading any tag's *value*. This is exactly the value-free metadata source the
  [decode ADR](2026-07-16-decoding-tag-bytes-into-typed-values.md) left open for its runtime
  type-code check.
- **The connectivity spike already proves the browse works, but the wrong way.** The spike is
  the throwaway code in the integration-test project that first proved the protocol against the
  real controller. Its `PlcTagLister` binds to `Tag<TMapper, TValue>`, the typed-mapper API that
  is being removed upstream
  ([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)). It is a proof of
  protocol, not production code, and it cannot be tested without a device.

One more constraint applies. The DataPort framework pieces this check will eventually plug into
(the DataPort base, the client lifecycle manager, the device nodes) do not exist in this repo yet.
We do not want the verification code to hard-depend on the framework's verification interfaces
(`ViciOne.Suite.DataPort.Extensions.Verification`) before there is anything to wire them into.

## Considered Options

- **Option 1: Browse the symbol table once, decode the raw bytes with a pure decoder, diff
  locally**. The result types are ours now, with an adapter onto the framework's verifier interface later
- **Option 2: Keep the spike's `Tag<TMapper, TValue>` listing** and build verification on top of it
- **Option 3: Read each configured tag's value and inspect the handle's metadata**
  (`ElementSize` / `ElementCount`) after the read

## Decision Outcome

Chosen: **Option 1**, browse the symbol table once through the existing `ILogixTagAccess`
boundary, decode the raw bytes with a pure decoder, and diff locally into result types we own. It
reuses the whole-exchange boundary the
[testable-interface ADR](2026-07-16-testable-libplctag-interface.md) already established. It keeps
the hard parts, the byte layout and the diff, unit-testable without a device. And it depends
neither on the mapper API being removed upstream nor on framework interfaces that have nothing to
plug into yet.

- **The browse rides `ILogixTagAccess`.** A symbol listing is a read of a specially named tag,
  nothing more. `LogixSchemaBrowser` asks the factory for an access to the pseudo-tag
  (`ILogixTagAccessFactory.CreateForSystemTag("@tags")`) and reads it like any other tag. Nothing
  above the `LogixTagAccess` adapter touches the sealed `Tag`. The browse handles are transient.
  They are read once and disposed immediately. The explicit-disposal rule that prevents the shutdown crash
  (see
  [tag-disposal-and-shutdown](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md))
  applies to them too.
- **Decoding is a pure function over bytes.** `LogixSymbolListingDecoder` reads the listing with
  `BinaryPrimitives`. It is the production replacement for the spike's `TagInfoPlcMapper`,
  rewritten to work without the removed mapper API. `SymbolType` unpacks the 16-bit type field
  each listing entry carries. That field holds a structure marker, the number of array dimensions,
  and the atomic type code. The result is one `LogixTypeDeclaration` per tag, the controller's own
  description of that tag.
- **The schema is a case-insensitive name lookup.** `LogixControllerSchema` keys tags by name,
  ignoring case, because Logix tag names are case-insensitive. Program tags are keyed by their
  qualified name (`Program:Main.Count`), the same form a configured `TagName` uses. `Resolve`
  pairs a data point with the controller's declaration for it, or with `null` when the controller
  has no such tag.
- **The verifier is ours, with no framework dependency.** `LogixConfigurationVerifier` diffs the
  configured data points against the schema and returns our own result types
  (`MisconfiguredLogixDataPoint`, `LogixConfigurationMismatch`). It does **not** reference the
  framework's `Extensions.Verification` interfaces. The thin adapter onto the framework's
  `IDataPointConfigurationVerifier<ILogixDataPoint>` comes later, with the work that builds the
  DataPort base and actually triggers verification after connect.
- **The check's scope tracks the data-point model.** The model today carries only scalar
  elementary types, so the diff checks four things. The tag exists, it is not an array, it is not
  a structure, and its atomic type matches. Element count, string size and UDT field layout will
  slot into the same `GetMismatches` method once the model grows those shapes.
  `LogixTypeDeclaration` already carries the fields for them.

This closes the open question the
[decode](2026-07-16-decoding-tag-bytes-into-typed-values.md) and
[testable-interface](2026-07-16-testable-libplctag-interface.md) ADRs left. The metadata source
for the runtime type-code check is the symbol-table browse, decoded from raw bytes.

### Amendment: the verifier projects off the tag manager

`LogixConfigurationVerifier` no longer holds its **own** `ILogixSchemaBrowser`. The tag manager
now owns the one browse of the symbol table (see
[Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)), so the
verifier depends on the manager instead. `VerifyAsync` calls `LoadSchemaAsync`, then per configured
data point takes the unified `ILogixTag` and projects `(tag.DataPoint,
tag.Metadata)` into a `LogixResolvedDataPoint`. `GetMismatches`, the whole verification rule,
is **unchanged**. Only its source moved.

This removes the **double browse** the original design left latent. The verifier browsed once for
the diff and the manager would browse again at connect for the poll. Now the table is read once and
the poll reads from the schema that was verified. The browser becomes an internal detail of
`LoadSchemaAsync`, and because that load is idempotent, running verification *is* the schema
warm-up rather than an extra device round trip.

### Consequences

Two obligations follow for later work. The browse reads and immediately disposes transient handles,
so the client-lifecycle code must order connect carefully. It must not let the shared session's
handle count drop to zero before the polling handles are created, because at zero handles libplctag
tears the session down and has to register it again (see [the shared
session](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)). And because the verifier
deliberately avoids the framework's verification interfaces for now, a thin adapter onto
`IDataPointConfigurationVerifier<ILogixDataPoint>`, plus whatever runs it after connect, must be
added with the work that builds the DataPort base.

### Enforcement

The unit suites hold each part in place without hardware. There are decoder tests against synthetic
`@tags` buffers, schema tests for case-insensitive and program-scoped lookup, verifier tests
covering every mismatch class, and browser tests against a fake `ILogixTagAccessFactory` that also
assert every transient access is disposed. A device-tier integration test against the CompactLogix L32E
(the test controller) pins the real `@tags` byte layout the synthetic buffers imitate.

## Pros and Cons of the Options

### Option 1: Browse the symbol table once, decode with a pure decoder, diff locally (chosen)

#### Pros

The check reads no tag values, and it doubles as the connection warm-up. Reading `@tags` performs
the same session registration and Forward Open (the CIP connection handshake) the polling handles
need anyway, so verification *is* the warm-up rather than an extra cost on top of it. The decoder,
the schema and the verifier are pure and tested without hardware. The decoder runs against synthetic
listing buffers and the verifier against hand-built schemas, which mirrors how the decode ADR tests
converters against captured buffers. Because `CipType`'s enum values *are* the CIP type codes,
"controller type differs from expected type" is a direct enum comparison, and the type table is
shared with the decode work rather than duplicated. And it depends neither on the mapper API being
removed upstream nor, for now, on the framework's verification interfaces, so it builds and tests
even though the DataPort base does not exist here yet.

#### Cons

libplctag has no load-everything call, so the browse is *N* reads, one for the controller plus one
per program, where the S7 addon gets its whole symbol tree from a single call of its client library
(AGLink). The `@udt` responses, whose string layout the wrapper's `Tag.GetString` currently hides,
must also be decoded by hand once structures are modelled. Program browsing goes only one level deep
in this version, with no programs nested inside programs, and the result types we own need the later
adapter before the framework's verifier can consume them.

### Option 2: Keep the spike's `Tag<TMapper, TValue>` listing (rejected)

#### Cons

It binds verification to the sealed `Tag` class through the mapper API upstream is removing. A sealed
`Tag` cannot be mocked from our assembly, which is the exact trap the [testable-interface
ADR](2026-07-16-testable-libplctag-interface.md) exists to avoid
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)). The spike listing
stays in the test project as a protocol proof, not as production input.

### Option 3: Read each configured tag's value and inspect the handle's metadata (rejected)

#### Cons

It reads the tag's value, which defeats the point of a value-free check. The wrapper also exposes
only `ElementSize` and `ElementCount` on a handle, not the CIP type code and not the UDT layout, so
it cannot detect a wrong data type at all, only a wrong size. That is strictly less information for
more I/O.

## More Information

Open questions:

- **Nested programs and UDT recursion.** This version browses controller tags plus one level of
  program tags. Programs nested inside programs, and the field-level `@udt/<id>` layout, arrive
  with structured data-point support.
- **The `@udt` string layout** that the wrapper's `Tag.GetString` currently hides, needed once
  structures are modelled and compared field by field.
References:

- Wire format:
  [CIP data types reference](../../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md)
  (the Logix symbol-type bitfield, the `@tags` entry layout)
- libplctag behaviour:
  [the shared session](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md) ·
  [tag disposal and shutdown](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)
- Related: [A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
  (whose type-code metadata source this closes)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)
  (removal of the typed-mapper API) ·
  [libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)
  (why `Tag` cannot be mocked, and the maintainer's advice to wrap it)
- S7 precedent: `Siemens.S7.Symbolic/Verification/DataPointConfigurationVerifier.cs` and
  `Siemens.S7.Symbolic/Client/SymbolicAccessManager.cs` (`LoadSymbolsFromDevice`)
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
