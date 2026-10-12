# Sharing the libplctag Client Code with Legacy

## Context and Problem Statement

The Legacy dataport talks to its controllers through libplctag, as Logix does. It needs the same client code around
the library: a pool with one connection for each controller, one cached tag handle for each data point, batched reads
and writes that survive a failed tag, and the byte layout of integers and floats. Only the data model and the
addressing differ. A copy of this code in Legacy would mean that every later fix has to be made twice
(issue #36).

## Considered Options

* **Option 1: A shared internal project** (`AllenBradley.LibPlcTag`, referenced by both dataports and bundled into
  each of their packages)
* **Option 2: Legacy references Logix** (Legacy uses the Logix client types directly)
* **Option 3: A copy in Legacy** (Legacy starts from a copy of the Logix client and the two drift apart)
* **Option 4: A separate NuGet package** (the shared project is published on its own and both dataports depend on it)

## Decision Outcome

Chosen: **Option 1**, because it keeps one copy of the code without tying Legacy to the Logix model. The project holds
only code that names no Logix or Legacy type: the tag access and its results, the tag cache, the client pool, the read
and write batches, the converter contract and the codecs for the elementary types. The symbol table, the tag
attributes for each protocol, and the converters for strings, timers, counters and bit arrays stay in each dataport,
because their layout differs between the two.

Option 2 would load the Logix model and its YAML manifest into the Legacy port for no use. Option 3 is the problem the
issue exists to avoid. Option 4 would add a version to keep in step and a package to publish, for an assembly that no
one outside this repository uses. Bundling costs nothing, because `dotnet publish` of a dataport copies the referenced
assembly into its output.

Three details of the shared code follow from this decision:

* The batches return their failures and do not throw. Logix throws `LogixTagException`, which is public and stays
  unchanged. The Legacy read client is documented to throw `ConnectionFailureException`. A shared exception type
  could serve only one of them.
* The pool needs a client that it can connect, disconnect and dispose. `ILogixClient` and `ILegacyClient` are public,
  and a public interface cannot extend an internal one, so `IConnectableClient` is the one public type in the shared
  assembly.
* The Logix converters stay one class per type and call the shared codecs. One generic converter could replace about
  20 of them, but it would change many tests, so it is left for a later change.

### Consequences

* A type moves into `AllenBradley.LibPlcTag` only when it names no dataport type. Anything specific to one protocol
  stays in that dataport.
* Every package and deploy script must contain the shared assembly. Both publish the dataport project, so the
  assembly is included as long as the project reference stays.
