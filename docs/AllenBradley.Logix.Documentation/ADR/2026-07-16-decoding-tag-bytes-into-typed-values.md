# Decoding Tag Bytes into Typed Values

## Context and Problem Statement

A read gives the raw bytes of a tag. The client must change these bytes into the .NET value of the data point. A write
must do the opposite. The code is in `src/AllenBradley.Logix/Client/TypeConversion/`. This decision was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The conversion must be type-safe in two ways:

1. At compile time, generic type parameters connect the data point, its converter and its .NET value. The converters
   use no `object` casts.
2. At run time, the configured type must agree with the controller. The controller, not the configuration, owns the
   data type of a tag. A data point configured as `DINT` can be a `REAL` on the controller.

The open question is the input of a converter. A converter can use the typed getters of the libplctag.NET wrapper, for
example `GetInt32` and `GetString`. Or it can decode the raw bytes itself. The typed mapper API of the wrapper
(`Tag<M,T>`) is deprecated, and upstream will remove it
([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)). The library moves to the base `Tag` with
raw buffers.

## Considered Options

1. **Raw bytes and a converter registry.** Each converter decodes and encodes raw bytes. A separate check compares the
   configured type with the controller. The S7 dataport has the same registry.
2. **The typed getters of the wrapper.** Each converter reads its value with `GetInt32`, `GetString` or a similar
   method of the handle.
3. **A hybrid.** Raw bytes only where necessary, and typed getters for all other types. The S7 dataport decodes this
   way.

## Decision Outcome

Chosen option: **Option 1, raw bytes and a converter registry**. A converter that decodes bytes is a pure function, so
we can test it against a byte array without a device. It also does not use the wrapper API that upstream will remove.

- There is one converter for each data-point type. It decodes a `ReadOnlySpan<byte>` into the value of the data point.
  It encodes a value into the bytes that the value occupies.
- A registry finds the converter by the concrete type of the data point. The registration takes this key from the type
  parameter of the converter. Thus, a converter registered for the wrong data point is a compile error.
- A data-point type without a converter causes an exception at the first lookup.
- The data point, not the converter, states what the tag must be: data type, shape, string capacity and element count.
- The check against the controller occurs one time, at connect.
  [Configuration verification](2026-07-21-verifying-configuration-against-the-symbol-table.md) compares each data
  point with the declared type in the symbol table. Reads and writes do not repeat the check.
- A converter does not know the width of the tag. It returns only the bytes of the value. The handle has the width
  that the controller declares, and it refuses a payload that is too long.
- The converters are in `Client/`, not in the domain model. They use no libplctag type. Only the libplctag adapter does
  ([Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md)).

### Consequences

- Good: The converters are tested against byte arrays, without hardware.
- Good: The converters do not depend on libplctag. If we replace libplctag, only the adapter changes.
- Bad: We are responsible for each byte layout. A wrong offset causes no error. It gives a wrong value that looks
  correct.
- Bad: The `STRING` layout comes from the libplctag source code, not from a capture of the device. A device test must
  still confirm it ([test-device setup](../../AllenBradley.Documentation/test-bench/test-device-setup.md)).
- Bad: The decode trusts the configured type. If verification misses a mismatch, a read gives a wrong value without an
  error.

## Why Not the Other Options

### Option 2: The typed getters of the wrapper

A typed getter reads from a live handle. Thus, we cannot test a converter against a saved byte array. The typed mapper
API is also deprecated.

### Option 3: A hybrid

The S7 dataport decodes raw bytes only where its client library, S7.Net, has bugs. It uses the typed values of S7.Net
for all other types. Nothing here forces such a split. One raw-byte path for all types is simpler, and all of it is
testable.

## More Information

- Explanation: [Values and their types](../explanation/client/values-and-their-types.md), with the byte layouts in
  detail
- Background: [CIP data types](../../AllenBradley.Documentation/protocol/cip/data-types.md) ·
  [Symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)
- Related decisions: [Verifying configuration against the symbol table](2026-07-21-verifying-configuration-against-the-symbol-table.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406), the removal of the typed mapper
  API
