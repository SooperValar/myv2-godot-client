# Game data

Everything in this folder ships with the client. Source art that only the build needs lives in `art/` instead.

It is stored in Git LFS, so run `git lfs pull` after cloning. The client reads it directly at run time.

| Folder | Holds |
|---|---|
| `archives/` | `bmplib.idx/.dat` and `pnglib.idx/.dat`, compiled from `art/sprites/` |
| `overrides/` | Loose sprites drawn instead of the archived sprite with the same number |
| `ui/` | UI images |
| `sounds/` | `.wav` sound effects |
| `cursors/` | Mouse cursors |

File names are lower case so they resolve the same way on Windows and Linux.
