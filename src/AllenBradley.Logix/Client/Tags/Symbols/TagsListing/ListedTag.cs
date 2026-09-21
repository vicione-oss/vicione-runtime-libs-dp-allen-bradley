using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;

/// <summary>
/// One entry of an <c>@tags</c> listing as <see cref="TagsDecoder"/> reads it: the name the tag is
/// listed under, and what it is declared to be. The listing names a structure only by its template id,
/// so a structure arrives with no data type; the lookup fills the string ones in from their templates.
/// </summary>
/// <param name="TagAddress">The name as the controller reports it, program-qualified for a program tag.</param>
/// <param name="TagDefinition">What the tag is declared to be.</param>
internal readonly record struct ListedTag(TagAddress TagAddress, TagDefinition TagDefinition);
