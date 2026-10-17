# Сплошные деревянные двери

Решётки удалены. Все двери изготовлены из толстых плотно прилегающих досок тёмного серого дерева, с непрерывными железными полосами. Геометрия сплошная. Каждая дверь — отдельный объект; единое полотно с общим пивотом внизу по центру; движение по вертикали Unity Y / Blender Z. Боковых створок и петель нет. Анимаций, автооткрывания, триггеров или скриптов нет.

Обычная дверь 6×4.5 м; малая 3×3.5 м. Врата входа/выхода 10×7 м со ступенчатым верхом: ширины 10/8/6 м на высотах 5.5/6.25/7 м. За счёт этого большие врата отличаются от прямоугольных обычных дверей. Материал их обрамления — тёмный красный камень.

Бюджет: обычные двери до 600 треугольников, большие врата до 900.

# door_wood

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Oak_Plank | -3.0 | -2.25 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | -2.25 | -1.5 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | -1.5 | -0.75 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | -0.75 | 0.0 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | 0.0 | 0.75 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | 0.75 | 1.5 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | 1.5 | 2.25 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Oak_Plank | 2.25 | 3.0 | -0.25 | 0.25 | 0 | 4.5 | MH_Wood_AshGray |
| Continuous_Iron_Strap | -2.94 | 2.94 | -0.34 | -0.25 | 0.99 | 1.17 | MH_Metal_Dark |
| Continuous_Iron_Strap | -2.94 | 2.94 | -0.34 | -0.25 | 3.42 | 3.6 | MH_Metal_Dark |
| Continuous_Iron_Strap | -2.94 | 2.94 | 0.25 | 0.34 | 0.99 | 1.17 | MH_Metal_Dark |
| Continuous_Iron_Strap | -2.94 | 2.94 | 0.25 | 0.34 | 3.42 | 3.6 | MH_Metal_Dark |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|

Треугольников: 144. Нет декора и компонентов поведения.

# door_wood_small

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Oak_Plank | -1.5 | -1.125 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | -1.125 | -0.75 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | -0.75 | -0.375 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | -0.375 | 0.0 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | 0.0 | 0.375 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | 0.375 | 0.75 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | 0.75 | 1.125 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Oak_Plank | 1.125 | 1.5 | -0.25 | 0.25 | 0 | 3.5 | MH_Wood_AshGray |
| Continuous_Iron_Strap | -1.44 | 1.44 | -0.34 | -0.25 | 0.77 | 0.95 | MH_Metal_Dark |
| Continuous_Iron_Strap | -1.44 | 1.44 | -0.34 | -0.25 | 2.66 | 2.8400000000000003 | MH_Metal_Dark |
| Continuous_Iron_Strap | -1.44 | 1.44 | 0.25 | 0.34 | 0.77 | 0.95 | MH_Metal_Dark |
| Continuous_Iron_Strap | -1.44 | 1.44 | 0.25 | 0.34 | 2.66 | 2.8400000000000003 | MH_Metal_Dark |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|

Треугольников: 144. Нет декора и компонентов поведения.

# gate_big

## Геометрия

Все размеры в метрах, Blender Z вверх. Пол 0. Имена с одинаковым префиксом различаются суффиксами объектов.

| Объект | X0 | X1 | Y0 | Y1 | Z0 | Z1 | Материал |
|---|---|---|---|---|---|---|---|
| Oak_Plank | -5.0 | -4.0 | -0.25 | 0.25 | 0 | 5.5 | MH_Wood_AshGray |
| Oak_Plank | -4.0 | -3.0 | -0.25 | 0.25 | 0 | 6.25 | MH_Wood_AshGray |
| Oak_Plank | -3.0 | -2.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | -2.0 | -1.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | -1.0 | 0.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | 0.0 | 1.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | 1.0 | 2.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | 2.0 | 3.0 | -0.25 | 0.25 | 0 | 7 | MH_Wood_AshGray |
| Oak_Plank | 3.0 | 4.0 | -0.25 | 0.25 | 0 | 6.25 | MH_Wood_AshGray |
| Oak_Plank | 4.0 | 5.0 | -0.25 | 0.25 | 0 | 5.5 | MH_Wood_AshGray |
| Continuous_Iron_Strap | -4.94 | 4.94 | -0.34 | -0.25 | 1 | 1.18 | MH_Metal_Dark |
| Continuous_Iron_Strap | -4.94 | 4.94 | -0.34 | -0.25 | 3 | 3.18 | MH_Metal_Dark |
| Continuous_Iron_Strap | -4.94 | 4.94 | -0.34 | -0.25 | 4.8 | 4.9799999999999995 | MH_Metal_Dark |
| Stepped_Iron_Crown | -5.0 | -4.0 | -0.34 | -0.25 | 5.34 | 5.5 | MH_Metal_Dark |
| Stepped_Iron_Crown | -4.0 | -3.0 | -0.34 | -0.25 | 6.09 | 6.25 | MH_Metal_Dark |
| Stepped_Iron_Crown | -3.0 | -2.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | -2.0 | -1.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | -1.0 | 0.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 0.0 | 1.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 1.0 | 2.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 2.0 | 3.0 | -0.34 | -0.25 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 3.0 | 4.0 | -0.34 | -0.25 | 6.09 | 6.25 | MH_Metal_Dark |
| Stepped_Iron_Crown | 4.0 | 5.0 | -0.34 | -0.25 | 5.34 | 5.5 | MH_Metal_Dark |
| Continuous_Iron_Strap | -4.94 | 4.94 | 0.25 | 0.34 | 1 | 1.18 | MH_Metal_Dark |
| Continuous_Iron_Strap | -4.94 | 4.94 | 0.25 | 0.34 | 3 | 3.18 | MH_Metal_Dark |
| Continuous_Iron_Strap | -4.94 | 4.94 | 0.25 | 0.34 | 4.8 | 4.9799999999999995 | MH_Metal_Dark |
| Stepped_Iron_Crown | -5.0 | -4.0 | 0.25 | 0.34 | 5.34 | 5.5 | MH_Metal_Dark |
| Stepped_Iron_Crown | -4.0 | -3.0 | 0.25 | 0.34 | 6.09 | 6.25 | MH_Metal_Dark |
| Stepped_Iron_Crown | -3.0 | -2.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | -2.0 | -1.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | -1.0 | 0.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 0.0 | 1.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 1.0 | 2.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 2.0 | 3.0 | 0.25 | 0.34 | 6.84 | 7 | MH_Metal_Dark |
| Stepped_Iron_Crown | 3.0 | 4.0 | 0.25 | 0.34 | 6.09 | 6.25 | MH_Metal_Dark |
| Stepped_Iron_Crown | 4.0 | 5.0 | 0.25 | 0.34 | 5.34 | 5.5 | MH_Metal_Dark |

## Пустые дверные/стыковочные якоря

Только Empty/Transform, без скриптов, графа или связей.

| Имя | X | Y | Z | Поворот Z | Ширина | Высота | Отдельный ассет двери |
|---|---|---|---|---|---|---|---|

Треугольников: 432. Нет декора и компонентов поведения.
