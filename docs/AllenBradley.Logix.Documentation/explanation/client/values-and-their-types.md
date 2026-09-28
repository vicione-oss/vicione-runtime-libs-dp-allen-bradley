# About values and their types

A value in this port gets its type from its data point. The converters that make the value know nothing about the tag
on the controller. This page explains why.

## Where the types come from

The framework and the engine exchange values without a .NET type. The outgoing port receives an `object?`, and the
framework reads the values of the incoming port through its own interfaces. The framework also runs its range
validation through the value. The framework documentation describes these parts. This port decides where a value gets
its .NET type, and which part decides that type.

On the wire, CIP gives each atomic type a fixed width and a byte order
([CIP data types](../../../AllenBradley.Documentation/protocol/cip/data-types.md)). Logix adds structures, and
`STRING` is the important one here
([symbolic tag data types](../../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)).
The handle gives the client the bytes in the layout of the controller, with the packing and the padding
([what the tag buffer holds](../../../AllenBradley.Documentation/libplctag/what-the-tag-buffer-holds.md)).

## Only a data point can make its value

A data point has a generic parameter for its .NET type, and only the data point can make a value of that type. The
value record is a private type inside the data point. Thus, a value and its data point cannot have different types
([decoding tag bytes into typed values](../../ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md)). With one
general value class and a type field, a caller could make a `REAL` value for a `DINT` data point. Nothing would find
the error before the bytes were on the wire.

There is no empty value and no quality flag. If a read gives no value, the batch does not include the data point
([reading and writing](reading-and-writing.md)). A default value is not possible, because zero and an empty string are
real contents of a tag. A placeholder with a quality flag is not possible either. The framework marks each value of
the port as valid, so the placeholder would reach the engine as a valid value.

## A value from the engine

A value from the engine enters through one method on the data point, `ConvertValue`. The method either wraps the
value, or it returns a failure that names the tag and the expected type. It does not throw the failure. The framework
logs it and drops the batch.

The method compares the exact runtime type, not the assignability. This prevents a problem with the CLR. The CLR lets
an array of unsigned 16-bit integers pass as an array of signed 16-bit integers. With a pattern match only, the value
40000 would go to the controller as -25536.

The range check is the second check. It is on the value, because the configuration that it needs is in the data point
of the value. Only two kinds of value can be out of range: a string longer than its capacity, and an array of the
wrong length. An atomic scalar is always in range, because its .NET type has the same range as
the Logix type.

## A converter knows nothing about the tag

A converter changes bytes into a typed value, and a typed value into bytes. There is one converter for each type of
data point. A registry finds it by the concrete type of the data point. The registration gets this key from the type
parameter of the converter. Thus, a converter registered for the wrong data point does not compile.

A converter states nothing about the tag. The data point holds the data type, the shape, the capacity of a string and
the element count of an array. The [verification](verification.md) compares them with the controller one time, at
connect. The verification page also tells why this check is not in the converters.

## Three byte layouts

There are three kinds of converter, because the controller uses three byte layouts.

An array of an atomic type has its elements one after the other, without padding. The client reads and writes the full
array in one request. The array converter gets the element format from the scalar converter of the same type. Thus,
the two cannot disagree about the width of a `DINT`.

A `STRING` has a 4-byte `.LEN` in front of its characters, and padding to a 4-byte boundary after them. Each
character is one Latin-1 byte, and a character outside Latin-1 becomes `?`. The S7 dataport uses the same encoding.
Thus, a value written through one port reads back the same through the other. The string converter has two more rules:

- The controller does not check its own `.LEN`. The converter limits it to the capacity and to the length of the
  reply. Thus, an incorrect `.LEN` cannot read past the reply or stop a poll.
- A write fills the full `.DATA` area, with zeros after the characters. The controller keeps the bytes after `.LEN`,
  and Studio 5000 shows them. Without the zeros, the end of the old value stays visible.

A `BOOL` array has its bits packed into 32-bit words. Element *i* is one bit of a word, not a slice of bytes. A write
sends full words. Studio 5000 declares a `BOOL` array only in multiples of 32. Thus, a full word contains no bit of a
different tag.

## The converter does not know the width of the tag

A converter returns only the bytes of the value. The client does not know the padding that the controller adds after
them. The libplctag handle has the width of the controller, and it refuses a payload that is longer. It sends a
shorter payload, but it keeps its old bytes after the end
([writing into the tag buffer](../../../AllenBradley.Documentation/libplctag/writing-into-the-tag-buffer.md)).

For a `STRING`, these old bytes are only padding. For an array, they are old elements. For this reason, the array
converters refuse a value with the wrong number of elements.

## What this means when you add a type

To add a data type, do these steps:

1. Add a data point record that names the type and the shape.
2. Add a converter for the byte layout.
3. Register the converter.
4. Add a row to the [data type support reference](../../reference/datatype-support.md).

The verification needs no change, unless the type has its own configuration, for example the capacity of a string. A
structure other than `STRING` is not a value in this port. The port opens it into members, and each member is a data
point of an atomic type ([the symbol table](symbol-table.md)).
