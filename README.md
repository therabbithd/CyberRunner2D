# CyberRunner2D 🏂❄️

Juego 2D de snowboard con físicas para Unity **6000.6.3f1** (URP + Input System).
Bajas una ladera nevada con tabla, haces volteretas 360º para puntuar, recoges power-ups de velocidad, puedes **invertir la gravedad al estilo Geometry Dash para rodar por el techo** (modo araña) y evitas partirte la cabeza contra la nieve.

> Flujo: `Menu → CharacterMenu / LevelSel → Level1 → Finish / Crash → reload` + `Credits`.

## Características

- Física 2D con `Rigidbody2D` + `SurfaceEffector2D` para deslizamiento.
- Rotación por torque y boost de velocidad.
- Sistema de trucos: cada voltereta suma `flipCount * 100` puntos (formato `0000` con TextMeshPro).
- 4 personajes seleccionables persistentes (`PlayerPrefs`): Dino (1), Frog (2), Cyber Dino (3) y Cyber Frog (4).
- Power-ups por `ScriptableObject`.
- **Obstáculos**: rocas cyberpunk repartidas por la ladera (capa `Floor`) que bloquean la tabla y provocan crash al golpearlas de cabeza (ver sección dedicada).
- **Modo araña**: pulsa `W` / `↑` para invertir la gravedad y salir disparado al techo; pulsa de nuevo para caer (`SpiderGravityController` + `SpiderCeilingTrack`, ver sección dedicada).
- **Salto**: pulsa `Espacio` para saltar (`PlayerController.JumpPlayer`). La dirección se invierte sola en modo araña (salta alejándose de la superficie en la que estás apoyado).
- **Menú de pausa** con `Escape`: Continuar / Reiniciar / Menú principal (`PauseMenu.cs` + prefab `PauseMenu.prefab`, UI editable en el Editor).
- 5 escenas: menú, personajes, selector de nivel, nivel jugable y créditos.
- Arte cartoon 2D + variantes cyberpunk (rocas, árboles y tiles de suelo) + fuente `Super Cartoon`.

## Requisitos

- Unity `6000.6.3f1` (ver `ProjectSettings/ProjectVersion.txt`).
- Paquetes clave (`Packages/manifest.json`): `URP 17.6.0`, `Input System 1.20.0`, `2D Sprite / SpriteShape / Tilemap`, `Cinemachine 6.6.0`, `UGUI`, `TextMesh Pro` y `com.unity.pipeline 0.8.0-exp.1` (control del Editor por CLI, ver sección *Herramientas de agente*).
- Clonar el repo y abrir con Unity Hub. Escena inicial: `Assets/Scenes/Menu.unity`.

## Cómo jugar

1. Abrir `Menu.unity` y pulsar Play.
2. Elegir personaje en `CharacterMenu` (Dino, Frog, Cyber Dino o Cyber Frog) y nivel en `LevelSel`.
3. Sobrevivir a `Level1` (esquivando las rocas) hasta la meta.

### Controles (`PlayerController.cs` + `Settings/InputSystem_Actions.inputactions`, acción `Move`)

| Entrada | Acción |
|---|---|
| `A` / `←` | Rotar hacia atrás (torque +) |
| `D` / `→` | Rotar hacia delante (torque −) |
| `↑` | Boost (`SurfaceEffector2D.speed = boostSpeed`) + partículas de nieve en contacto con `Floor` |
| `W` (o botón oeste del mando) | **Modo araña**: invierte la gravedad (acción `Spider`). Solo funciona con contacto en suelo o techo; en el aire se ignora |
| `Espacio` (o botón sur del mando) | **Saltar** (acción `Jump`): impulso que aleja de la superficie. Solo con contacto (suelo normal / techo invertido). La dirección se invierte en modo araña |
| `Escape` | Pausa / reanuda (`PauseMenu`) |

- `↑` hace doble función a propósito (boost + araña, como en GD con un solo botón); `W` solo invierte la gravedad y `Espacio` solo salta. Si molesta, ajusta las acciones `Move` / `Spider` / `Jump` en `InputSystem_Actions`.
- Caída de cabeza contra capa `Floor` (suelo/techo/rocas) → `CrashDetector`: pierde control, `bloodParticles`, recarga en 1 s. **Excepción**: durante el vuelo tras invertir la gravedad hay inmunidad temporal (`SpiderGravityController.IsCrashImmune`, 1,2 s) para no morir con la cabeza por el camino.
- Tocar meta (tag `Player`) → `FinishLine`: `finishParticles`, recarga en 1 s (`reloadDelay`).

