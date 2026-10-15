"""Compone png/lamina-arte.png con todas las piezas, para revisarlas de un vistazo."""
import os
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
PNG = os.path.join(HERE, "png")
FONT = "/System/Library/Fonts/Supplemental/Arial.ttf"
font = ImageFont.truetype(FONT, 22) if os.path.exists(FONT) else ImageFont.load_default()
title = ImageFont.truetype(FONT, 30) if os.path.exists(FONT) else font
TINTS = {"menta": (168, 213, 186), "lavanda": (201, 194, 236), "mantequilla": (247, 233, 176), "cielo": (191, 224, 236)}


def tint(img, rgb):
    """Lo mismo que hará SpriteBatch.Draw con un Color: multiplicar cada canal."""
    r, g, b, a = img.split()
    r = r.point(lambda v: v * rgb[0] // 255)
    g = g.point(lambda v: v * rgb[1] // 255)
    b = b.point(lambda v: v * rgb[2] // 255)
    return Image.merge("RGBA", (r, g, b, a))


rows = [
    ("Flores · un nivel, dos, tres", ["flor-nivel-1", "flor-nivel-2", "flor-nivel-3"]),
    ("Oruga en gris (dos cuadros) y teñida con los cuatro colores", ["oruga-gris-1", "oruga-gris-2"] + [f"tint:{k}" for k in TINTS]),
    ("Mariposa · tres cuadros de aleteo", ["mariposa-1", "mariposa-2", "mariposa-3", "sombra"]),
    ("Cartas de acción 1–8 (sin texto) y reverso", [f"carta-{n}" for n in range(1, 9)] + ["carta-reverso"]),
    ("Baldosas e iconos", ["baldosa-cesped", "baldosa-tierra", "icono-pa", "icono-construccion", "icono-oruga-gris"]),
]
W, pad, gap = 1500, 40, 28
cells = []
y = pad
for label, names in rows:
    imgs = []
    for n in names:
        if n.startswith("tint:"):
            imgs.append(tint(Image.open(os.path.join(PNG, "oruga-gris-1.png")).convert("RGBA"), TINTS[n[5:]]))
        else:
            im = Image.open(os.path.join(PNG, n + ".png")).convert("RGBA")
            if n.startswith(("carta", "icono", "oruga", "mariposa", "sombra")):
                im = im.resize((im.width * 2, im.height * 2), Image.LANCZOS) if n.startswith(("icono",)) else im
            imgs.append(im)
    h = max(i.height for i in imgs)
    cells.append((y, label, imgs, h))
    y += 44 + h + gap
sheet = Image.new("RGBA", (W, y + pad), (255, 255, 255, 255))
d = ImageDraw.Draw(sheet)
for y0, label, imgs, h in cells:
    d.text((pad, y0), label, fill=(55, 71, 60), font=font)
    x = pad
    for im in imgs:
        sheet.alpha_composite(im, (x, y0 + 40 + (h - im.height)))
        x += im.width + gap
sheet.convert("RGB").save(os.path.join(PNG, "lamina-arte.png"))
print("lamina-arte.png", sheet.size)
