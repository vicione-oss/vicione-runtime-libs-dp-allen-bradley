# Decoding Tag Bytes into Typed Values

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/TypeConversion/`. That directory holds
`IDataPointConverter`, `DataPointConverter<,>`, `DataPointConverterRegistry`, and the per-type
converters. It was made
under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

Batch read and write must be **type-safe** in two senses, and both must hold:

- **At compile time.** The pieces that handle one data point (the batch entry, the byte decoder,
  and the .NET value it produces) are tied together by generic type parameters, not by `object`
  casts scattered through the code.
- **At run time.** A Logix tag is addressed by name, and the controller, not our configuration,
  owns its data type. A configured data point can simply disagree with the controller. It might be
  configured as a 32-bit integer but actually be a float. (The sibling Siemens S7 addon does not
  have this problem, because its absolute addresses fix the type in the address itself.) So the
  controller's actual type must be checked before any bytes are interpreted.

The open design choice is what the converters consume. They could read the wrapper's typed getters
(`GetInt32`, `GetString`, …), or they could decode the raw bytes themselves. One fact weighs on
that choice. The wrapper's typed-mapper API (`Tag<M,T>`), which the connectivity spike used, is
being removed upstream
([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)). The library's
direction is the base `Tag` plus raw buffers, with all marshalling owned by the caller.

## Considered Options

- **Option 1: A converter registry that decodes raw bytes**, guarded by a runtime CIP type-code
  check (the pattern the sibling S7 addon uses)
- **Option 2: Converters built on the wrapper's typed getters** such as `GetInt32`, `GetString`, …
- **Option 3: A hybrid**, raw bytes only where needed and typed getters elsewhere (how the S7 addon
  ended up)

## Decision Outcome

Chosen: **Option 1: a converter registry that decodes raw bytes**, guarded by a CIP type-code
check. Raw bytes keep the codecs testable against captured buffers and independent of the wrapper
surface that is being removed. The type-code check supplies the run-time half of "type-safe".

```csharp
internal interface IDataPointConverter
{
    ushort CipTypeCode { get; }                                    // expected controller type
    int ByteSize(ILogixDataPoint dp);
    ILogixDataPointValue Decode(ILogixDataPoint dp, ReadOnlySpan<byte> buffer);
    void Encode(ILogixDataPoint dp, object value, Span<byte> buffer);
}
```

- The registry boundary is the non-generic `IDataPointConverter`. A typed base class
  `DataPointConverter<TDataPoint, TDomain>` performs the one unavoidable cast from `object` in a
  single place. The registry is a `FrozenDictionary` keyed by data-point type, and a test fails
  the build if any data-point type has no converter.
- **Every converter decodes a `ReadOnlySpan<byte>`.** CIP transmits scalars little-endian, and
  .NET is little-endian too, so a scalar is a direct `BinaryPrimitives` read with no byte swap.
  The structural cases are the Logix `STRING` and packed BOOL arrays. The `STRING` is a structure
  holding a `DINT` length (Logix's 32-bit integer) followed by 82 `SINT` bytes (8-bit integers)
  and padding. Their exact layouts are confirmed by capturing buffers from the real controller.
- **Before any decode**, the converter's expected CIP type code and byte size are compared with
  the controller's actual type. A mismatch fails the read with a clear error instead of silently
  misreading bytes. This is the run-time half of "type-safe".
- Conversion lives in `Client/`, **never** in the domain core. The domain declares *what* a data
  point exchanges (`ITypedDataPoint<TDomain>` fixes the .NET type). The client owns *how* to
  produce that value from libplctag's bytes. An architecture test enforces that only `Client`
  depends on the libplctag assembly.

### Consequences

The decision supports scalar elementary types today. Before structures can be modelled, two things
must come out of the buffer-capture work against real DINT, REAL, STRING and BOOL buffers. The first
is the exact layout `GetBuffer` returns for a structure tag, with or without the two-byte `A0 02`
marker, since that decides every STRING and UDT offset. The second is the marshalling mechanism,
either `StructLayout(Sequential, Pack = 4)` with `Marshal` or explicit `BinaryPrimitives` codecs.

### Enforcement

Three standing checks hold this decision in place. The completeness test fails the build when a
data-point type lacks a converter. Each converter's unit tests pin its codec to captured buffers,
so a layout regression fails without hardware. The architecture test fails the build if anything
outside `Client` references the libplctag assembly.

## Pros and Cons of the Options

### Option 1: A converter registry that decodes raw bytes (chosen)

#### Pros

The hard part needs no hardware to test. Every converter decodes a raw byte span, so each one runs
against captured buffers, which are byte dumps taken once from the real controller. That byte-layout
knowledge does not depend on libplctag either. If the client library were ever swapped, the codecs
would survive and only the `ILogixTagAccess` adapter would be rewritten (see [A testable interface
over libplctag](2026-07-16-testable-libplctag-interface.md)). It also avoids the upstream removal of
the mapper API completely, because we never touch that API. And the S7 addon's registry pattern
ports over directly, bringing the exhaustive dictionary, the single boundary cast, and the
completeness test with it.

#### Cons

The type-code check needs a source for the controller's actual type. This ADR fixed where the check
sits, before any decode, but left the metadata source open and stood a byte-size guard in for now.
That gap has since been closed by [Verifying configuration against the controller symbol
table](2026-07-21-verifying-configuration-against-the-symbol-table.md), which reads the type from
the symbol table. One thing is still unconfirmed. The bytes `GetBuffer` returns for a structure tag
might open with the two-byte `A0 02` marker that CIP uses to flag an abbreviated structure, plus the
template id, or they might carry only the member bytes. Which one it is decides every STRING and UDT
offset, where a UDT is a user-defined type that the PLC programmer defines. Confirming it is an
output of the buffer-capture work.

### Option 2: Converters on the wrapper's typed getters (rejected)

#### Cons

It ties the codecs to the API surface that upstream is removing, and it rules out testing against
captured buffers, because a typed getter cannot be pointed at a saved byte dump. The typed getters
are still fine for a quick throwaway experiment. They just cannot be the production converter input.

### Option 3: The hybrid: raw bytes where needed, typed getters elsewhere (rejected)

The sibling S7 addon decodes raw bytes only for the cases where its client library (S7.Net) lays
the bytes out incorrectly, and uses the library's typed values everywhere else.

#### Cons

That split existed only to work around S7.Net's bugs. Nothing forces a split here, so going all-raw
is simpler. There is one uniform path, and it is uniformly testable.

## More Information

The runtime type-code check needed a source for the controller's actual type. That was left open
here and has since been settled by [Verifying configuration against the controller symbol
table](2026-07-21-verifying-configuration-against-the-symbol-table.md), which reads it from the
symbol table.

- Wire formats:
  [CIP data types reference](../../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md)
  (little-endian scalars, the Logix `STRING` structure, BOOL packing, the symbol-type bitfield)
- Related: [A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)
  (removal of the typed-mapper API)
- S7 precedent: `Siemens.S7.Absolute/Client/DataItems/Conversion/` and S7's
  conversion-architecture ADR
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
