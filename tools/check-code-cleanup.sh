#!/bin/sh
# Rejects C# that JetBrains' Full Cleanup would still change, using the settings in .editorconfig.
# Cleanup rewrites the files it checks, so the changes it wants are left in the working tree to
# review and stage.
#
# Usage: tools/check-code-cleanup.sh --staged     check the staged files (the pre-commit hook)
#        tools/check-code-cleanup.sh [files...]   check these files, or every tracked one

cd "$(git rev-parse --show-toplevel)" || exit 1

case "$1" in
    --staged)
        files=$(git diff --cached --name-only --diff-filter=ACMR -- '*.cs')
        partly_staged=$(printf '%s\n' "$files" | while read -r path; do
            [ -n "$path" ] && ! git diff --quiet -- "$path" && echo "$path"
        done)
        if [ -n "$partly_staged" ]; then
            printf '%s\n' "$partly_staged"
            echo "These have unstaged changes too. Stage or stash them so the cleanup check sees what you're committing."
            exit 1
        fi
        ;;
    "") files=$(git ls-files -- '*.cs') ;;
    *) files=$(printf '%s\n' "$@") ;;
esac

[ -z "$files" ] && exit 0

if ! dotnet jb cleanupcode --help > /dev/null 2>&1; then
    echo "The JetBrains cleanup tool isn't installed. Run: dotnet tool restore"
    exit 1
fi

before=$(printf '%s\n' "$files" | while read -r path; do echo "$(git hash-object -- "$path") $path"; done)
include=$(printf '%s\n' "$files" | paste -sd ';' -)
dotnet jb cleanupcode TheLastGate.sln --profile="Built-in: Full Cleanup" --include="$include" --verbosity=ERROR > /dev/null
case $? in
    0) ;;
    # Cleanup builds the solution first and stops if it doesn't compile. Without a build it would
    # strip usings of namespaces that a test-first commit's code doesn't define yet.
    4)
        echo "The solution doesn't compile, so the JetBrains cleanup check was skipped."
        exit 0
        ;;
    *)
        echo "JetBrains cleanup failed. Run it with --verbosity=WARN to see why."
        exit 1
        ;;
esac

changed=$(printf '%s\n' "$before" | while read -r hash path; do
    [ "$(git hash-object -- "$path")" = "$hash" ] || echo "$path"
done)
if [ -n "$changed" ]; then
    printf '%s\n' "$changed"
    echo "JetBrains cleanup changed these. Review the changes, stage them and commit again."
    exit 1
fi
