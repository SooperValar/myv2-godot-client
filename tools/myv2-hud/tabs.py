from PIL import Image
OUT='/mnt/user-data/outputs/hud'
TOP,BOTTOM=273,288          # rows below the panel's bottom border, down to the tab's own bottom border
OVERLAY_TAB_FROM=267        # overlay rows from the panel's bottom border down (frame y 270)

def tab(frame,tpl,x0,x1,dividers):
    def col(x): return tpl.crop((x,TOP,x+1,BOTTOM))
    for x in range(x0,x1+1): frame.paste(col(140+(x-x0)%40),(x,TOP))          # panel texture
    for i,x in enumerate(range(134,138)): frame.paste(col(x),(x0+i,TOP))     # left double border
    for i,x in enumerate(range(231,235)): frame.paste(col(x),(x1-3+i,TOP))   # right double border
    for d in dividers: frame.paste(col(185),(d,TOP))
    frame.paste(tpl.crop((140,BOTTOM-4,141,BOTTOM)).resize((x1-x0-6,4)),(x0+4,BOTTOM-4))  # bottom border under the texture
    for i,x in enumerate(range(134,138)): frame.paste(tpl.crop((x,BOTTOM-4,x+1,BOTTOM)),(x0+i,BOTTOM-4))
    for i,x in enumerate(range(231,235)): frame.paste(tpl.crop((x,BOTTOM-4,x+1,BOTTOM)),(x1-3+i,BOTTOM-4))

for fr,ov in [('18001','18099'),('00001','00099')]:
    frame=Image.open(f'{OUT}/{fr}.png').convert('RGBA'); overlay=Image.open(f'{OUT}/{ov}.png').convert('RGBA')
    tpl=frame.copy(); tpl.alpha_composite(overlay,(134,3))
    tab(frame,tpl,3,66,[])
    tab(frame,tpl,90,234,[136])
    frame.save(f'{OUT}/{fr}.png')
    cleared=overlay.copy(); px=cleared.load()
    for y in range(OVERLAY_TAB_FROM,cleared.height):
        for x in range(cleared.width): px[x,y]=(0,0,0,0)
    cleared.save(f'{OUT}/{ov}.png')
    show=frame.copy(); show.alpha_composite(cleared,(134,3))
    show.crop((0,240,250,300)).resize((1000,240),Image.NEAREST).save(f'/tmp/hud/tabs_{fr}.png')
print('ok')
