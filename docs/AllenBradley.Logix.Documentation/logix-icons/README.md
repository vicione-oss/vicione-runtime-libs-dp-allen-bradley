# Logix Tree-Editor Icons

The SVG sources for the icons the DataPort tree editor shows for the Logix port, and the script that
embeds them in `src/AllenBradley.Logix/allen-bradley-logix.yaml`. The YAML carries each icon as base64
in its `Icons:` section. The files here are what you edit; the `Content:` lines are generated.

The setup is the one the S7 repo uses for its icons, with the same script, so a developer who moves
between the two repos finds the same workflow.

## Folder structure

| Folder / file        | Purpose                                                                                     |
|----------------------|---------------------------------------------------------------------------------------------|
| `dataport-concepts/` | One SVG per icon ID. The file name must be exactly `<id>.svg`.                               |
| `update-script/`     | The sync script: `UpdateLogixIcons.cs`, a .NET 10 file-based app, plus its included helpers. |
| `preview.html`       | Every icon at 16, 20, 24, 32 and 64 px on a light and a dark background. Open it in a browser. |
| `.gitattributes`     | Keeps the SVGs at LF on every checkout. See [Line endings](#line-endings).                  |

## Registered icon IDs

Below the root, each icon follows the shape Studio 5000 Logix Designer uses for the same thing in its
Controller Organizer, redrawn as a single-colour glyph. Rockwell lists those icons on the
[Controller Organizer icons](https://www.rockwellautomation.com/en-nz/docs/studio-5000-logix-designer/37-00/contents-ditamap/studio-5000-logix-designer/controller-organizer/source-control-status-in-the-controller-organizer/controller-organizer-icons.html)
help page. The root carries the brand instead, as the S7 root does.

| Icon ID               | Used by                                              | Shape it follows                                                          |
|-----------------------|------------------------------------------------------|---------------------------------------------------------------------------|
| `allen-bradley-logix` | `Root`                                               | The Allen-Bradley logo: the octagon badge with `A-B` cut out and the band below it, left solid because `QUALITY` cannot be drawn at 16 px |
| `controllogix`        | `DeviceControlLogix5X70`, `DeviceControlLogix5X80`   | Backplane and module: a chassis holding a controller (display window at the top) and two I/O modules |
| `compactlogix`        | `DeviceCompactLogix5X70`, `DeviceCompactLogix5X80`   | Module: the controller and two I/O modules clipped together, with the DIN rail showing at both ends |
| `controller-tags`     | `ControllerTags5X70`, `ControllerTags5X80`           | Tags folder: a price tag at 45°, point to the upper right                 |
| `program-tags`        | `ProgramTags5X70`, `ProgramTags5X80`                 | Program: three squares on a vertical line, with a loop from the second to the third |
| `udt`                 | `Udt5X70`, `Udt5X80`                                 | Data type: the digits `101` over `010`                                    |
| `array-container`     | The ten `…ArrayContainer` nodes                      | None. Square brackets around three elements, after the `[i]` in the node names |

The badge says Allen-Bradley, not Logix. A Legacy port would get the same badge, and telling the two
roots apart is left until that port exists. The device icons tell the two
[form factors](../../AllenBradley.Documentation/controllers/controller-families.md#two-form-factors-not-four-generations)
apart: a ControlLogix sits in a chassis, a CompactLogix clips onto a DIN rail. The generation is not
drawn. It is in the node name, and a 5X70 and a 5X80 of one family would differ by a detail too small
to read at 16 px.

Studio 5000 has no organizer icon for an array; its arrays live in the tag editor. The array-container
glyph is ours.

`datapoint` is the tree editor's built-in icon, used by every tag node, including the whole-array
nodes such as `Bool[]`. It is not declared in `Icons:`, so there is nothing here to sync.

## Drawing rules

The icons must stay readable at 16 px. Every source obeys the same rules:

- `viewBox="0 0 32 32"`, the size the S7 icons use.
- A margin of 2 units on every side. Nothing is drawn outside the live area from 2 to 30.
- Size a shape by how big it looks, not by its bounding box. The brackets and the `udt` digits fill
  24×24, the chassis 28×24, and the badge 28×28, because its round corners are empty. The solid tag
  looks bigger than its box and gets 22×22. It sits one unit left of and below the centre, so that
  its mass, not its box, is centred. Compare a new icon with the others in `preview.html`.
- Drawn on a 16-px grid. Every horizontal and vertical edge lies on an even coordinate, so it lands
  on a whole pixel at 16 px and renders without blur. A slanted or curved edge is anti-aliased at any
  size, so its end points may fall between grid lines.
- No line thinner than 2 units. That is one pixel at 16 px; a 1-unit line is half a pixel and turns
  into a grey smear.
- Filled paths only, with `fill="currentColor"` and no stroke, so the editor can colour the icon for
  its theme.
- A 45° edge renders cleanly. Use a curve only where the shape needs one, such as the hole in the tag
  or the rounded corners of the badge.

The badge is the one icon traced from an original rather than drawn from scratch. It keeps the
proportions of the
[Allen-Bradley logo](https://commons.wikimedia.org/wiki/File:Allen-Bradley_logo.svg): a regular
octagon with rounded corners, a thin-and-thick `A` with a flat top, and a `B` whose lower bowl is the
larger. Its straight edges were then moved onto the grid, and the hyphen and the gap above the band
widened to one pixel, because at their true size they vanish at 16 px.

Check a new or changed icon in `preview.html` at 16 px before you run the script.

## Updating the icons

Run the script from the repo root:

```bash
dotnet run docs/AllenBradley.Logix.Documentation/logix-icons/update-script/UpdateLogixIcons.cs
```

The YAML is the source of truth. For every icon ID declared in its `Icons:` section, the script looks
for `<id>.svg` in `dataport-concepts/`, base64-encodes the file's bytes and rewrites that icon's
`Content:` line and nothing else. It rewrites every declared icon on each run; identical content gives
no diff. At the end it lists any declared icon that has no SVG.

To search other folders, pass them as arguments, first match wins. Each argument is an absolute path,
a path relative to the current directory, or a folder name under `logix-icons/`.

### Adding a new icon

1. Add an entry to the `Icons:` section of `allen-bradley-logix.yaml`:

   ```yaml
     - Id: <id>
       Content: ""
       ContentType: Base64
   ```

2. Set `Icon: <id>` on the nodes that should show it.
3. Draw `dataport-concepts/<id>.svg` to the rules above, and add the ID to the `icons` list in
   `preview.html`.
4. Run the script.

## Line endings

The base64 in the YAML encodes the file's bytes, line endings included. With `core.autocrlf=true`, a
Windows checkout would turn the SVGs into CRLF and a Linux checkout would not, and the same icon would
encode differently on each. The `.gitattributes` here pins the SVGs to LF, so the script writes the
same `Content:` on every machine.
