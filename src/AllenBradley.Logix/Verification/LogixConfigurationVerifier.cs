using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// Verifies configured data points against the controller's actual symbol table by projecting off the
/// tag manager: it loads the schema once, gets the tag per data point, and reports the tags
/// whose declared type, shape or very existence disagrees with the configuration.
/// </summary>
/// <remarks>
/// The manager owns the one browse of the symbol table (ADR-002), so the verifier is a projection of its
/// tags — <c>(tag.DataPoint, tag.Metadata)</c> — not a second browser: the schema the poll reads
/// from is the schema verified. The model carries only scalar elementary types today, so the diff checks
/// existence, that the tag is neither an array nor a structure, and that its atomic type matches. Element
/// count, string size and UDT layout drop into <see cref="GetMismatches"/> unchanged once the model grows
/// those shapes.
/// <para>
/// This verifier is deliberately free of the <c>Extensions.Verification</c> seam. The lifecycle slice
/// adapts <see cref="MisconfiguredLogixDataPoint"/> onto the framework's result and runs this after connect.
/// </para>
/// </remarks>
/// <param name="tagManager">Owns the controller schema and joins it onto each data point's tag.</param>
internal sealed class LogixConfigurationVerifier(ILogixTagManager tagManager)
{
    /// <summary>
    /// Loads the schema, then returns one entry per <b>misconfigured</b> data point; a fully matching
    /// configuration returns an empty list.
    /// </summary>
    /// <exception cref="Client.Schema.LogixSchemaException">The symbol table could not be browsed.</exception>
    public async Task<IReadOnlyList<MisconfiguredLogixDataPoint>> VerifyAsync(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        // Idempotent, and the same load the poll relies on: verification doubles as the schema warm-up.
        await tagManager.LoadSchemaAsync(cancellationToken).ConfigureAwait(false);

        var misconfigured = new List<MisconfiguredLogixDataPoint>();
        foreach (var dataPoint in dataPoints)
        {
            var tag = tagManager.TagFor(dataPoint);
            var mismatches = GetMismatches(new LogixResolvedDataPoint(tag.DataPoint, tag.Metadata));
            if (mismatches.Count > 0)
            {
                misconfigured.Add(new MisconfiguredLogixDataPoint(dataPoint, mismatches));
            }
        }

        return misconfigured;
    }

    internal static IReadOnlyList<LogixConfigurationMismatch> GetMismatches(LogixResolvedDataPoint resolved)
    {
        var dataPoint = resolved.DataPoint;

        if (resolved.Device is not { } device)
        {
            return [new LogixConfigurationMismatch($"Tag '{dataPoint.TagName}' was not found on the controller.")];
        }

        // The converter is the single source of the expected type and the comparison, shared with the
        // runtime type gate (ADR-003): verification and the poll cannot disagree on what a mismatch is. The
        // verifier only renders the kind the converter reports.
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);
        var mismatch = converter.CompareTo(device);
        return mismatch switch
        {
            LogixTypeMismatch.None => [],
            LogixTypeMismatch.Array =>
            [
                new LogixConfigurationMismatch(
                    $"Tag '{dataPoint.TagName}' is a {device.DimensionCount.Value}-dimensional array on the controller, " +
                    "but a scalar is configured."),
            ],
            LogixTypeMismatch.Structure =>
            [
                new LogixConfigurationMismatch(
                    $"Tag '{dataPoint.TagName}' is a structure on the controller, " +
                    $"but a scalar of type {converter.ExpectedType} is configured."),
            ],
            LogixTypeMismatch.AtomicType =>
            [
                new LogixConfigurationMismatch(
                    $"Data type mismatch for tag '{dataPoint.TagName}': configured {converter.ExpectedType}, " +
                    $"controller reports {Describe(device.AtomicType)}."),
            ],
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolved), mismatch, "Unhandled type mismatch kind."),
        };
    }

    private static string Describe(CipType? atomicType) => atomicType switch
    {
        null => "a structure",
        { } type when Enum.IsDefined(type) => type.ToString(),
        { } type => $"CIP type 0x{(byte)type:X2}",
    };
}