## Mecánica araña (modo Spider)

Estilo Geometry Dash: una pulsación invierte `Rigidbody2D.gravityScale` y el jugador **sale disparado hacia el techo** (vuelo casi-teletransporte); otra pulsación lo devuelve al suelo, donde sigue rodando boca abajo hasta caer. Solo se puede invertir con **contacto en suelo o techo** (sin vuelo infinito) y con un **cooldown** anti-spam. Nunca se rota el root físico (rota libre con el torque de los flips): solo se voltean los **visuales** (eje Y) y se **espejan los colliders** en vertical, así el frente sigue siendo el frente y el sentido de marcha no cambia.

> Valores de ajuste en `Level1` (objeto `player`): `normalGravity 1.5`, `snapImpulse 38`, `toggleCooldown 0.2`, `angularDampOnToggle 0.5`, `maxSnapSpeed 48`, `crashGraceTime 1.2`, `checkDistance 1.85`, `checkRadius 0.25`, máscara `Floor`.

### `SpiderGravityController.cs` — función por función

Estado público (solo lectura, para HUD/audio/tests): `IsInverted`, `IsGrounded`, `IsCeilinged`, `IsCrashImmune` (true durante `crashGraceTime` tras invertir) y evento `OnGravitySwitched(bool inverted)`.

| Función | Qué hace |
|---|---|
| `Awake()` | Cachea `Rigidbody2D`, `PlayerController`, colliders de tabla/cabeza (+ sus offsets base) y el hijo visual. Fuerza `CollisionDetectionMode2D.Continuous` si la escena trae `Discrete` (anti-tunneling) y crea `GroundCheck`/`CeilingCheck` si faltan (`EnsureChecksExist`). |
| `Start()` | Respeta el `gravityScale` de escena si difiere del serializado; aplica gravedad, flip visual y espejo de colliders iniciales. |
| `OnEnable()` / `OnDisable()` | Suscribe/desuscribe la acción `Spider` (`TrySubscribeInputAction` / `UnsubscribeInputAction` con `InputSystem.actions.FindAction("Spider")`). |
| `Update()` | Fallback por teclas (`W` / `↑` con el nuevo Input System) **solo si la acción `Spider` no existe**; si existe, el input lo gestiona `OnSpiderPerformed`. (`Espacio` queda para el salto.) |
| `FixedUpdate()` | Actualiza `IsGrounded`/`IsCeilinged` (`UpdateContactFlags`) y **limita la velocidad vertical** a `maxSnapSpeed` durante el vuelo (evita atravesar el techo, que es un Edge sin grosor). |
| `OnSpiderPerformed(ctx)` | Callback de la acción: solo el flanco de pulsación llama a `TryToggle()`. También sirve para botón táctil (`TryToggle()` es público). |
| `TryToggle()` | Portero: ignora si está deshabilitado, sin control del jugador, en cooldown o **sin contacto** (ni suelo ni techo). Si pasa, llama a `ToggleGravity()`. |
| `ToggleGravity()` | El corazón: alterna `IsInverted`, aplica `gravityScale` negado (`ApplyGravityScale` + `WakeUp`), espeja colliders en Y (`ApplyColliderMirror`), anula `v.y`, aplica impulso `snapImpulse` hacia la otra superficie con tope `maxSnapSpeed`, amortigua el giro (`angularVelocity *= 0.5`), pone `PlayerController.TorqueDirection = -1` boca abajo, voltea visuales (`ApplyVisualFlip`), arma `suppressFlipCountOnce` y dispara `OnGravitySwitched`. |
| `ApplyGravityScale()` | `gravityScale = ±|normalGravity|` según `IsInverted` + `WakeUp()` (un cuerpo dormido con gravedad negativa no despierta solo). |
| `ApplyVisualFlip()` | `visualsRoot.localScale.y = ±magnitud`: boca abajo sin tocar el root físico ni sus colliders. |
| `ApplyColliderMirror()` | Offsets de tabla y cabeza: `Y base × (−1 si invertido)`, **X intacta**. La tabla queda mirando a la nueva superficie y la cabeza (trigger mortal) deja de llegar primera al techo. |
| `UpdateContactFlags()` | `OverlapCircle` contra capa `Floor` en puntos de **espacio mundo** (`centro ± checkDistance`, no en los hijos que rotan con el torque: a 90° los checks quedarían a los lados). Reposiciona los `Transform` existentes para no romper los Gizmos. |
| `EnsureChecksExist()` | Autocrea `GroundCheck (0,−1.85,0)` y `CeilingCheck (0,+1.85,0)` (justo fuera de la tabla, cuya base está en Y −1.57) y apunta `groundMask` a `Floor` si viene vacía. |
| `FindVisualsChild()` | Localiza el primer hijo con `SpriteRenderer` (contenedor Dino/Frog) como `visualsRoot` si no se asignó. |
| `ConsumeSuppressFlipCount()` | Lo llama `PlayerController.CalculateFlips()`: consume el flag de un solo uso para **no regalar un flip** con el salto brusco del snap. |
| `OnDrawGizmosSelected()` | Esferas suelo (verde/amarilla) y techo (cian/roja) en la vista Scene. |

