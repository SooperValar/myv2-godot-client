from PIL import Image
U='/mnt/user-data/uploads/MyV2'
OUT='/mnt/user-data/outputs/hud'
ROW=14
ROW_SRC=(88,102); BORDER_SRC=(102,106); SHIFT_START=106; SHIFT_END=330   # frame coordinates

def copy_rows(src,out,sy0,sy1,dy,xmax):
    for y in range(sy0,sy1):
        ty=y+dy
        if 0<=ty<out.height and 0<=y<src.height:
            out.paste(src.crop((0,y,xmax,y+1)),(0,ty))

def keep_cutout(src,out):
    # The frame is see-through where the world shows; only panel pixels move, never that shape.
    sa=src.getchannel('A'); res=Image.composite(out,src,sa.point(lambda a:255 if a>0 else 0))
    res.putalpha(sa); return res

def extend(src,yoff,xmax,shift_end):
    out=src.copy()
    copy_rows(src,out,SHIFT_START-yoff,shift_end-yoff,ROW,xmax)          # skill panel, update row, hp box down a row
    copy_rows(src,out,ROW_SRC[0]-yoff,ROW_SRC[1]-yoff,ROW,xmax)          # a copy of the last attribute row
    copy_rows(src,out,BORDER_SRC[0]-yoff,BORDER_SRC[1]-yoff,ROW,xmax)    # the panel's bottom border below it
    return out

def move_dividers(img,y0,y1,moves):
    orig=img.copy()
    for old,new in moves:
        img.paste(orig.crop((old-1,y0,old,y1)),(old,y0))        # the panel texture over the old divider
    for old,new in moves:
        img.paste(orig.crop((old,y0,old+1,y1)),(new,y0))        # the divider at its new place
    return img

for name in ['00001','18001']:
    src=Image.open(f'{U}/Godot-Client/data/overrides/{name}.png').convert('RGBA')
    out=keep_cutout(src,extend(src,0,250,SHIFT_END))
    out=move_dividers(out,283+ROW,329+ROW,[(113,97),(136,129)])
    out.save(f'{OUT}/{name}.png')
for num,name in [('100099','00099'),('118099','18099')]:
    src=Image.open(f'{U}/Client/Claude outputs/hud_src/{num}.png').convert('RGBA')
    extend(src,3,src.width,src.height+3).save(f'{OUT}/{name}.png')
for num,name in [('118097','18097'),('118098','18098')]:
    src=Image.open(f'{U}/Client/Claude outputs/hud_src/{num}.png').convert('RGBA')
    out=src.copy()
    copy_rows(src,out,ROW_SRC[0]-3,ROW_SRC[1]-3,ROW,src.width)
    copy_rows(src,out,BORDER_SRC[0]-3,BORDER_SRC[1]-3,ROW,src.width)
    out.save(f'{OUT}/{name}.png')
# preview: frame left column before / after, both themes, 2x
pv=Image.new('RGBA',(4*260*2+30,360*2),(30,30,30,255))
x=0
for name in ['18001','00001']:
    b=Image.open(f'{U}/Godot-Client/data/overrides/{name}.png').convert('RGBA').crop((0,0,260,360)).resize((520,720),Image.NEAREST)
    a=Image.open(f'{OUT}/{name}.png').crop((0,0,260,360)).resize((520,720),Image.NEAREST)
    pv.paste(b,(x,0)); pv.paste(a,(x+525,0)); x+=1060
pv.save('/tmp/hud/preview.png'); print('ok')
