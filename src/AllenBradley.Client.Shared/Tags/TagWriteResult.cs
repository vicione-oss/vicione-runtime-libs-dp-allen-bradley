namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

/// <summary>
/// The outcome of one <see cref="Access.ITagAccess.WriteAsync"/>. A write produces no data, so this
/// is success or a reason only.
/// </summary>
internal readonly record struct TagWriteResult
{
    private TagWriteResult(string? error) => Error = error;

    /// <summary>Why the write failed; <c>null</c> when it succeeded.</summary>
    public string? Error { get; }

    public bool Succeeded => Error is null;

    public static TagWriteResult Ok() => new(null);

    public static TagWriteResult Failed(string error) => new(error);
}
