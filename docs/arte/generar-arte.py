"""Genera el arte provisional de la partida de Torres.

Cada pieza se escribe como SVG en svg/ y se rasteriza a PNG al doble de tamaño en png/
con rsvg-convert. El estilo sale del prototipo (Prototipo-Pantallas-Torres.html): mismas
flores con un par de hojas por nivel, mismas orugas y la misma mariposa.

Uso: python3 generar-arte.py   (requiere rsvg-convert en el PATH)
"""
import math
import os
import shutil
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
SVG_DIR = os.path.join(HERE, "svg")
PNG_DIR = os.path.join(HERE, "png")
SCALE = 2

# Paleta del tema del cliente (Ui/Theme.cs) y del prototipo
INK = "#37473C"
LEAF, LEAF_LIGHT, STEM = "#A8D5BA", "#C7E3D1", "#6FAF8B"
PETAL, PETAL_LINE = "#F5C9C6", "#D89A96"
HEART, HEART_LINE = "#F7E9B0", "#D9C271"
GRASS_A, GRASS_B, GRASS_TUFT = "#D5E5C8", "#E3EFD8", "#B9D3A6"
SOIL_A, SOIL_B, BRICK = "#EFE4CC", "#F5EDD9", "#C2AC7E"
LAVENDER, LAVENDER_LINE, LAVENDER_INK = "#C9C2EC", "#A79DDD", "#3B3568"
SKY, SKY_LINE = "#BFE0EC", "#7BAEC4"
CARD_FACE, CARD_LINE = "#F9FBF5", "#CFDCC4"
PLAYER_COLORS = {
    "menta": ("#A8D5BA", "#5E9A78"),
    "lavanda": ("#C9C2EC", "#8F82CE"),
    "mantequilla": ("#F7E9B0", "#C7A94A"),
    "cielo": ("#BFE0EC", "#7BAEC4"),
}
# La oruga base es blanca con trazo gris: al teñirla con el color del jugador
# (SpriteBatch.Draw con Color) el blanco toma el color y el gris lo oscurece.
GRAY_CATERPILLAR = ("#FFFFFF", "#7D7D7D")

# Mapa propuesto: letra = jardinera, dígito = altura inicial
CASTLES = ["........",
           ".AA..BB.",
           ".A....B.",
           "...CC...",
           "...CC...",
           ".D....E.",
           ".DD..EE.",
           "........"]
HEIGHTS = ["........",
           ".21..12.",
           ".1....1.",
           "...11...",
           "...11...",
           ".1....1.",
           ".21..12.",
           "........"]


def f(v):
    return f"{v:.1f}"


# ----------------------------------------------------------------- piezas

def flower_head(x, y, r, petal=PETAL, line=PETAL_LINE):
    g = ""
    for i in range(5):
        a = i / 5 * math.pi * 2 - math.pi / 2
        px, py = x + math.cos(a) * r * .6, y + math.sin(a) * r * .45
        g += (f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(r * .56)}" ry="{f(r * .4)}" fill="{petal}" '
              f'stroke="{line}" stroke-width=".9" transform="rotate({a * 180 / math.pi + 90:.0f} {f(px)} {f(py)})"/>')
    return g + f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .34)}" fill="{HEART}" stroke="{HEART_LINE}" stroke-width=".9"/>'


def leaf_pair(x, ly, r, s):
    return (f'<path d="M{f(x)} {f(ly)} C {f(x + s * r * .5)} {f(ly - r * .5)} {f(x + s * r * 1.25)} {f(ly - r * .28)} '
            f'{f(x + s * r * 1.35)} {f(ly + r * .12)} C {f(x + s * r * 1.05)} {f(ly + r * .5)} {f(x + s * r * .35)} '
            f'{f(ly + r * .44)} {f(x)} {f(ly)} Z" fill="{LEAF}" stroke="{STEM}" stroke-width=".9"/>'
            f'<path d="M{f(x)} {f(ly)} C {f(x - s * r * .45)} {f(ly - r * .34)} {f(x - s * r * .95)} {f(ly - r * .1)} '
            f'{f(x - s * r * 1.02)} {f(ly + r * .2)} C {f(x - s * r * .7)} {f(ly + r * .42)} {f(x - s * r * .25)} '
            f'{f(ly + r * .36)} {f(x)} {f(ly)} Z" fill="{LEAF_LIGHT}" stroke="{STEM}" stroke-width=".8"/>')


