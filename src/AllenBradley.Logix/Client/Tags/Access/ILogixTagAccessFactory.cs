using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Creates access to a data point's tag. Splitting creation from reuse keeps
/// <see cref="Lifetime.CachingLogixTagManager"/> clear of libplctag, so the reuse rules can be tested
/// in-process against a fake factory rather than a real controller.
/// </summary>
internal interface ILogixTagAccessFactory
{
    /// <summary>Creates access to <paramref name="dataPoint"/>'s tag, bound to this factory's controller.</summary>
    ILogixTagAccess Create(ILogixDataPoint dataPoint);

    /// <summary>
    /// Creates access to one of the names the controller answers schema with — <c>@tags</c>,
    /// <c>Program:&lt;name&gt;.@tags</c> or <c>@udt/&lt;id&gt;</c>. Used to browse the symbol table for
    /// configuration verification; the caller owns disposing the transient access once it has read.
    /// </summary>
    /// <remarks>
    /// These are libplctag pseudo-names, not tags: they turn into a Symbol object (<c>0x6B</c>) or
    /// Template object (<c>0x6C</c>) browse and appear in no listing. In particular they are nothing to
    /// do with a <em>system tag</em>, which is a real tag carrying bit <c>0x1000</c> in its symbol type.
    /// </remarks>
    ILogixTagAccess CreateForSchemaTag(TagName tagName);
}
