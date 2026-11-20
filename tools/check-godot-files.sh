#!/bin/sh
# Rejects Godot scenes (.tscn) and resources (.tres) still in hand-written form. The Godot editor
# gives each file a uid, a uid on every file reference, and a unique_id on every scene node.
# Third-party addons in game/addons/ are skipped, because they are shipped as their authors wrote them.
#
# Usage: tools/check-godot-files.sh --staged     check the staged files (the pre-commit hook)
#        tools/check-godot-files.sh --hook       check the file in a Claude Code hook's input
#        tools/check-godot-files.sh [files...]   check these files, or every tracked one

problems() {
    case "$1" in game/addons/* | */game/addons/*) return ;; esac
    case "$1" in *.tscn) scene=1 ;; *) scene=0 ;; esac
    CHECKED_FILE="$1" SCENE="$scene" awk '
        BEGIN { name = ENVIRON["CHECKED_FILE"]; scene = ENVIRON["SCENE"] == 1 }
        NR == 1 && $0 !~ /uid="uid:\/\// { print name ": the header has no uid" }
        /^\[ext_resource / && $0 !~ /uid="uid:\/\// { missing_uids++ }
        scene && /^\[node / && $0 !~ /unique_id=/ { missing_ids++ }
        END {
            if (missing_uids) print name ": " missing_uids " file references have no uid"
            if (missing_ids) print name ": " missing_ids " nodes have no unique_id"
        }'
}

report() {
    if [ -n "$1" ]; then
        printf '%s\n' "$1"
        echo "Open and save these in the Godot editor so it writes them in its own format."
        return 1
    fi
}

case "$1" in
    --staged)
        found=$(git diff --cached --name-only --diff-filter=ACMR -- '*.tscn' '*.tres' | while read -r path; do
            git show ":$path" | problems "$path"
        done)
        report "$found"
        ;;
    --hook)
        path=$(sed -n 's/.*"file_path"[[:space:]]*:[[:space:]]*"\(\([^"\\]\|\\.\)*\)".*/\1/p' | head -n 1 | tr '\\' '/' | tr -s '/')
        case "$path" in
            *.tscn | *.tres)
                [ -f "$path" ] || exit 0
                report "$(problems "$path" < "$path")" >&2 || exit 2
                ;;
        esac
        ;;
    *)
        if [ $# -eq 0 ]; then
            set -- $(git ls-files -- '*.tscn' '*.tres')
        fi
        found=$(for path in "$@"; do problems "$path" < "$path"; done)
        report "$found"
        ;;
esac