def plant(x, y, lv, u, shadow=True):
    """Flor de altura lv con la base en (x, y); u = altura de un nivel en píxeles."""
    g = f'<ellipse cx="{f(x)}" cy="{f(y)}" rx="{f(u * .66)}" ry="{f(u * .24)}" fill="rgba(70,86,66,.15)"/>' if shadow else ""
    top = y - lv * u
    g += (f'<path d="M{f(x)} {f(y)} C {f(x - u * .14)} {f(y - lv * u * .45)} {f(x + u * .14)} {f(y - lv * u * .72)} '
          f'{f(x)} {f(top)}" stroke="{STEM}" stroke-width="{max(1.8, u * .14):.1f}" fill="none" stroke-linecap="round"/>')
    for i in range(1, lv + 1):
        g += leaf_pair(x, y - i * u + u * .1, u * (.5 - .05 * (i - 1)), 1 if i % 2 else -1)
    return g + flower_head(x, top, u * .5)


def caterpillar(x, y, s, fill, line, flip=False, frame=0):
    """Oruga centrada en (x, y). frame 0 = reposo, frame 1 = estirada (reptar)."""
    d = -1 if flip else 1
    stretch = 1.12 if frame else 1.0
    g = ""
    for i in range(3, -1, -1):
        hump = (i % 2 == 1) if frame == 0 else (i % 2 == 0 and i > 0)
        cx = x - d * i * s * .5 * stretch
        cy = y - (s * .14 if hump else 0)
        g += f'<circle cx="{f(cx)}" cy="{f(cy)}" r="{f(s * (.4 - i * .035))}" fill="{fill}" stroke="{line}" stroke-width="{f(max(.9, s * .05))}"/>'
    g += (f'<path d="M{f(x + d * s * .12)} {f(y - s * .34)} l {f(d * s * .14)} {f(-s * .26)} M{f(x + d * s * .3)} '
          f'{f(y - s * .3)} l {f(d * s * .05)} {f(-s * .3)}" stroke="{line}" stroke-width="{s * .09:.2f}" fill="none" stroke-linecap="round"/>')
    g += f'<circle cx="{f(x + d * s * .22)}" cy="{f(y - s * .06)}" r="{f(s * .07)}" fill="#2B2B2B"/>'
    g += f'<path d="M{f(x + d * s * .14)} {f(y + s * .1)} q {f(d * s * .08)} {f(s * .06)} {f(d * s * .16)} 0" stroke="{line}" stroke-width="{s * .05:.2f}" fill="none" stroke-linecap="round"/>'
    return f"<g>{g}</g>"


BUTTERFLY_WINGS = (
    '<path d="M19.4 20c-2-6-7.2-9.4-11-7.6-3.4 1.6-2.6 7.4 1 10.4 2.6 2.2 6.6 3 10 2.4Z" fill="#F5C9C6" stroke="#D89A96" stroke-width="1.1"/>'
    '<path d="M20.6 20c2-6 7.2-9.4 11-7.6 3.4 1.6 2.6 7.4-1 10.4-2.6 2.2-6.6 3-10 2.4Z" fill="#BFE0EC" stroke="#8FBFD1" stroke-width="1.1"/>'
    '<path d="M19.6 20c-1.6 3.6-5 6-8 6-2.6 0-3.4-2.6-1.6-4.6" fill="#F7E9B0" stroke="#D9C271" stroke-width="1.1"/>'
    '<path d="M20.4 20c1.6 3.6 5 6 8 6 2.6 0 3.4-2.6 1.6-4.6" fill="#F7E9B0" stroke="#D9C271" stroke-width="1.1"/>')
