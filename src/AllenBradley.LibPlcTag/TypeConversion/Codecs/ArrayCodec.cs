namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>
/// The wire format of a one-dimensional array of an elementary type: <paramref name="count"/> elements in
/// index order, one after the other with no padding, each in the format of <paramref name="element"/>.
/// </summary>
internal sealed class ArrayCodec<T>(ElementCodec<T> element, int count)
{
    /// <summary>How many bytes the whole array occupies on the wire.</summary>
    internal int Size => count * element.Size;

    /// <summary>Reads the array from a buffer that holds exactly <see cref="Size"/> bytes.</summary>
    /// <exception cref="ArgumentException">The buffer is not <see cref="Size"/> bytes long.</exception>
    internal T[] Decode(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length != Size)
        {
            throw new ArgumentException(
                $"{count} elements of {element.Size} bytes take {Size} bytes, but the buffer holds {buffer.Length}.",
                nameof(buffer));
        }

        var elements = new T[count];
        for (var index = 0; index < count; index++)
        {
            elements[index] = element.Decode(buffer.Slice(index * element.Size, element.Size));
        }

        return elements;
    }

    /// <summary>Returns the <see cref="Size"/> bytes that <paramref name="values"/> occupy on the wire.</summary>
    /// <exception cref="ArgumentException"><paramref name="values"/> does not hold exactly the counted elements.</exception>
    internal byte[] Encode(T[] values)
    {
        if (values.Length != count)
        {
            throw new ArgumentException(
                $"The array holds {count} elements, but {values.Length} were given.", nameof(values));
        }

        var buffer = new byte[Size];
        for (var index = 0; index < count; index++)
        {
            element.Encode(values[index], buffer.AsSpan(index * element.Size, element.Size));
        }

        return buffer;
    }
}
