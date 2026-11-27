"""MyV2 HUD frame: a Help button in the option grid's first blank cell, under Hide (Cody, Nov 27).
The Hide box copied one row down, its label replaced by "Help" made of the frame's own letters
(H and e of "Hide", l of "Slow", p of "Options"). Same layout in both themes (00001, 18001).
Usage: python3 helpbtn.py <in.png> <out.png> [--dark]   (needs slots.py beside it; --dark for 18001)"""
import sys, os
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import importlib.util
spec = importlib.util.spec_from_file_location('slotlib', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'slots.py'))
src = open(spec.origin).read().replace('main(sys.argv[1], sys.argv[2])', '')
lib = {}
exec(src, lib)

GRID = (1083, 666)          # the option buttons (hud_view.tscn OptionButtons), 4 x 3 cells of 48 x 15
CELL_W, CELL_H = 48, 15


def cell(col, row, inset=2):
    x, y = GRID[0] + col * CELL_W, GRID[1] + row * CELL_H
    return (x + inset, y + inset, x + CELL_W - inset, y + CELL_H - 1)


def main(src_path, dst, dark):
    img = Image.open(src_path).convert('RGBA')
    orig = img.copy()
    box = orig.crop((1130, 680, 1181, 698))                     # the Hide box with its borders
    img.paste(box, (1130, 680 + CELL_H))
    lib['erase'](img, cell(1, 2))
    hide = lib['word'](orig, cell(1, 1))[0]
    slow = lib['word'](orig, cell(2, 0))[0]
    opts = lib['word'](orig, cell(0, 2))[0]
    parts = [(hide, 0, 6), (hide, 16, 19), (slow, 5, 7), (opts, 8, 12)]
    word, x = {}, 0
    for glyphs, c0, c1 in parts:
        for (gx, gy), c in glyphs.items():
            if c0 <= gx <= c1:
                word[(x + gx - c0, gy)] = c
        x += c1 - c0 + 2
    size = (x - 1, max(y for _, y in word) + 1)
    top = lib['word'](orig, cell(1, 1))[2][1] + CELL_H            # the same height as Hide's label
    lib['stamp'](img, word, size, GRID[0] + CELL_W + CELL_W // 2, top)
    if dark:                    # the dark theme's outline around the grid: a piece beside Help, and its bottom closed
        px = img.load()
        for y in range(697, 713):
            px[1181, y] = px[1081, y]
        px[1131, 712] = px[1128, 712]
        px[1132, 712] = px[1127, 712]
        px[1130, 711] = px[1126, 711]
    img.save(dst)


main(sys.argv[1], sys.argv[2], '--dark' in sys.argv)
