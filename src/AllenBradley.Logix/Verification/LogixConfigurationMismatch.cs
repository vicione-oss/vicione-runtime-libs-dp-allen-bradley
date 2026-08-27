namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// One way a configured data point disagrees with the controller — a wrong data type, an unexpected
/// array or structure, or a tag that is not there at all. The <see cref="Description"/> names the tag
/// and states expected versus actual.
/// </summary>
/// <param name="Description">A human-readable statement of the mismatch.</param>
internal readonly record struct LogixConfigurationMismatch(string Description);
