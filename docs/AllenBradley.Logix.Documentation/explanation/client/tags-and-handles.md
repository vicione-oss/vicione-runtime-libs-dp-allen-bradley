# About tags and handles

The library gives the client a handle for each tag. This page says why there is one per data point and not one per tag
name, why it is cached and borrowed instead of created and owned, and why one operation at a time on it is enough.

## The handle is the expensive thing

The library works in tag handles, each one standing in for a tag on the controller
([CONTEXT.md, "Tag handle"](../../../../CONTEXT.md)). There is no connection handle to cache. The first read of a
handle does the session registration, the Forward Open and the symbol resolution, and every operation after that
reuses them. A handle that has been read once is warm. Throw it away and that setup goes with it
([the shared session](../../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

Two more facts about the handle shape everything below. Its buffer holds the payload in the controller's own layout,
and its width is the controller's rather than the client's
([what the tag buffer holds](../../../AllenBradley.Documentation/libPlcTag/what-the-tag-buffer-holds.md)). And two
operations that overlap on one handle are refused with a busy status, with a completion mispairing in the wrapper
underneath
([concurrent operations on a handle](../../../AllenBradley.Documentation/libPlcTag/concurrent-operations-on-a-handle.md)).

## One warm handle per data point

![How a tag is created](diagrams/tag-creation.svg)

The warm handle is the reusable resource, so the client keeps one per data point for the life of the connection. It is
built on the first read or write and cached until the connection goes away
([reusing and releasing tag handles](../../ADR/2026-07-16-reusing-and-releasing-tag-handles.md)). The cache key is the
data point record itself, poll frequency included. One tag configured at two frequencies is two data points and two
handles. Keying by tag address instead would save a handle and put two polling jobs in a queue behind each other on the
gate of the handle they now share. Two handles cost one connection setup each and nothing after that.

A tag is borrowed, never owned. The manager disposes it, and a consumer that disposed one would break every other
holder of it. Every handle has to be freed by hand. One left to its finalizer is freed after CLR teardown and takes the
process down with it
([tag disposal and shutdown](../../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)). So the
manager frees every handle it holds even when one of them throws, and logs the reason instead of raising it.

Each tag the manager hands out ties together three things that do not change: the configured data point, what the
controller declares at its path, and the access to the handle. The batches read and write through it, and
[verification](verification.md) reads the first two off it. We wanted the declaration the verifier compares to be the
one stamped onto the very handle the polls use, so that no second lookup can disagree with the first.

## One operation at a time, as a whole

The access to a handle is a chain, and each link answers one of the library facts above.

The outermost link lets one whole operation at a time onto a handle. The library's own API keeps the device sync apart
from the buffer access, and two callers interleaving those calls on one handle read each other's bytes. So the client
exposes operations, a read that returns bytes and a write that takes them, and never the buffer in between
([operations, not accessors](../../ADR/2026-07-16-operations-not-accessors-over-libplctag.md)). The gate is per handle,
so separate handles do not contend and the fan-out of a batch is untouched
([reading and writing](reading-and-writing.md)).

Below it, an adapter runs the operation against the native handle and maps the library's exception onto a failed
result, leaving only cancellation to throw. That is where a device failure turns into data, which is what lets a batch
report every failed tag instead of the first one.

A factory builds the chain and binds the same connection attributes onto every handle. The one attribute that differs
per data point is the element count, and it counts what the controller counts rather than what a value holds. The
library treats every tag as an array and puts that count on the request as it stands, so a `BOOL` array counts the
32-bit words its bits are packed into, and a scalar counts one. The element size is left unset, because the library
ignores it for Allen-Bradley and takes the width from the controller's own declaration.

The alternative was the library's typed mapper API. We left it alone because it is being removed upstream, and because
its half-encoded buffer on a failed write is the kind of state the operations design exists to rule out. The access
interface is also the seam the unit tests fake. The cache rules and the batches are tested against a factory that
builds no native handle at all.

## What this means at run time

The first poll after a connect is slower than the second, because every handle it touches is warmed on that poll. A
reconnect pays the same price again, because a disconnect frees every handle. A tag whose declaration changed under a
running port is not verified again until the next connect. Until then its handle refuses a payload of the wrong width,
or its reply fails to decode, and that comes back as that tag's failure on every operation.
