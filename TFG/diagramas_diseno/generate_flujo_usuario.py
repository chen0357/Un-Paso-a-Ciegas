"""Figura A — estilo simple (como la 1ª versión), sin líneas que crucen cajas."""
from pathlib import Path

import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Polygon
from matplotlib.lines import Line2D

OUT = Path(__file__).with_name("flujo_usuario.png")

fig, ax = plt.subplots(figsize=(11.5, 14.5), dpi=200)
ax.set_xlim(0, 100)
ax.set_ylim(4, 100)
ax.axis("off")
fig.patch.set_facecolor("white")
ax.set_facecolor("white")


def box(x, y, w, h, text, fc="#F5F5F5", ec="#333", lw=1.3, fs=10.5, weight="normal"):
    ax.add_patch(
        FancyBboxPatch(
            (x - w / 2, y - h / 2), w, h,
            boxstyle="round,pad=0.2,rounding_size=0.45",
            linewidth=lw, edgecolor=ec, facecolor=fc, zorder=3,
        )
    )
    ax.text(x, y, text, ha="center", va="center", fontsize=fs, fontweight=weight, zorder=4, linespacing=1.15)


def diamond(x, y, w, h, text, fc="#FFF8E7"):
    verts = [(x, y + h / 2), (x + w / 2, y), (x, y - h / 2), (x - w / 2, y)]
    ax.add_patch(Polygon(verts, closed=True, facecolor=fc, edgecolor="#333", lw=1.3, zorder=3))
    ax.text(x, y, text, ha="center", va="center", fontsize=10, zorder=4, linespacing=1.1)


def arrow(a, b, color="#333", lw=1.35, rad=0, style="-|>", ms=12):
    ax.add_patch(
        FancyArrowPatch(
            a, b, arrowstyle=style, mutation_scale=ms, lw=lw, color=color,
            connectionstyle=f"arc3,rad={rad}", zorder=2,
        )
    )


def poly(pts, color="#333", lw=1.35):
    xs, ys = zip(*pts)
    ax.plot(xs, ys, color=color, lw=lw, zorder=2, solid_capstyle="round")
    arrow(pts[-2], pts[-1], color=color, lw=lw)


# Title
ax.text(50, 97.5, "Figura A. Flujo de usuario de la experiencia",
        ha="center", va="center", fontsize=15, fontweight="bold")

# ===== MAIN (center) =====
CX = 50
box(CX, 91.5, 28, 4.4, "Menú principal\nIniciar · Cómo jugar · Configuración · Salir", fc="#EAF2FF", fs=11, weight="bold")
box(CX, 84.8, 24, 2.8, "Seleccionar nivel (Level_Street)", fs=11)
box(CX, 79.8, 26, 3.0, "Seleccionar modo visual\n(3 modos; se guarda)", fs=10.5)
box(CX, 73.8, 26, 3.2, "Carga de escena + HUD\n(HP, tiempo, objetivos)", fs=10.5)
box(CX, 67.2, 24, 2.8, "task_1 — Cruzar la acera", fs=11)
box(CX, 62.4, 24, 2.8, "task_2 — Cruzar el cruce", fs=11)
box(CX, 57.4, 26, 2.8, "task_3 — Entrada al metro (GoalZone)", fs=10.5)
box(CX, 50.8, 24, 3.4, "Resultados (éxito)\ntiempo · colisiones · pistas", fc="#E8F6E8", fs=10.5)
diamond(CX, 42.5, 17, 5.8, "¿Reiniciar escena\no volver al menú?")
box(CX, 33.5, 20, 2.8, "Fin de sesión / Salir", fc="#EEEEEE", fs=11)

# Main arrows
ys_chain = [91.5, 84.8, 79.8, 73.8, 67.2, 62.4, 57.4, 50.8, 42.5, 33.5]
hs = [2.2, 1.4, 1.5, 1.6, 1.4, 1.4, 1.4, 1.7, 2.9, 1.4]
for i in range(len(ys_chain) - 1):
    arrow((CX, ys_chain[i] - hs[i]), (CX, ys_chain[i + 1] + hs[i + 1]))
ax.text(CX + 3.5, 38.0, "Salir", fontsize=9.5, color="#333")

