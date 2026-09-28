# About verification

The port compares its configuration with the controller one time, at connect. This page explains what the port
compares, in which sequence, and why the write port does the comparison too.

## Find the mistake one time

The framework runs a verification step after connect and before the first poll. If the step reports a data point as
misconfigured, the connect fails. The framework owns the step, and the port supplies the comparison. The S7 dataport
compares each data point with the symbol table of its controller. This port does the same over CIP
([verifying configuration against the symbol table](../../ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md)).

The alternative is to let a configuration mistake show as a bad read. Some mistakes would then give one more warning
in the log on each poll. Other mistakes would give no error at all. For example, a `DINT` configured as an `INT` gives
incorrect values that look like data.

A configuration that does not agree with the controller is an engineering mistake. The port must find it one time, at
connect, with a message that names the tag and both sides.

## One comparison in one place

![One comparison, reached once at connect](diagrams/declared-type-and-verification.svg)

One function compares the declared type with the configuration, and the verifier is its only caller. An earlier design
also compared the types on each read and each write, and each converter stated the type that it needed. Thus, three
parts used one rule, and each operation paid for a check of something that changes only with a download. The
comparison moved to connect, and the converters now state nothing
([values and their types](values-and-their-types.md)).

The verifier does not read from the controller. It gets the declared types from the tag objects that the polls use.
The tag manager attached the declared type to each tag object when it created it
([tags and handles](tags-and-handles.md)). Thus, the verified declared type is the declared type that the polls use,
and no second lookup can give a different answer.

## What the verifier compares, and in which sequence

The verifier does these checks for each data point, and it stops at the first mismatch:

1. It checks that the tag exists. If the [symbol table](symbol-table.md) has no declared type for the path, the
   verifier reports that the controller does not have the tag.
2. It compares the shape: scalar or array. If the shape is different, a later comparison has no meaning.
3. It compares the data type.
4. It compares what the shape adds: the capacity of a string, or the element count of an array. An atomic scalar adds
   nothing.

The comparison function itself reports no mismatch when the declared type is missing. Thus, a missing tag gives one
finding, not two.

## Why the write port verifies too

The verification step is optional in the framework, and a port that only writes could skip it. This port does not
skip it, because a failed write has a high cost. The outgoing port waits for a controller that is down, and it
writes a failed batch again without a limit. Thus, a value whose type does not agree with the controller would be
written again forever. The log would only say that the controller refused it.

With the verification, the port gives one message that names the tag and the two types, and the port does not start.

## What this means for the operator

One incorrect tag stops the full port at connect. This is intentional. The message names the tag and the part that
does not agree. This part is the existence, the shape, the data type, the capacity or the element count. Correct the
configuration or the controller project. A change to the port does not correct it.

## After connect

The port does not verify again while it runs. During this time, a download can change the type of a verified tag. The
client does not compare types at run time. It finds only the changes that give an incorrect length:

1. An atomic scalar that became narrower gives a reply that is too short. The read of that tag fails.
2. An array whose element type changed its width gives a reply with the incorrect length. The read of that tag fails.
3. An array that became shorter than the configured count makes the controller refuse the request.
4. A write payload that is longer than the handle makes libplctag refuse the write.

Other changes give no error. A `DINT` that became a `REAL` has the same width, and the client decodes its bytes as a
`DINT`. A scalar that became wider decodes from its first bytes. An array that became longer gives its first elements,
because the request contains the configured element count.

The next connect of a new client finds each of these changes. The client pool keeps the old client while one port of
the controller still uses it ([connecting](connecting.md#what-this-means-at-run-time)). Thus, after a download that
changes a configured tag, restart all ports of that controller.
