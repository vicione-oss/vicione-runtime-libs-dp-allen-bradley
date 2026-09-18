using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
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
            return
            [
                new MismatchingConfiguration($"Tag '{dataPoint.TagAddress.Value}' was not found on the controller.")
            ];
        }
        var mismatch = LogixTypeComparison.Compare(resolved);

        // A capacity or an element count is reported only for the shape that configures one, so the casts
        // in those two branches cannot fail.
        return mismatch switch
        {
            LogixTypeMismatch.None => [],
            LogixTypeMismatch.Rank =>
            [
                new MismatchingConfiguration(
                    $"Shape mismatch for tag '{dataPoint.TagAddress.Value}': " +
                    $"configured {Describe(dataPoint.DimensionCount)}, " +
                    $"controller reports {Describe(device.DimensionCount)}."),
            ],
            LogixTypeMismatch.DataType =>
            [
                new MismatchingConfiguration(
                    $"Data type mismatch for tag '{dataPoint.TagAddress.Value}': configured {dataPoint.DataTypeName.Value}, " +
                    $"controller reports {Describe(device.DataType)}."),
            ],
            LogixTypeMismatch.StringCapacity =>
            [
                new MismatchingConfiguration(
                    $"Capacity mismatch for tag '{dataPoint.TagAddress.Value}': the configured {dataPoint.DataTypeName.Value} " +
                    $"holds {Describe(((StringDataPoint)dataPoint).MaxLength)} characters, " +
                    $"but the controller declares {Describe(device.MaxLength)}."),
            ],
            LogixTypeMismatch.ElementCount =>
            [
                new MismatchingConfiguration(
                    $"Element count mismatch for tag '{dataPoint.TagAddress.Value}': the configured " +
                    $"{dataPoint.DataTypeName.Value} holds {Describe(((ILogixArrayDataPoint)dataPoint).ElementCount)} elements, " +
                    $"but the controller declares {Describe(device.ElementCount)}."),
            ],
            LogixTypeMismatch.ElementIndexOutOfRange =>
            [
                new MismatchingConfiguration(
                    $"Element index {Describe(dataPoint.TagPath.ArrayElementIndex)} is out of range for tag " +
                    $"'{device.TagAddress.Value}', which the controller declares with " +
                    $"{Describe(device.ElementCount)} elements."),
            ],
            LogixTypeMismatch.ElementOfScalar =>
            [
                new MismatchingConfiguration(
                    $"Tag '{device.TagAddress.Value}' is a scalar on the controller, " +
                    "but an element of it is configured."),
            ],
            _ => throw new ArgumentOutOfRangeException(
                nameof(mismatch), mismatch, "Unhandled type mismatch kind."),
        };
    }

    private static string Describe(ElementIndex? index) =>
        index?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";

    private static string Describe(AllenBradleyDataType dataType) =>
        dataType == AllenBradleyDataType.Unknown ? "a type this addon does not model" : dataType.Name.Value;

    private static string Describe(StringMaxLength? maxLength) =>
        maxLength?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";

    private static string Describe(ElementCount? elementCount) =>
        elementCount?.Value.ToString(CultureInfo.InvariantCulture) ?? "none";

    private static string Describe(DimensionCount dimensionCount) =>
        dimensionCount.IsScalar ? "a scalar" : $"a {dimensionCount.Value}-dimensional array";
}
