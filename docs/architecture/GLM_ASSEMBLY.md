# GLM_ASSEMBLY — сборка гейм-сцены MazeHell

Что и куда вешать в редакторе, чтобы сцена `Game` запустилась. Неймспейсы: `MazeHell.Session`, `MazeHell.Player`, `MazeHell.Enemies`, `MazeHell.UI`, `MazeHell.Level`, `MazeHell.Audio`, `MazeHell.Combat`, `MazeHell.Presentation`, `MazeHell.Input`.

## Сцена Game

Один корневой объект `GameRoot` с компонентами:

| Компонент | Назначение |
|---|---|
| `GameComposition` | композитор рана (все поля ниже) |
| `Pauser` | единственный писатель `Time.timeScale` |

Дочерний объект `World` (Transform без коллайдеров) → в поле `_worldRoot`. В него фабрики кладут игрока, HUD и корни уровней.

Полей у `GameComposition` ровно столько, сколько ниже; имена — serialized-имена, Unity сам сматчит по типу и порядку.

### Configurations (12 ассетов из Assets/MazeHell/Scriptables)

| Поле | Тип | Ассет |
|---|---|---|
| `_movementConfig` | `PlayerMovementConfig` | PlayerMovement |
| `_combatConfig` | `PlayerCombatConfig` | PlayerCombat |
| `_shotgunConfig` | `ShotgunConfig` | Shotgun |
| `_enemyConfig` | `EnemyConfig` | Imp |
| `_generationConfig` | `LevelGenerationConfig` | LevelGeneration |
| `_presentationConfig` | `LevelPresentationConfig` | LevelPresentation |
| `_cameraConfig` | `CameraMotionConfig` | CameraMotion |
| `_hudConfig` | `HudMotionConfig` | HudMotion |
| `_animations` | `AnimationLibrary` | AnimationLibrary |
| `_audioConfig` | `AudioConfig` | Audio |
| `_interfaceConfig` | `InterfaceConfig` | Interface |
| `_localizationConfig` | `LocalizationConfig` | Localization |

### Prefabs

| Поле | Тип | Ассет | Требования к префабу |
|---|---|---|---|
| `_playerPrefab` | `PlayerRig` | Prefabs/Gameplay/Player | на корне: `CharacterController`, `PlayerMotor`, `PlayerLook`, `Health`, `Shotgun`, `PlayerDefense`, `CameraMotion`, `PlayerRig` (уже есть). В `PlayerRig` должны быть привязаны: контроллер, все компоненты, `Camera`, `_lookPivot` (пивот обзора, высота задаётся из `EyeHeight`), `_motionPivot` (чайлд пивота для боб/импульсов), `_muzzleLight` (`Light`, Point, выключен) |
| `_enemyPrefab` | `EnemyRig` | Prefabs/Gameplay/Imp | на корне: `NavMeshAgent`, `CapsuleCollider`, `Health`, `DamageTarget`, `EnemyPerception`, `EnemyLocomotion`, `EnemyBrain`, `EnemySpriteAnimator`, `SpriteRenderer`, `EnemyPresentation` + `EnemyRig` с привязками (уже есть) |
| `_hudPrefab` | `HudView` | Prefabs/Gameplay/HUD | на корне обязателен `HudView` со всеми привязками (панели, тексты, 8×`MenuClick`-кнопок, `HudSpring`, `HealthPortrait`, `WeaponSpriteAnimator`, `ScreenFade` Image). `SafeArea` — приветствуется. `HudPresenter` и `HudScreens` навешиваются `HudFactory` в рантайме — в префабе их быть не должно |
| `_healingPickupPrefab` | `GameObject` | любой FBX/префаб подборки | `SphereCollider`-триггер добавит фабрика; меш произвольный |

### Scene

| Поле | Тип | Что назначить |
|---|---|---|
| `_worldRoot` | `Transform` | объект `World` |
| `_input` | `DesktopInput` | компонент на `GameRoot` (или рядом), нужен `Unity.InputSystem` |
| `_pauser` | `Pauser` | компонент на `GameRoot` |
| `_musicSource` | `AudioSource` | на `GameRoot`, `playOnAwake=false`, loop не важен (включает `GameAudio`) |
| `_sfxSources` | `AudioSource[]` | пул из `AudioConfig.SfxPoolCapacity` источников, `playOnAwake=false` |

`GameAudio` создаётся композитором на `GameRoot` в рантайме.

## Сцена Menu

