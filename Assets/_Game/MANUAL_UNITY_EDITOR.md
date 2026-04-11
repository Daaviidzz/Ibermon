# Manual de configuración — Unity Editor
## Ibermon: integración con la API REST

Este manual te guía paso a paso por todo lo que debes hacer **en el Editor de Unity** para que el juego funcione a través de la API. Los scripts ya están creados; aquí solo se describe la configuración en el editor.

---

## ÍNDICE

1. [Renombrar ScriptableObjects](#1-renombrar-scriptableobjects)
2. [Configurar Resources folders](#2-configurar-resources-folders)
3. [Configurar ApiSetup en la escena Portada](#3-configurar-apisetup-en-la-escena-portada)
4. [Crear la escena Login](#4-crear-la-escena-login)
5. [Modificar la escena Partidas](#5-modificar-la-escena-partidas)
6. [Modificar la escena Portada](#6-modificar-la-escena-portada)
7. [Añadir escenas al Build Settings](#7-añadir-escenas-al-build-settings)
8. [Verificar el Player (JugadorSpawn)](#8-verificar-el-player-jugadorspawn)
9. [Flujo de juego resultante](#9-flujo-de-juego-resultante)
10. [Resolución de problemas frecuentes](#10-resolución-de-problemas-frecuentes)

---

## 1. Renombrar ScriptableObjects

> **MUY IMPORTANTE.** El nombre de cada ScriptableObject debe coincidir **exactamente** (incluidas mayúsculas) con el campo `nombre` del catálogo de la API. Si no coinciden, los ibermon no se cargarán.

### 1.1 PokemonBase (Ibermon)

Los nombres que usa la API son (basado en el seed de la base de datos):

| # | Nombre API | Tipo |
|---|-----------|------|
| 1 | Ignifor | Fuego |
| 2 | Ignifer | Fuego |
| 3 | Infernor | Fuego |
| 4 | Aquillo | Agua |
| 5 | Aquillon | Agua |
| 6 | Aquastorm | Agua |
| 7 | Verdino | Planta |
| 8 | Verdior | Planta |
| 9 | Floradon | Planta |
| 10 | Chispon | Eléctrico |
| ... | (resto según tu seed) | ... |

**Pasos:**
1. Abre el panel **Project** de Unity.
2. Navega a la carpeta donde están tus assets de PokemonBase (ej: `Assets/_Game/Data/Pokemons/`).
3. Selecciona cada ScriptableObject de PokemonBase.
4. En el Inspector, cambia el campo `Name` para que coincida con el nombre de la API.
5. **También renombra el archivo del asset** (clic derecho → Rename) para que coincida. Unity usa el nombre del campo interno `name`, no el del archivo, pero mantenerlos iguales evita confusiones.

> **Consejo:** Si no tienes ScriptableObjects para los nuevos ibermon, créalos:  
> Clic derecho en Project → Create → Pokemons → New Pokemon.

### 1.2 MoveBase (Movimientos)

Los nombres que usa la API son (ejemplos del seed):

| # | Nombre API | Tipo |
|---|-----------|------|
| 1 | Placaje | Normal |
| 2 | Aranazo | Normal |
| 3 | Grunido | Normal |
| 4 | Latigo | Normal |
| 5 | Ataque Rapido | Normal |
| 6 | Ascuas | Fuego |
| 7 | Pantalla Humo | Normal |
| 8 | Lanzallamas | Fuego |
| 9 | Rueda de Fuego | Fuego |
| 10 | Pistola Agua | Agua |
| 11 | Cola | Normal |
| 12 | Hidrobomba | Agua |
| 13 | Surf | Agua |
| 14 | Latigo Cepa | Planta |
| 15 | Hoja Afilada | Planta |
| 16 | Rayo Solar | Planta |
| 17 | Polvo Veneno | Veneno |
| 18 | Impactrueno | Eléctrico |
| 19 | Trueno | Eléctrico |
| 20 | Rayo | Eléctrico |

**Pasos:** igual que los PokemonBase, renombra cada asset de MoveBase para que el campo `Name` coincida con la API.

---

## 2. Configurar Resources folders

Los ScriptableObjects deben estar en carpetas dentro de `Resources/` para que `IbermonConverter` los pueda cargar dinámicamente.

### 2.1 PokemonBase → `Resources/Pokemons/`

1. En el panel Project, navega a `Assets/_Game/` (o donde corresponda).
2. Crea la carpeta `Resources/` si no existe (clic derecho → Create → Folder).
3. Dentro de `Resources/`, crea la carpeta `Pokemons/`.
4. **Mueve** todos tus assets de PokemonBase a `Assets/.../Resources/Pokemons/`.

> **Ruta final:** `Assets/_Game/Resources/Pokemons/Ignifor.asset`, etc.

### 2.2 MoveBase → `Resources/Moves/`

1. Dentro de `Resources/`, crea la carpeta `Moves/`.
2. **Mueve** todos tus assets de MoveBase a `Assets/.../Resources/Moves/`.

> **Ruta final:** `Assets/_Game/Resources/Moves/Placaje.asset`, etc.

**Nota:** Si los MoveBase ya están referenciados en los `LearnableMoves` de los PokemonBase, `IbermonConverter` también los encuentra por esa vía. Pero tenerlos en `Resources/Moves/` es más robusto.

---

## 3. Configurar ApiSetup en la escena Portada

`ApiSetup` es el punto de entrada de toda la infraestructura API. Debe estar en **la primera escena que se carga** (Portada).

**Pasos:**
1. Abre la escena **Portada** en Unity.
2. En la jerarquía, crea un GameObject vacío: clic derecho → Create Empty.
3. Nómbralo `_ApiSetup` (el guion bajo lo pone primero en la lista).
4. Con `_ApiSetup` seleccionado, en el Inspector haz clic en **Add Component**.
5. Busca `ApiSetup` y añádelo.
6. En el Inspector de `ApiSetup`, configura la **Base Url**:
   - Desarrollo local: `http://localhost:8000`
   - Red local (otro PC): `http://192.168.X.X:8000`
   - Producción: `https://api.ibermon.com`

> `ApiSetup` crea automáticamente: ApiManager, todos los servicios REST, **SessionManager** y **CatalogoCache**. No necesitas añadir nada más manualmente.

---

## 4. Crear la escena Login

### 4.1 Crear la escena

1. En el menú de Unity: **File → New Scene** (o duplica una escena existente simple).
2. Guárdala como `Login` en `Assets/_Game/Scenes/Login.unity`.

### 4.2 Estructura de la jerarquía

Crea la siguiente jerarquía en la escena Login:

```
Canvas (UI Scale Mode: Scale With Screen Size, Reference: 1920x1080)
├── PanelLogin
│   ├── Título (TextMeshProUGUI: "Iniciar sesión")
│   ├── InputUsuario (TMP_InputField, Placeholder: "Usuario")
│   ├── InputPassword (TMP_InputField, Placeholder: "Contraseña", Content Type: Password)
│   ├── BotonLogin (Button + TextMeshProUGUI: "Entrar")
│   ├── BotonIrRegistro (Button + TextMeshProUGUI: "¿No tienes cuenta? Regístrate")
│   └── TextoError (TextMeshProUGUI, color rojo, texto vacío)
│
├── PanelRegistro (inicialmente desactivado)
│   ├── Título (TextMeshProUGUI: "Crear cuenta")
│   ├── InputUsuario (TMP_InputField, Placeholder: "Nombre de usuario")
│   ├── InputEmail (TMP_InputField, Placeholder: "Email")
│   ├── InputPassword (TMP_InputField, Placeholder: "Contraseña", Content Type: Password)
│   ├── BotonRegistrar (Button + TextMeshProUGUI: "Crear cuenta")
│   ├── BotonIrLogin (Button + TextMeshProUGUI: "Ya tengo cuenta")
│   └── TextoError (TextMeshProUGUI, color rojo, texto vacío)
│
└── PanelCargando (inicialmente desactivado)
    └── TextoCargando (TextMeshProUGUI: "Cargando...")
```

### 4.3 Añadir el controlador

1. Crea un GameObject vacío en la escena Login → nómbralo `LoginController`.
2. Añade el componente **`LoginUI`**.
3. Asigna en el Inspector todas las referencias:

| Campo en LoginUI | Objeto de la jerarquía |
|---|---|
| `panelLogin` | PanelLogin |
| `inputUsuarioLogin` | PanelLogin/InputUsuario |
| `inputPasswordLogin` | PanelLogin/InputPassword |
| `botonLogin` | PanelLogin/BotonLogin |
| `botonIrRegistro` | PanelLogin/BotonIrRegistro |
| `textoErrorLogin` | PanelLogin/TextoError |
| `panelRegistro` | PanelRegistro |
| `inputUsuarioRegistro` | PanelRegistro/InputUsuario |
| `inputEmailRegistro` | PanelRegistro/InputEmail |
| `inputPasswordRegistro` | PanelRegistro/InputPassword |
| `botonRegistrar` | PanelRegistro/BotonRegistrar |
| `botonIrLogin` | PanelRegistro/BotonIrLogin |
| `textoErrorRegistro` | PanelRegistro/TextoError |
| `panelCargando` | PanelCargando |
| `textoCargando` | PanelCargando/TextoCargando |
| `escenaMenuPrincipal` | `"MenuPrincipal"` |

### 4.4 Conectar los botones (OnClick)

1. Selecciona `BotonLogin` → en el Inspector, sección **On Click()**, haz clic en `+`.
2. Arrastra `LoginController` al campo del objeto.
3. En el dropdown de función, elige `LoginUI → OnClickLogin`.
4. Repite para los demás botones:
   - `BotonIrRegistro` → `LoginUI.MostrarRegistro`
   - `BotonRegistrar` → `LoginUI.OnClickRegistrar`
   - `BotonIrLogin` → `LoginUI.MostrarLogin`

### 4.5 Desactivar PanelRegistro y PanelCargando

1. Selecciona `PanelRegistro` en la jerarquía.
2. En el Inspector, **desmarca la casilla** junto al nombre (desactiva el GameObject).
3. Haz lo mismo con `PanelCargando`.

> `LoginUI.Start()` activa el panel correcto automáticamente.

---

## 5. Modificar la escena Partidas

La escena Partidas ha sido reescrita para mostrar partidas desde la API. Necesita nueva UI.

### 5.1 Estructura de la jerarquía requerida

```
Canvas
├── PanelListaPartidas
│   ├── ScrollView
│   │   └── Viewport
│   │       └── Content          ← este es el "contenedorPartidas" (asignar al script)
│   ├── BotonNueva (Button: "Nueva Partida")
│   └── BotonVolver (Button: "Volver")
│
├── PanelNuevaPartida (inicialmente desactivado)
│   ├── Título (TextMeshProUGUI: "Nueva Partida")
│   ├── DropdownPersonaje (TMP_Dropdown)
│   ├── DropdownStarter (TMP_Dropdown)
│   ├── BotonConfirmar (Button: "¡Jugar!")
│   └── BotonCancelar (Button: "Cancelar")
│
└── PanelCargando (inicialmente desactivado)
    └── TextoCargando (TextMeshProUGUI: "Cargando...")
```

> El `Content` del ScrollView debe tener el componente **Vertical Layout Group** y **Content Size Fitter** (Vertical Fit: Preferred Size).

### 5.2 Asignar referencias en MenuPartida

Selecciona el GameObject que tiene `MenuPartida` y asigna:

| Campo | Objeto |
|---|---|
| `panelLista` | PanelListaPartidas |
| `contenedorPartidas` | PanelListaPartidas/ScrollView/Viewport/Content |
| `botonNueva` | PanelListaPartidas/BotonNueva |
| `botonVolver` | PanelListaPartidas/BotonVolver |
| `panelNuevaPartida` | PanelNuevaPartida |
| `dropdownPersonaje` | PanelNuevaPartida/DropdownPersonaje |
| `dropdownStarter` | PanelNuevaPartida/DropdownStarter |
| `botonConfirmarNueva` | PanelNuevaPartida/BotonConfirmar |
| `botonCancelarNueva` | PanelNuevaPartida/BotonCancelar |
| `panelCargando` | PanelCargando |
| `textoCargando` | PanelCargando/TextoCargando |
| `prefabEntradaPartida` | (prefab PartidaEntry — ver paso 5.3) |
| `escenaJuego` | `"PuebloFuenlabrada"` (o el nombre de tu escena inicial) |
| `posicionarPlayer` | El GameObject que tiene `CrearYPosicionarPlayer` |

### 5.3 Crear el prefab PartidaEntry

1. En la jerarquía, crea un GameObject → `UI → Button` (o crea un Panel con Button).
2. Dale la siguiente estructura:

```
PartidaEntry (Button)
├── TextoPersonaje (TextMeshProUGUI)
├── TextoMapa (TextMeshProUGUI)
├── TextoMedallas (TextMeshProUGUI)
├── TextoTiempo (TextMeshProUGUI)
└── BotonEliminar (Button, pequeño, texto "X")  ← opcional
```

3. Añade el componente **`PartidaEntryUI`** al objeto raíz.
4. Asigna las referencias en `PartidaEntryUI`:
   - `textoPersonaje`, `textoMapa`, `textoMedallas`, `textoTiempo`, `botonEliminar`
5. En el **Button** raíz, en **On Click()**, añade una entrada:
   - Objeto: el propio `PartidaEntry`
   - Función: `PartidaEntryUI → OnClickSeleccionar`
6. En `BotonEliminar → On Click()`:
   - Objeto: el `PartidaEntry`
   - Función: `PartidaEntryUI → OnClickEliminar`
7. **Arrastra** el objeto de la jerarquía al panel **Project** para crear el prefab.
8. **Elimina** el objeto de la jerarquía (ya está guardado como prefab).
9. Asigna el prefab al campo `prefabEntradaPartida` del `MenuPartida`.

### 5.4 Conectar botones de MenuPartida

| Botón | Función |
|---|---|
| BotonNueva → OnClick | `MenuPartida.BotonNueva` |
| BotonVolver → OnClick | `MenuPartida.BotonVolver` |
| BotonConfirmar → OnClick | `MenuPartida.OnClickConfirmarNueva` |
| BotonCancelar → OnClick | `MenuPartida.OnClickCancelarNueva` |

### 5.5 Desactivar paneles

Desactiva en el Inspector (desmarca el checkbox):
- `PanelNuevaPartida`
- `PanelCargando`

`PanelListaPartidas` puede quedar activo (el script lo gestiona).

---

## 6. Modificar la escena Portada

La portada debe redirigir a la escena **Login** en lugar de ir directamente al menú principal.

**Pasos:**
1. Abre la escena **Portada**.
2. Selecciona el GameObject que tiene `ControllPortadaInicio`.
3. En el Inspector, cambia el campo `escenaDestino` de `"MenuPrincipal"` a `"Login"`.

---

## 7. Añadir escenas al Build Settings

Todas las escenas deben estar en **File → Build Settings**.

**Orden recomendado:**
```
0. Portada
1. Login          ← NUEVA
2. MenuPrincipal
3. Partidas
4. Opciones
5. Creditos
6. PuebloFuenlabrada
7. Combate
... (resto de tus escenas de mapa)
```

**Pasos:**
1. **File → Build Settings**.
2. Arrastra la escena `Login.unity` desde el panel Project hasta la lista.
3. Ordénalas como arriba (arrastra para reordenar).
4. Cierra Build Settings.

---

## 8. Verificar el Player (JugadorSpawn)

El componente `JugadorSpawn` ya gestiona la visibilidad del Player entre escenas. Está configurado para ocultarlo en "Combate" y "Opciones". El Player **no debe estar visible** en las escenas de menú, pero como se crea en `MenuPartida`, esto es automático.

**Verificación opcional:** si ves el Player en escenas de menú, añade sus nombres a la condición en `JugadorSpawn.OnSceneLoaded`:

```csharp
if (scene.name == "Combate" || scene.name == "Opciones" ||
    scene.name == "Login"   || scene.name == "MenuPrincipal" || scene.name == "Partidas")
```

---

## 9. Flujo de juego resultante

Una vez configurado todo, el flujo será:

```
Portada
  │  (pulsar Enter / tocar pantalla)
  ▼
Login
  │  (introducir credenciales → botón "Entrar")
  │  → Llama a la API: POST /auth/login
  │  → Carga catálogos: GET /catalogo/ibermon, GET /catalogo/movimientos
  ▼
MenuPrincipal
  │  (botón "Jugar")
  ▼
Partidas
  │  → Carga lista: GET /partidas/
  │
  ├─ [Cargar partida] → GET /partidas/{id} + GET /partidas/{id}/ibermon/equipo
  │    → SessionManager.IniciarConPartida(...)
  │    → Carga el mapa guardado con el equipo de la API
  │
  └─ [Nueva partida] → POST /partidas/ + GET /partidas/{id}/ibermon/equipo
       → SessionManager.IniciarConPartida(...)
       → Carga la escena inicial
  ▼
Escenas de mapa (PuebloFuenlabrada, etc.)
  │  (el equipo se carga desde SessionManager al iniciar PokemonParty)
  │
  ├─ [Combate salvaje / entrenador]
  │    → Escena Combate
  │    → Al terminar: PATCH /partidas/{id}/ibermon/{ibId} para cada ibermon
  │    → Si captura: POST /partidas/{id}/ibermon/
  │    → Vuelve al mapa
  │
  └─ [Centro Ibermon / Guardar]  ← (por implementar en el mapa)
       → SessionManager.GuardarPartida(mapa, posicion, dinero, ...)
       → PUT /partidas/{id}/guardar
```

---

## 10. Resolución de problemas frecuentes

### "ibermon_catalogo_id=X no está en el catálogo"
→ La API no tiene datos. Asegúrate de que el servidor FastAPI está corriendo y el seed de la BD se ejecutó (`python seed.py` o el comando que uses).

### "No se encontró PokemonBase 'Ignifor' en Resources/Pokemons/"
→ El asset no está en la carpeta correcta, o su campo `Name` en el Inspector no coincide exactamente con el nombre del catálogo. Revisa paso 1 y 2.

### "No se encontró MoveBase 'Placaje'"
→ El asset de movimiento no está en `Resources/Moves/` ni referenciado en ningún `LearnableMoves`. Revisa paso 2.2.

### "ApiManager no encontrado"
→ `ApiSetup` no está en la escena Portada (primera escena). Revisa paso 3.

### "[401] Credenciales incorrectas" siempre
→ Verifica la `Base Url` en el Inspector de `ApiSetup`. Si usas `localhost`, la API debe estar corriendo en el mismo PC.

### Los ibermon no tienen movimientos
→ Sus `movimientos_aprendidos` en la BD están vacíos. Los movimientos que Init() asigna por nivel son el fallback. Verifica que el seed añade movimientos al ibermon inicial.

### La escena Login no aparece en Build
→ Revisa el paso 7 (Build Settings). La escena debe estar añadida y marcada con checkbox.

---

## Resumen rápido (checklist)

- [ ] Renombrar todos los PokemonBase ScriptableObjects para que coincidan con la API
- [ ] Renombrar todos los MoveBase ScriptableObjects para que coincidan con la API
- [ ] Mover PokemonBase a `Resources/Pokemons/`
- [ ] Mover MoveBase a `Resources/Moves/`
- [ ] Añadir `ApiSetup` a la escena Portada y configurar la URL
- [ ] Crear la escena `Login` con la UI descrita en el paso 4
- [ ] Actualizar la escena `Partidas` con la UI descrita en el paso 5
- [ ] Crear el prefab `PartidaEntry` con `PartidaEntryUI`
- [ ] Cambiar `ControllPortadaInicio.escenaDestino` a `"Login"`
- [ ] Añadir `Login` a Build Settings
- [ ] Probar: Portada → Login → Partidas → Juego
