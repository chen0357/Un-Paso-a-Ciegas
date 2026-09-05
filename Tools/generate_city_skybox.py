"""Build a city skybox panorama: mixed textured + procedural buildings with gaps."""
from __future__ import annotations

import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance, ImageFilter

W = 4096
H = 2048
HORIZON = H // 2
SEED = 21

ZENITH = (82, 116, 160)
HORIZON_SKY = (198, 204, 212)
GROUND_NEAR = (84, 86, 90)

HOUSE_DIR = Path(__file__).resolve().parents[1] / "Assets" / "Model" / "house" / "3d66.com_JCI54559350289.fbm"
OUT_PATH = Path(__file__).resolve().parents[1] / "Assets" / "Settings" / "Skybox" / "CityPanorama.png"

FACADE_CROPS = [
    ("3d66-export-JCI54559350289-003.jpg", (0.00, 0.42, 1.00, 1.00)),
    ("3d66-export-JCI54559350289-004.jpg", (0.00, 0.00, 1.00, 0.50)),
    ("3d66-export-JCI54559350289-005.jpg", (0.00, 0.00, 0.86, 0.92)),
    ("3d66-export-JCI54559350289-006.jpg", (0.00, 0.00, 0.40, 0.50)),
    ("3d66-export-JCI54559350289-007.jpg", (0.00, 0.00, 0.55, 0.42)),
    ("3d66-export-JCI54559350289-008.jpg", (0.00, 0.00, 0.88, 0.95)),
    ("3d66-export-JCI54559350289-009.jpg", (0.00, 0.00, 1.00, 0.48)),
    ("3d66-export-JCI54559350289-010.jpg", (0.00, 0.00, 1.00, 0.50)),
    ("3d66-export-JCI54559350289-011.jpg", (0.00, 0.00, 1.00, 0.50)),
    ("3d66-export-JCI54559350289-012.jpg", (0.00, 0.00, 1.00, 0.48)),
    ("3d66-export-JCI54559350289-016.jpg", (0.00, 0.00, 1.00, 0.52)),
    ("3d66-export-JCI54559350289-018.jpg", (0.00, 0.00, 1.00, 0.55)),
    ("3d66-export-JCI54559350289-020.jpg", (0.00, 0.00, 1.00, 0.50)),
]

PROCEDURAL_COLORS = [
    (176, 158, 136),
    (164, 148, 134),
    (204, 190, 172),
    (138, 136, 134),
    (168, 118, 96),
    (150, 154, 158),
    (190, 178, 162),
    (128, 122, 116),
    (184, 170, 148),
    (146, 140, 132),
    (186, 142, 98),
    (152, 118, 102),
]

WINDOW_LIT = (214, 208, 186)
WINDOW_DARK = (62, 64, 70)
ROOF = (92, 90, 88)


def lerp(a: int, b: int, t: float) -> int:
    return int(a + (b - a) * t)


def lerp_rgb(a: tuple[int, int, int], b: tuple[int, int, int], t: float) -> tuple[int, int, int]:
    t = max(0.0, min(1.0, t))
    return (lerp(a[0], b[0], t), lerp(a[1], b[1], t), lerp(a[2], b[2], t))


def fill_gradient(image: Image.Image, y0: int, y1: int, c0: tuple[int, int, int], c1: tuple[int, int, int], power: float = 1.0) -> None:
    draw = ImageDraw.Draw(image)
    span = max(1, y1 - y0)
    for y in range(y0, y1):
        t = ((y - y0) / span) ** power
        draw.line((0, y, W, y), fill=lerp_rgb(c0, c1, t))


def load_facades() -> list[Image.Image]:
    facades: list[Image.Image] = []
    for filename, box in FACADE_CROPS:
        path = HOUSE_DIR / filename
        src = Image.open(path).convert("RGB")
        w, h = src.size
        crop = src.crop((int(box[0] * w), int(box[1] * h), int(box[2] * w), int(box[3] * h)))
        facades.append(crop)
    return facades


def paste_wrapped(dst: Image.Image, src: Image.Image, x: int, y: int) -> None:
    dw, _ = dst.size
    sw, sh = src.size
    x %= dw
    mask = src if src.mode == "RGBA" else None
    if x + sw <= dw:
        dst.paste(src, (x, y), mask)
        return
    split = dw - x
    left = src.crop((0, 0, split, sh))
    right = src.crop((split, 0, sw, sh))
    dst.paste(left, (x, y), left if left.mode == "RGBA" else None)
    dst.paste(right, (0, y), right if right.mode == "RGBA" else None)


def apply_haze(building: Image.Image, haze: float) -> Image.Image:
    if haze <= 0:
        return building
    overlay = Image.new("RGB", building.size, HORIZON_SKY)
    building = Image.blend(building, overlay, haze)
    return ImageEnhance.Color(building).enhance(max(0.25, 1.0 - haze * 1.15))


