using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration;

/// <summary>
/// libplctag's ab_server as an SLC 500 with the data files in <see cref="AbServerDataFiles"/>. It runs on a free
/// TCP port from the first test of <see cref="AbServerCollection"/> to the last. The binaries are in the
/// <c>binaries</c> folder beside this file.
/// </summary>
public sealed class AbServer : IAsyncLifetime
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ProbeInterval = TimeSpan.FromMilliseconds(50);
    private static readonly OperationTimeout OperationTimeout = new(TimeSpan.FromSeconds(5));

    private readonly ConcurrentQueue<string> _output = new();
    private Process? _process;

    /// <summary>How a client reaches the server: the loopback address, without a route path.</summary>
    internal LegacyClientInformation ClientInformation { get; } = new(
        LegacyControllerFamily.Slc500,
        new ConnectionEndpoint(IPAddress.Loopback.ToString()),
        new TcpPort(FreeLoopbackPort()),
        CipRoutePath.Direct,
        OperationTimeout);

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _process = new Process { StartInfo = StartInfo(ClientInformation.TcpPort) };
        _process.OutputDataReceived += (_, line) => Collect(line.Data);
        _process.ErrorDataReceived += (_, line) => Collect(line.Data);
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await WaitUntilTheServerAcceptsConnectionsAsync(_process);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_process is null)
        {
            return;
        }

        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();
    }

    private static ProcessStartInfo StartInfo(TcpPort port)
    {
        var startInfo = new ProcessStartInfo(ExecutablePath())
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("--plc=SLC500");
        startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $"--port={port.Value}"));
        foreach (var dataFile in AbServerDataFiles.All)
        {
            startInfo.ArgumentList.Add(dataFile.TagArgument);
        }

        return startInfo;
    }

    private static string ExecutablePath()
    {
        var (runtimeIdentifier, fileName) = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 when OperatingSystem.IsWindows() => ("win-x64", "ab_server.exe"),
            Architecture.X64 when OperatingSystem.IsLinux() => ("linux-x64", "ab_server"),
            _ => throw new PlatformNotSupportedException(
                $"There is no ab_server binary for {RuntimeInformation.RuntimeIdentifier}."),
        };

        return Path.Combine(AppContext.BaseDirectory, "Integration", "ab_server", "binaries", runtimeIdentifier, fileName);
    }

    // The port can be taken by another process before ab_server binds it. The start then fails with the
    // output of ab_server in the message.
    private static ushort FreeLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private async Task WaitUntilTheServerAcceptsConnectionsAsync(Process process)
    {
        using var timeout = new CancellationTokenSource(StartTimeout);
        while (!timeout.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw StartFailure($"ab_server stopped with exit code {process.ExitCode}");
            }

            if (await AcceptsConnectionsAsync(timeout.Token))
            {
                return;
            }

            await Task.Delay(ProbeInterval, CancellationToken.None);
        }

        throw StartFailure($"ab_server did not accept a connection within {StartTimeout.TotalSeconds} seconds");
    }

    private async Task<bool> AcceptsConnectionsAsync(CancellationToken cancellationToken)
    {
        using var probe = new TcpClient();
        try
        {
            await probe.ConnectAsync(IPAddress.Loopback, ClientInformation.TcpPort.Value, cancellationToken);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private InvalidOperationException StartFailure(string reason) =>
        new($"{reason}. Its output:{Environment.NewLine}{string.Join(Environment.NewLine, _output)}");

    private void Collect(string? line)
    {
        if (line is not null)
        {
            _output.Enqueue(line);
        }
    }
}
