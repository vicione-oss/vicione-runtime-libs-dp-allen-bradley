# Scripts

Utility scripts for deployment and automation.

## Deployment Scripts

- `deploy-logix-to-local-vicione.*` — deploys **AllenBradley.Logix** (ControlLogix / CompactLogix,
  symbolic tag addressing)
- `deploy-legacy-to-local-vicione.*` — deploys **AllenBradley.Legacy** (PLC-5 / SLC 500 / MicroLogix,
  data-file addressing)

Each script cleans its project, builds and publishes it in the requested configuration, and copies the
build outputs into the appropriate Vicione dependencies folder (creating the target directory if
needed). Both take the same parameters; the examples below use the Logix script.

### Parameters

1. **vo-suite-folder** (required): path to your local Vicione suite installation directory
2. **Build configuration** (optional): `Release` or `Debug` (default: `Debug`)
3. **Runtime identifier** (optional): `win-x64`, `linux-x64`, or `linux-arm64` for
   platform-specific builds

The scripts detect automatically whether the second parameter is a build configuration or
a runtime identifier — you can pass just a runtime identifier and Debug is assumed.

### Usage Examples

**Windows (Command Prompt):**

```cmd
# Debug build, framework-dependent (includes all platforms)
scripts\deploy-logix-to-local-vicione.cmd C:\path\to\vo-suite

# Release build for Windows x64
scripts\deploy-logix-to-local-vicione.cmd C:\path\to\vo-suite Release win-x64

# Debug build for Linux ARM64
scripts\deploy-logix-to-local-vicione.cmd C:\path\to\vo-suite linux-arm64
```

**Unix-like systems (Bash):**

```bash
# Debug build, framework-dependent
sh scripts/deploy-logix-to-local-vicione.sh /path/to/vo-suite

# Release build for Linux x64
sh scripts/deploy-logix-to-local-vicione.sh /path/to/vo-suite Release linux-x64
```

### Target Layout

Output lands under the suite's standalone cache, keyed by the version in the repo's `VERSION` file:

```text
<vo-suite>/src/Core.OS/bin/Debug/net10.0/Cache_Standalone/ViciOne.Suite.ClusterManagement/
  Dependencies/ViciOne.Suite.DataPort.AllenBradley.<Logix|Legacy>/<VERSION>/
```

`ViciOne.Suite.DataPort.AllenBradley.Logix` and `ViciOne.Suite.DataPort.AllenBradley.Legacy` are the
assembly names from the csproj files and match `FULLNAME_LOGIX` and `FULLNAME_LEGACY` in
`.gitlab-ci.yml`; keep script, csproj and pipeline in sync if a dataport is ever renamed.