### `SpiderCeilingTrack.cs` — función por función

Helper del tramo de techo. En `Level1` va sobre `LevelSpriteShape (2)` (objeto a y ≈ 17.6, rotado 180° en X; el borde del techo queda a y ≈ 22 y la cara superior a y ≈ 30; capa `Floor`, `speed −10`, `negateSpeed = true`).

| Función | Qué hace |
|---|---|
| `Start()` | Aplica el signo al entrar en Play para que `negateSpeed` mande aunque el `speed` serializado quedara en +10. |
| `OnValidate()` | Reaplica el signo al editar valores en el Inspector. |
| `ConfigureAsCeiling()` (ContextMenu) | Un clic: capa `Floor` + `BoxCollider2D` sólido (`UsedByEffector`, tamaño `trackSize`) + `SurfaceEffector2D` (`useFriction`/`useBounce`, `forceScale 0.1`) y marca dirty en el Editor. |
| `ApplySpeed()` | `speed = ±|ceilingSpeed|` según `negateSpeed`. **Por qué el techo necesita negativo**: la fuerza tangencial del `SurfaceEffector2D` depende de la normal; tocando desde abajo la normal apunta hacia abajo y la tangente se invierte, así que con `+10` el jugador frena o va marcha atrás. |

### Funciones tocadas en otros scripts por el modo araña

| Script / función | Cambio y por qué |
|---|---|
| `PlayerController.TorqueDirection` (propiedad, `1` / `−1`) | La pone `ToggleGravity()`: boca abajo el torque se espeja para que `A`/`D` **no se sientan al revés** (`PlayerTorque` multiplica por ella). |
| `PlayerController.BoostPlayer()` | Actualiza **todos** los `SurfaceEffector2D` (suelo + techo) pero **conservando el signo de cada uno** (`target × dir`): sin esto, el boost reescribía el techo a `+30` y el jugador volvía a ir marcha atrás arriba. |
| `PlayerController.CalculateFlips()` | Al inicio, consume `spider.ConsumeSuppressFlipCount()` y resetea el conteo: ignora el salto de rotación del snap. |
| `PlayerController.Start()` | Cachea `allEffectors` y referencia a `SpiderGravityController`. |
| `CrashDetector.OnTriggerEnter2D()` | Si `spider.IsCrashImmune`, ignora el contacto con `Floor`: es el vuelo (y el impacto de aterrizaje) tras invertir, no un golpe mortal. |

## Menú de pausa

`PauseMenu.cs` (componente en la raíz del prefab `Assets/Prefabs/PauseMenu.prefab`) + una instancia del prefab en `Level1`. **Toda la UI es editable en el Editor** (Prefab Mode): Canvas, panel, título y botones son GameObjects normales, no código.

| Función | Qué hace |
|---|---|
| `Awake()` | Fija `Time.timeScale = 1` (por si una escena anterior quedó congelada) y arranca despausado. |
| `Update()` | `Escape` (`Keyboard.current.escapeKey`) alterna pausa/reanuda. |
| `Pause()` / `Resume()` / `SetPaused(bool)` | Activa/desactiva el objeto `pauseMenuRoot` (el Canvas) y pone `Time.timeScale` a `0` / `1`. |
| `Restart()` | `timeScale = 1` y recarga la escena activa. |
| `QuitToMenu()` | `timeScale = 1` y carga la escena `menuSceneName` (`"Menu"`). |
| `OnDestroy()` | Restaura `Time.timeScale = 1` para no dejar el juego congelado. |

