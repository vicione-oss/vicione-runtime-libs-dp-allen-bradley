using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration;

/// <summary>One data file that <see cref="AbServer"/> creates at its start.</summary>
/// <param name="Type">The type of the data file.</param>
/// <param name="Number">The number of the data file.</param>
/// <param name="ElementCount">How many elements the data file has.</param>
internal readonly record struct SimulatedDataFile(DataFileType Type, FileNumber Number, ushort ElementCount)
{
    /// <summary>The data file as the <c>--tag</c> argument of ab_server gives it — <c>--tag=N7[100]</c>.</summary>
    internal string TagArgument =>
        string.Create(CultureInfo.InvariantCulture, $"--tag={Type.Letter}{Number.Value}[{ElementCount}]");
}

/// <summary>
/// The data files on <see cref="AbServer"/>. ab_server accepts no other type and no other file number, so a
/// test can address only these files.
/// </summary>
internal static class AbServerDataFiles
{
    internal static readonly SimulatedDataFile BinaryFile = new(DataFileType.Binary, new FileNumber(3), 20);
    internal static readonly SimulatedDataFile IntegerFile = new(DataFileType.Integer, new FileNumber(7), 100);
    internal static readonly SimulatedDataFile FloatFile = new(DataFileType.Float, new FileNumber(8), 50);
    internal static readonly SimulatedDataFile StringFile = new(DataFileType.String, new FileNumber(18), 5);
    internal static readonly SimulatedDataFile LongFile = new(DataFileType.Long, new FileNumber(19), 10);

    internal static IReadOnlyList<SimulatedDataFile> All { get; } =
        [BinaryFile, IntegerFile, FloatFile, StringFile, LongFile];
}
