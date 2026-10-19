# ab_server binaries

The Legacy integration tests run against `ab_server`, the PLC simulator of libplctag. The `AbServer` fixture starts
the binary for the platform of the test run as an SLC 500. These files are copies from the libplctag release
[v2.7.2](https://github.com/libplctag/libplctag/releases/tag/v2.7.2). Nobody changed or rebuilt them.

| File                    | Release asset                     | SHA-256                                                            |
|-------------------------|-----------------------------------|--------------------------------------------------------------------|
| `linux-x64/ab_server`   | `libplctag_2.7.2_ubuntu_x64.zip`  | `665ef6442abf0f4e374a7ea5d26382348b38e02833d8fb5f12c75f8b0d18f816` |
| `win-x64/ab_server.exe` | `libplctag_2.7.2_windows_x64.zip` | `27f7bb8dd5e1b82bae9037c61da7795e05c561b19d423165c1925f39100282f5` |

The Linux binary is linked statically, so it runs on any x64 Linux. The Windows binary needs only system DLLs. The
Linux binary must have mode `100755` in git, because a Linux checkout cannot start it otherwise. To add a new version,
run `git add --chmod=+x linux-x64/ab_server`.

## Why 2.7.2 and not the version of the client

The client uses the core 2.6.0 that the `libplctag` 1.5.2 package ships. `ab_server` knows the `B3` file only
from version 2.6.10. Version 2.7.2 is the newest release. The server is a separate process, and the client talks to it only over EtherNet/IP and
PCCC, so the two versions do not have to match. The integration tests write and read back all five data files through
the client with core 2.6.0.

## What the server cannot simulate

The server knows only the data files `B3`, `N7`, `F8`, `ST18` and `L19`, with these file numbers. It has no status
file, so the tests do not call `ConnectAsync`, which reads `S2:0`. It has no timer, counter, control, I/O or `A`
files, and it refuses sub-elements such as `T4:0.ACC`. A test that needs any of these stays a unit test.

## License

libplctag and `ab_server` are available under the
[Mozilla Public License 2.0](https://github.com/libplctag/libplctag/blob/v2.7.2/LICENSE-1.MPL) or the
[GNU LGPL 2 or later](https://github.com/libplctag/libplctag/blob/v2.7.2/LICENSE-2.LGPL). The source of these
binaries is the tag `v2.7.2` (commit `7712ed7f`) in the
[libplctag repository](https://github.com/libplctag/libplctag/tree/v2.7.2/src/tools/ab_server).
