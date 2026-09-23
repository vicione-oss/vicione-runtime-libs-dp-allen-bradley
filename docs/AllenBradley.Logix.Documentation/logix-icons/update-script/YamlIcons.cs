using System.Text;

// Reads the icon IDs declared in a descriptor's "Icons:" section and rewrites
// individual icons' Content: lines. All matching is scoped to that section so it
// never collides with node definitions elsewhere in the file.
internal static class YamlIcons
{
    private const string IconsSectionKey = "Icons:";
    private const string IdLinePrefix = "  - Id: "; // icon entries are indented two spaces
    private const string ContentPrefix = "    Content: "; // the Content line is indented four spaces

    // UTF-8 without a BOM, matching the on-disk descriptor encoding on read & write.
    private static readonly UTF8Encoding s_utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);


    extension(FileInfo yamlFile)
    {
        // Return the icon IDs declared in the "Icons:" section, in order.
        public IReadOnlyList<string> DeclaredIconIds() =>
            File.ReadLines(yamlFile.FullName, s_utf8NoBom)
                .SkipWhile(line => line != IconsSectionKey) // before the Icons: block
                .Skip(1) // the Icons: line itself
                .TakeWhile(line => !IsTopLevelKey(line)) // until the next top-level key
                .Where(line => line.StartsWith(IdLinePrefix, StringComparison.Ordinal))
                .Select(line => line[IdLinePrefix.Length..])
                .ToList();

        // Rewrite the Content line of an icon within the "Icons:" section. The Content
        // is always rewritten (no change detection). Line endings are preserved, so
        // identical content yields no git diff.
        public void UpdateIconContent(string iconId, string base64EncodedSvg)
        {
            // Read as text and split keeping each line's terminator, so CRLF/LF survive.
            var lines = SplitKeepingLineEndings(File.ReadAllText(yamlFile.FullName, s_utf8NoBom));
            var targetIdLine = IdLinePrefix + iconId;

            var inIconsSection = false;
            var inTargetIcon = false;

            for (var index = 0; index < lines.Count; index++)
            {
                var content = lines[index].TrimEnd('\r', '\n');

                if (content == IconsSectionKey)
                {
                    inIconsSection = true;
                    continue;
                }

                if (inIconsSection && IsTopLevelKey(content))
                    inIconsSection = false; // left the Icons: block at the next top-level key

                if (!inIconsSection)
                    continue;

                if (content == targetIdLine)
                {
                    inTargetIcon = true;
                    continue;
                }

                if (!inTargetIcon || !content.StartsWith(ContentPrefix, StringComparison.Ordinal))
                    continue;

                var lineEnding = lines[index][content.Length..]; // preserve this line's terminator
                lines[index] = ContentPrefix + base64EncodedSvg + lineEnding;
                inTargetIcon = false;
            }

            File.WriteAllText(yamlFile.FullName, string.Concat(lines), s_utf8NoBom);
        }
    }
    
    // A non-indented, non-empty line is the next top-level YAML key, i.e. the end
    // of the Icons: block.
    private static bool IsTopLevelKey(string line) =>
        line.Length > 0 && !char.IsWhiteSpace(line[0]);

    // Split text into lines, keeping each line's trailing newline (CRLF or LF). A
    // final line without a trailing newline is kept as-is, so re-joining the lines
    // reproduces the original text byte-for-byte.
    private static List<string> SplitKeepingLineEndings(string text)
    {
        var lines = new List<string>();
        var lineStart = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n')
                continue;

            lines.Add(text.Substring(lineStart, i - lineStart + 1));
            lineStart = i + 1;
        }

        if (lineStart < text.Length)
            lines.Add(text[lineStart..]);

        return lines;
    }
}
