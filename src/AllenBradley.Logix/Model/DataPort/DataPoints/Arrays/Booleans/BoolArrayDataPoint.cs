using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;

/// <summary>
/// A one-dimensional Logix <c>BOOL</c> array tag, read whole and carried as one <see cref="bool"/> array
/// of the declared length — one entry per bit, which is what the tag declares and what Studio 5000 shows.
/// <para>
/// It is the one array whose elements do not occupy whole bytes: the controller packs them
/// <see cref="BoolsPerWord"/> to a 32-bit word, allocates and counts those words, and so
/// declares the tag as a <c>DWORD</c> array. Studio 5000 answers that by refusing any length that is not
/// a multiple of them, which is what keeps a whole-array write off its neighbours' bits.
/// </para>
/// </summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="ElementCount">The number of bits the tag is declared with in Studio 5000.</param>
public sealed record BoolArrayDataPoint(
    TagName TagName,
    PollFrequency PollFrequency,
    Channels Channels,
    ElementCount ElementCount)
    : LogixArrayDataPoint<bool>(TagName, PollFrequency, Channels, ElementCount)
{
    /// <summary>
    /// How many <c>BOOL</c>s the controller packs into one 32-bit word. Studio 5000 declares no
    /// <c>BOOL</c> array whose length is not a multiple of this.
    /// </summary>
    public const int BoolsPerWord = 32;

    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Bool;

    /// <summary>
    /// How many 32-bit words the declared bits are packed into — the count the controller knows the tag
    /// by, and so the count a request for it carries. Rounded up rather than required to divide evenly,
    /// so a count Studio 5000 could not have declared still covers every word it would touch.
    /// </summary>
    public ElementCount WordCount => new((ElementCount.Value + BoolsPerWord - 1) / BoolsPerWord);

    /// <summary>
    /// The number of <c>BOOL</c>s a packed array of <paramref name="wordCount"/> words holds: the
    /// <c>2</c> the <c>@tags</c> listing reports for a <c>BOOL[64]</c> is 64 of them. The controller
    /// counts the words because it knows the tag as a <c>DWORD</c> array; everyone above counts bits.
    /// </summary>
    internal static ElementCount ElementCountOfPackedWords(uint wordCount) => new(wordCount * BoolsPerWord);
}
