namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// How the controller's type declaration disagrees with what a converter decodes, or
/// <see cref="None"/> when they match. This is the single verdict both the runtime type gate
/// (<see cref="IDataPointConverter.ConflictsWith"/>) and configuration verification project off, so a
/// degraded read and a connect-time verification error cannot drift on what counts as a mismatch
/// (ADR-003). The verifier renders the kind reported here; the gate only asks whether it is
/// <see cref="None"/>.
/// </summary>
internal enum LogixTypeMismatch
{
    /// <summary>The declaration matches the converter's expected scalar type — or is absent, which is unverifiable.</summary>
    None,

    /// <summary>The controller reports an array where a scalar is configured.</summary>
    Array,

    /// <summary>The controller reports a structure where a scalar is configured.</summary>
    Structure,

    /// <summary>The controller reports a different elementary CIP type.</summary>
    AtomicType,
}