BUTTERFLY_BODY = ('<path d="M20 12.5c1 2.4 1 12 0 16.5-1-4.5-1-14.1 0-16.5Z" fill="#5F7165"/>'
                  '<path d="M20 12.5 17 8.4M20 12.5 23 8.4" stroke="#5F7165" stroke-width="1.2" stroke-linecap="round"/>')


def butterfly(x, y, size, open_ratio=1.0):
    """Mariposa de 40x40 unidades escalada a size, con las alas abiertas en open_ratio."""
    k = size / 40
    return (f'<g transform="translate({f(x)} {f(y)}) scale({k:.3f})">'
            f'<g transform="translate(20 0) scale({open_ratio:.2f} 1) translate(-20 0)">{BUTTERFLY_WINGS}</g>'
            f'{BUTTERFLY_BODY}</g>')


def sun(x, y, r, rays):
    g = ""
    for i in range(rays):
        a = i / rays * math.pi * 2 - math.pi / 2
        g += (f'<path d="M{f(x + math.cos(a) * r * 1.3)} {f(y + math.sin(a) * r * 1.3)} L {f(x + math.cos(a) * r * 1.8)} '
              f'{f(y + math.sin(a) * r * 1.8)}" stroke="{HEART_LINE}" stroke-width="{f(r * .22)}" stroke-linecap="round"/>')
    return g + f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r)}" fill="{HEART}" stroke="{HEART_LINE}" stroke-width="1.2"/>'


def arrow(d, color=LAVENDER_LINE):
    return (f'<path d="{d}" stroke="{color}" stroke-width="1.6" fill="none" stroke-dasharray="3 2.4" stroke-linecap="round" '
            f'marker-end="url(#flecha)"/>')


ARROW_DEF = ('<defs><marker id="flecha" viewBox="0 0 10 10" refX="7" refY="5" markerWidth="5" markerHeight="5" orient="auto">'
             f'<path d="M1 1 L8 5 L1 9" fill="none" stroke="{LAVENDER_LINE}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></marker></defs>')


def drop(x, y, r):
    return (f'<path d="M{f(x)} {f(y - r * 1.5)} C {f(x + r * .9)} {f(y - r * .4)} {f(x + r)} {f(y + r * .2)} {f(x + r)} {f(y + r * .3)} '
            f'A {f(r)} {f(r)} 0 0 1 {f(x - r)} {f(y + r * .3)} C {f(x - r)} {f(y + r * .2)} {f(x - r * .9)} {f(y - r * .4)} {f(x)} {f(y - r * 1.5)} Z" '
            f'fill="{SKY}" stroke="{SKY_LINE}" stroke-width="1"/>'
            f'<path d="M{f(x - r * .45)} {f(y + r * .15)} q 0 {f(r * .45)} {f(r * .4)} {f(r * .55)}" stroke="#FFFFFF" stroke-width="{f(r * .22)}" fill="none" stroke-linecap="round"/>')


def scissors(x, y, s):
    return (f'<g transform="translate({f(x)} {f(y)}) scale({s / 30:.3f})">'
            f'<path d="M-2 -2 L14 -14 M2 -2 L-14 -14" stroke="#8DA093" stroke-width="3" stroke-linecap="round"/>'
            f'<circle cx="-7" cy="6" r="5" fill="none" stroke="{BLUSH_LINE}" stroke-width="2.6"/>'
            f'<circle cx="7" cy="6" r="5" fill="none" stroke="{BLUSH_LINE}" stroke-width="2.6"/>'
            f'<circle cx="0" cy="-2" r="1.6" fill="{INK}"/></g>')


BLUSH_LINE = "#D89A96"


