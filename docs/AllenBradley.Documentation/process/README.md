# Developer process

How an Allen-Bradley dataport gets built, phase by phase. Each phase is a how-to guide written after
the fact, from the slice that actually went through it, so the traps in them are real ones rather than
imagined.

The phases are ordered. Each assumes the previous one is finished.

| Phase | Guide | Status |
|-------|-------|--------|
| 1. Bootstrap the project | [bootstrap-the-project.md](bootstrap-the-project.md) | Written |
| 2. CI pipeline | *planned* | Builds the solution and runs the unit suite on a machine that is not a developer laptop |
| 3. Walking skeleton | *planned* | The first dataport the engine can actually discover: device nodes, YAML manifest, communication configuration |
| 4. Data types | *planned* | Adding Logix types one family at a time |

The repo-wide build conventions these guides assume — the test platform, the package feeds, central
package management — are documented in [`AGENTS.md`](../../../AGENTS.md) at the repo root.
