# Reconnecting the Session

For Allen-Bradley over EtherNet/IP, libplctag opens a dropped session again by itself. A caller does not need to
destroy and create its handles again. This page tells how the core finds a dropped link, how it retries, and what a
caller sees during an outage. The source is the session state machine in `src/protocols/ab/session.c`.

## How the core finds a dropped link

The core finds a dropped link only when a request fails. Each send and each receive has a timeout of 2 seconds
(`SESSION_DEFAULT_TIMEOUT`). After a socket error or a timeout, the core closes the session in this sequence:

1. It sends a Forward Close, if the session uses connected messaging.
2. It unregisters the session.
3. It closes the socket.

The core also closes a session after 5 seconds without requests (`SESSION_DISCONNECT_TIMEOUT`). This is
intentional. The session then waits in `SESSION_WAIT_RECONNECT`, and the next request opens it again. Thus, without
traffic, the core does not find a pulled cable until the next read or write.

## How the core retries

The core waits a fixed 5 seconds before each retry (`RETRY_WAIT_MS`). There is no backoff and no jitter. There is
no retry limit, so the core retries every 5 seconds for as long as the handle exists. A failed retry closes the socket
and waits 5 seconds again. Core 2.7.1 replaces the fixed wait with a backoff from 100 ms up to 10 s.

The delay is hard-coded. A `FIXME` in the source says that it must become a tag attribute. No such attribute
exists in 2.6.0.

After the wait, the core does the full connect sequence again. It opens the socket, registers the session and
sends a Forward Open. The existing handles continue to work.

## What a caller sees

Reads and writes during the outage fail. A request that is on the wire when the link drops fails with the error of
the send or the receive, usually `PLCTAG_ERR_TIMEOUT` or an error for a bad connection. A request that a caller makes
during the 5-second wait stays in the queue of the session. If the caller's own timeout runs out first, the read or
write returns `PLCTAG_ERR_TIMEOUT` and the core aborts the request, so it is not sent later. After the session is up
again, new calls succeed. Thus, after a short outage, the first call can succeed only about 5 seconds after the
controller answers again.

Core 2.6.0 does not tell the state of the connection. It has no `connection_status` tag attribute and no
`PLCTAG_EVENT_CONN_STATUS_*` events. The only events are those of a tag: created, read and write started and
completed, aborted and destroyed. Later cores add both.

## What this means for the client

The client has no reconnect code, and it needs none. It keeps its handles after a failed read or write, and the next
request after a successful retry succeeds. The client treats a failed operation as a temporary failure of that tag.
The framework then decides what occurs: its circuit breaker limits the polls, and its write queue writes a failed batch
again ([Connecting](../../AllenBradley.Logix.Documentation/explanation/client/connecting.md#what-this-means-at-run-time)).

The client cannot see the state of the session, because core 2.6.0 does not give it. Its `IsConnected` tells only
that the check at connect succeeded: the load of the symbol table for Logix, the read of `S2:0` for Legacy
([checking that the controller answers](checking-that-the-controller-answers.md)).

## How it was checked

The facts come from the source of core `v2.6.0`, the version that the `libplctag` 1.5.2 package ships through
`libplctag.NativeImport` 1.0.41. Only the AB protocol path was checked. Modbus has its own reconnect code.

## Source references

| Location | Role |
|----------|------|
| `session.c:1467-1477` | Send and receive with `SESSION_DEFAULT_TIMEOUT` (2 s, `session.h:44`). A failure starts the close sequence |
| `session.c:1179-1187` | A failed request leads to `SESSION_DISCONNECT`, or to `SESSION_UNREGISTER` without connected messaging |
| `session.c:1214-1248` | The close sequence: Forward Close, unregister, close the socket, then retry or wait for a request |
| `session.c:67` | `RETRY_WAIT_MS`: the fixed retry delay of 5 s |
| `session.c:1254-1255` | `FIXME - make this a tag attribute.` beside the calculation of the retry delay |
| `session.c:69`, `session.c:1190-1199` | `SESSION_DISCONNECT_TIMEOUT`: the close after 5 s without requests |
| `session.c:1274-1288` | `SESSION_WAIT_RECONNECT`: after an idle close, the next request opens the session again |
| `libplctag.h:269-279` | The `PLCTAG_EVENT_*` values. None of them is about the connection |
