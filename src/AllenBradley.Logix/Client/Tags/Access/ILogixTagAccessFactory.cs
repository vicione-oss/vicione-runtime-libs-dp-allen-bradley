using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Creates access to a data point's tag. Splitting creation from reuse keeps
/// <see cref="Lifetime.CachingLogixTagManager"/> clear of libplctag, so the reuse rules can be tested
/// against a fake factory rather than a real controller.
/// </summary>
internal interface ILogixTagAccessFactory
{
    /// <summary>Creates access to <paramref name="dataPoint"/>'s tag.</summary>
    ILogixTagAccess CreateAccessForDatapoint(ILogixDataPoint dataPoint);

    /// <summary>
    /// Creates access to a given tag-address. Used to browse the symbol table for
    /// configuration verification.
    /// </summary>
    ILogixTagAccess CreateAccessForTagAddress(TagAddress tagAddress);
}