def dandelion_seed(x, y, s):
    g = f'<path d="M{f(x)} {f(y)} L {f(x)} {f(y - s)}" stroke="#8DA093" stroke-width="1.2"/>'
    for i in range(9):
        a = math.pi + i / 8 * math.pi
        g += (f'<path d="M{f(x)} {f(y - s)} L {f(x + math.cos(a) * s * .55)} {f(y - s + math.sin(a) * s * .55)}" '
              f'stroke="#B8C6BC" stroke-width="1"/>'
              f'<circle cx="{f(x + math.cos(a) * s * .55)}" cy="{f(y - s + math.sin(a) * s * .55)}" r="1.4" fill="#FFFFFF" stroke="#B8C6BC" stroke-width=".7"/>')
    return g


# ----------------------------------------------------------------- cartas

CARD_W, CARD_H = 48, 66


def card_frame(inner):
    return (f'<rect x=".75" y=".75" width="{CARD_W - 1.5}" height="{CARD_H - 1.5}" rx="5" fill="{CARD_FACE}" stroke="{CARD_LINE}" stroke-width="1.5"/>'
            f'<rect x="4" y="16" width="{CARD_W - 8}" height="{CARD_H - 20}" rx="3" fill="#EDF3E6"/>'
            + inner)


def card_art(n):
    """Ilustración de cada carta. Sin texto: el número y el efecto los escribe el cliente con la fuente."""
    cx, cy = CARD_W / 2, 40
    if n == 1:
        return sun(cx, cy - 2, 6.5, 6)
    if n == 2:
        return f'<circle cx="{cx}" cy="{cy - 2}" r="14" fill="#FCF6DF"/>' + sun(cx, cy - 2, 7.5, 7)
    if n == 3:
        return (plant(cx + 8, 60, 3, 13, shadow=False) + caterpillar(cx - 10, 50, 7, *PLAYER_COLORS["menta"])
                + arrow(f"M{cx - 8} 44 C {cx - 10} 30 {cx - 2} 22 {cx + 4} 22"))
    if n == 4:
        return (caterpillar(cx + 2, 52, 8, *PLAYER_COLORS["lavanda"]) + caterpillar(cx - 13, 52, 7, *PLAYER_COLORS["menta"])
                + arrow(f"M{cx - 12} 44 C {cx - 6} 26 {cx + 8} 26 {cx + 14} 44"))
    if n == 5:
        return (plant(cx - 11, 60, 2, 12, shadow=False) + plant(cx + 11, 60, 1, 12, shadow=False)
                + arrow(f"M{cx - 9} 40 C {cx - 4} 30 {cx + 6} 30 {cx + 10} 38"))
    if n == 6:
        return (dandelion_seed(cx + 2, 46, 16) + caterpillar(cx + 2, 50, 7, *PLAYER_COLORS["menta"])
                + arrow(f"M{cx - 14} 58 C {cx - 16} 46 {cx - 12} 36 {cx - 8} 32"))
    if n == 7:
        g = (f'<path d="M8 58 Q {cx} 54 {CARD_W - 8} 58 L {CARD_W - 8} 62 L 8 62 Z" fill="{SOIL_A}" stroke="{BRICK}" stroke-width="1"/>'
             + plant(cx - 10, 58, 1, 11, shadow=False) + plant(cx + 10, 58, 2, 11, shadow=False))
        return g + arrow(f"M{cx - 16} 50 C {cx - 8} 38 {cx + 2} 46 {cx + 9} 30")
    if n == 8:
        return plant(cx - 4, 60, 3, 12, shadow=False) + scissors(cx + 11, 38, 15)
    raise ValueError(n)


def card_back():
    g = (f'<rect x=".75" y=".75" width="{CARD_W - 1.5}" height="{CARD_H - 1.5}" rx="5" fill="{LAVENDER}" stroke="{LAVENDER_LINE}" stroke-width="1.5"/>'
         f'<rect x="4.5" y="4.5" width="{CARD_W - 9}" height="{CARD_H - 9}" rx="3" fill="none" stroke="#EBE8F8" stroke-width="1"/>')
    for (x, y) in ((10, 11), (38, 11), (10, 55), (38, 55)):
        g += flower_head(x, y, 4, "#EBE8F8", LAVENDER_LINE)
    return g + butterfly(CARD_W / 2 - 14, CARD_H / 2 - 15, 28)


