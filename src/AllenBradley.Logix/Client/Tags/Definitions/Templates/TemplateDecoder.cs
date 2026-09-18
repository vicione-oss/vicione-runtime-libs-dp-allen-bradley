using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions.Templates;

/// <summary>
/// Decodes the raw bytes of an <c>@udt/&lt;id&gt;</c> read into one <see cref="TemplateDefinition"/>:
/// libplctag's 14-byte header, then the Read Template payload — the member descriptors, the template's
/// name, and the member names, each name zero-terminated. A pure function over a span, like
/// <see cref="TagsDecoder"/>, so the layout is pinned by synthetic buffers rather than by a device.
/// </summary>
internal static class TemplateDecoder
{
    // Rockwell appends encoding information to the template's name behind this separator.
    private const char EncodingSuffixSeparator = ';';

    /// <exception cref="FormatException"><paramref name="template"/> ends before the layout it announces.</exception>
    public static TemplateDefinition Decode(ReadOnlySpan<byte> template)
    {
        if (template.Length < TemplateHeader.Size)
        {
            throw new FormatException(
                $"A template read of {template.Length} bytes is shorter than its {TemplateHeader.Size}-byte header.");
        }

        var header = TemplateHeader.ReadFrom(template);
        var descriptors = ReadDescriptors(header, template[TemplateHeader.Size..]);
        var names = template[(TemplateHeader.Size + descriptors.Length * TemplateMemberDescriptor.Size)..];

        var templateName = ReadZeroTerminated(ref names);
        var members = new TemplateMember[descriptors.Length];
        for (var i = 0; i < members.Length; i++)
        {
            members[i] = ToMember(descriptors[i], ReadZeroTerminated(ref names));
        }

        return new TemplateDefinition(
            new TemplateId(header.TemplateId),
            new TemplateName(TrimEncodingSuffix(templateName)),
            new StructureHandle(header.StructureHandle),
            new StructureSize(header.StructureSize),
            members);
    }

    private static TemplateMemberDescriptor[] ReadDescriptors(in TemplateHeader header, ReadOnlySpan<byte> payload)
    {
        var descriptorBytes = header.MemberCount * TemplateMemberDescriptor.Size;
        if (payload.Length < descriptorBytes)
        {
            throw new FormatException(
                $"Template {header.TemplateId} announces {header.MemberCount} members but carries " +
                $"{payload.Length} bytes of descriptors, fewer than the {descriptorBytes} they need.");
        }

        var descriptors = new TemplateMemberDescriptor[header.MemberCount];
        for (var i = 0; i < descriptors.Length; i++)
        {
            descriptors[i] = TemplateMemberDescriptor.ReadFrom(payload[(i * TemplateMemberDescriptor.Size)..]);
        }

        return descriptors;
    }

    // A structure member holds no atomic type of its own: its data type is the template it names,
    // which the loader reads next.
    private static TemplateMember ToMember(in TemplateMemberDescriptor descriptor, string name)
    {
        var symbolType = descriptor.SymbolType;
        var isStruct = SymbolType.IsStruct(symbolType);
        var dimensionCount = SymbolType.DimensionCount(symbolType);
        var dataType = isStruct ? AllenBradleyDataType.Unknown : SymbolType.AtomicType(symbolType);
        var isScalarBool = !isStruct && dimensionCount == 0 && dataType == AllenBradleyDataType.Bool;

        return new TemplateMember(
            Name: new UdtMemberName(name),
            Offset: new MemberOffset(descriptor.Offset),
            DataType: dataType,
            TemplateId: SymbolType.TemplateId(symbolType),
            DimensionCount: new DimensionCount(dimensionCount),
            ElementCount: dimensionCount == 0
                ? ElementCount.Scalar
                : SymbolType.ToElementCount(symbolType, descriptor.Info),
            BitPosition: isScalarBool ? new BitPosition(descriptor.Info) : null);
    }

    private static string ReadZeroTerminated(ref ReadOnlySpan<byte> names)
    {
        var end = names.IndexOf((byte)0);
        if (end < 0)
        {
            throw new FormatException("A template's name run ends without its zero terminator.");
        }

        var name = Encoding.ASCII.GetString(names[..end]);
        names = names[(end + 1)..];
        return name;
    }

    private static string TrimEncodingSuffix(string templateName)
    {
        var separator = templateName.IndexOf(EncodingSuffixSeparator, StringComparison.Ordinal);
        return separator < 0 ? templateName : templateName[..separator];
    }
}
