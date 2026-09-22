# About connecting

A Logix controller has no socket for us to open. This page says what the client does instead, what state a connect
leaves behind, and why two dataports on one controller end up on the same session.

## There is nothing to connect to

The vicione dataport interface hands us a connect and a disconnect. It expects us to hold the connection between the
two, and it expects a later connect to undo a disconnect. Only a dispose is final.

`libplctag` has nothing for us to hold. No connect call, no connection object, no status flag. The session to the
controller opens by itself on the first read of the first handle, and every handle after that to the same gateway,
route path and PLC type uses the same one
([the shared session](../../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

## Connect loads the symbol table

![Who owns the connection](diagrams/client-connection-lifecycle.svg)

Connect does the one thing that has to happen before any tag can be read. It loads the controller's
[symbol table](symbol-table.md). If the table comes back, the controller is reachable and speaks CIP, which is as close
to a connection check as we get, and verification needs the table anyway. If the load fails, the client reports a
connection failure. The framework then retries the connect instead of the poll.

The tag manager holds what the connection owns, which is the symbol table and the handles opened so far. A disconnect
throws away both. The next connect loads the table again and re-opens the handles it needs, and a dispose ends the
manager for good.

One detail there is not optional. A handle left to its finalizer is freed after CLR teardown, and the process dies with
it ([tag disposal and shutdown](../../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)). So a
disconnect frees every handle even when one of them throws on the way, and the pool disposes a client whose disconnect
has already failed.

## Why two ports share one session

A controller only has so many CIP connections, and every session costs one. Packing matters more. `libplctag` packs
requests per session, so a read and a write travel in the same packet only if they sit on the same session. A session
per port would spend twice the slots and never pack a read together with a write. A session per group of data points
would leave almost nothing to pack. We settled on one session per controller
([maximizing throughput with one shared connection](../../ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md)).
One session per poll-frequency class is written down as a later option, to be taken if somebody measures a reason for
it.

The pool is where that is enforced. Both dataports (incoming and outgoing) ask it for a client, it keeps one client per controller identity,
and whoever releases last disconnects and disposes it. If two callers ask at the same time, the second waits for the
first caller's connect, so nobody receives a client whose symbol table is still loading. A caller that gives up drops
its own wait and leaves the connect running for the others. A failed connect is not remembered. The entry disappears
with the last reference, and the next caller tries the controller again. Shutting the pool down never waits for a
connect in flight, because a controller that stopped answering is a likely reason to shut down at all.

## What counts as the same connection

The pool keys on the controller identity: endpoint, TCP port, CIP route path, and the timeout for one operation.
Endpoint and port stay apart in the model and are joined only where `libplctag` wants them as one string
([the port in the gateway attribute](../../../AllenBradley.Documentation/libPlcTag/the-port-in-the-gateway-attribute.md)).

Two ports on one controller with different timeouts will produce two separate clients.

## What this means at run time

Nothing tells us that a controller went away, because there is no connection to lose. We find out from reads that fail
and writes that throw, and from there the framework's retry and circuit breaker do the work. A reconnect loads the
symbol table again, so a tag that somebody added to the controller in the meantime shows up at that point.
