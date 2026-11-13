# Source art

Source files the build turns into game data. Nothing in this folder ships with the client.

It is stored in Git LFS, so run `git lfs pull` after cloning.

| Folder | Holds | Built into |
|---|---|---|
| `sprites/` | Sprites `NNNNN.bmp` (24-bit) and `NNNNN.png` | `data/archives/bmplib` and `data/archives/pnglib` |

File names are lower case so they resolve the same way on Windows and Linux.
