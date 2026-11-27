"""MyV2 HUD frame: equipment boxes as MyV2 uses them (Cody, Nov 27).
Back goes where TLG's Card (tarot) box was, Belt where Back was, Legs where Belt was.
Works on the frame picture of both themes (00001 light, 18001 dark); same layout in both.
Usage: python3 slots.py <in.png> <out.png>"""
import sys, random
from PIL import Image

random.seed(7)
SLOT = {  # the client's 32x32 item slots (hud_view.tscn)
    'neck': (701, 19), 'card': (778, 18), 'back': (773, 57), 'belt': (739, 74),
    'leftring': (777, 95), 'rightring': (701, 95), 'arms': (705, 57),
}
NECK_BOX = (699, 17, 734, 52)      # a plain stand-alone box, 36x36
CARD_BOX = (775, 15, 812, 52)      # TLG's dashed tarot box, 38x38
NEW_BACK_BOX = (776, 16)           # the plain box's top left there, centred on the old card slot


def lum(p):
    return (p[0] + p[1] + p[2]) / 3


def inner(name, inset=2):
    x, y = SLOT[name]
    return (x + inset, y + inset, x + 32 - inset, y + 32 - inset)


def fill_level(img, rect):
    px = img.load()
    vals = sorted(lum(px[x, y]) for x in range(rect[0], rect[2]) for y in range(rect[1], rect[3]))
    return vals[len(vals) // 4]          # the fill, below the text


def text_mask(img, rect):
    px = img.load()
    lvl = fill_level(img, rect)
    return {(x, y) for x in range(rect[0], rect[2]) for y in range(rect[1], rect[3]) if lum(px[x, y]) > lvl + 45}


def dilate(mask):
    out = set(mask)
    for x, y in mask:
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                out.add((x + dx, y + dy))
    return out


def erase(img, rect):
    """the label goes; its pixels take the colours of the fill around them"""
    px = img.load()
    hole = {p for p in dilate(text_mask(img, rect)) if rect[0] - 1 <= p[0] <= rect[2] and rect[1] - 1 <= p[1] <= rect[3]}
    while hole:
        done = []
        for x, y in hole:
            near = [(x + dx, y + dy) for dx in (-1, 0, 1) for dy in (-1, 0, 1) if (dx or dy) and (x + dx, y + dy) not in hole]
            if near:
                done.append(((x, y), px[random.choice(near)]))
        if not done:
            break
        for p, c in done:
            px[p] = c
            hole.discard(p)


def word(img, rect):
    """the label's pixels relative to its top left, and its size"""
    px = img.load()
    m = text_mask(img, rect)
    x0, y0 = min(p[0] for p in m), min(p[1] for p in m)
    x1, y1 = max(p[0] for p in m), max(p[1] for p in m)
    return {(x - x0, y - y0): px[x, y] for x, y in m}, (x1 - x0 + 1, y1 - y0 + 1), (x0, y0)


def letters(glyphs):
    """split a word into letters at empty columns: [(pixels, width)]"""
    cols = sorted({x for x, _ in glyphs})
    groups, cur = [], [cols[0]]
    for c in cols[1:]:
        if c == cur[-1] + 1:
            cur.append(c)
        else:
            groups.append(cur); cur = [c]
    groups.append(cur)
    out = []
    for g in groups:
        out.append(({(x - g[0], y): c for (x, y), c in glyphs.items() if g[0] <= x <= g[-1]}, len(g)))
    return out


def spacing(glyphs):
    cols = sorted({x for x, _ in glyphs})
    gaps = [b - a - 1 for a, b in zip(cols, cols[1:]) if b - a > 1]
    return min(gaps) if gaps else 1


def stamp(img, glyphs, size, centre_x, top):
    px = img.load()
    x0 = centre_x - size[0] // 2
    for (x, y), c in glyphs.items():
        px[x0 + x, top + y] = c


def main(src, dst):
    img = Image.open(src).convert('RGBA')
    orig = img.copy()
    back, back_size, back_at = word(orig, inner('back'))
    belt, belt_size, belt_at = word(orig, inner('belt'))
    gap = spacing(belt)
    # Legs from the picture's own letters (columns within each word; letters can touch, so by position):
    # L and e of "Left", g of "Ring", s of "Arms"; all words share the cap line, so rows line up
    lr, rr = SLOT['leftring'], SLOT['rightring']
    left = word(orig, (lr[0] + 2, lr[1] + 2, lr[0] + 30, lr[1] + 15))[0]
    ring = word(orig, (rr[0] + 2, rr[1] + 15, rr[0] + 30, rr[1] + 30))[0]
    arms = word(orig, inner('arms'))[0]
    parts = [(left, 0, 5), (left, 7, 10), (ring, 16, 20), (arms, 20, 23)]
    legs, x = {}, 0
    for glyphs, c0, c1 in parts:
        for (gx, gy), c in glyphs.items():
            if c0 <= gx <= c1:
                legs[(x + gx - c0, gy)] = c
        x += c1 - c0 + 2
    x -= 1
    legs_size = (x, max(y for _, y in legs) + 1)

    # 1. the dashed Card box goes; a plain box (the Neck box's) takes its place, with "Back" in it
    frame = orig.crop((CARD_BOX[2] + 1, CARD_BOX[1], CARD_BOX[2] + 18, CARD_BOX[3] + 1))  # plain frame right of it
    for x in (CARD_BOX[0], CARD_BOX[0] + 17, CARD_BOX[2] - 16):
        img.paste(frame, (x, CARD_BOX[1]))
    # with a 2-pixel margin: the dark theme's boxes have a second, outer border line there
    box = orig.crop((NECK_BOX[0] - 2, NECK_BOX[1] - 2, NECK_BOX[2] + 3, NECK_BOX[3] + 3))
    w, h = box.size              # the Neck box touches the Head column on its right: give it its own right and bottom edges
    bp = box.load()
    for k in range(3):
        for y in range(h):
            bp[w - 1 - k, y] = bp[k, y]
    for k in range(3):
        for x in range(w):
            bp[x, h - 1 - k] = bp[x, k]
    img.paste(box, (NEW_BACK_BOX[0] - 2, NEW_BACK_BOX[1] - 2))
    erase(img, inner('card'))
    neck_top = word(orig, inner('neck'))[2][1] - SLOT['neck'][1]
    stamp(img, back, back_size, SLOT['card'][0] + 16, SLOT['card'][1] + neck_top)

    # 2. the old Back box says Belt, 3. the old Belt box says Legs (same height as the labels they replace)
    erase(img, inner('back'))
    stamp(img, belt, belt_size, SLOT['back'][0] + 16, back_at[1])
    erase(img, inner('belt'))
    stamp(img, legs, legs_size, SLOT['belt'][0] + 16, belt_at[1])
    img.save(dst)


main(sys.argv[1], sys.argv[2])
