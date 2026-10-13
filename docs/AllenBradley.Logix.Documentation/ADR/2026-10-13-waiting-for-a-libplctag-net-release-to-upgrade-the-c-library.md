# Waiting for a libplctag.NET Release to Upgrade the C Library

## Context and Problem Statement

Both dataports use version 1.5.2 of the `libplctag` NuGet package, the latest release of the .NET wrapper. The wrapper
depends on `libplctag.NativeImport` 1.0.41. That package contains the C# bindings and the native binaries of the C
library, which libplctag calls the core. The core in it is version 2.6.3.

Two newer core versions add features that would improve the check at connect. Core 2.6.14 can read the `@identity` tag
of any EtherNet/IP device, and core 2.7.0 reports the state of the session in the `connection_status` attribute of the
`@connection` tag. Today the Logix client reads `@tags` and the Legacy client reads `S2:0`
([checking that the controller answers](../../AllenBradley.Documentation/libplctag/checking-that-the-controller-answers.md)).
No release of the wrapper ships either core version.

## Considered Options

* **Option 1: Wait for a wrapper release** (keep `libplctag` 1.5.2 and core 2.6.3 until libplctag.NET publishes a
  release with a newer core)
* **Option 2: Write our own bindings** (build a newer core ourselves and call it from C# through our own P/Invoke
  declarations)

## Decision Outcome

Chosen: **Option 1**, because both clients already work on core 2.6.3. Reads and writes succeed, and each connect
checks that the controller answers. The newer features would only make that check more precise.

Option 2 would move the work of the libplctag.NET project to us. We would build the core for each operating system and
processor that ViciOne supports, ship the binaries in our packages, and update the P/Invoke declarations for every core
release. A wrong declaration corrupts memory without an error. The integration tests find such a bug only on the
platform where they run. A better connect check does not justify this permanent work.

### Consequences

* The clients can use only the features of core 2.6.3. A feature of a later core requires a new decision first.
* Some pages in `AllenBradley.Documentation/libplctag/` were checked against a later core and say so, for example
  [reconnecting the session](../../AllenBradley.Documentation/libplctag/reconnecting-the-session.md) against 2.7.1.
  When the dataport behaves differently from such a page, check the page against 2.6.3.

## More Information

Revisit this decision when libplctag.NET releases a wrapper with core 2.7.0 or later, or when we need a feature that
only a newer core has. The prerelease `libplctag.NativeImport` 2.0.0-alpha.5 does not help, because it also contains
core 2.6.3 ([the port in the gateway attribute](../../AllenBradley.Documentation/libplctag/the-port-in-the-gateway-attribute.md)).
