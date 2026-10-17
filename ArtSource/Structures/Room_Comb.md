# room_comb — Гребёнка

Основная комната 40×40×8 м. Северные/южные карманы глубиной 5 м, свободная широкая центральная арена; четыре колонны (±6, ±6).

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Geo_Floor | -20.5 | 20.5 | -20.5 | 20.5 | -0.2 | 0 | MH_Floor_Cracked |
| Geo_Ceiling | -20.5 | 20.5 | -20.5 | 20.5 | 8 | 8.2 | MH_Ceiling_Basalt |
| Geo_Wall_N_L | -20.5 | -3.0 | 20 | 20.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_N_R | 3.0 | 20.5 | 20 | 20.5 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_N_Lintel | -3 | 3 | 20 | 20.5 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_N_0 | -3.0 | 3.0 | 19.88 | 20 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_N_1 | -2.25 | 2.25 | 19.88 | 20 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_N_2 | -1.5 | 1.5 | 19.88 | 20 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_S_L | -20.5 | -3.0 | -20.5 | -20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_R | 3.0 | 20.5 | -20.5 | -20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_S_Lintel | -3 | 3 | -20.5 | -20 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_S_0 | -3.0 | 3.0 | -20 | -19.88 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_S_1 | -2.25 | 2.25 | -20 | -19.88 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_S_2 | -1.5 | 1.5 | -20 | -19.88 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_W_L | -20.5 | -20 | -20 | -3.0 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_W_R | -20.5 | -20 | 3.0 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_W_Lintel | -20.5 | -20 | -3 | 3 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_W_0 | -20 | -19.88 | -3.0 | 3.0 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_W_1 | -20 | -19.88 | -2.25 | 2.25 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_W_2 | -20 | -19.88 | -1.5 | 1.5 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Wall_E_L | 20 | 20.5 | -20 | -3.0 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_E_R | 20 | 20.5 | 3.0 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Wall_E_Lintel | 20 | 20.5 | -3 | 3 | 4.5 | 8 | MH_Wall_Hellstone |
| Geo_Arch_E_0 | 19.88 | 20 | -3.0 | 3.0 | 4.5 | 5 | MH_Wall_Hellstone |
| Geo_Arch_E_1 | 19.88 | 20 | -2.25 | 2.25 | 5 | 5.5 | MH_Wall_Hellstone |
| Geo_Arch_E_2 | 19.88 | 20 | -1.5 | 1.5 | 5.5 | 6 | MH_Wall_Hellstone |
| Geo_Tooth_-1_0 | -18 | -17.5 | -20 | -15 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_-1_1 | -7.5 | -7 | -20 | -15 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_-1_2 | 7 | 7.5 | -20 | -15 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_-1_3 | 17.5 | 18 | -20 | -15 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_1_0 | -18 | -17.5 | 15 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_1_1 | -7.5 | -7 | 15 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_1_2 | 7 | 7.5 | 15 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Geo_Tooth_1_3 | 17.5 | 18 | 15 | 20 | 0 | 8 | MH_Wall_Hellstone |
| Column_0_Base | -7 | -5 | -7 | -5 | 0 | 0.5 | MH_Column_Band |
| Column_0_Shaft | -6.65 | -5.35 | -6.65 | -5.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_0_Capital | -7 | -5 | -7 | -5 | 7.5 | 8 | MH_Column_Band |
| Column_1_Base | -7 | -5 | 5 | 7 | 0 | 0.5 | MH_Column_Band |
| Column_1_Shaft | -6.65 | -5.35 | 5.35 | 6.65 | 0.5 | 7.5 | MH_Column_Stone |
| Column_1_Capital | -7 | -5 | 5 | 7 | 7.5 | 8 | MH_Column_Band |
| Column_2_Base | 5 | 7 | -7 | -5 | 0 | 0.5 | MH_Column_Band |
| Column_2_Shaft | 5.35 | 6.65 | -6.65 | -5.35 | 0.5 | 7.5 | MH_Column_Stone |
| Column_2_Capital | 5 | 7 | -7 | -5 | 7.5 | 8 | MH_Column_Band |
| Column_3_Base | 5 | 7 | 5 | 7 | 0 | 0.5 | MH_Column_Band |
| Column_3_Shaft | 5.35 | 6.65 | 5.35 | 6.65 | 0.5 | 7.5 | MH_Column_Stone |
| Column_3_Capital | 5 | 7 | 5 | 7 | 7.5 | 8 | MH_Column_Band |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|
| Door_N | 0 | 20.5 | 0 | 0 | 6 | 4.5 | door_wood |
| Door_S | 0 | -20.5 | 0 | 180 | 6 | 4.5 | door_wood |
| Door_W | -20.5 | 0 | 0 | 90 | 6 | 4.5 | door_wood |
| Door_E | 20.5 | 0 | 0 | -90 | 6 | 4.5 | door_wood |

Треугольников: 552. Нет декора и компонентов поведения.
