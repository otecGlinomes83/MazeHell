# Рендер структур и WebGL

Проверено 2026-10-16, Unity 6000.4.0f1 / URP 17.4.0.

## Выбор рендера

В проверенных требованиях Яндекс Игр и разделе настройки Unity нет требования использовать Built-in. Требуется работающая HTML5/WebGL-игра и корректная интеграция SDK. Unity официально поддерживает WebGL как для Built-in, так и для URP. По решению пользователя восстановлен URP.

Источники:
- https://yandex.ru/dev/games/doc/ru/concepts/requirements — технические требования, в частности 1.20 (запуск в заявленных браузерах), 1.21 (все файлы до 100 МБ в распакованном виде), 1.22 (index.html в корне, имена без пробелов и кириллицы).
- https://yandex.ru/dev/games/doc/ru/sdk/unity/setting-up — рекомендации по настройке Unity.
- https://docs.unity3d.com/6000.0/Documentation/Manual/render-pipelines-feature-comparison.html — таблица Platform Support, WebGL поддержан Built-in и URP.
- https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-graphics.html — ограничения браузерной графики.

## Текущая конфигурация

- Graphics и все Quality: `Assets/MazeHell/Settings/MazeHell_Web_RPAsset.asset`.
- Renderer: `MazeHell_Web_Renderer`, обычный Forward, до 8 дополнительных источников на объект. Это настройка текущей демонстрационной сцены; её производительность на целевых телефонах ещё не измерялась.
- SSAO: половинное разрешение, 4 выборки, Depth Normals, radius 0.65, intensity 0.8, falloff 35 м.
- Volume: `MazeHell_Structure_PostProcessing`, Neutral tonemapping, exposure +0.2, contrast +8, saturation -8. Камера использует FXAA.
- Белые источники: Rooms intensity 45, Corridors 18. Слабое фоновое заполнение RGB 0.045/0.047/0.05. Чёрные переходы остаются URP/Unlit, без света.
- Тени точечных источников: штатный Low tier = 128 px на грань, атлас 2048, shadow distance 35 м. В этой версии URP custom tier читает `Light.shadowResolution`, а не `Light.shadowCustomResolution`; старое назначение custom tier заменено штатным Low. Источник: локальный `UniversalRenderPipeline.cs`, InitializeShadowData.
- Gate Light отсутствует. Пользовательские runtime-компоненты — только StructureLightingController и StructureLight; Volume и UniversalAdditionalCamera/LightData — штатные компоненты URP.

## Материалы

Камень Smoothness 0.08, дерево 0.12, металл 0.22. Стены, пол, потолок и дерево используют 5 повторно используемых normal maps. Карты получены импортёром Unity из яркости исходных текстур: Normal map / Create from Grayscale, Bumpiness 0.08; max size 1024, mipmaps, linear, bilinear, repeat, compression. Это приближённый рельеф, а не запекание high-poly. Колонны повторно используют карту красного камня, потолок — карту пола. Декодирование и упаковку нормалей для целевой платформы выполняет Unity. Плоским служебным материалам и чёрному переходу normal map не добавлена.

## Проверка сборки

Собирается отдельный технический просмотр `Assets/MazeHell/Scenes/StructureTestMap.unity` в `Builds/WebGL_Structures/`, Gzip + Decompression Fallback. Это проверка рендера и сборки, а не готовая к модерации игра: SDK и игровой процесс в рамках этой задачи не добавляются. Итог сборки записывается в `webgl_build_validation.json`.

## Результат проверки

WebGL-сборка успешно завершена: 0 ошибок, 1 предупреждение, 25 044 545 байт файлов. При дополнительной распаковке транспортного Gzip — около 69.53 МБ. Первоначальное предупреждение о некомпилированных изменениях исчезло после повторной сборки; оставшееся предупреждение относится к служебному пакету Unity Pipeline без runtime config: он выключен в Player, к URP это не относится.

Локальный Chromium / WebGL 2 / SwiftShader загрузил сцену, прогресс загрузки завершён, canvas отрисован; исключений JavaScript и ошибок console.error не обнаружено. Визуально проверены рельеф, тени, матовые поверхности и отсутствие розовых материалов. Это проверка совместимости на программном GPU, не измерение производительности на телефонах. Кадр: ArtPreview/Structures/webgl_browser.png.

В stdout Unity обнаружены три сообщения `shader is not supported on this GPU`: CoreCopy, StencilDitherMaskSeed, HDRDebugView. Это не устранено; скрывать сообщения или менять код пакетов не стали. Для CoreCopy/HDRDebugView Unity зарегистрировала UUM-73639 (https://issuetracker-mig.prd.it.unity3d.com/issues/shader-errors-are-logged-in-webgl-when-building-a-urp-project). StencilDitherMaskSeed требует shader target 4.5 согласно локальному исходнику пакета; применение этого шейдера текущей сценой не подтверждено. Сцена в проверенном браузере отображается, но вывод о готовности к модерации или совместимости со всеми устройствами из этого не следует. Полный браузерный журнал: webgl_browser_validation.json.

Активная платформа Editor — WebGL. В Build Settings включена StructureTestMap; прежние сцены сохранены выключенными.
