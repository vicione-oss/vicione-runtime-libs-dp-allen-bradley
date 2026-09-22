# About reading and writing

This page follows one poll and one write through the client, and says why a failed tag costs the two directions
different things.

## What the client is asked for

The framework asks two things of it. The incoming port groups its data points by poll frequency, schedules a polling
job per group, and hands each group to the read seam. It wants one typed value back per point. The outgoing port queues
engine values, converts each one into a typed value, and hands a batch to the write seam, which answers with success or
an exception and nothing else. Polling jobs, the write queue, retries and the circuit breaker all belong to the
framework and are documented with the package.

`libplctag` sets the other boundary. Every handle to a controller shares one session, and that session packs whatever
requests are in flight into one packet. There is no timer to tune. Requests pack only because they arrive while the
session thread is still busy with the previous packet
([the shared session](../../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)). So the throughput of a
poll comes down to how many requests the client has in flight at once.

## The shape of a batch

![One poll and one write, and how each fails](diagrams/read-write-paths.svg)

Both seams share one client underneath, which is what puts the two ports on one connection and one handle cache
([maximizing throughput with one shared connection](../../ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md)).
Each operation becomes a batch, and a batch runs in two phases. First it resolves everything it will need, a converter
and a handle per point, before any I/O. Then it starts one request per tag, waits for all of them, and sorts the
outcomes ([reading and writing a group of tags](../../ADR/2026-07-16-reading-and-writing-a-group-of-tags.md)).

The fan-out is not an optimisation somebody added later. A loop that read one tag after another would never give the
session two requests to pack, and a poll of a hundred tags would cost a hundred round trips. Resolving first is what
makes a configuration mistake, say a data point with no converter, show up once and before the controller is touched,
rather than halfway through a poll.

A device failure comes back as a result, never as an exception. Waiting on a set of tasks rethrows only the first
exception of the set, and every other tag's outcome would go down with it. Cancellation still throws, because that is
the caller's decision and not the device's.

## Why a read degrades and a write fails whole

A poll is a sample, and the next sample is already scheduled. A tag that would not read loses its own value, the tags
that did answer come back in the group's order, and the failures go to the log as a warning. A read throws only when
nothing came back at all. That last rule is there for the framework's circuit breaker. An empty list would make a dead
controller look like a poll of an empty group, and nothing would ever trip.

A write is state. The outgoing port waits for a controller that is down instead of writing it off, so a failed write is
retried, and the client has to make that retry safe. It does so twice over. Every value is encoded before anything goes
out, so one value that will not encode fails the batch while the controller has seen nothing, and the message says as
much. After sending, every failed tag is named in one exception, and the values it does not name were written. Writing
a value that already landed does no harm, because a tag write sets state rather than raising an event.

We looked at three other options. Throwing at the first failure is what a plain `await` gives for free, and it hides
every failure after that one. A placeholder value with a bad quality flag would keep the list of a degraded read
complete, but it invents a payload the tag never held. The [values page](values-and-their-types.md) says why we dropped
it. Per-value write results would let a write degrade the way a read does, but the write seam has nowhere to carry
them, and adding one would mean re-explaining the framework's queue.

## What this means for a reader of the values

A caller that reads a group must not assume the list is as long as the group. It is as long as the number of tags that
answered, in the group's order, and a missing point is a failed read that was logged. A caller that writes gets either
silence or an exception naming every tag it needs to look at. Neither direction fails because a data point's type
contradicts the controller. That was settled before the first poll ([verification](verification.md)).

The gate that lets one operation at a time onto a handle limits parallelism per handle, not across handles
([tags and handles](tags-and-handles.md)). A batch of a hundred different tags still fans out a hundred requests. Two
batches that share a tag queue behind each other on that one handle.