def make_textured_building(src: Image.Image, scale: float, rng: random.Random, haze: float) -> Image.Image:
    sw, sh = src.size
    height = max(28, int(sh * scale))
    scaled_w = max(10, int(sw * scale))
    resized = src.resize((scaled_w, height), Image.Resampling.LANCZOS)

    min_w = max(20, int(height * 0.5))
    max_w = min(scaled_w, int(height * 1.4))
    width = rng.randint(min_w, max(min_w, max_w))
    if scaled_w > width:
        x0 = rng.randint(0, scaled_w - width)
        building = resized.crop((x0, 0, x0 + width, height))
    else:
        building = resized

    if rng.random() > 0.5:
        building = building.transpose(Image.Transpose.FLIP_LEFT_RIGHT)

    building = ImageEnhance.Brightness(building).enhance(0.94 + rng.random() * 0.12)
    return apply_haze(building, haze)


def make_procedural_building(width: int, height: int, rng: random.Random, haze: float) -> Image.Image:
    shade = rng.randint(-14, 10)
    base = rng.choice(PROCEDURAL_COLORS)
    facade = (
        max(40, min(230, base[0] + shade)),
        max(40, min(230, base[1] + shade)),
        max(40, min(230, base[2] + shade)),
    )
    antenna_h = rng.randint(6, 14) if rng.random() > 0.74 and width > 12 else 0
    canvas_h = height + antenna_h
    img = Image.new("RGBA", (width, canvas_h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    top = antenna_h
    draw.rectangle((0, top, width - 1, canvas_h - 1), fill=facade + (255,))
    roof_h = 2 if height < 70 else 3
    draw.rectangle((0, top, width - 1, top + roof_h), fill=ROOF + (255,))

    pad = 2
    cell_w = 4
    cell_h = 5
    cols = max(1, (width - pad * 2) // cell_w)
    rows = max(1, (height - roof_h - 6) // cell_h)
    for row in range(rows):
        for col in range(cols):
            wx = pad + col * cell_w + 1
            wy = top + roof_h + 3 + row * cell_h + 1
            color = WINDOW_LIT if rng.random() > 0.42 else WINDOW_DARK
            draw.rectangle((wx, wy, wx + 1, wy + 2), fill=color + (255,))

    if antenna_h > 0:
        ax = width // 2
        draw.line((ax, 1, ax, top + 1), fill=(70, 70, 72, 255))

    if rng.random() > 0.85 and width > 16:
        tank_w = min(8, width - 4)
        tx = (width - tank_w) // 2
        draw.rectangle((tx, top, tx + tank_w, top + 4), fill=(120, 118, 116, 255))

    rgb = apply_haze(img.convert("RGB"), haze)
    out = Image.new("RGBA", img.size)
    out.paste(rgb)
    out.putalpha(img.split()[-1])
    return out


def place_skyline(
    pano: Image.Image,
    facades: list[Image.Image],
    rng: random.Random,
    min_h: int,
    max_h: int,
    haze: float,
    gap_chance: float,
    min_gap: int,
    max_gap: int,
    textured_chance: float,
    tex_scale_min: float,
    tex_scale_max: float,
) -> None:
    x = rng.randint(0, 24)
    while x < W:
        if rng.random() < gap_chance:
            x += rng.randint(min_gap, max_gap)
            continue

        if facades and rng.random() < textured_chance:
            scale = rng.uniform(tex_scale_min, tex_scale_max)
            building = make_textured_building(rng.choice(facades), scale, rng, haze)
        else:
            height = rng.randint(min_h, max_h)
            if rng.random() < 0.1:
                height = int(height * 1.3)
            width = rng.randint(max(18, int(height * 0.45)), max(22, int(height * 1.05)))
            building = make_procedural_building(width, height, rng, haze)

        paste_wrapped(pano, building, x, HORIZON - building.size[1])
        x += building.size[0] + rng.randint(8, 28)
        if rng.random() < 0.18:
            x += rng.randint(24, 72)


def generate() -> Image.Image:
    rng = random.Random(SEED)
    pano = Image.new("RGB", (W, H), ZENITH)
    fill_gradient(pano, 0, HORIZON, ZENITH, HORIZON_SKY, power=0.72)
    fill_gradient(pano, HORIZON, H, HORIZON_SKY, GROUND_NEAR, power=1.15)

    facades = load_facades()

    place_skyline(
        pano, facades, rng, 88, 150, haze=0.06, gap_chance=0.32, min_gap=22, max_gap=88,
        textured_chance=0.55, tex_scale_min=0.085, tex_scale_max=0.12,
    )

    haze_band = Image.new("RGB", (W, 22), HORIZON_SKY)
    haze_band.putalpha(50)
    rgba = pano.convert("RGBA")
    rgba.alpha_composite(haze_band, (0, HORIZON - 11))
    pano = rgba.convert("RGB")
    pano = pano.filter(ImageFilter.GaussianBlur(radius=0.45))
    return pano


def main() -> None:
    OUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    image = generate()
    image.save(OUT_PATH, format="PNG", optimize=True)
    print(f"Wrote {OUT_PATH} ({image.size[0]}x{image.size[1]})")


if __name__ == "__main__":
    main()
