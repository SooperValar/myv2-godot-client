from PIL import Image
OUT='/mnt/user-data/outputs/hud'
for f in ['00099','18099']:
    p=f'{OUT}/{f}.png'
    im=Image.open(p).convert('RGBA'); orig=im.copy(); H=im.height
    for old,new in [(25,31),(38,41)]: im.paste(orig.crop((old-1,0,old,H)),(old,0))
    for old,new in [(25,31),(38,41)]: im.paste(orig.crop((old,0,old+1,H)),(new,0))
    im.save(p)
