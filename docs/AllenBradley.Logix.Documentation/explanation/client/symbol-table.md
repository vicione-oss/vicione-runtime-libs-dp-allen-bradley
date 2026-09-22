# About the symbol table

This page says what the client reads from the controller at connect, why it reads all of it up front, and the one
question the result answers.

## What a controller will tell you about its tags

A Logix controller exposes its tags as Symbol objects and its structure layouts as Template objects, and `libplctag`
reaches both through pseudo-addresses. `@tags` lists the controller-scoped tags, each with its name and a symbol type
that packs the structure bit, the array rank and either an elementary type code or a template id
([symbolic tag data types](../../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)).
`Program:<name>.@tags` lists one program's tags. `@udt/<id>` returns a single template with its members, their offsets
and their types
([reading a UDT definition](../../../AllenBradley.Documentation/libPlcTag/reading-a-udt-definition.md)).

Two things the listings leave out shape the design. A listing never names a member or an element, so `Motor.Speed` and
`Readings[3]` appear in none of them. Only `Motor` and `Readings` do. And a listing names a structure by template id
alone, so a structured tag arrives with no data type of its own. Its members, and whether it is a string, are in the
template.

## Three reads, all at connect

![How the symbol table is loaded](diagrams/symbol-table-loader.svg)

The load reads the controller listing, then one listing per program it found there, then one template per distinct id
the listed tags name. It follows the templates that members name in turn, and skips an id it has already read. Every
read opens a handle of its own and disposes it straight after, because a handle for a pseudo-address is not worth
keeping warm. Any read the controller refuses, and any reply that will not decode, fails the load, and a failed load is
a failed connect ([connecting](connecting.md)).

Reading every template at connect, rather than the first time a path needs one, has a price. A project with many
user-defined types pays one read per type on every connect. We pay it because the lookup has to be synchronous and in
memory. It runs when a handle is first created, under the lock that guards the handle cache, and it cannot do I/O
there. Lazy template reads would also let a connect succeed against a controller whose templates cannot be read, and
the first sign of that would be a poll. One load at connect proves the whole table readable.

Nested programs are not followed as this is not yet supported.

`Program:Parent.Child.Tag`
([tag scoping](../../../AllenBradley.Documentation/protocol/allen-bradley-extension/tag-scoping.md)). Add-On
Instruction locals cannot be reached from outside at all, so they are in no listing to begin with.

## One question, answered by a walk

The result is the symbol table: the listed tags by address, and the templates by id. Names are matched without regard
to case, because the controller keeps the case a tag was declared with but resolves it either way, and a configuration
that spells it differently is not wrong.

The table answers one question: what the controller declares the value at a given path to be. The answer is a declared
type, the device's side of what [verification](verification.md) compares. The walk starts at the listed tag, which is
the program and the tag name with nothing behind them, because that is all a listing names. From there it follows each
member of the path into the template of the type reached so far, and stops at the first member the templates do not
know. If the path ends in a subscript, it steps into the array and answers with a scalar of the element type. Last, a
structure whose template has a `.DATA` member of `SINT` elements is answered as a string of that member's length. That
is how the port recognises a string, and it is why a project's own string types, declared narrower or wider than the
built-in one, are strings to the port just as `STRING` is.

A path that stops short anywhere on the walk answers nothing. The tag is not listed, the member is not in its template,
a member was asked of an array or of an elementary type, or the subscript is past the end. The verifier reports that
nothing as a tag the controller does not have. It does not tell the four cases apart, because the address in the report
already shows which part of the path is wrong.

## Where the walk stops today

A system structure such as `TIMER` or `COUNTER` has no template the controller serves, so a path into one of its
members cannot be resolved and comes back as missing. Whether those, and Add-On Instruction instances, get opened the
same way a user-defined type does is still open
([data-type support](../../reference/datatype-support.md)). A structure that is not a string is never a value in this
port. It is only ever opened into members.

The walk's own nodes, one per listed tag and one per template member, belong to the lookup alone. Nothing outside the
symbols folder names them, so the shape of a listing can change without the rest of the client noticing.
