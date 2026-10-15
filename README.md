# SnowSurfer2D 🏂❄️

Juego 2D de snowboard con físicas para Unity **6000.6.3f1** (URP + Input System).
Bajas una ladera nevada con tabla, haces volteretas 360º para puntuar, recoges power-ups de velocidad y evitas partirte la cabeza contra la nieve.

> Flujo: `Menu → CharacterMenu / LevelSel → Level1 → Finish / Crash → reload` + `Credits`.

## Características

- Física 2D con `Rigidbody2D` + `SurfaceEffector2D` para deslizamiento.
- Rotación por torque y boost de velocidad.
- Sistema de trucos: cada voltereta suma `flipCount * 100` puntos (formato `0000` con TextMeshPro).
- 2 personajes seleccionables persistentes (`PlayerPrefs`).
- Power-ups por `ScriptableObject`.
- 5 escenas: menú, personajes, selector de nivel, nivel jugable y créditos.
- Arte cartoon 2D original + fuente `Super Cartoon`.

## Requisitos

- Unity `6000.6.3f1` (ver `ProjectSettings/ProjectVersion.txt`).
- Paquetes clave (`Packages/manifest.json`): `URP 17.6.0`, `Input System 1.20.0`, `2D Sprite / SpriteShape / Tilemap`, `Cinemachine 6.6.0`, `UGUI`, `TextMesh Pro`.
- Clonar el repo y abrir con Unity Hub. Escena inicial: `Assets/Scenes/Menu.unity`.

## Cómo jugar

1. Abrir `Menu.unity` y pulsar Play.
2. Elegir personaje en `CharacterMenu` y nivel en `LevelSel`.
3. Sobrevivir a `Level1` hasta la meta.

### Controles (`PlayerController.cs` + `Settings/InputSystem_Actions.inputactions`, acción `Move`)

| Entrada | Acción |
|---|---|
| `A` / `←` | Rotar hacia atrás (torque +) |
| `D` / `→` | Rotar hacia delante (torque −) |
| `W` / `↑` | Boost (`SurfaceEffector2D.speed = boostSpeed`) + partículas de nieve en contacto con `Floor` |

- Caída de cabeza contra capa `Floor` → `CrashDetector`: pierde control, `bloodParticles`, recarga en 1 s.
- Tocar meta (tag `Player`) → `FinishLine`: `finishParticles`, recarga en 1 s (`reloadDelay`).

## Qué hay en cada fichero / carpeta

### `Assets/Scenes/` — 5 escenas

| Escena | Qué es |
|---|---|
| `Menu.unity` | Menú principal (`MenuManager`): Jugar → `LevelSel`, Personajes → `CharacterMenu`, Créditos → `Credits`, Salir. |
| `CharacterMenu.unity` | Elección Dino (1) / Frog (2), guarda en `PlayerPrefs("SelectedCharacter")` (`CharacterMenuManager`). |
| `LevelSel.unity` | Selector por convención `"Level" + index` (`LevelSelManager.gotolevel`). |
| `Level1.unity` | Nivel jugable: jugador + `SurfaceEffector2D`, suelo `Floor`, `FinishLine`, `CrashDetector`, `PowerUpManager`, `ScoreManager`, Cinemachine. |
| `Credits.unity` | Créditos + botón `Back.prefab` + enlace externo configurable (`CreditsManager.socialMediaURL`). |

### `Assets/Scripts/` — 10 scripts

| Fichero | Clase | Qué hace |
|---|---|---|
| `PlayerController.cs` | `PlayerController` | Núcleo: torque, boost, personaje activo (`ApplySelectedCharacter`), conteo de flips (`CalculateFlips` → `ScoreManager.AddScore`), nieve en `OnCollisionEnter/Exit2D`, `ApplyPowerUp` / `RemovePowerUp` (`Speed` y `Torque`). |
| `CrashDetector.cs` | `CrashDetector` | Trigger vs capa `Floor`: `CanControlPlayer=false`, `bloodParticles`, `reloadScene()` a 1 s. |
| `FinishLine.cs` | `FinishLine` | Trigger vs tag `Player`: `finishParticles`, `reloadScene()` tras `reloadDelay` (1 s). |
| `ScoreManager.cs` | `ScoreManager` | `AddScore(int)` → `TextMeshProUGUI` en formato `"0000"`. |
| `PowerUpManager.cs` | `PowerUpManager` | Recogible vs tag `Player`: oculta `SpriteRenderer` hijo, `PowerUpParticles`, `ApplyPowerUp`; `Update()` → cuenta atrás `TimeLeft` y al expirar reaparece + `RemovePowerUp`. |
| `PowerUpScriptableObject.cs` | `PowerUpScriptableObject` | Dato `CreateAssetMenu PowerUps/PowerUpData`: `PowerUpType`, `TimeLimit`, `PowerUpValue`. |
| `MenuManager.cs` | `MenuManager` | `PlayGame()→LevelSel`, `Credits()→Credits`, `gotoselChar()→CharacterMenu`, `BackToMenu()→Menu`, `QuitGame()`. |
| `CharacterMenuManager.cs` | `CharacterMenuManager` | `selectCharacter(int)` → `PlayerPrefs.SetInt + Save()`, `BackToMenu()`. |
| `LevelSelManager.cs` | `LevelSelManager` | `gotolevel(int)` → `"Level"+index`, `BackToMenu()`. |
| `CreditsManager.cs` | `CreditsManager` | `BackToMenu()`, `SocialMediaLink()` → `Application.OpenURL(socialMediaURL)`. |

