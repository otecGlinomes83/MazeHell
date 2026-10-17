# room_mini_cover — Миниарена — укрытия

Миниарена увеличена до 24×24×8 м. Два смещённых прямоугольных укрытия, широкие обходы и открытый центр.

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Geo_Floor | -12.5 | 12.5 | -12.5 | 12.5 | -0.2 | 0 | MH_Floor_Cracked |
| Geo_Ceiling | -12.5 | 12.5 | -12.5 | 12.5 | 8 | 8.2 | MH_Ceiling_Basalt |
| Geo_Wall_N_L | -12.5 | -3.0 | 12 | 12.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_N_R | 3.0 | 12.5 | 12 | 12.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_N_Lintel | -3 | 3 | 12 | 12.5 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_N_0 | -3.0 | 3.0 | 11.88 | 12 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_N_1 | -2.25 | 2.25 | 11.88 | 12 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_N_2 | -1.5 | 1.5 | 11.88 | 12 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_S_L | -12.5 | -3.0 | -12.5 | -12 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_R | 3.0 | 12.5 | -12.5 | -12 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_Lintel | -3 | 3 | -12.5 | -12 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_S_0 | -3.0 | 3.0 | -12 | -11.88 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_S_1 | -2.25 | 2.25 | -12 | -11.88 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_S_2 | -1.5 | 1.5 | -12 | -11.88 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_W_L | -12.5 | -12 | -12 | -3.0 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_W_R | -12.5 | -12 | 3.0 | 12 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_W_Lintel | -12.5 | -12 | -3 | 3 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_W_0 | -12 | -11.88 | -3.0 | 3.0 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_W_1 | -12 | -11.88 | -2.25 | 2.25 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_W_2 | -12 | -11.88 | -1.5 | 1.5 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_E_L | 12 | 12.5 | -12 | -3.0 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_E_R | 12 | 12.5 | 3.0 | 12 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_E_Lintel | 12 | 12.5 | -3 | 3 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_E_0 | 11.88 | 12 | -3.0 | 3.0 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_E_1 | 11.88 | 12 | -2.25 | 2.25 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_E_2 | 11.88 | 12 | -1.5 | 1.5 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Cover_W | -7 | -5 | -7 | -2 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Cover_E | 5 | 7 | 2 | 7 | 0 | 8 | MH_Wall_Hellstone |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|
| Door_N | 0 | 12.5 | 0 | 0 | 6 | 4.5 | door_wood |
| Door_S | 0 | -12.5 | 0 | 180 | 6 | 4.5 | door_wood |
| Door_W | -12.5 | 0 | 0 | 90 | 6 | 4.5 | door_wood |
| Door_E | 12.5 | 0 | 0 | -90 | 6 | 4.5 | door_wood |

Треугольников: 336. Нет декора и компонентов поведения.
