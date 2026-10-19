# Checking That the Controller Answers

libplctag has no connect call, so a client that must know if a controller answers has to send a request. This page
compares the four requests that can do this, and the libplctag version that each one needs. The session itself opens
by itself on the first request ([the shared session](the-shared-session.md)).

## The four methods

| Method            | Needs                                                   | PLC types                             | What a success shows                                                                    |
|-------------------|---------------------------------------------------------|---------------------------------------|-----------------------------------------------------------------------------------------|
| Read `@tags`      | `libplctag` 1.5.2 (the current package)                 | Logix (symbolic addresses)            | The PLC answers a CIP request.                                                          |
| Read `S2:0`       | `libplctag` 1.5.2 (the current package)                 | PLC-5, SLC 500, MicroLogix            | The PLC answers a PCCC request.                                                         |
| Read `@identity`  | core 2.6.14 or later, `plc=generic`, `NativeImport`     | All EtherNet/IP devices               | The EtherNet/IP endpoint answers. It also gives the product name, revision and serial number. |
| Tag `@connection` | core 2.7.0 or later, `NativeImport`                     | All AB types (PCCC is not documented) | The TCP connection and the EtherNet/IP session are open.                                |

The `libplctag` 1.5.2 package ships core 2.6.0. Thus, only the first two methods are available today. There is no
newer release of the package, and we do not build our own bindings to a newer core
([waiting for a libplctag.NET release](../../AllenBradley.Logix.Documentation/ADR/2026-10-13-waiting-for-a-libplctag-net-release-to-upgrade-the-c-library.md)).

## Read `@tags`

The read gets the symbol table of a Logix controller. It is a CIP request, so a success shows that the controller
answers CIP. The Logix client does this read at each connect, because it needs the symbol table anyway
([about connecting](../../AllenBradley.Logix.Documentation/explanation/client/connecting.md)).

## Read `S2:0`

File 2 is the status file. All PCCC controllers have this file, and nobody can delete it. Thus, `S2:0` exists on each
PLC-5, SLC 500 and MicroLogix, and it does not depend on the program.

1. The read goes the same route as the reads of the data points. This includes DH+ through a bridge.
2. The value of the word is not important. Do not use the value. Use only the status of the read.
3. A timeout or a connection error tells that the PLC is not reachable.
4. A PCCC error tells that the PLC is reachable, because the PLC sent the error.

## Read `@identity`

The read gets the Identity object of the device.

1. The `Tag` class of `libplctag` 1.5.2 cannot send `plc=generic`, so the read needs `NativeImport`.
2. Behind a NET-ENI, an ENET module or a DHRIO module, the adapter answers and not the PLC. Thus, a success does not
   show that the PLC is reachable.

## Tag `@connection`

The tag gives the `connection_status` attribute of the session
([reconnecting the session](reconnecting-the-session.md#what-a-caller-sees)).

1. Do not use the `Tag` class of `libplctag` 1.5.2 for this tag. Its callback throws `NotImplementedException` for the
   events 100 to 105. This stops the process.
2. Use `NativeImport` and read the `connection_status` attribute.
3. The adapter problem of `@identity` also applies here.
4. If no data request occurs for approximately 30 seconds, the session closes. The status then stays at `IDLE_WAIT`.
   Thus, a status that is not `UP` does not tell that the controller is not reachable.

## What the clients use

| Client | Check at connect | Why                                                                                     |
|--------|------------------|-----------------------------------------------------------------------------------------|
| Logix  | Read `@tags`     | The client needs the symbol table before it can create a tag handle.                    |
| Legacy | Read `S2:0`      | The status file exists on each controller, and the read uses the route of the data points. |

The Legacy client uses the `S2:0` handle one time and disposes it. A failed read fails the connect with a
`ConnectionFailureException`. The client does not separate a PCCC error from a timeout yet, because the shared tag
access gives a failure as text only.

## How it was checked

The facts come from a research of the `libplctag` and `libplctag.NET` sources and documentation in October 2026. Nobody
checked the `S2:0` read against a PCCC controller yet, because the test bench has none
([test-device setup](../test-bench/test-device-setup.md)).