# ----------------------------------------------------------------- baldosas e iconos

def grass_tile(size=64):
    g = f'<rect width="{size}" height="{size}" fill="{GRASS_B}"/>'
    tufts = [(8, 12), (40, 6), (26, 30), (54, 36), (12, 50), (44, 56), (60, 18), (30, 62)]
    for x, y in tufts:
        for dx in (-2.4, 0, 2.4):
            for ox in (-size, 0, size):  # repetir en el borde para que la baldosa encaje
                g += (f'<path d="M{f(x + ox + dx)} {f(y)} q {f(dx * .4)} -3 {f(dx * .9)} -5" stroke="{GRASS_TUFT}" '
                      f'stroke-width="1.1" fill="none" stroke-linecap="round"/>')
    return g


def soil_tile(size=64):
    g = f'<rect width="{size}" height="{size}" fill="{SOIL_B}"/>'
    for x, y, r in [(10, 9, 1.6), (34, 18, 1.2), (52, 8, 1.8), (20, 40, 1.4), (46, 44, 1.2), (6, 58, 1.3), (58, 56, 1.6), (30, 58, 1)]:
        g += f'<ellipse cx="{x}" cy="{y}" rx="{r * 1.3}" ry="{r}" fill="#E4D5B4"/>'
    return g


def build_icon():
    return plant(12, 21, 1, 11, shadow=False)


# ----------------------------------------------------------------- tablero

def board(width=700, height=452, pieces=None, highlights=(), selected=None):
    """Jardín en escorzo, como en el prototipo: filas del fondo estrechas, las del frente anchas."""
    pieces = pieces or {}
    y_far, y_near, w_far, w_near, cx, k = 92, 412, 404, 656, width / 2, .45
    fv = lambda v: v * (1 + k) / (1 + k * v)
    P = lambda u, v: (cx + (u - .5) * (w_far + (w_near - w_far) * fv(v)), y_far + (y_near - y_far) * fv(v))
    U = lambda v: 15 + 12 * fv(v)
    pt = lambda a: f"{f(a[0])} {f(a[1])}"
    nb = lambda r, c: "." if r < 0 or c < 0 or r > 7 or c > 7 else CASTLES[r][c]

    ground, beds, edges, items = "", "", "", []
    ground += f'<path d="M {" L ".join(pt(p) for p in (P(0, 0), P(1, 0), P(1, 1), P(0, 1)))} Z" fill="#DDEAD1"/>'
    for r in range(8):
        for c in range(8):
            u0, u1, v0, v1, ch = c / 8, (c + 1) / 8, r / 8, (r + 1) / 8, CASTLES[r][c]
            A, B, C, D = P(u0, v0), P(u1, v0), P(u1, v1), P(u0, v1)
            q = " L ".join(pt(p) for p in (A, B, C, D))
            if ch == ".":
                ground += f'<path d="M {q} Z" fill="{GRASS_A if (r + c) % 2 else GRASS_B}"/>'
                continue
            beds += f'<path d="M {q} Z" fill="{SOIL_A if (r + c) % 2 else SOIL_B}"/>'
            e = []
            if nb(r - 1, c) != ch: e.append(f"M {pt(A)} L {pt(B)}")
            if nb(r + 1, c) != ch: e.append(f"M {pt(D)} L {pt(C)}")
            if nb(r, c - 1) != ch: e.append(f"M {pt(A)} L {pt(D)}")
            if nb(r, c + 1) != ch: e.append(f"M {pt(B)} L {pt(C)}")
            if e:
                edges += f'<path d="{" ".join(e)}" stroke="{BRICK}" stroke-width="2.4" fill="none" stroke-linecap="round"/>'

    for r in range(8):
        for c in range(8):
            key = (r, c)
            lv = int(HEIGHTS[r][c]) if HEIGHTS[r][c] != "." else 0
            cv, cu = (r + .5) / 8, (c + .5) / 8
            b, u = P(cu, cv), U(cv)
            g = ""
            if key in highlights:
                g += (f'<ellipse cx="{f(b[0])}" cy="{f(b[1])}" rx="{f(u * .8)}" ry="{f(u * .3)}" fill="rgba(168,213,186,.35)" '
                      f'stroke="#84C1A0" stroke-width="2" stroke-dasharray="4 3"/>')
            if key == selected:
                g += f'<ellipse cx="{f(b[0])}" cy="{f(b[1])}" rx="{f(u * .85)}" ry="{f(u * .32)}" fill="none" stroke="{LAVENDER_LINE}" stroke-width="2.4"/>'
            if lv:
                g += plant(b[0], b[1], lv, u)
            top = b[1] - lv * u
            piece = pieces.get(key)
            if piece and piece != "mariposa":
                if not lv:
                    g += f'<ellipse cx="{f(b[0])}" cy="{f(b[1] + u * .05)}" rx="{f(u * .55)}" ry="{f(u * .2)}" fill="rgba(70,86,66,.18)"/>'
                g += caterpillar(b[0] + u * .34, top - (u * .7 if lv else u * .3), u * .9, *PLAYER_COLORS[piece], flip=c > 3)
            if piece == "mariposa" or pieces.get(("mariposa",)) == key:
                w = u * 3
                g += butterfly(b[0] - w / 2, top - u * 2.2, w)
            if g:
                items.append(g)
    return (f'<defs><radialGradient id="gsun" cx=".5" cy=".16" r=".8"><stop offset="0" stop-color="#FCF6DF"/>'
            f'<stop offset="1" stop-color="#FCF6DF" stop-opacity="0"/></radialGradient></defs>'
            f'<rect width="{width}" height="{height}" fill="#F7FAF3"/><rect width="{width}" height="{height}" fill="url(#gsun)"/>'
            + ground + beds + edges + "".join(items))


