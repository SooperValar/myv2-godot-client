# CLAUDE.md

This file guides Claude Code when working in this repository.

## End goal

A new **Godot 4 .NET (C#)** client for **The Last Gate**, an MMORPG built on a modified Mercenaries of Astonia v2 engine. It connects to the **existing, unchanged server**; the server is not being rewritten.

Stages:
1. **Test project (done):** prove the risky parts work (zlib stream, sprite archives, isometric drawing).
2. **MVP (done):** log in, walk, fight, chat, inventory and equipment, stats, sound.
3. **Feature parity (current stage)** with the current client: shop, depot, skill tree, skill planner, item tooltips, buff watch, and the other client mods. Everything from the earlier Godot attempt (tag `old-main`) has been carried over. Only after parity will current players be asked to switch.

Pixel accuracy matters. The world view should match the current client at the 1280×720 virtual resolution.

## Repository layout

| Folder | Holds |
|---|---|
| `game/` | The Godot project: `project.godot`, `TheLastGate.csproj` and Godot's own `TheLastGate.sln`. Godot can't see anything outside this folder. |
| `src/TheLastGate.Core/` | Code with no Godot dependency (file formats, protocol, caching, updates), shared by the game, tools and tests |
| `src/TheLastGate.Starter/` | The small executable Velopack installs as the app's entry point. It answers Velopack's install and update hooks without starting Godot, then starts the game. Ships with every build. |
| `tools/SpriteCompiler/` | Console tool that builds the sprite archives. Never referenced by the game. |
| `tests/TheLastGate.Tests/` | xUnit tests for Core and the tools; they run without the engine |
| `art/` | Source art the build turns into game data. Not shipped. |
| `data/` | Game data the client reads at run time. Everything here ships. |

The root `TheLastGate.sln` holds every project and is the one to open in Rider. Godot builds `game/TheLastGate.sln`, which only lists the game, so failing tests never stop the game from running.

## Reference code (read-only from here)

**Current client (C/C++, SDL2 + OpenGL + ImGui):** `C:\Code\thelastgate-sdl\Source\Client`

This is the source of truth for the protocol, file formats and drawing. Where to look:

| Topic | File(s) |
|---|---|
| Login handshake, `xcrypt`, start of the zlib stream | `src/net/connection.c` |
| Tick framing, zlib inflate, command dispatch (`sv_cmd`), every `sv_*` handler | `src/net/socket.c` |
| Packet IDs (`SV_*`, `CL_*`), map tile struct (`struct cmap`), player struct (`struct cplayer`) | `common.h` |
| `NETWORKING_VERSION`, `RENDERDIST`, `MAXSPRITE` | `inter.h` |
| Sprite archives (`pnglib`, `gx00`), magenta colour key | `src/graphics/loader.c` |
| Isometric projection (`sdl_copysprite`) | `src/graphics/sdl.c` |
| World draw order and layers (`eng_display`), character/item animation (`eng_char`, `eng_item`, `speedo`) | `engine.c` |
| Old UI hit-testing, client commands sent to the server (`cmd*`) | `inter.c` |
| Planner, combat and buff models (pure logic, with tests) | `src/planner/`, `src/mods/`, `tests/` |

**Where the game data came from:** the SDL repo's `Resources\Packaged Files\` and `Resources\Gfx Compiler\gfx\`. They were copied once into `art/` and `data/` and are now reference only.

**Server source:** the live server is `C:\Code\TheLastGateReal`. `C:\Code\thelastgate-sdl\Source\Server` is out of date, so don't rely on it. Use the server only to answer protocol questions the client source can't.

**Related plans in the C client repo (local only):** `C:\Code\thelastgate-sdl\docs\superpowers\plans\`. These include the C → C++ migration plan, whose Phase 4 (moving the old UI to ImGui) is waiting on how this Godot work turns out.

## Game data and art (in Git LFS)

Everything under `art/` and `data/` is stored in Git LFS, so run `git lfs pull` after cloning. Each folder has a README listing what it holds. File names are lower case.

- `art/sprites/` holds the source sprites `NNNNN.bmp` and `NNNNN.png`. They are never shipped.
- `data/archives/` holds `bmplib` (built from the BMPs, stored as RGB565) and `pnglib` (the PNGs, stored unchanged).

**Adding or changing art:**
1. Put the file in `art/sprites/`.
2. Run `dotnet run --project tools/SpriteCompiler` and read its report. It lists any file it skipped and why.
3. Commit the source and the rebuilt archives together.

`ArchiveEquivalenceTests` fails if the archives don't match a fresh compile.

While the SDL client is still live, new art is also added to the SDL repo by hand. Nothing here writes to that repo.

## Build and run

```powershell
dotnet build TheLastGate.sln
dotnet test tests/TheLastGate.Tests
sh tools/check-code-cleanup.sh
sh tools/check-godot-files.sh
dotnet run --project tools/SpriteCompiler
powershell -ExecutionPolicy Bypass -File tools/view-sprites.ps1
godot --path game --editor
godot --path game -- --start-as=<character name>
bash tools/package-release.sh <win|linux> <version> <channel> <notes-file> <releases-dir>
```
- `--start-as` presses Start in the launcher only if the character it has loaded has that name, so a check in game can begin without clicking through.
- `tools/package-release.sh` exports the game into `build/`, adds `data/` and the Starter, and packs it with Velopack (`vpk` 1.2.161 must be on PATH). Channels are `win` and `linux` for releases, and `test-win` and `test-linux` for test builds, which real clients never see.
- Every Release Please release is packaged by `.github/workflows/package.yml` for Windows and Linux and attached to that release. To make a test release, push a tag `updater-test-v<version>`. The version must be higher than any earlier test release and never used before, because GitHub won't recreate a tag that belonged to an immutable release (0.1.0 to 0.1.5 are used up). It builds on the `test-win` and `test-linux` channels as a prerelease, and test builds find the newest published one through GitHub's API. Releases are immutable once published, so every release stays a draft until CI has attached its files. Delete test releases afterwards with `gh release delete <tag> -y`.
- PowerShell scripts in `tools/` run with `-ExecutionPolicy Bypass`, because this machine blocks scripts by default. It applies to that one run only.
- Godot version: 4.7.2-stable .NET (mono), installed through winget. It isn't on PATH as `godot`. The console executable is `%LOCALAPPDATA%\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe`
- .NET SDK version: 10.0.403. Every project targets .NET 8.
- Claude may build and run tests without asking. Run only the tests that cover the changed code (`--filter`), and the full suite at the end of a finished chain of work.
- Connecting to the live game server is allowed. The host and proxy addresses are `MHOST` / `MPROXY` in `C:\Code\thelastgate-sdl\Source\Client\common.h` (the `#ifdef` there chooses between two sets), on port 5555 (`host_port` in `src/net/connection.c`). Keep test sessions short, and don't send commands that change a real character's state (giving, dropping, trading or deleting items, raising stats) unless the user asks.
- Passwords are never stored, in files, settings or source. The player types theirs in the launcher each time, for the character loaded from its `.moa` file.
- After cloning, run `git config core.hooksPath .githooks` once. The pre-commit hook runs two checks on what is staged:
  - `tools/check-godot-files.sh` rejects a `.tscn` or `.tres` without a `uid` in its header and on every `ext_resource`, or with a node that has no `unique_id`. A Claude Code hook runs the same check whenever Claude writes one.
  - `tools/check-code-cleanup.sh` rejects C# that JetBrains' Full Cleanup would still change, using the settings in `.editorconfig`. It leaves its changes in the working tree to review and stage.
- A hand-written scene is fine as long as it passes that check. Never commit an editor save of `game/Hud/hud_view.tscn`: the editor bakes the HUD's bitmap fonts into it, so its nodes are written by hand. The editor also reorders `game/themes/OptionsTheme.tres` when it saves; leave that out of a commit unless the theme was changed on purpose.
- The editor sometimes saves `game/Hud/HudView.cs` with tab indents. Running `sh tools/check-code-cleanup.sh` turns them back into spaces.
- `game/TheLastGate.csproj` and `game/TheLastGate.sln` are generated by the Godot editor. Don't write or change the `Godot.NET.Sdk` version by hand.
- Dependencies must not need a licence key, even a free one.

## Architecture guidelines

- **Keep protocol and file-format code free of Godot types** by putting it in `TheLastGate.Core`. Plain C# over `ReadOnlySpan<byte>` / `BinaryPrimitives` (little-endian) can be tested with `dotnet test`, without the engine.
- **The game references Core, never the tools.** Code only the build needs (the sprite compiler and its image decoding) stays in `tools/`, so it never ships to players.
- **All packet reads go through a bounds-checked `PacketReader`.** Every command type needs an exact length, because one wrong length loses the client's place in the tick stream.
- **One inflater for the whole session,** started after login succeeds. Decompressing each frame on its own fails from the second frame on.
- Sprites load on first use into a size-limited cache. Never import the archives as Godot resources.
- Networking runs off the main thread. The game state is updated once per tick on the main thread.
- The main screen recreates the current client's sprite HUD. Overlay windows (options, world map, planner) are styled Godot Controls. Layout lives in scenes and containers, not in computed coordinates, and `[Tool]` scripts give runtime content a preview in the editor.
- Scenes sit beside their scripts, named in snake_case (`game/Planner/planner_window.tscn` beside `PlannerWindowView.cs`).
- Text is drawn in a swappable font set, never a hard-wired font. Everything in game uses the HUD's `IFontSet`, which `WindowFonts` puts on every window under the HUD. The launcher has its own `ILauncherFontSet`.
- Text inputs are plain `LineEdit` and `TextEdit` nodes, so `TextFields` can keep hotkeys and chat keys away from a field while it is being typed in. Numbers are typed, not picked from drop-downs.
- When a behaviour detail is unclear (a limit, a timing, a volume), follow the current SDL client.

## Code style

`.editorconfig` holds the rules and `Directory.Build.props` makes the build enforce them, so a violation fails `dotnet build`.

- C# with the usual .NET conventions and nullable reference types enabled. Naming is an error when broken: PascalCase for types, members, constants and `static readonly` fields; camelCase for locals, parameters and private fields.
- **Namespaces follow folders**, starting from the project's root namespace (`game/Config/` is `TheLastGate.Config`). This is an error too, so the game keeps its code directly under `game/`, not in a `src/` folder.
- **`var` wherever it compiles** without changing the type. This is an error too. Keep an explicit type only for a declaration with no initializer, a collection expression, or `stackalloc`.
- **Descriptive names**, at least 3 letters. No `i`, `n`, `buf`, `out`, `tmp`. For example `tileIndex`, `payloadBytes`, `spriteNumber`.
- No warning suppressions (`#pragma warning disable`, `[SuppressMessage]`). Fix the warning, or leave it visible.
- Every `switch` over an enum ends with a default that throws `UnreachableException`.
- Pull constants out with names when the number has meaning (sprite numbers, packet sizes, layout offsets), not for every literal.

## Comments

Code should explain itself through its names, small methods and named constants. A comment is the last resort, used only for what the code can't say. The default is no comment. If something seems to need one, first try a clearer name or a clearer expression, for example `recencyOrder.Count > 1` instead of a comment saying the newest entry is never evicted.

**Write a comment only when the reason isn't visible in the code and can't be made visible:**
- a protocol or file-format quirk (`// pnglib has no length field, so each PNG runs to the next stored offset`)
- a workaround, or a trade-off a reviewer would otherwise question
- a rule that has to match something outside the code (`// 0x1F widens to 0xFF, not 0xF8, matching how SDL expands 565`)

**Don't write a comment that:**
- says what the next line does (`// Read the index`, `// Loop over the sprites`)
- repeats a name (`// The sprite cache` above `class SpriteCache`, `/// <summary>Gets the width</summary>`)
- summarises what a class or method is for, or the design around it (`// The client's own copy of the current character. Loading copies it here, saving copies it out` above `CharacterStore`). If a class needs that explained, its name or its methods should say it.
- explains a choice the code already shows (`// Paths are compared ignoring case` above code using `OrdinalIgnoreCase`)
- describes parameters or return values that the names and types already make clear
- cites where code came from ("ported from socket.c:539"). Pointers to reference files belong in this file.
- keeps old code commented out. Delete it; Git has it.

When code changes, update or delete its comments in the same change. A stale comment is worse than none.

## Working style

- Work through a numbered step in pieces: first the tests with stubs that throw `NotImplementedException`, so every commit builds; then the Core code that passes them; then the view. Snapshot each piece as it is done, and commit the pieces in order once the user has checked the result, in game when there is something to see.
- Work on a branch, use Conventional Commits, and squash-merge into `main` with player-facing changelog lines, only when the user clearly says to merge. The `committing-and-merging` skill (`.claude/skills/`) has the details. Release Please (`.github/workflows/release-please.yml`) builds the changelog from `main` and keeps a release PR open. Merging that PR tags the release and raises the version in `Directory.Build.props`; merge it only when the user says to release.
- Plans and specs from superpowers skills (`docs/superpowers/`, `.superpowers/`) stay local and are never committed. They are listed in `.gitignore`.
