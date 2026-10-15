"""Comprueba el mapa propuesto contra R 2.2, DR-08 y la geometria de las jardineras."""
from collections import deque

# Letra = jardinera, digito = altura inicial de la flor. '.' = suelo.
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

N = 8
cell = lambda r, c: CASTLES[r][c] if 0 <= r < N and 0 <= c < N else None
orth = lambda r, c: [(r + dr, c + dc) for dr, dc in ((1, 0), (-1, 0), (0, 1), (0, -1)) if 0 <= r + dr < N and 0 <= c + dc < N]

# 1. Simetria de 90 grados: el jardin se ve igual desde cualquier lado
sym = all((CASTLES[r][c] == ".") == (CASTLES[c][7 - r] == ".") and HEIGHTS[r][c] == HEIGHTS[c][7 - r]
          for r in range(N) for c in range(N))
print("Simetria 90:", sym)

# 2. Ninguna jardinera toca a otra ortogonalmente (R 2.2)
bad = [((r, c), (a, b)) for r in range(N) for c in range(N) if cell(r, c) != "."
       for a, b in orth(r, c) if cell(a, b) not in (".", cell(r, c))]
print("Contactos ortogonales prohibidos:", bad or "ninguno")

# 3. Cada letra es un solo bloque conexo ortogonal (DR-08)
letters = sorted({ch for row in CASTLES for ch in row} - {"."})
for L in letters:
    cells = [(r, c) for r in range(N) for c in range(N) if CASTLES[r][c] == L]
    seen, q = {cells[0]}, deque([cells[0]])
    while q:
        r, c = q.popleft()
        for a, b in orth(r, c):
            if cell(a, b) == L and (a, b) not in seen:
                seen.add((a, b)); q.append((a, b))
    door = any(HEIGHTS[r][c] == "1" and any(cell(a, b) == "." for a, b in orth(r, c)) for r, c in cells)
    grow = sorted({(a, b) for r, c in cells for a, b in orth(r, c) if cell(a, b) == "."
                   and all(cell(x, y) in (".", L) for x, y in orth(a, b))})
    print(f"  {L}: superficie {len(cells)}, conexa {len(seen) == len(cells)}, puerta nivel 1 {door}, "
          f"casillas donde puede crecer ya: {len(grow)}")

# 4. Casillas de frontera: tocan dos jardineras, nadie puede construir ahi
border = [(r, c) for r in range(N) for c in range(N) if cell(r, c) == "."
          and len({cell(a, b) for a, b in orth(r, c)} - {"."}) >= 2]
print("Casillas de frontera (sin construccion posible):", border)

# 5. El suelo queda conectado (las orugas pueden cruzar el jardin)
ground = [(r, c) for r in range(N) for c in range(N) if cell(r, c) == "."]
seen, q = {ground[0]}, deque([ground[0]])
while q:
    r, c = q.popleft()
    for a, b in orth(r, c):
        if cell(a, b) == "." and (a, b) not in seen:
            seen.add((a, b)); q.append((a, b))
print("Suelo conectado:", len(seen) == len(ground), f"({len(ground)} casillas de suelo)")
flowers = 64 - len(ground)
print("Flores iniciales:", flowers, "· niveles totales:", sum(int(h) for row in HEIGHTS for h in row if h != "."))

# 6. Territorio potencial: casillas de suelo mas cercanas a cada jardinera (empate = disputada)
dist = {}
q = deque()
for r in range(N):
    for c in range(N):
        if cell(r, c) != ".":
            dist[(r, c)] = (0, {cell(r, c)}); q.append((r, c))
while q:
    r, c = q.popleft()
    d, owners = dist[(r, c)]
    for a, b in orth(r, c):
        if cell(a, b) != ".":
            continue
        if (a, b) not in dist:
            dist[(a, b)] = (d + 1, set(owners)); q.append((a, b))
        elif dist[(a, b)][0] == d + 1:
            dist[(a, b)][1].update(owners)
terr = {L: 0 for L in letters}
contested = 0
for (r, c), (d, owners) in dist.items():
    if d == 0:
        continue
    if len(owners) == 1:
        terr[next(iter(owners))] += 1
    else:
        contested += 1
print("Territorio potencial por jardinera:", terr, "· disputadas:", contested)
