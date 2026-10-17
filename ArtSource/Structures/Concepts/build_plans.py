"""Presentation plans from Structures specification coordinates; no game assets."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).parent
S = 17
im = Image.new('RGB', (2100, 1660), '#171719')
d = ImageDraw.Draw(im)
FONT = '/usr/share/fonts/liberation/LiberationSans-Regular.ttf'
def text(x, y, value, size=22, color='#e4ddcc'):
    d.text((x, y), value, font=ImageFont.truetype(FONT, size), fill=color)

text(55, 25, 'MAZEHELL / ПЛАНЫ ПО КООРДИНАТАМ ТЗ', 34)
text(55, 78, 'Горизонтальный разрез ниже перемычек • север сверху • все полы на отметке 0', 21)

def panel(col, row, title, subtitle):
    global ox, oy
    ox, oy = col * 690 + 350, row * 730 + 465 + row * 40
    x, y = col * 690 + 45, row * 730 + 135
    text(x, y, title, 25)
    text(x, y+37, subtitle, 17, '#aaa99f')

def box(x0, x1, y0, y1, color='#ba6857'):
    d.rectangle((round(ox+x0*S),round(oy-y1*S),round(ox+x1*S),round(oy-y0*S)), fill=color)

def floor(x0=-14.5,x1=14.5,y0=-14.5,y1=14.5):
    box(x0,x1,y0,y1,'#303034')
    for n in range(-14,15,2):
        if x0 <= n <= x1:
            d.line((ox+n*S,oy-y1*S,ox+n*S,oy-y0*S),fill='#3a393b')
        if y0 <= n <= y1:
            d.line((ox+x0*S,oy-n*S,ox+x1*S,oy-n*S),fill='#3a393b')

def outer(gate=False):
    for a,b in [(-14.5,-3),(3,14.5)]: box(a,b,-14.5,-14)
    for a,b in ([(-14.5,-5),(5,14.5)] if gate else [(-14.5,-3),(3,14.5)]):
        box(a,b,14,14.5,'#ed963e' if gate else '#ba6857')
    for x0,x1 in [(-14.5,-14),(14,14.5)]:
        for a,b in ([(-14,14)] if gate else [(-14,-3),(3,14)]): box(x0,x1,a,b)
    box(-3,3,-14.12,-13.88,'#8ac3bd')
    box(-5 if gate else -3,5 if gate else 3,13.88,14.12,'#ed963e' if gate else '#8ac3bd')
    if not gate:
        for x in [-14,14]: box(x-.12,x+.12,-3,3,'#8ac3bd')

def mirrored(rects, color='#c3b58b'):
    for sx in [-1,1]:
        for sy in [-1,1]:
            for a,b,c,e in rects:
                x=sorted([sx*a,sx*b]); y=sorted([sy*c,sy*e]); box(*x,*y,color)

def columns(coords):
    for x,y in coords: box(x-.9,x+.9,y-.9,y+.9,'#d0c8b4')

def dimensions():
    y=oy+263
    d.line((ox-238,y,ox+238,y),fill='#888780',width=2)
    for x in [ox-238,ox+238]: d.line((x,y-6,x,y+6),fill='#888780',width=2)
    text(ox-30,y+7,'28 м',18)

panel(0,0,'01 / КВАРТАЛЫ','28 × 28 м • крест 12 м • 4 подкомнаты')
floor(); outer()
mirrored([(-14,-6,5.5,6),(-6.5,-6,6,8.5),(-6.5,-6,11.5,14)])
mirrored([(-6.4,-6.1,8.5,11.5)],'#8ac3bd')
columns([(x,y) for x in [-3,3] for y in [-3,3]]); dimensions()

panel(1,0,'02 / КОЛЬЦО','Блок 14 × 14 м • проходы в арену 4 м')
floor(); outer()
for a,b in [(-7,-2),(2,7)]:
    box(a,b,6.5,7); box(a,b,-7,-6.5); box(6.5,7,a,b); box(-7,-6.5,a,b)
mirrored([(-14,-10,9.5,10),(-10.5,-10,10,10.5),(-10.5,-10,13.5,14)])
mirrored([(-10.4,-10.1,10.5,13.5)],'#8ac3bd')
columns([(x,y) for x in [-3,3] for y in [-3,3]]); dimensions()

panel(2,0,'03 / ГРЕБЁНКА','Зубья 5 м • прямая трасса E–W шириной 6 м')
floor(); outer()
for a,b in [(-12,-11.5),(-5.5,-5),(5,5.5),(11.5,12)]:
    box(a,b,9,14); box(a,b,-14,-9)
columns([(x,y) for x in [-4,4] for y in [-4,4]]); dimensions()

panel(0,1,'04 / ВРАТА','Врата 10 м • тамбур 10 → 8 → 6 м')
floor(); floor(-6.5,6.5,14.5,16.5); outer(True)
for a,b,c,e in [(-6.5,-4,14.5,15.5),(4,6.5,14.5,15.5),(-6.5,-3,15.5,16.5),(3,6.5,15.5,16.5)]: box(a,b,c,e)
mirrored([(12.5,14,8,8.5),(12.5,14,3.5,4)],'#ba6857')
columns([(x,y) for x in [-4,4] for y in [-9,-3,3]]); dimensions()

panel(1,1,'05 / ПРЯМОЙ КОРИДОР','Внутри 6 × 24 м • оба торца открыты')
floor(-3.5,3.5,-12,12)
box(-3.5,-3,-12,12); box(3,3.5,-12,12)
for y in [-12,12]: box(-3,3,y-.08,y+.08,'#8ac3bd')
text(ox+80,oy-30,'24 м',20); text(ox-23,oy+220,'6 м',20)

panel(2,1,'06 / CORRIDOR_L','По таблице ТЗ: южный и западный выходы')
floor(-3.5,3.5,-12,12.5); floor(-12.5,-3.5,-3.5,3.5)
for a,b,c,e in [(-3.5,-3,-12,-3),(-3.5,-3,3,12),(3,3.5,-12,12),(-3.5,3.5,12,12.5),(-12.5,-3.5,3,3.5),(-12.5,-3.5,-3.5,-3)]: box(a,b,c,e)
box(-3,3,-12.08,-11.92,'#8ac3bd'); box(-12.08,-11.92,-3,3,'#8ac3bd')
text(ox+80,oy-140,'Глухой',17); text(ox+80,oy-115,'рукав',17)

text(55,1580,'Терракота — стены   /   Кость — внутренние стены и колонны   /   Бирюза — проёмы   /   Оранжевый — врата',20)
text(55,1615,'Стены 6 м высотой, 0.5 м толщиной. Решётки и перемычки над плоскостью разреза условно опущены.',19,'#aaa99f')
im.save(OUT/'02_floor_plans.png')
