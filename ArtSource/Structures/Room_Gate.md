# room_gate — Врата входа/выхода

Основной зал 40×40×8 м. Шесть колонн в парах X=±6, Y=−12/−4/4. Одни большие ступенчатые деревянные врата 10×7 на севере, обычная дверь на юге, четыре боковых ниши. Тамбур сужается 10→8→6 м и заканчивается на Y=22.5. Чёрные коридоры — только геометрия, без триггеров загрузки.

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Geo_Floor | -20.5 | 20.5 | -20.5 | 20.5 | -0.2 | 0 | MH_Floor_Cracked |
| Geo_Ceiling | -20.5 | 20.5 | -20.5 | 20.5 | 8 | 8.2 | MH_Ceiling_Basalt |
| Geo_Wall_N_L | -20.5 | -5.0 | 20 | 20.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_N_R | 5.0 | 20.5 | 20 | 20.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Gate_Header | -5 | 5 | 20 | 20.5 | 7 | 8 | MH_Wall_Hellstone |
| Geo_Gate_Shoulder_Outer | -5 | -4 | 20 | 20.5 | 5.5 | 7 | MH_Wall_Hellstone |
| Geo_Gate_Shoulder_Inner | -4 | -3 | 20 | 20.5 | 6.25 | 7 | MH_Wall_Hellstone |
| Geo_Gate_Frame_Pier | -6.5 | -5 | 19.2 | 20 | 0 | 5.5 | MH_Column_Stone |
| Geo_Gate_Frame_StepLow | -6.5 | -4 | 19.2 | 20 | 5.5 | 5.9 | MH_Column_Stone |
| Geo_Gate_Frame_RiserLow | -4.4 | -4 | 19.2 | 20 | 5.9 | 6.25 | MH_Column_Stone |
| Geo_Gate_Frame_StepHigh | -4.4 | -3 | 19.2 | 20 | 6.25 | 6.65 | MH_Column_Stone |
| Geo_Gate_Frame_RiserHigh | -3.4 | -3 | 19.2 | 20 | 6.65 | 7 | MH_Column_Stone |
| Geo_Gate_Shoulder_Outer | 4 | 5 | 20 | 20.5 | 5.5 | 7 | MH_Wall_Hellstone |
| Geo_Gate_Shoulder_Inner | 3 | 4 | 20 | 20.5 | 6.25 | 7 | MH_Wall_Hellstone |
| Geo_Gate_Frame_Pier | 5 | 6.5 | 19.2 | 20 | 0 | 5.5 | MH_Column_Stone |
| Geo_Gate_Frame_StepLow | 4 | 6.5 | 19.2 | 20 | 5.5 | 5.9 | MH_Column_Stone |
| Geo_Gate_Frame_RiserLow | 4 | 4.4 | 19.2 | 20 | 5.9 | 6.25 | MH_Column_Stone |
| Geo_Gate_Frame_StepHigh | 3 | 4.4 | 19.2 | 20 | 6.25 | 6.65 | MH_Column_Stone |
| Geo_Gate_Frame_RiserHigh | 3 | 3.4 | 19.2 | 20 | 6.65 | 7 | MH_Column_Stone |
| Geo_Gate_Frame_Crown | -3.4 | 3.4 | 19.2 | 20 | 7 | 7.4 | MH_Column_Stone |
| Geo_Wall_S_L | -20.5 | -3.0 | -20.5 | -20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_R | 3.0 | 20.5 | -20.5 | -20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_Lintel | -3 | 3 | -20.5 | -20 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_S_0 | -3.0 | 3.0 | -20 | -19.88 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_S_1 | -2.25 | 2.25 | -20 | -19.88 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_S_2 | -1.5 | 1.5 | -20 | -19.88 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_W | -20.5 | -20 | -20 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_E | 20 | 20.5 | -20 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | -20 | -18.5 | 5.5 | 6 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | -20 | -18.5 | 10 | 10.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | -20 | -18.5 | -6 | -5.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | -20 | -18.5 | -10.5 | -10 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | 18.5 | 20 | 5.5 | 6 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | 18.5 | 20 | 10 | 10.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | 18.5 | 20 | -6 | -5.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Niche | 18.5 | 20 | -10.5 | -10 | 0 | 8 | MH_Wall_Hellstone |
| Column_0_Base | -7 | -5 | -13 | -11 | 0 | 0.5 | MH_Column_Band |
| Column_0_Shaft | -6.65 | -5.35 | -12.65 | -11.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_0_Capital | -7 | -5 | -13 | -11 | 7.5 | 8 | MH_Column_Band |
| Column_1_Base | -7 | -5 | -5 | -3 | 0 | 0.5 | MH_Column_Band |
| Column_1_Shaft | -6.65 | -5.35 | -4.65 | -3.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_1_Capital | -7 | -5 | -5 | -3 | 7.5 | 8 | MH_Column_Band |
| Column_2_Base | -7 | -5 | 3 | 5 | 0 | 0.5 | MH_Column_Band |
| Column_2_Shaft | -6.65 | -5.35 | 3.35 | 4.65 | 0.5 | 7.5 | MH_Column_Stone |
| Column_2_Capital | -7 | -5 | 3 | 5 | 7.5 | 8 | MH_Column_Band |
| Column_3_Base | 5 | 7 | -13 | -11 | 0 | 0.5 | MH_Column_Band |
| Column_3_Shaft | 5.35 | 6.65 | -12.65 | -11.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_3_Capital | 5 | 7 | -13 | -11 | 7.5 | 8 | MH_Column_Band |
| Column_4_Base | 5 | 7 | -5 | -3 | 0 | 0.5 | MH_Column_Band |
| Column_4_Shaft | 5.35 | 6.65 | -4.65 | -3.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_4_Capital | 5 | 7 | -5 | -3 | 7.5 | 8 | MH_Column_Band |
| Column_5_Base | 5 | 7 | 3 | 5 | 0 | 0.5 | MH_Column_Band |
| Column_5_Shaft | 5.35 | 6.65 | 3.35 | 4.65 | 0.5 | 7.5 | MH_Column_Stone |
| Column_5_Capital | 5 | 7 | 3 | 5 | 7.5 | 8 | MH_Column_Band |
| Geo_Tambour_1 | -6.5 | -4 | 20.5 | 21.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tambour_2 | -6.5 | -3 | 21.5 | 22.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tambour_1 | 4 | 6.5 | 20.5 | 21.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tambour_2 | 3 | 6.5 | 21.5 | 22.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tambour_Floor | -6.5 | 6.5 | 20.5 | 22.5 | -0.2 | 0 | MH_Floor_Cracked |
| Geo_Tambour_Ceiling | -6.5 | 6.5 | 20.5 | 22.5 | 8 | 8.2 | MH_Ceiling_Basalt |
| Geo_Tambour_Exit_Header | -3 | 3 | 22 | 22.5 | 6 | 8 | MH_Wall_Hellstone |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|
| Gate_N | 0 | 20.5 | 0 | 0 | 10 | 7 | gate_big |
| Door_S | 0 | -20.5 | 0 | 180 | 6 | 4.5 | door_wood |
| Anchor_Corridor_N | 0 | 22.5 | 0 | 0 | 6 | 6 | — |

Треугольников: 732. Нет декора и компонентов поведения.