def schematic(cell=46, pad=34):
    """Vista cenital del mapa, para el documento: jardineras, alturas y coordenadas."""
    names = {"A": "J1", "B": "J2", "C": "J3", "D": "J4", "E": "J5"}
    size = pad + cell * 8 + 12
    g = f'<rect width="{size}" height="{size}" fill="#FFFFFF"/>'
    for i in range(8):
        g += (f'<text x="{pad + cell * i + cell / 2}" y="{pad - 12}" font-family="DejaVu Sans, sans-serif" font-size="12" '
              f'fill="#8DA093" text-anchor="middle">{i + 1}</text>'
              f'<text x="{pad - 14}" y="{pad + cell * i + cell / 2 + 4}" font-family="DejaVu Sans, sans-serif" font-size="12" '
              f'fill="#8DA093" text-anchor="middle">{i + 1}</text>')
    for r in range(8):
        for c in range(8):
            x, y, ch = pad + c * cell, pad + r * cell, CASTLES[r][c]
            fill = (GRASS_A if (r + c) % 2 else GRASS_B) if ch == "." else SOIL_A
            g += f'<rect x="{x}" y="{y}" width="{cell}" height="{cell}" fill="{fill}" stroke="#FFFFFF" stroke-width="1"/>'
            if ch != ".":
                h = int(HEIGHTS[r][c])
                for i in range(h):
                    g += f'<circle cx="{x + cell / 2 + (i - (h - 1) / 2) * 11}" cy="{y + cell - 11}" r="3.6" fill="{STEM}"/>'
                g += (f'<text x="{x + cell / 2}" y="{y + 21}" font-family="DejaVu Sans, sans-serif" font-size="13" '
                      f'font-weight="bold" fill="{INK}" text-anchor="middle">{names[ch]}</text>')
    nb = lambda r, c: "." if r < 0 or c < 0 or r > 7 or c > 7 else CASTLES[r][c]
    for r in range(8):
        for c in range(8):
            ch = CASTLES[r][c]
            if ch == ".":
                continue
            x, y = pad + c * cell, pad + r * cell
            if nb(r - 1, c) != ch: g += f'<path d="M{x} {y} h{cell}" stroke="{BRICK}" stroke-width="3"/>'
            if nb(r + 1, c) != ch: g += f'<path d="M{x} {y + cell} h{cell}" stroke="{BRICK}" stroke-width="3"/>'
            if nb(r, c - 1) != ch: g += f'<path d="M{x} {y} v{cell}" stroke="{BRICK}" stroke-width="3"/>'
            if nb(r, c + 1) != ch: g += f'<path d="M{x + cell} {y} v{cell}" stroke="{BRICK}" stroke-width="3"/>'
    return size, g


