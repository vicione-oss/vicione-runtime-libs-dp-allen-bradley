namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>Quality of an <see cref="ILogixDataPointValue"/>.</summary>
public enum LogixQuality
{
    /// <summary>The value was read (or is to be written) successfully.</summary>
    Good,

    /// <summary>The read failed; the value is not trustworthy.</summary>
    Bad,
}
