# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Ibermon** is a Pokemon-like 2D RPG built in Unity 6 (version 6000.4.1f1) with Universal Render Pipeline (URP). Game state is synced with a FastAPI REST backend. Supports PC keyboard and mobile touch input.

## Build & Development

This is a Unity project — there are no CLI build commands. All development happens through the **Unity Editor**:
- Open the project in Unity 6000.4.1f1 (LTS)
- Unity recompiles C# automatically on file save
- Build via `File > Build Settings` in the editor
- Scenes are ordered in `ProjectSettings/EditorBuildSettings.asset`

Key packages (from `Packages/manifest.json`): DOTween, TextMesh Pro, Input System 1.19.0, URP 17.4.0.

## Code Architecture

All game code lives under `_Game/Scripts/`. Third-party assets are under `_ThirdParty/`.

### Singleton Managers (DontDestroyOnLoad)
These persist across scenes and are the backbone of the game:

- **`ApiManager.cs`** — HTTP client with JWT auth; all API calls go through here
- **`SessionManager.cs`** — Holds active partida (save), team (equipo), session stats; syncs pokemon state to API after battles via `SincronizarEquipo()`
- **`GameManager.cs`** — Player spawning and persistence
- **`CatalogoCache.cs`** — Pre-loaded ID↔Name mappings for pokemon/moves (populated at startup via `ApiSetup.cs` which runs at `DefaultExecutionOrder -100`)
- **`ControlesMoviles.cs`** — Mobile virtual joystick singleton

### Battle System (`Batalla/`)
`BattleSystem.cs` is a Finite State Machine (FSM) with states: `START → ACTIONSELECTION → MOVESELECTION → RUNNINGTURN → PARTYSCREEN → BATTLEOVER`.

Turn resolution (`RunTurns` coroutine):
1. Determine order by move Priority + pokemon Speed
2. Execute moves with damage formula: `d = ((2*L+10)/250) * Power * (Atk/Def) + 2`, multiplied by type effectiveness, critical hit, and RNG (0.85–1.0)
3. Apply post-turn effects (poison, etc.)
4. Check for faint → `HandlePokemonFainted` → `CheckBattleOver`

Battle context is passed between scenes via the static class `BattleData.cs` (wild pokemon) or trainer pokemon list.

### Data Model (`Pokemons/`)
- `PokemonBase.cs` — ScriptableObject template (base stats, types, learnable moves). Assets live in `Resources/Pokemons/*.asset`
- `Pokemon.cs` — Runtime instance (current HP, level, moves, status, boosts)
- `MoveBase.cs` — ScriptableObject template. Assets in `Resources/Moves/*.asset`
- `Move.cs` — Runtime instance (PP remaining)

**Critical**: ScriptableObject asset names must match API catalog names exactly (e.g., `"Ignifor"`). `IbermonConverter.EnsureResourcesLoaded()` discovers all pokemon/move assets via `Resources.LoadAll()`.

### API Integration (`ApiRest/`)
Services talk to a FastAPI backend. All use `ApiManager` for HTTP calls with JWT.

| Service | Responsibility |
|---|---|
| `AuthService.cs` | Login/logout, JWT token |
| `PartidaService.cs` | Save game CRUD |
| `IbermonJugadorService.cs` | Team pokemon management |
| `ItemJugadorService.cs` | Inventory |
| `CatalogoService.cs` | Pokemon/move catalogs |

**Data flow for catching a pokemon**:
`BattleSystem.ThrowPokeball()` → `PlayerParty.AddPokemon()` (local) → `SessionManager.AnadirIbermon()` → `POST /partidas/{id}/ibermon`

**Data flow after battle victory**:
`SessionManager.SincronizarEquipo()` → `IbermonConverter.ToActualizarRequest()` for each pokemon → `PUT /partidas/{id}/ibermon/{id}`

### Scene Transitions (`Transiciones/`)
- `JugadorSpawn.cs` stores the destination position and previous scene
- `CrearYPosicionarPlayer.cs` instantiates player at the correct spawn point on scene load
- `CambioDeEscena.cs` handles loading with transitions

### Input Handling
Dual-input: keyboard (PC) or virtual joystick + buttons (mobile). The `esMovil` flag in `BattleSystem.cs` switches between input sources. Battle input uses a 0.2s throttle to prevent double-inputs.

## Spanish Naming Convention
The entire codebase uses Spanish for game-domain terms. Key vocabulary:
- **Partida** = save/game session
- **Equipo** = team (of pokemon)
- **Ibermon/Pokémon** = game's pokemon equivalent
- **Combate** = battle
- **Movimiento** = move/movement (context-dependent)
- **Jugador** = player
- **Catalogo** = catalog of game entities
- **Escenas** = scenes
- **Pueblos** = towns