# ----------------------------------------------------------------- salida

def svg(w, h, body, defs=""):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{defs}{body}</svg>'


def write(name, w, h, body, defs="", scale=SCALE):
    path = os.path.join(SVG_DIR, name + ".svg")
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(svg(w, h, body, defs))
    subprocess.run(["rsvg-convert", "-z", str(scale), "-o", os.path.join(PNG_DIR, name + ".png"), path], check=True)


def main():
    if not shutil.which("rsvg-convert"):
        raise SystemExit("Falta rsvg-convert (brew install librsvg)")
    os.makedirs(SVG_DIR, exist_ok=True)
    os.makedirs(PNG_DIR, exist_ok=True)

    # Flores: lienzo de 64x128, base de la flor en (32, 120). Ese punto es el ancla al dibujar.
    for lv in (1, 2, 3):
        write(f"flor-nivel-{lv}", 64, 128, plant(32, 120, lv, 28))

    # Oruga en gris para teñir, dos cuadros. Lienzo de 56x32, ancla en el centro de la base (28, 28).
    for frame in (0, 1):
        write(f"oruga-gris-{frame + 1}", 56, 32, caterpillar(33, 21, 16, *GRAY_CATERPILLAR, frame=frame))
    write("sombra", 48, 16, '<ellipse cx="24" cy="8" rx="20" ry="6" fill="rgba(70,86,66,.22)"/>')

    # Mariposa, tres cuadros de aleteo. Lienzo de 48x48.
    for i, ratio in enumerate((1.0, .62, .28)):
        write(f"mariposa-{i + 1}", 48, 48, butterfly(4, 4, 40, ratio))

    # Cartas: 48x66 como MatchLayout.CardWidth/CardHeight
    for n in range(1, 9):
        write(f"carta-{n}", CARD_W, CARD_H, card_frame(card_art(n)), ARROW_DEF)
    write("carta-reverso", CARD_W, CARD_H, card_back())

    # Baldosas repetibles para el suelo del jardín y el interior de las jardineras
    write("baldosa-cesped", 64, 64, grass_tile())
    write("baldosa-tierra", 64, 64, soil_tile())

    # Iconos de 24x24 para la barra lateral y los PA
    write("icono-pa", 24, 24, drop(12, 14, 6.5))
    write("icono-construccion", 24, 24, build_icon())
    write("icono-oruga-gris", 24, 24, caterpillar(15, 15, 8, *GRAY_CATERPILLAR))

    # Composiciones para el documento (no son recursos del juego)
    pieces = {(1, 1): "menta", (2, 4): "lavanda", (3, 3): "mantequilla", (6, 6): "cielo", (4, 4): "mariposa"}
    write("mapa-propuesto-escorzo", 700, 452, board(pieces=pieces), scale=2)
    write("mapa-propuesto-resaltado", 700, 452,
          board(pieces={(2, 4): "lavanda", (4, 4): "mariposa"}, highlights={(1, 4), (3, 4), (2, 3), (2, 5)}, selected=(2, 4)), scale=2)
    size, body = schematic()
    write("mapa-propuesto-cenital", size, size, body, scale=2)

    print("Listo:", len(os.listdir(PNG_DIR)), "PNG en", PNG_DIR)


if __name__ == "__main__":
    main()
