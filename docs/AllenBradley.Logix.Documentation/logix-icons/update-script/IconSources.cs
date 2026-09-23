using System.Runtime.CompilerServices;

// Everything about locating icon sources: anchors the icon search root, resolves
// folder arguments to typed directories, builds pathlib-style file/folder objects,
// and finds + base64-encodes the SVG source for a declared icon.
internal static class IconSources
{
    // docs/AllenBradley.Logix.Documentation/logix-icons — this script file's directory's parent. Every script file
    // shares that folder, so [CallerFilePath] anchors correctly no matter which
    // file captures it.
    public static DirectoryInfo RootFolder([CallerFilePath] string sourcePath = "") =>
        new FileInfo(sourcePath).Directory!.Parent!;

    // Resolve a folder argument to an existing directory: an absolute or
    // CWD-relative path, or a bare sub-folder name under docs/AllenBradley.Logix.Documentation/logix-icons/. Returns
    // null when none of those exist.
    public static DirectoryInfo? ResolveFolder(this string requested, DirectoryInfo iconsRoot)
    {
        if (Directory.Exists(requested))
            return new DirectoryInfo(requested);

        var underIconsRoot = iconsRoot.SubDirectory(requested);
        return underIconsRoot.Exists ? underIconsRoot : null;
    }

    // Locate the SVG for an icon: the file name must equal "<id>.svg" (strict, no
    // prefix/fuzzy fallback). Folders are searched in order; the first one that
    // contains the file wins. Returns null when no folder has it.
    public static FileInfo? FindSvg(this IReadOnlyList<DirectoryInfo> folders, string iconId)
    {
        return folders.Select(directory => directory.FileFromSegments($"{iconId}.svg"))
            .FirstOrDefault(candidate => candidate.Exists);
    }

    // The base64 encoding of the file's raw bytes (no line wrapping).
    public static string ToBase64(this FileInfo file) =>
        Convert.ToBase64String(File.ReadAllBytes(file.FullName));

    // pathlib-style builders for typed file-system objects from a directory.
    extension(DirectoryInfo directory)
    {
        // The file at "<directory>/<segments...>" (not required to exist).
        public FileInfo FileFromSegments(params string[] segments)
        {
            string[] parts = [directory.FullName, .. segments];
            return new FileInfo(Path.Combine(parts));
        }

        // The named subdirectory of this directory (not required to exist).
        public DirectoryInfo SubDirectory(string name) =>
            new(Path.Combine(directory.FullName, name));
    }
}
