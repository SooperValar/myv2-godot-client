#!/usr/bin/env bash
# Exports the game for one platform and packs it with Velopack into a releases folder.
# Usage: tools/package-release.sh <win|linux> <version> <channel> <notes-file> <releases-dir> <myv2-art-dir>
# GODOT overrides the Godot console executable; vpk 1.2.161 must be on PATH.
# The art folder holds MyV2's gx00.dat/.idx, pnglib.dat/.idx, GFX/ and sfx/; the game reads them beside its executable.
set -euo pipefail

if [ $# -ne 6 ]; then
    echo "usage: $0 <win|linux> <version> <channel> <notes-file> <releases-dir> <myv2-art-dir>" >&2
    exit 2
fi

platform=$1
version=$2
channel=$3
notes=$4
releases=$5
art=$6

repo=$(cd "$(dirname "$0")/.." && pwd)
app="$repo/build/app-$platform"
starter_out="$repo/build/starter-$platform"

case $platform in
    win)
        preset="Windows Desktop"
        runtime=win-x64
        game=TheLastGate.exe
        starter=TheLastGateStarter.exe
        icon="$repo/game/images/app-icon.ico"
        ;;
    linux)
        preset="Linux"
        runtime=linux-x64
        game=TheLastGate.x86_64
        starter=TheLastGateStarter
        icon="$repo/game/images/app-icon.png"
        ;;
    *)
        echo "platform must be win or linux" >&2
        exit 2
        ;;
esac

godot=${GODOT:-"$LOCALAPPDATA/Microsoft/WinGet/Packages/GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe"}

# The native AOT link step looks for vswhere, which the Visual Studio installer doesn't put on PATH.
vswhere_folder="/c/Program Files (x86)/Microsoft Visual Studio/Installer"
if [ -d "$vswhere_folder" ]; then
    PATH="$PATH:$vswhere_folder"
fi

rm -rf "$app" "$starter_out"
mkdir -p "$app" "$releases"

"$godot" --headless --path "$repo/game" --import
"$godot" --headless --path "$repo/game" --export-release "$preset" "$app/$game"
if ! ls -d "$app"/data_TheLastGate_* > /dev/null 2>&1; then
    echo "the export made no .NET assemblies; check the Godot output above" >&2
    exit 1
fi

cp -r "$repo/data" "$app/data"
for art_file in gx00.dat gx00.idx pnglib.dat pnglib.idx GFX sfx; do
    if [ ! -e "$art/$art_file" ]; then
        echo "the MyV2 art folder has no $art_file" >&2
        exit 1
    fi
    cp -r "$art/$art_file" "$app/"
done

dotnet publish "$repo/src/TheLastGate.Starter" -c Release -r "$runtime" -o "$starter_out"
cp "$starter_out/$starter" "$app/"

vpk pack \
    --packId MercenariesOfAsgardV2 \
    --packTitle "Mercenaries of Asgard v2" \
    --packVersion "$version" \
    --channel "$channel" \
    --runtime "$runtime" \
    --mainExe "$starter" \
    --packDir "$app" \
    --outputDir "$releases" \
    --releaseNotes "$notes" \
    --icon "$icon"