Estructura del prefab (`PauseMenu` raíz → `PauseMenuCanvas`):

| Objeto | Componentes |
|---|---|
| `PauseMenu` (activo) | `PauseMenu` (con `menuSceneName` y `pauseMenuRoot` → el Canvas). Debe quedar activo para que `Update()` lea `Escape`. |
| `PauseMenuCanvas` (inactivo por defecto) | `Canvas` (`ScreenSpaceOverlay`, `sortingOrder 100`) + `CanvasScaler` (`ScaleWithScreenSize` 1920×1080) + `GraphicRaycaster`. |
| `Dim` | `Image` negro α0.6 a pantalla completa (bloquea clics al juego). |
| `Panel` | `Image` + `VerticalLayoutGroup` (padding 40/40/36/36, spacing 18) con título `PAUSA` (TMP) y 3 botones (`Continuar`→`Resume`, `Reiniciar`→`Restart`, `Menu principal`→`QuitToMenu`). |

Para editar el menú: doble clic en `Assets/Prefabs/PauseMenu.prefab` y modifica textos, colores, tamaños o añade botones (los `onClick` se cablean en el Inspector al componente `PauseMenu`). El nivel ya tiene un `EventSystem` con `InputSystemUIInputModule`; el prefab no lo necesita.

## Obstáculos (rocas cyberpunk)

`Level1` tiene un root `rocks` con 8 rocas repartidas por la bajada, usando el sprite `rock_cyberpunk.png` (`rock_cyberpunk_0`). Cada roca es un `GameObject` con `SpriteRenderer` (sorting order 0, `flipX` alternado) y `PolygonCollider2D` **sólido** en la capa `Floor`, apoyada sobre el terreno real (raycast) y con escala 0.40–0.50.

| # | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|---|---|---|---|---|---|---|---|
| x | 8 | 18 | 30 | 42 | 54 | 62 | 74 | 84 |
| escala | 0.45 | 0.40 | 0.50 | 0.42 | 0.45 | 0.40 | 0.50 | 0.45 |

- **Cómo bloquean**: la tabla (`CapsuleCollider2D`) choca físicamente y desestabiliza el deslizamiento.
- **Crash**: el trigger de cabeza (`CircleCollider2D`, trigger) al tocar cualquier collider de la capa `Floor` —incluidas las rocas— dispara `CrashDetector` (pierde control + recarga), igual que estamparse contra el suelo.
- Evitan los power-ups (x ≈ 11 y 70) y la meta (x ≈ 90.6).
- Se generan con el script de agente `AgentScripts/BuildRocks.cs` (`run_script`): hace `Physics2D.Raycast` contra la capa `Floor` desde y=20 (por debajo del techo) para asentar cada roca en la superficie y luego guarda la escena. `AgentScripts/VerifyRocks.cs` valida capa, collider y sprite.

## Qué hay en cada fichero / carpeta

### `Assets/Scenes/` — 5 escenas

| Escena | Qué es |
|---|---|
| `Menu.unity` | Menú principal (`MenuManager`): Jugar → `LevelSel`, Personajes → `CharacterMenu`, Créditos → `Credits`, Salir. |
| `CharacterMenu.unity` | Elección entre 4 personajes: Dino (1), Frog (2), Cyber Dino (3) y Cyber Frog (4), guarda en `PlayerPrefs("SelectedCharacter")` (`CharacterMenuManager`). |
| `LevelSel.unity` | Selector por convención `"Level" + index` (`LevelSelManager.gotolevel`). |
| `Level1.unity` | Nivel jugable: jugador + `SurfaceEffector2D`, suelo `Floor`, `FinishLine`, `CrashDetector`, `PowerUpManager`, `ScoreManager`, Cinemachine, **techo araña** (`LevelSpriteShape (2)` + `SpiderCeilingTrack`), `PauseMenu` (instancia de prefab), **rocas obstáculo** (root `rocks`, capa `Floor`), nubes decorativas y checks de suelo/techo autocreados. |
| `Credits.unity` | Créditos + botón `Back.prefab` + enlace externo configurable (`CreditsManager.socialMediaURL`). |