`MenuComposition` уже собирает меню. Кнопка Start грузит `Interface.GameSceneName` (сейчас `Game`). Никаких иных зависимостей от Game-сцены у меню нет.

## Lifecycle

1. `GameComposition.Awake`: валидация ссылок (fail-fast) → курсор в Lock → `RecordStorage` → `Localization` → `GameAudio` (`Setup` + `SetVolume` + `StartMusic`) → `ShotNoise` → фабрики (`LevelGeometryFactory.Setup`, `PlayerFactory`, `HudFactory`, `EnemyFactory`, `PickupFactory`) → игрок → HUD (+`HudPresenter`/`HudScreens`) → `RunSession` + `GameLoop` → подписки → `session.Start(1, seed)`. Сид: `UseFixedSeed ? BaseSeed : случайный`, дальше цепочка `System.Random(runSeed)`.
2. `RunSession.Start` синхронно строит уровень 1 (`LevelGeometryFactory.Build`: геометрия, статик-батчинг, пек NavMesh по `GeometryRoot`, `LevelResources` держит меши/навмеш-данные) и спавнит врагов+пикапы. FSM: `Generating → Fighting`.
3. `Update`: `audio.Tick(unscaled)` → в `Dead` выход (ввод отключён) → пулл `DesktopInput` → Esc тогглит `Pauser` (+`ui_click` работает в паузе) → в паузе выход → `session.Tick(scaledDt)` → `gameLoop.Tick` (порядок: Look → Motor → Defense → Shotgun → MuzzleFlash → PlayerFeedback → на каждого врага Brain → Animator → Presentation).
4. `LateUpdate`: `CameraMotion.Tick` → `HudPresenter.LateTick`. Независимого HUD-шейка нет — только пружина от камеры.
5. Уровень пройден: `EnemyRegistry.CountChanged == 0` → HUD+гейт, однократно `gate_horn`+`gate_move`, `ExitGate.SetRemainingEnemies(0)` физически поднимает лист.
6. Игрок вошёл в тёмный коридор (`dot(pos-CorridorStart, CorridorDirection) >= 0` + латеральные границы, CorridorStart/End — восточный коридор с инset-отступом): FSM `Transition` — фейд, музыка/сердцебиение по прогрессу. Во время коридора **предрасчитывается чистый лабиринт следующего уровня** (`MazeGenerator`, тот же детерминизм, что в фабрике); тяжёлая геометрия строится в конце коридора за полным фейдом: `Generating` выдерживает чёрный кадр по ScreenFadeDuration, затем `NavMeshSurface.RemoveData` → `SetActive(false)` → `Destroy(root)` (deferred; меши/навмеш освобождает `LevelResources.OnDestroy`) → `Build(level+1, seed)` → телепорт игрока, ре-бинд Look, сброс дистанции шагов, `CameraMotion.ResetMotion`, `HudSpring.ResetState`, сплэш → `Fighting`. Старый и новый уровень никогда не живут в физике/навмеше одновременно.
7. Смерть: `session` ловит `Health.IsDead` → `Dead`, в compositor: рекорд (`level/kills/time`), курсор в None, Pauser.RequestPause и `HudScreens.ShowDeath`. Retry/Menu снимают паузу и загружают Game/Menu. Здоровье игрока между уровнями сохраняется; лечение — киллы и подборы (`heal`-кью для подбора) (`HealingPickup`, лечит только если `Heal > 0`, сил на игроке нет).

## Аудио-контракт (Audio.asset)

Обязательные id: `heartbeat`, `footstep`, `jump`, `land`, `dash`, `block`, `player_hit`, `shotgun_single`, `shotgun_volley`, `shotgun_reload`, `imp_footstep`, `imp_alert`, `imp_windup`, `imp_lunge`, `imp_pain`, `imp_death`, `heal`, `gate_horn`, `gate_move`, `ui_click`. Отсутствующий id = исключение `GameAudio` (fail-fast по канону). `gate_horn` — глобальный (SpatialBlend 0), `gate_move` — пространственный звук в позиции ворот, `ui_click` — `ignoreListenerPause = true`.

## Известные ограничения

- Геометрия уровня строится синхронно в кадр — на слабых устройствах возможен фриз при смене уровня (за чёрным фейдом); предрасчёт лабиринта вынесен в коридор.
- `HUD.prefab` без `HudView` не соберётся — `HudFactory` падает с внятным исключением.
- Зависимости передаются через конструкторы/Setup и пассивные Rig/View bindings, service locator не используется.
- Сцены и префабы уже собраны; этот документ описывает сохранённые связи, ручная первичная сборка не требуется.
