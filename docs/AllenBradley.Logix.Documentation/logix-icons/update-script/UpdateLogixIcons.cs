#!/usr/bin/env dotnet
#:include IconSources.cs
#:include YamlIcons.cs

// Embed SVG icons (base64) into the YAML adapter descriptors.
//
// .NET 10 file-based app — run with:
//   dotnet run docs/AllenBradley.Logix.Documentation/logix-icons/update-script/UpdateLogixIcons.cs [folder ...]
//
// The YAML descriptor is the source of truth: for every icon declared in the
// "Icons:" section of allen-bradley-logix.yaml the script finds a matching SVG
// across the search folders and writes its base64 encoding into the icon's
// Content: line. Only that single line is rewritten, so the diff stays minimal
// and line endings are preserved.
//
// Helpers live in the included files:
//   IconSources.cs — anchor/resolve icon folders, build paths, find & encode SVGs
//   YamlIcons.cs   — read declared icon IDs and patch Content: lines

var iconsRoot = IconSources.RootFolder();
// iconsRoot is docs/AllenBradley.Logix.Documentation/logix-icons, so the repo root is
// three directories up: logix-icons -> AllenBradley.Logix.Documentation -> docs -> repo root.
var repoRoot = iconsRoot.Parent!.Parent!.Parent!;

FileInfo[] yamlFiles =
[
    repoRoot.FileFromSegments("src", "AllenBradley.Logix", "allen-bradley-logix.yaml")
];

string[] defaultFolderNames = ["dataport-concepts"];

// Positional args are the folders to search; none means use the defaults.
var requestedFolderNames = args.Length > 0 ? args : defaultFolderNames;

// Resolve each requested folder to an existing directory.
var iconFolders = new List<DirectoryInfo>();
foreach (var requestedFolderName in requestedFolderNames)
{
    var folder = requestedFolderName.ResolveFolder(iconsRoot);
    if (folder is null)
    {
        Console.Error.WriteLine($"Error: icons directory not found: {requestedFolderName}");
        return 1;
    }

    iconFolders.Add(folder);
}

var existingYamlFiles = yamlFiles.Where(file => file.Exists).ToList();

var updatedIcons = 0;
var missingIconFiles = 0;

// Output is grouped per descriptor: the file name is the group heading and the
// icon lines below it omit it.
foreach (var descriptor in existingYamlFiles)
{
    var iconIds = descriptor.DeclaredIconIds();
    if (iconIds.Count == 0)
        continue;

    Console.WriteLine(descriptor.Name);

    foreach (var iconId in iconIds)
    {
        var svg = iconFolders.FindSvg(iconId);
        if (svg is null)
        {
            Console.WriteLine($"  ✗ {iconId} — no source file found");
            missingIconFiles++;
            continue;
        }

        descriptor.UpdateIconContent(iconId, svg.ToBase64());
        Console.WriteLine($"  ✓ {iconId} ← {svg.Directory!.Name}/{svg.Name}");
        updatedIcons++;
    }
}

Console.WriteLine();
if (updatedIcons == 0 && missingIconFiles == 0)
{
    Console.WriteLine("No icons declared — nothing to do.");
}
else
{
    var summary = $"Done — updated {updatedIcons} icon(s).";
    if (missingIconFiles > 0)
        summary += $" {missingIconFiles} missing source(s).";
    Console.WriteLine(summary);
}

return 0;