### `Assets/Scripts/` — 13 scripts

| Fichero | Clase | Qué hace |
|---|---|---|
| `PlayerController.cs` | `PlayerController` | Núcleo: torque (× `TorqueDirection` en modo araña), boost multi-effector con signo, **salto** (`JumpPlayer`, dirección invertida en modo araña), personaje activo (`ApplySelectedCharacter`), conteo de flips (`CalculateFlips` → `ScoreManager.AddScore`, con supresión del snap), nieve en `OnCollisionEnter/Exit2D`, `ApplyPowerUp` / `RemovePowerUp` (`Speed`, `Torque` y `Spider`). |
| `SpiderGravityController.cs` | `SpiderGravityController` | Modo araña: inversión de gravedad, snap al techo/suelo, espejo de colliders, flip visual, cooldown, inmunidad de crash, detección suelo/techo. Ver sección dedicada. |
| `SpiderCeilingTrack.cs` | `SpiderCeilingTrack` | Tramo de techo: configura `BoxCollider2D` + `SurfaceEffector2D` con signo (`negateSpeed`). Ver sección dedicada. |
| `PauseMenu.cs` | `PauseMenu` | Pausa con `Escape` (`timeScale 0/1`): Continuar / Reiniciar / Menú. UI en `PauseMenu.prefab` (editable). Ver sección dedicada. |
| `CrashDetector.cs` | `CrashDetector` | Trigger vs capa `Floor`: `CanControlPlayer=false`, `bloodParticles`, `reloadScene()` a 1 s. **Ignora contactos durante `spider.IsCrashImmune`** (vuelo tras invertir). |
| `FinishLine.cs` | `FinishLine` | Trigger vs tag `Player`: `finishParticles`, `reloadScene()` tras `reloadDelay` (1 s). |
| `ScoreManager.cs` | `ScoreManager` | `AddScore(int)` → `TextMeshProUGUI` en formato `"0000"`. |
| `PowerUpManager.cs` | `PowerUpManager` | Recogible vs tag `Player`: oculta `SpriteRenderer` hijo, `PowerUpParticles`, `ApplyPowerUp`; `Update()` → cuenta atrás `TimeLeft` y al expirar reaparece + `RemovePowerUp`. |
| `PowerUpScriptableObject.cs` | `PowerUpScriptableObject` | Dato `CreateAssetMenu PowerUps/PowerUpData`: `PowerUpType`, `TimeLimit`, `PowerUpValue`. |
| `MenuManager.cs` | `MenuManager` | `PlayGame()→LevelSel`, `Credits()→Credits`, `gotoselChar()→CharacterMenu`, `BackToMenu()→Menu`, `QuitGame()`. |
| `CharacterMenuManager.cs` | `CharacterMenuManager` | `selectCharacter(int)` → `PlayerPrefs.SetInt + Save()`, `BackToMenu()`. |
| `LevelSelManager.cs` | `LevelSelManager` | `gotolevel(int)` → `"Level"+index`, `BackToMenu()`. |
| `CreditsManager.cs` | `CreditsManager` | `BackToMenu()`, `SocialMediaLink()` → `Application.OpenURL(socialMediaURL)`. |

### `Assets/Prefabs/` — 3 prefabs

| Prefab | Contenido |
|---|---|
| `Back.prefab` | Botón UI `Back` (200×30, arriba-izq) con TextMeshPro `Volver al menu` → `CreditsManager.BackToMenu`. |
| `PowerUp.prefab` | Contenedor de `PowerUpParticles` (amarillo, `playOnAwake=false`, rate 10, speed 5). Base para instanciar con `PowerUpManager` + `ScriptableObject`. |
| `PauseMenu.prefab` | Menú de pausa completo y reutilizable: `PauseMenu` + `PauseMenuCanvas` (Canvas, escaler 1920×1080) + `Dim` + `Panel` con título `PAUSA` y botones Continuar/Reiniciar/Menú principal cableados. Se edita en Prefab Mode. |

### `Assets/PowerUpsData/` — 4 assets (`PowerUpScriptableObject`)

