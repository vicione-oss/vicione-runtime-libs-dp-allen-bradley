using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// Reports the configured data points whose declared type, shape or very existence disagrees with the
/// controller's own tag definitions.
/// </summary>
internal sealed class LogixConfigurationVerifier(ILogixClient client)
    : IDataPointConfigurationVerifier<ILogixDataPoint>
{
    /// <summary>
    /// Returns one entry per misconfigured data point; a fully matching configuration returns an empty
    /// list.
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

        // The only place a configured type is read against the controller's declaration; the poll trusts
        // the verdict (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);
        var mismatch = LogixTypeComparison.Compare(converter, resolved);
        return mismatch switch
        {
            LogixTypeMismatch.None => [],
            LogixTypeMismatch.Rank =>
            [
                new MismatchingConfiguration(
                    $"Shape mismatch for tag '{dataPoint.TagName}': " +
                    $"configured {Describe(converter.ExpectedDimensionCount)}, " +
                    $"controller reports {Describe(device.DimensionCount)}."),
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
            LogixTypeMismatch.StringCapacity =>
            [
                new MismatchingConfiguration(
                    $"Capacity mismatch for tag '{dataPoint.TagName}': the configured {converter.ExpectedTypeName} " +
                    $"holds {Describe(converter.MaxLengthFor(dataPoint))} characters, " +
                    $"but the controller declares {Describe(device.MaxLength)}."),
            ],
            LogixTypeMismatch.ElementCount =>
            [
                new MismatchingConfiguration(
                    $"Element count mismatch for tag '{dataPoint.TagName}': the configured " +
                    $"{converter.ExpectedTypeName} holds {Describe(converter.ElementCountFor(dataPoint))} elements, " +
                    $"but the controller declares {Describe(device.ElementCount)}."),
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

    private static string Describe(StringMaxLength? maxLength) =>
        maxLength?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";

    private static string Describe(ElementCount? elementCount) =>
        elementCount?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";

    private static string Describe(DimensionCount dimensionCount) =>
        dimensionCount.IsScalar ? "a scalar" : $"a {dimensionCount.Value}-dimensional array";
}
