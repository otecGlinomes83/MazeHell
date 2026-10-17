# MAZEHELL — текущая статическая сборка

Открыт `ArtSource/Structures/mazehell_structures.blend`, workspace 20. Проект со скином закрыт. Общий файл содержит только архитектурную сцену.

Unity-сцена: `Assets/MazeHell/Scenes/StructureTestMap.unity`. Сохранена композиция из всех типов комнат, прямых/двойных коридоров, угла и длинных чёрных входного/выходного проходов. Основные залы теперь 40×40 м, миниарены 24×24 м, высота комнат 8 м. Небольшие подкомнаты сохранены. Правую ветвь с миниаренами разнесли, чтобы после увеличения помещений не было пересечений.

Для статической сборки используются два C#-файла света в Assets/MazeHell/Scripts/Level: StructureLightingController и StructureLight. Удалены AnimatedDoor, StructureModule, ModuleSlot, LevelTransitionZone, StructureFlyCamera, PreviewLightBudget, StructureKitBuilder и StructureKitAudit, а также их компоненты, связи, триггеры и старые тестовые сценарии. Статические префабы не содержат MonoBehaviour. Именованные дверные якоря — обычные пустые Transform.

Для полёта использовать встроенный Scene View Unity (ПКМ + WASD), без отдельного скрипта камеры. Камера Game неподвижна. Двери установлены закрытыми, подготовлены для подъёма и опускания цельным полотном.

Для света выбрать корень **MAZEHELL / Static architecture → Structure Lighting Controller**. Rooms / Corridors настраивают группы через OnValidate; пользовательские значения сохранены. Нет динамического включения источников, хоткеев или графа.

Обычные двери: сплошное тёмное серое дерево. Большие врата: тот же тёмный материал, единое тяжёлое полотно, широкая высокая ступенчатая форма. Колонны и обрамления врат — исходный красный камень с более тёмным оттенком. Потолок — отдельный MH_Ceiling_Basalt. Декор не добавлялся.

Исходники геометрии для Blender — `Tools/Structures/build_structures.py`; подготовка отдельно открываемых .blend — `Tools/Structures/finish_blend_files.py`. Это авторские инструменты вне Unity Assets, не игровая логика. Полное описание размеров/материалов — README.md, точные координаты — structures_manifest.json и модульные .md.

Проверка: static_validation.txt. Проверены 369 боксов импортированных мешей, отсутствие межмодульных пересечений, триггеров, missing scripts и лишних пользовательских компонентов. Изображения revised_* в ArtPreview/Structures показывают текущую версию; более старые кадры относятся к первой сборке.

Рендер: URP Forward (MazeHell_Web_RPAsset), normal maps и матовые поверхности; SSAO + Neutral tonemapping. Подробности в README.md и WEBGL_RENDERING.md.