| Asset | `PowerUpType` | `TimeLimit` | `PowerUpValue` | Efecto real |
|---|---|---|---|---|
| `PowerUpShortSpeed.asset` | `Speed` | 1 s | 10 | +10 a `baseSpeed`/`boostSpeed`. |
| `PowerUpLongSpeed.asset` | `Speed` | 3 s | 10 | +10 a `baseSpeed`/`boostSpeed`. |
| `PowerUpTorque.asset` | `Torque` | 10 s | 1 | +1 a `torqueamount` (respuesta de giro) durante 10 s. |
| `PowerUpSpider.asset` | `Spider` | 6 s | 12 | Sube la velocidad de subida en modo araña: +12 a `snapImpulse` y `maxSnapSpeed` del `SpiderGravityController` (el impulso al invertir la gravedad es más rápido). Colocado en `Level1` (x≈45). Reutiliza el antiguo asset `PowerUpJump` (mismo GUID), ya retirado. |

### `Assets/Sprites/` — sprites de personaje en juego

- `Dino.png` → `characterSprites[0]`, id 1.
- `Frog.png` → `characterSprites[1]`, id 2.
- `../SnowSurferAssets/cyber_dino.png` → `characterSprites[2]`, id 3 (Cyber Dino).
- `../SnowSurferAssets/CYBER_FROG.png` → `characterSprites[3]`, id 4 (Cyber Frog).

`CharacterMenu` tiene 4 botones (`dino`, `Frog`, `Cyberpunk dino`, `Cyberpunk frog`) que llaman a `CharacterMenuManager.selectCharacter(1..4)`; `PlayerController.ApplySelectedCharacter()` activa el hijo visual correspondiente.

### `Assets/SnowSurferAssets/` — arte del nivel (23 PNG)

- **Cartoon:** `Snowboarding Dino.png`, `Snowboarder Frog.png` (promo/menú), `Tree 1/2/3/4.png`, `Tree Fallen.png`, `Rock 1/2.png`, `Post 1/2.png`, `Cloud 1/2/3.png`, `Gift Bag.png` (caja regalo / base visual power-up).
- **Cyberpunk:** `rock_cyberpunk.png` (rocas obstáculo de `Level1`), `treecyberpunk.png` y los personajes `cyber_dino.png` + `CYBER_FROG.png` (usados como `characterSprites[2]`/`[3]`).
- **Suelo (SpriteShape):** `floortile.png` (borde) + `floortilefill.png` (relleno), referenciados por `Sprite Shape Profile.asset`. Los antiguos `Floor Tile.png` / `Floor Tile Fill.png` ya no se usan en el nivel.
- `Sprite Shape Profile.asset`: perfil del SpriteShape (borde + fill actuales).

### `Assets/Fonts/` + `Assets/TextMesh Pro/`

- `Super Cartoon.ttf` + `Super Cartoon SDF.asset`: fuente principal de UI.
- `Assets/TextMesh Pro/`: paquete stock (fuentes, shaders, settings). No tocar salvo `TMP Settings` si hace falta.

### `Assets/Settings/`

- `UniversalRP.asset`, `Renderer2D.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `DefaultVolumeProfile.asset`: render 2D URP.
- `InputSystem_Actions.inputactions`: mapa `Player` (`Move` Vector2 + `Spider` Button: `W`, `↑`, gamepad oeste + `Jump` Button: `Espacio`, gamepad sur) y mapa `UI`.
- `Lit2DSceneTemplate` / `URP2DSceneTemplate.unity`: plantillas.
- `Assets/Welcome/`: contenido de bienvenida del template 2D, prescindible.

## Guía de estilo visual (sprites y demás arte)

Estilo: **cartoon 2D casual invernal**, pensado para leerse a toda velocidad bajando la ladera. Todo lo nuevo debe imitarlo.

### 1. Pilares

1. **Contorno negro grueso y cerrado siempre.** Personajes, pinos, nieve, rocas y nubes llevan outline negro. Sin outline = no entra.
2. **Color plano saturado, máximo 2–3 tonos por objeto** (base + luz o sombra). Nada de degradados realistas, ruido ni texturas fotográficas.
3. **Formas blob redondeadas y exageradas.** Dientes enormes, ojos saltones, lengua fuera, nieve en pegotes, nubes hinchadas.
4. **Contraste invierno:** fondo frío (cian/blanco/verde pino) vs personaje cálido y chillón (teal, lima, morado, rosa).

### 2. Paleta de referencia (aprox., sacada de los PNG)

| Uso | Color | Hex aprox. |
|---|---|---|
| Nieve / suelo base | Cian brillante | `#2FBDEF` |
| Sombra suelo | Azul oscuro | `#123E6B` |
| Tope nieve | Blanco | `#FFFFFF` |
| Sombra nieve | Celeste claro | `#CDEFFC` |
| Pino | Verde oscuro | `#1E6B4A` |
| Dino | Verde teal | `#2ED4A0` |
| Rana | Verde lima | `#7ED321` |
| Sudadera / tabla Dino / caja | Morado | `#5B2D8E` / `#7B3FA4` |
| Lengua / gemas | Rosa | `#FF8FB2` / `#D94F8A` |
| Tablas / pantalón Frog | Azul | `#2B7FFF` |
| Roca / nube | Gris claro | `#D9D9D9` |
| Outline universal | Negro | `#0A0A0A` |