# ===== LEFT: optionals from menu (simple, like v1) =====
LX = 20
box(LX, 91.5, 16, 3.6, "Cómo jugar\n(páginas + Volver)", fc="#F5F5F5", fs=9.5)
box(LX, 85.5, 16, 3.0, "Configuración\nvolumen / háptica", fc="#F5F5F5", fs=9.5)
arrow((CX - 14, 92.5), (LX + 8, 92.5), color="#666", style="<|-|>", ms=10)
ax.text(32, 94.0, "opcional", fontsize=8.5, color="#666", ha="center")
arrow((CX - 14, 89.8), (LX + 8, 86.5), color="#666", ms=10)

# ===== LEFT: pause (ONE box, like v1) + destinations written inside =====
ax.text(LX, 71.5, "durante el juego", fontsize=9.5, color="#8A6D00", ha="center", style="italic")
box(
    LX, 65.0, 20, 9.5,
    "Menú de pausa\n"
    "· Continuar → gameplay\n"
    "· Configuración → ajustes*\n"
    "· Reiniciar → carga escena\n"
    "· Volver → menú principal",
    fc="#FFF6D6", ec="#8A6D00", lw=1.5, fs=9.5,
)
arrow((CX - 12, 62.4), (LX + 10, 65.0), color="#8A6D00", rad=0.12, ms=11)
ax.text(33, 64.5, "abrir", fontsize=8.5, color="#8A6D00")

box(LX, 54.5, 18, 3.2, "Audio espacial del objetivo\n(continuo en partida)", fc="#EEF6FF", ec="#3A6EA5", fs=9.0)
ax.plot([LX + 9, CX - 13], [54.5, 57.4], color="#3A6EA5", lw=1.0, ls=":", zorder=1)
ax.text(32, 55.5, "en paralelo", fontsize=8.0, color="#3A6EA5")

# ===== RIGHT: failure (like v1) =====
box(78, 62.4, 15, 2.8, "Salud agotada", fc="#FDECEC", fs=10)
box(78, 54.5, 17, 3.4, "Resultados (fracaso)\ntiempo · colisiones · pistas", fc="#FDECEC", fs=9.5)
arrow((CX + 12, 62.4), (78 - 7.5, 62.4), color="#B00020", ms=11)
ax.text(68, 63.6, "daño", fontsize=9, color="#B00020", ha="center")
arrow((78, 60.9), (78, 56.3), color="#B00020", ms=11)
poly([(78, 52.7), (78, 42.5), (CX + 8.5, 42.5)], color="#B00020")

# Reiniciar — right outer loop (simple)
poly([(CX + 3.5, 44.5), (90, 44.5), (90, 73.8), (CX + 13.2, 73.8)], color="#1B6B2E")
ax.text(92.2, 59, "Reiniciar", fontsize=9.5, color="#1B6B2E", rotation=90, va="center")

# Volver al menú — far-left lane, above left boxes, into menú from top-left
poly(
    [
        (CX - 3.5, 42.5),
        (3, 42.5),
        (3, 95.5),
        (CX - 14.5, 95.5),
        (CX - 14.5, 91.5 + 2.3),
    ],
    color="#1A4F8B",
)
ax.text(
    8.5, 72, "Volver al menú", fontsize=9, color="#1A4F8B", ha="center", va="center",
    bbox=dict(boxstyle="round,pad=0.2", fc="white", ec="none", alpha=0.95),
)

# Note under fin
notes = (
    "* Volumen y háptica se ajustan en Configuración (menú principal o pausa).\n"
    "El contador de pistas aumenta al completar cada tarea (no hay botón «pedir pista»).\n"
    "Flujo del prototipo jugable actual (Level_Street)."
)
ax.text(50, 27.5, notes, ha="center", va="top", fontsize=9.5, linespacing=1.35,
        bbox=dict(boxstyle="round,pad=0.4", fc="#FAFAFA", ec="#CCC", lw=0.8))

legend = [
    Line2D([0], [0], color="#1A4F8B", lw=2.2, label="Volver al menú"),
    Line2D([0], [0], color="#1B6B2E", lw=2.2, label="Reiniciar escena"),
    Line2D([0], [0], color="#8A6D00", lw=2.2, label="Pausa"),
    Line2D([0], [0], color="#B00020", lw=2.2, label="Fracaso (salud)"),
]
ax.legend(handles=legend, loc="lower right", fontsize=9.5, frameon=True)

fig.savefig(OUT, dpi=200, bbox_inches="tight", facecolor="white", pad_inches=0.15)
print(f"Saved: {OUT.resolve()}")
