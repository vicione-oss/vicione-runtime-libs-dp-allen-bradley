# Verifying Configuration Against the Controller Symbol Table

## Context and Problem Statement

The framework runs a verification step after connect. In this step, the port compares each configured data point with
what the controller declares for its tag: data type, shape and size. The step reports each mismatch. Thus, a wrong
configuration is found one time, at connect, and not as a bad read on each poll. The S7 dataport does the same with its
symbol table. This decision was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The question is where the client gets the declared types of the controller. These facts limit the answer:

- libplctag has no API for tag information and no connect call.
- The symbol table of the controller is available only through pseudo-tags. `@tags` lists the controller tags.
  `Program:<name>.@tags` lists the tags of one program. `@udt/<id>` gives one template, which is the layout of a
  structure.
- A read of a pseudo-tag returns raw bytes: names, type codes, array dimensions and template ids. It reads no tag
  values.
- The wrapper has a typed mapper that can list tags. Upstream will remove it
  ([libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)).

## Considered Options

1. **Browse the symbol table one time at connect.** Decode the raw bytes with pure functions, and compare in memory.
2. **Read the value of each configured tag,** and look at the attributes of the handle after the read.

## Decision Outcome

Chosen option: **Option 1**. It reads no tag values. The decoders and the comparison are testable without a device. It
does not use the API that upstream will remove.

- Connect loads the symbol table. It reads `@tags`, then the `@tags` of each program, then each template that a tag
  names, nested templates included.
- Each read goes through our access interface
  ([Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md)). Each read has its own handle,
  and the client disposes it immediately after the read.
- Pure decoders change the bytes into the symbol table. The lookup ignores the case of names, because the controller
  does too.
- If the load fails, the connect fails.
- The verifier implements the verifier interface of the framework and reports the mismatch types of the framework. The
  framework logs them and stops the connect.
- The verifier gets the declared type from the same tag objects that the polls use. There is no second lookup that
  could give a different answer.
- One function compares a data point with its declared type. It compares the shape first, then the data type, then the
  string capacity or the element count. It stops at the first mismatch, because a type comparison has no meaning when
  the shape is different.
- Reads and writes do not compare again. The type of a tag changes only with a download to the controller.

### Consequences

- Good: The load of the symbol table also shows that the controller is reachable and speaks CIP. Connect has no other
  check.
- Good: The decoders and the comparison are tested against synthetic buffers, without hardware.
- Bad: The load needs many reads: one for the controller tags, one for each program and one for each template. The S7
  dataport gets its full symbol tree with one call.
- Bad: Reads and writes trust the verification. A mismatch that the verification does not see gives a wrong value, or
  a tag failure that looks like a device fault.
- Bad: A download that changes the type of a tag while the port runs is such a mismatch. The port finds it only at the
  next connect.
- Open: Programs inside programs are not browsed.

## Why Not the Other Options

### Option 2: Read each tag and look at the handle

It reads tag values, but the check must not need them. The handle shows only the element size and the element count.
It does not show the type code, so it cannot find a wrong data type.

## More Information

- Explanation: [Verification](../explanation/client/verification.md) ·
  [The symbol table](../explanation/client/symbol-table.md)
- Background: [Symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md) ·
  [Tag browsing](../../AllenBradley.Documentation/protocol/allen-bradley-extension/tag-browsing.md) ·
  [Reading a UDT definition](../../AllenBradley.Documentation/libplctag/reading-a-udt-definition.md)
- Related decisions: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
- Upstream: [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406), the removal of the typed mapper
  API
