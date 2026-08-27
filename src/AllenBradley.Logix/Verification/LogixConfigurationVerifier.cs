using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// Verifies configured data points against the controller's actual symbol table by projecting off the
/// tag manager: it loads the schema once, gets the tag per data point, and reports the tags
/// whose declared type, shape or very existence disagrees with the configuration. This is what
/// <c>CreateConfigurationVerifier</c> returns, so a misconfigured tag aborts the connect-process.
/// </summary>
/// <remarks>
/// The manager owns the one browse of the symbol table (ADR-002), so the verifier is a projection of its
/// tags — <c>(tag.DataPoint, tag.Metadata)</c> — not a second browser: the schema the poll reads
/// from is the schema verified.
/// </remarks>
/// <param name="tagManager">Owns the controller schema and joins it onto each data point's tag.</param>
internal sealed class LogixConfigurationVerifier(ILogixTagManager tagManager)
    : IDataPointConfigurationVerifier<ILogixDataPoint>
{
    /// <summary>
    /// Loads the schema, then returns one entry per <b>misconfigured</b> data point; a fully matching
    /// configuration returns an empty list.
    /// </summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be browsed.</exception>
    public async ValueTask<IReadOnlyList<MisconfiguredDataPoint<ILogixDataPoint>>> Verify(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        // Idempotent, and the same load the poll relies on: verification doubles as the schema warm-up.
        await tagManager.LoadTagDefinitionsAsync(cancellationToken).ConfigureAwait(false);

        var misconfigured = new List<MisconfiguredDataPoint<ILogixDataPoint>>();
        foreach (var dataPoint in dataPoints)
        {
            var tag = tagManager.TagFor(dataPoint);
            var mismatches = GetMismatches(new ResolvedDataPoint(tag.DataPoint, tag.Metadata));
            if (mismatches.Count > 0)
            {
                misconfigured.Add(new MisconfiguredDataPoint<ILogixDataPoint>(dataPoint, mismatches));
            }
        }

        return misconfigured;
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
        // declaration (ADR-003) — the poll trusts the verdict reached here, so a mismatch that gets past
        // it is a mismatch nothing downstream will catch. The verifier only renders the kind it reports.
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);
        var mismatch = LogixTypeComparison.Compare(converter, resolved.TagDefinition);
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
}