### `Assets/Prefabs/` — 2 prefabs

| Prefab | Contenido |
|---|---|
| `Back.prefab` | Botón UI `Back` (200×30, arriba-izq) con TextMeshPro `Volver al menu` → `CreditsManager.BackToMenu`. |
| `PowerUp.prefab` | Contenedor de `PowerUpParticles` (amarillo, `playOnAwake=false`, rate 10, speed 5). Base para instanciar con `PowerUpManager` + `ScriptableObject`. |

### `Assets/PowerUpsData/` — 3 assets (`PowerUpScriptableObject`)

| Asset | `PowerUpType` | `TimeLimit` | `PowerUpValue` | Efecto real |
|---|---|---|---|---|
| `PowerUpShortSpeed.asset` | `Speed` | 1 s | 10 | +10 a `baseSpeed`/`boostSpeed`. |
| `PowerUpLongSpeed.asset` | `Speed` | 3 s | 10 | +10 a `baseSpeed`/`boostSpeed`. |
| `PowerUpTorque.asset` | `Torque` | 10 s | 1 | +1 a `torqueamount` (respuesta de giro) durante 10 s. |

### `Assets/Sprites/` — personajes en juego

- `Dino.png` → `characterSprites[0]`, id 1.
- `Frog.png` → `characterSprites[1]`, id 2.

### `Assets/SnowSurferAssets/` — arte del nivel (17 PNG)

`Snowboarding Dino.png`, `Snowboarder Frog.png` (arte promo / menú), `Floor Tile.png` + `Floor Tile Fill.png` (suelo SpriteShape), `Tree 1/2/3/4.png`, `Tree Fallen.png`, `Rock 1/2.png`, `Post 1/2.png`, `Cloud 1/2/3.png`, `Gift Bag.png` (caja regalo / base visual power-up), `Sprite Shape Profile.asset`.

### `Assets/Fonts/` + `Assets/TextMesh Pro/`

- `Super Cartoon.ttf` + `Super Cartoon SDF.asset`: fuente principal de UI.
- `Assets/TextMesh Pro/`: paquete stock (fuentes, shaders, settings). No tocar salvo `TMP Settings` si hace falta.

### `Assets/Settings/`

- `UniversalRP.asset`, `Renderer2D.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `DefaultVolumeProfile.asset`: render 2D URP.
- `InputSystem_Actions.inputactions`: mapa `Move` (Vector2).
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

### 3. Personajes (Dino y Frog)

- Vista lateral, sobre tabla (morada Dino, azul Frog).
- Accesorios obligatorios para personalidad: Dino con gafas de sol + sudadera morada; Frog con pantalón azul de flores + lengua rosa fuera.
- Expresión gamberra: sonrisa con dientes (Dino) / ojos enormes (Frog).
- Sombreado mínimo: un highlight claro en lomo/panza, nada más.
- Nuevos personajes: misma altura visual que Dino/Frog, misma paleta saturada, tabla de color distinto al cuerpo.

### 4. Entorno

- **Suelo (`Floor Tile.png`):** banda cian con sombra inferior azul oscuro, línea superior negra ondulada gruesa, tope blanco con sombra celeste en ondas. Repetir con SpriteShape, no estirar el PNG.
- **Pinos (`Tree 1-4`, `Tree Fallen`):** verde oscuro + pegotes blancos de nieve con borde negro, tronco marrón simple.
- **Nubes (`Cloud 1-3`):** gris muy claro casi blanco, filo blanco brillante, sin outline negro (única excepción, van al fondo).
- **Rocas / postes (`Rock`, `Post`):** gris facetado con un solo brillo blanco; postes de madera simples.
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

## Bugs corregidos (2026-10-14)

- ✅ Errata `PowerUpScriptcriptableObject.cs` → renombrado a `PowerUpScriptableObject.cs` manteniendo el GUID `e47996f0...` (sin cambios de código: la clase ya se llamaba bien).
- ✅ `PowerUpType = Torque` ahora suma `PowerUpValue` a `torqueamount` en `ApplyPowerUp` y lo revierte en `RemovePowerUp` (con suelo en `baseTorqueAmount`).
- ✅ `PowerUpManager` ahora llama a `CountDownPowerUpTime()` desde `Update()`, guarda el `activePowerUp`, reinicia `TimeLeft` en cada recogida y llama `RemovePowerUp` al expirar → el boost es temporal.
- ✅ Escena `level1.unity` → `Level1.unity` (mismo GUID `8c9cfa26...`); `LevelSelManager` construye `"Level"+index` y `EditorBuildSettings.asset` actualizado.