### 3. Personajes (Dino, Frog y variantes Cyber)

- Vista lateral, sobre tabla (morada Dino, azul Frog).
- Accesorios obligatorios para personalidad: Dino con gafas de sol + sudadera morada; Frog con pantalón azul de flores + lengua rosa fuera.
- Expresión gamberra: sonrisa con dientes (Dino) / ojos enormes (Frog).
- Sombreado mínimo: un highlight claro en lomo/panza, nada más.
- Variantes **Cyber** (`cyber_dino.png`, `CYBER_FROG.png`): mismo personaje con acabado cyberpunk (líneas de neón cian/magenta, paneles y avisos) para los ids 3 y 4 del selector.
- Nuevos personajes: misma altura visual que Dino/Frog, misma paleta saturada, tabla de color distinto al cuerpo.

### 4. Entorno

- **Suelo (`floortile.png` + `floortilefill.png`):** banda cian con sombra inferior azul oscuro, línea superior negra ondulada gruesa, tope blanco con sombra celeste en ondas. Repetir con SpriteShape, no estirar el PNG. (Los antiguos `Floor Tile.png` / `Floor Tile Fill.png` quedan obsoletos.)
- **Pinos (`Tree 1-4`, `Tree Fallen`; variante `treecyberpunk.png`):** verde oscuro + pegotes blancos de nieve con borde negro, tronco marrón simple; la variante cyberpunk añade líneas de neón.
- **Nubes (`Cloud 1-3`):** gris muy claro casi blanco, filo blanco brillante, sin outline negro (única excepción, van al fondo).
- **Rocas / postes (`Rock 1/2`, `Post 1/2`; obstáculo `rock_cyberpunk.png`):** las rocas cartoon son gris facetado con un solo brillo blanco; el obstáculo cyberpunk (`rock_cyberpunk.png`) es roca oscura con circuitos de neón cian/magenta y avisos naranja, y es el que usa `Level1`.
- **Recogibles (`Gift Bag.png`):** caja morada + gemas rosa + cruz teal, outline negro. Reutilizar este lenguaje para cualquier power-up nuevo.

### 5. UI, tipografía y efectos

- Solo **Super Cartoon** (con su asset SDF) para títulos, puntuación y botones. Textos de UI en español (`Volver al menu`).
- Marcador con formato `0000`.
- Partículas planas y simples: nieve blanca al deslizar, amarilla en power-up, roja en crash, a juego con la meta. Sin sprites realistas.

### 6. Reglas para añadir assets nuevos

- PNG con transparencia, importado como `Sprite (2D and UI)`, mismo `Pixels Per Unit` que los existentes.
- Outline negro del mismo grosor visual (~4–6 px a resolución de trabajo).
- Máximo 3 tonos por objeto + 1 para outline.
- Nombres `PascalCase` con espacio como los actuales (`Tree 5.png`, `Rock 3.png`) y su `.meta` correspondiente.
- Probar siempre sobre el cian del suelo: si no se distingue la silueta a velocidad de boost, se descarta.

## Herramientas de agente (Unity Pipeline + `AgentScripts/`)

