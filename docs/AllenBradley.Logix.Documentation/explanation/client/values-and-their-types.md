# About values and their types

A value in this port takes its type from the data point it belongs to, and the converters that build it know nothing
about the tag on the controller. This page says why.

## Where types come from

The framework exchanges untyped values with the engine. The outgoing port receives an `object?`, and what the incoming
port publishes is a value the framework inspects through its own interfaces. The framework also runs its range
validation through the value. Both are documented with the package. What this port decides is where a value becomes
typed between the engine and the wire, and who gets to say what that type is.

On the wire, CIP fixes every elementary type to a width and a byte order
([CIP data types reference](../../../AllenBradley.Documentation/protocol/cip/cip-datatypes-reference.md)), and Logix
adds the structures that matter here, `STRING` most of all
([symbolic tag data types](../../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)).
The handle gives the client the payload in the controller's own layout, packing and padding included
([what the tag buffer holds](../../../AllenBradley.Documentation/libPlcTag/what-the-tag-buffer-holds.md)).

## A value is made only by its data point

A data point is generic in the .NET type it exchanges, and it is the only thing that can make a value of that type. The
value record is nested inside the point, one for the scalar shape and one shared by every array shape, so a payload and
its point cannot disagree about their type
([decoding tag bytes into typed values](../../ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md)). A general value
class with a type field would let a caller build a `REAL` value against a `DINT` point, and nothing would notice until
the bytes were on the wire.

There is no valueless value and no quality flag beside the payload. A read that produced nothing leaves its point out
of the batch ([reading and writing](reading-and-writing.md)). Inventing a default and passing it off as the tag's
contents was never an option, because zero and the empty string are things a tag really does hold. The quality flag
failed for a different reason. Every value the port publishes is one that was read, so the flag would always say good,
and a flag that always says good is a flag nobody checks.

Going the other way, an untyped engine value enters through a single door on the data point, which either wraps it or
returns a failure that names the tag and the type it wanted. Returned rather than thrown, so the framework can log it
and drop the batch. The match is on the exact runtime type, not on assignability, and that is a deliberate defence
against the CLR. The CLR treats an array of unsigned 16-bit integers as assignable to an array of signed ones, so a
pattern match alone would let a value of 40000 through and write it to the controller as -25536.

Range is the second gate. It lives on the value because the configuration it is judged against already sits there. Only
the configured shapes have anything to say, a string longer than its declared capacity or an array of the wrong length.
The elementary scalars are exactly their .NET types and are always in range.

## A converter knows nothing about the tag

A converter turns bytes into a typed value and back. There is one per data-point type, found through a registry keyed
by the point's concrete type, and it states nothing about the tag it expects. The data type, the shape, a string's
capacity and an array's element count all belong to the data point, and [verification](verification.md) reads them
there once, at connect.

That was not the first design. An earlier version had each converter declare the type it required and compared it
against the controller's declaration on every read and every write. We moved the comparison to connect time after it
became clear that a tag's type only changes with a download. Checking it on every operation meant paying every poll for
something that happens once a month. What is left at read time is cheap: a buffer whose length does not fit the
configured shape is that tag's failure.

The converters split three ways, because the bytes are laid out three ways:

- An array of an elementary type is contiguous, unpadded elements, transferred whole. Its converter takes the element
  format from the scalar converter of the same type, so the two cannot disagree about how wide a `DINT` is.
- A `STRING` is a length header in front of its characters, and the whole structure is padded to a boundary. Characters
  are Latin-1, one byte each, which matches the sibling S7 dataport, so a value written through one port reads back the
  same through the other. The length the controller reports is a claim rather than a fact, and it is clamped to what
  the reply and the declared capacity allow. A write fills the whole character area, zeroes included, because the
  controller keeps whatever sits past the length and Studio 5000 would otherwise show the tail of the previous value.
- A `BOOL` array is bits packed into 32-bit words, so element *i* is a bit of a word and not a slice of bytes.

No converter says how wide a tag is. It returns the bytes the value occupies, and nothing in the client knows about the
padding the controller adds behind them. The `libplctag` handle does know. It is the controller's width, it refuses a
payload longer than itself before anything is sent, and it would send a shorter one with its own tail left intact
([writing into the tag buffer](../../../AllenBradley.Documentation/libPlcTag/writing-into-the-tag-buffer.md)). That
second behaviour is why the array converters refuse a value with the wrong number of elements instead of letting it go
out short.

## What this means when a type is added

A new data type needs a data point record that names its type and shape, a converter for its byte layout, one
registration, and a row in the [data-type support reference](../../reference/datatype-support.md). Verification needs
nothing new unless the type carries configuration of its own, the way a string carries a capacity. A structure other
than `STRING` is not a value in this port at all. It is opened into members, and each member is a data point of an
elementary type ([the symbol table](symbol-table.md)).
