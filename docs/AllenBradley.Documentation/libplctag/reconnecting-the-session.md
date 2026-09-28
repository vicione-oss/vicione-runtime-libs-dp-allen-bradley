# Reconnecting the Session

For Allen-Bradley over EtherNet/IP, libplctag opens a dropped session again by itself. A caller does not need to
destroy and create its handles again. This page tells how the core finds a dropped link, how it retries, and what a
caller sees during an outage. The source is the session state machine in `src/libplctag/protocols/ab/session.c`.

## How the core finds a dropped link

The core finds a dropped link only when a request fails. Each send and each receive has a timeout of 2 seconds
(`SESSION_DEFAULT_TIMEOUT`). After a socket error or a timeout, the core closes the session in this sequence:

1. It sends a Forward Close.
2. It unregisters the session.
3. It closes the socket.

The core also closes a session without requests after an inactivity timeout. This is intentional. The session then
waits in `IDLE_WAIT`, and the next request opens it again. Thus, without traffic, the core does not find a pulled
cable until the next read or write.

## How the core retries

The core retries with an exponential backoff. The first delay is 100 ms. Each retry doubles the delay, up to a maximum
of 10 s, and adds a random jitter (`calc_retry_time`). There is no retry limit, so the core retries for as long as the
handle exists.

The delays are hard-coded. A `FIXME` in the source says that they must become a tag attribute. No such attribute
exists yet.

After a successful retry, the core does the full connect sequence again. It opens the socket, registers the session and
sends a Forward Open. The existing handles continue to work.

## What a caller sees

Reads and writes during the outage fail. They return an error, usually `PLCTAG_ERR_TIMEOUT` or an error for a bad
connection. The core does not queue them for a later replay. After the session is up again, new calls succeed.

The core gives the state of the connection in two ways: the `connection_status` tag attribute, and the
`PLCTAG_EVENT_CONN_STATUS_*` events. The states are `UP`, `DOWN`, `CONNECTING`, `DISCONNECTING`, `IDLE_WAIT` and
`ERR_WAIT`. `ERR_WAIT` means that the core lost the connection and waits for the next retry.

## What this means for the client

The client has no reconnect code, and it needs none. It keeps its handles after a failed read or write, and the next
request after a successful retry succeeds. The client treats a failed operation as a temporary failure of that tag.
The framework then decides what occurs: its circuit breaker limits the polls, and its write queue writes a failed batch
again ([Connecting](../../AllenBradley.Logix.Documentation/explanation/client/connecting.md#what-this-means-at-run-time)).

The client does not read `connection_status` and does not listen for the events. Its `IsConnected` tells only that the
symbol table was loaded.

## How it was checked

The facts come from the vendored source of core 2.7.1 (commit `bdb10aea`, August 2026). Only the AB protocol path was
checked. Modbus has its own reconnect code. The `libplctag` 1.5.2 package that this repository builds with ships core
2.6.3, and nobody checked this page against that version.

## Source references

| Location | Role |
|----------|------|
| `session.c:1909-1918` | Send and receive with `SESSION_DEFAULT_TIMEOUT` (2 s). A failure starts the close sequence |
| `session.c:1278` | `calc_retry_time`: the backoff from 100 ms, doubled on each retry, capped at 10 s, with jitter |
| `session.c:1637` | `FIXME - make this a tag attribute.` beside the calculation of the retry delay |
| `session.c:1666` | `IDLE_WAIT`: after an idle close, the next request opens the session again |
| `libplctag.h:358` | The `PLCTAG_EVENT_CONN_STATUS_*` events |
| `libplctag.h:576` | The `PLCTAG_CONN_STATUS_*` values of the `connection_status` attribute |