- El paquete `com.unity.pipeline` permite controlar un Editor **abierto** por CLI con `unity command ...` (`set_autotick`, `recompile`, `run_tests`, `eval`, `run_script`, etc.). Workflow en la skill `.opencode/skills/unity-pipeline`.
- `AgentScripts/` (en la raíz, **fuera de `Assets/`** para no entrar en builds ni disparar recompilación) contiene utilidades de desarrollo:
  - `BuildRocks.cs` — genera las 8 rocas obstáculo de `Level1` (raycast contra `Floor` + `PolygonCollider2D`) y guarda la escena.
  - `VerifyRocks.cs` — valida capa, collider, sprite y estado *dirty* de las rocas.
  - `SpiderCheck.cs`, `SpiderContactDiag.cs`, `SpiderGroundedTest.cs`, `SpiderPlayTest.cs`, `SpiderState.cs`, `SpiderZoneToggle.cs` — diagnóstico y pruebas del modo araña.
- Ejemplo: `unity command run_script --file AgentScripts/BuildRocks.cs --entry BuildRocks.All`.

## Bugs corregidos (2026-10-14)

- ✅ Errata `PowerUpScriptcriptableObject.cs` → renombrado a `PowerUpScriptableObject.cs` manteniendo el GUID `e47996f0...` (sin cambios de código: la clase ya se llamaba bien).
- ✅ `PowerUpType = Torque` ahora suma `PowerUpValue` a `torqueamount` en `ApplyPowerUp` y lo revierte en `RemovePowerUp` (con suelo en `baseTorqueAmount`).
- ✅ `PowerUpManager` ahora llama a `CountDownPowerUpTime()` desde `Update()`, guarda el `activePowerUp`, reinicia `TimeLeft` en cada recogida y llama `RemovePowerUp` al expirar → el boost es temporal.
- ✅ Escena `level1.unity` → `Level1.unity` (mismo GUID `8c9cfa26...`); `LevelSelManager` construye `"Level"+index` y `EditorBuildSettings.asset` actualizado.

## Novedades (2026-10-17)

- ✅ **Rocas obstáculo (cyberpunk)**: root `rocks` en `Level1` con 8 rocas (`rock_cyberpunk.png`) en capa `Floor` con `PolygonCollider2D` sólido; bloquean la tabla y provocan crash al golpearlas de cabeza. Generadas con `AgentScripts/BuildRocks.cs`. Ver sección dedicada.
- ✅ **4 personajes**: además de Dino (1) y Frog (2), ahora Cyber Dino (3) y Cyber Frog (4) (`cyber_dino.png`, `CYBER_FROG.png`); `CharacterMenu` tiene 4 botones.
- ✅ **Suelo y arte cyberpunk**: el SpriteShape usa `floortile.png` + `floortilefill.png`; añadidos `rock_cyberpunk.png` y `treecyberpunk.png`.
- ✅ **Power-up de salto** (`PowerUpJump.asset`, tipo `Jump`): +8 a `jumpForce` durante 5 s (12 → 20). Nueva rama en `PlayerController.ApplyPowerUp/RemovePowerUp` con `baseJumpForce` para revertir; sirve igual en suelo y en modo araña. Instancia colocada en `Level1` en la zona araña (x ≈ 70, sobre la pendiente).
- ✅ **Salto con `Espacio`**: nueva acción `Jump` (`Espacio` / botón sur del mando) y `PlayerController.JumpPlayer()`. El impulso siempre aleja de la superficie: arriba en gravedad normal, abajo en modo araña (usa el signo de `rb.gravityScale`). Solo salta con contacto (suelo normal / techo invertido). `Espacio` se quitó de `Spider` para no solaparse (la araña queda en `W` / `↑` / botón oeste).

## Novedades (2026-10-16)

- ✅ **Modo araña**: `SpiderGravityController` (inversión de gravedad estilo GD con snap casi-teletransporte: `snapImpulse 38`, `maxSnapSpeed 48`, `crashGraceTime 1.2`, `Continuous` anti-tunneling), `SpiderCeilingTrack` (tramo de techo con `negateSpeed`) y retoques en `PlayerController` (`TorqueDirection`, boost con signo, supresión de flip del snap) y `CrashDetector` (inmunidad durante el vuelo). Detalle función por función arriba.
- ✅ **Techo en dirección correcta**: `LevelSpriteShape (2)` con `speed −10` + `negateSpeed = true` (la normal invertida voltea la tangente del effector) y `BoostPlayer()` conserva el signo por effector para que el boost no lo rompa.
- ✅ **Menú de pausa**: `PauseMenu.cs` + prefab editable `PauseMenu.prefab` instanciado en `Level1`; `Escape` pausa/reanuda (`timeScale`), botones Continuar / Reiniciar / Menú principal.
