# The Port Lives in the Gateway String

libplctag has no port attribute. Neither the native attribute string nor the .NET `Tag` exposes one,
and that is not an omission. The gateway attribute carries host and port, and the native core splits
them apart. Write `10.0.0.1` and you get EtherNet/IP's registered port. Write `10.0.0.1:44819` and
you get 44819.

This is a library fact, not our decision. It is why the port is a property of ours that never
reaches libplctag under its own name. The client joins address and port back into one string on the
way down.

## Where the split happens

In the 2.6.3 core, the gateway string reaches the socket by way of `str_split(host, ":")`. Both
messaging paths do it, `session_handler` for unconnected requests and `conn_handler` for connected
ones. The second part of the split goes through `str_to_int` and straight into
`socket_connect_tcp_start(sock, host, port)`.

| Gateway string  | What the core does                                     |
|-----------------|--------------------------------------------------------|
| `10.0.0.1`      | Logs *"Using default port"*, connects on **44818**     |
| `10.0.0.1:2222` | Logs *"Using special port %d"*, connects on **2222**   |

`modbus_plc_handler` has the same shape for Modbus TCP, with 502 as its default. One parsing rule,
three call sites.

Nothing above the core participates. The .NET wrapper hands `Tag.Gateway` down as an opaque string,
so the colon is invisible until the connect.

## What it means for the client

The manifest asks for the two apart, `ConnectionEndpoint` for the address and `TcpPort` for the port,
defaulted to 44818, and `GatewayAttribute` joins them. It sits next to `LogixTagAccessFactory` in the
libplctag layer rather than on `LogixClientInformation`, because the joining is the library's rule and
nothing else in the port has a use for the joined string.

Splitting what the library joins buys three things.

The editor can check a port. A `UInt16` with a minimum of 1 is a spinner an integrator cannot typo
into the hostname. Had we asked for `host:port` in one field, validating it would have meant parsing
the colon ourselves, and a malformed port would have reached the native core before anything
objected.

The port is part of the pool key. `LogixClientInformation` carries it, so record equality, the "same
connection" test `LogixClientPool` keys on, tells two ports on one address apart. That is what they
are: two sessions.

One spelling reaches the wire. The attribute is always `host:port`, so the same controller cannot
arrive twice under two spellings. The core logs *"Using special port 44818"* where a bare host would
have logged *"Using default port"*, and connects to the same socket either way.

## Source references

| Location                            | Role                                                              |
|-------------------------------------|-------------------------------------------------------------------|
| `session_handler`                   | Splits the gateway string for unconnected messaging                |
| `conn_handler`                       | The same split for connected messaging                            |
| `str_split(host, ":")` → `str_to_int` | Host and port out of one attribute                               |
| `socket_connect_tcp_start(sock, host, port)` | Where the parsed port lands                              |
| `modbus_plc_handler`                | Same rule for Modbus TCP, default **502**                          |
| Default when no colon is present    | **44818**, EtherNet/IP's registered port                           |

Verified against the native binary in this tree: `plctag.dll` from
`libplctag.NativeImport 2.0.0-alpha.5` reports core 2.6.3 and carries both log strings,
`Using default port` and `Using special port %d.`
