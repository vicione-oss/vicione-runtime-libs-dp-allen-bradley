using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// Verifies configured data points against the controller's actual tag-definitions: it asks the client to
/// resolve each data point against the metadata on the device and reports the tags whose declared type, shape or very
/// existence disagrees with the configuration. This is what <c>CreateConfigurationVerifier</c> returns,
/// so a misconfigured tag aborts the connect-process.
/// </summary>
internal sealed class LogixConfigurationVerifier(ILogixClient client)
    : IDataPointConfigurationVerifier<ILogixDataPoint>
{
    /// <summary>
    /// Resolves every data point against the metadata on the device, then returns one entry per
    /// <b>misconfigured</b> one; a fully matching configuration returns an empty list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    public async ValueTask<IReadOnlyList<MisconfiguredDataPoint<ILogixDataPoint>>> Verify(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        var resolvedDataPoints = await client.ResolveDataPoints(dataPoints, cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. from resolved in resolvedDataPoints
            let mismatches = GetMismatches(resolved)
            where mismatches.Count > 0
            select new MisconfiguredDataPoint<ILogixDataPoint>(resolved.DataPoint, mismatches)
        ];
    }

    internal static IReadOnlyList<MismatchingConfiguration> GetMismatches(ResolvedDataPoint resolved)
    {
        var dataPoint = resolved.DataPoint;

        if (resolved.TagDefinition is not { } device)
        {
            return [new MismatchingConfiguration($"Tag '{dataPoint.TagName}' was not found on the controller.")];
        }

        // The converter is the single source of the expected type, and LogixTypeComparison the single
        // comparison. This is the only place a configured type is read against the controller's own
        // declaration (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md) — the poll
        // trusts the verdict reached here, so a mismatch that gets past it is a mismatch nothing
        // downstream will catch. The verifier only renders the kind it reports.
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);
        var mismatch = LogixTypeComparison.Compare(converter, resolved);
        return mismatch switch
        {
            LogixTypeMismatch.None => [],
            LogixTypeMismatch.Array =>
            [
                new MismatchingConfiguration(
                    $"Tag '{dataPoint.TagName}' is a {device.DimensionCount.Value}-dimensional array on the controller, " +
                    "but a scalar is configured."),
            ],
            LogixTypeMismatch.Structure =>
            [
                new MismatchingConfiguration(
                    $"Tag '{dataPoint.TagName}' is a structure on the controller, " +
                    $"but a scalar of type {converter.ExpectedTypeName} is configured."),
            ],
            LogixTypeMismatch.AtomicType =>
            [
                new MismatchingConfiguration(
                    $"Data type mismatch for tag '{dataPoint.TagName}': configured {converter.ExpectedTypeName}, " +
                    $"controller reports {Describe(device.DataType)}."),
            ],
            LogixTypeMismatch.Atomic =>
            [
                new MismatchingConfiguration(
                    $"Tag '{dataPoint.TagName}' is an elementary {Describe(device.DataType)} on the controller, " +
                    $"but the structured type {converter.ExpectedTypeName} is configured."),
            ],
            // Rendered in characters, because that is what both sides of the comparison are and what the
            // configuration is written in. The converter is asked for the configured capacity rather
            // than the data point, which this method only knows through ILogixDataPoint.
            LogixTypeMismatch.StringCapacity =>
            [
                new MismatchingConfiguration(
                    $"Capacity mismatch for tag '{dataPoint.TagName}': the configured {converter.ExpectedTypeName} " +
                    $"holds {Describe(converter.MaxLengthFor(dataPoint))} characters, " +
                    $"but the controller declares {Describe(device.MaxLength)}."),
            ],
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolved), mismatch, "Unhandled type mismatch kind."),
        };
    }

    private static string Describe(AllenBradleyDataType? dataType) => dataType switch
    {
        null => "a structure",
        AllenBradleyDataType.Unknown => "a type this addon does not model",
        { } type => type.ToString(),
    };

    // A capacity is absent only for a type that has none to declare, which the capacity mismatch itself
    // rules out on the configured side; the device side is null only for a structure the listing gave
    // nothing to size.
    private static string Describe(StringMaxLength? maxLength) =>
        maxLength?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";
}
