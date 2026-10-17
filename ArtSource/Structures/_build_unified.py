"""Собирает ТЗ_ЕДИНОЕ.md из модульных спек этой папки.

Запуск: python3 _build_unified.py
После любых правок README.md / Room_*.md / Corridors.md / Doors.md / Decor.md — пересобрать.
"""
import re
from pathlib import Path

BASE = Path(__file__).resolve().parent
PARTS = [
    ('ЧАСТЬ 1 — ОБЩИЙ СТАНДАРТ (исполняется во всех строениях без исключений)', 'README.md'),
    ('ЧАСТЬ 2 — КОМНАТА room_quarters «Кварталы» (крест + 4 подкомнаты)', 'Room_Quarters.md'),
    ('ЧАСТЬ 3 — КОМНАТА room_ring «Кольцо» (петля + карманы)', 'Room_Ring.md'),
    ('ЧАСТЬ 4 — КОМНАТА room_comb «Гребёнка» (трасса + карманы)', 'Room_Comb.md'),
    ('ЧАСТЬ 5 — КОМНАТА room_gate «Врата» (вход = выход)', 'Room_Gate.md'),
    ('ЧАСТЬ 6 — КОРИДОРЫ corridor_straight / corridor_corner', 'Corridors.md'),
    ('ЧАСТЬ 7 — ДВЕРИ door_wood / door_wood_small / gate_big', 'Doors.md'),
    ('ЧАСТЬ 8 — ДЕКОР decor_* (будущий этап)', 'Decor.md'),
    ('ЧАСТЬ 9 — МИНИАРЕНА room_mini_pillars', 'Room_Mini_Pillars.md'),
    ('ЧАСТЬ 10 — МИНИАРЕНА room_mini_cover', 'Room_Mini_Cover.md'),
]

HEADER = """# ТЗ ЕДИНОЕ — статические структуры MAZEHELL

Актуально: расширенные комнаты, сплошные тёмные деревянные двери, тёмный красный камень колонн/обрамлений и отдельный потолок. Никакой игровой логики, графов, автооткрывания или скрипта полёта. Оставлена только общая настройка света. Части собираются из модульных документов; точные меши описаны в structures_manifest.json. Декор — будущий этап.
"""


def shift_headings(md: str) -> str:
    out, fence = [], False
    for line in md.split('\n'):
        if line.strip().startswith('```'):
            fence = not fence
            out.append(line)
            continue
        if not fence and re.match(r'^#{1,5} ', line):
            out.append('#' + line)
        else:
            out.append(line)
    return '\n'.join(out)


def build() -> None:
    chunks = [HEADER]
    for title, fname in PARTS:
        md = (BASE / fname).read_text()
        md = re.sub(r'^#[^\n]*\n+', '', md)  # drop the module's own H1
        chunks.append(f'\n---\n\n# {title}\n\n' + shift_headings(md).strip() + '\n')
    out = BASE / 'ТЗ_ЕДИНОЕ.md'
    out.write_text(''.join(chunks))
    print('OK ->', out, f'({out.stat().st_size} bytes)')


if __name__ == '__main__':
    build()
