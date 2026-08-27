using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Creates access to a data point's tag. Splitting creation from reuse keeps
/// <see cref="CachingLogixTagManager"/> clear of libplctag, so the reuse rules can be tested
/// in-process against a fake factory rather than a real controller.
/// </summary>
internal interface ILogixTagAccessFactory
{
    /// <summary>Creates access to <paramref name="dataPoint"/>'s tag, bound to this factory's controller.</summary>
    ILogixTagAccess Create(ILogixDataPoint dataPoint);

    /// <summary>
    /// Creates access to a controller <em>system</em> tag by name — <c>@tags</c>,
    /// <c>Program:&lt;name&gt;.@tags</c> or <c>@udt/&lt;id&gt;</c>. Used to browse the symbol table for
    /// configuration verification; the caller owns disposing the transient access once it has read.
    /// </summary>
    ILogixTagAccess CreateForSchemaTag(TagName tagName);
}
