# ApiRest — Documentación de uso

Capa de comunicación entre Unity y la API REST de Ibermon (FastAPI + MongoDB).

---

## Índice

1. [Configuración inicial](#1-configuración-inicial)
2. [AuthService — Registro y login](#2-authservice--registro-y-login)
3. [PartidaService — Gestión de partidas](#3-partidaservice--gestión-de-partidas)
4. [IbermonJugadorService — Equipo e ibermon](#4-ibermonjugadorservice--equipo-e-ibermon)
5. [ItemJugadorService — Inventario](#5-itemjugadorservice--inventario)
6. [CatalogoService — Datos del juego](#6-catalogoservice--datos-del-juego)
7. [Gestión de errores](#7-gestión-de-errores)
8. [Flujo completo de ejemplo](#8-flujo-completo-de-ejemplo)

---

## 1. Configuración inicial

### Añadir ApiSetup a la escena

1. En la **primera escena** del juego (menú principal / splash), crea un **GameObject vacío**.
2. Nómbralo `_ApiSetup` (el guion bajo lo pone primero en la jerarquía).
3. Añádele el componente **`ApiSetup`**.
4. En el Inspector, configura la **Base Url** según el entorno:

| Entorno | URL |
|---|---|
| Desarrollo local | `http://localhost:8000` |
| Red local (otro PC) | `http://192.168.X.X:8000` |
| Producción | `https://api.ibermon.com` |

`ApiSetup` crea automáticamente el singleton `ApiManager` y todos los servicios con `DontDestroyOnLoad`, por lo que **solo hace falta añadirlo una vez**.

### Acceder a los servicios

Desde cualquier script del juego:

```csharp
ApiSetup.Auth
ApiSetup.Partida
ApiSetup.IbermonJugador
ApiSetup.ItemJugador
ApiSetup.Catalogo
```

### Patrón de callbacks

Todos los métodos usan dos callbacks:

```csharp
Action<T> onSuccess   // se llama con el resultado si todo va bien
Action<string> onError  // se llama con el mensaje de error si algo falla
```

Las llamadas son **asíncronas** (no bloquean el hilo principal de Unity).

---

## 2. AuthService — Registro y login

### Registrar un usuario nuevo

```csharp
ApiSetup.Auth.Registrar(
    username: "david",
    email: "david@correo.com",
    password: "miPassword123",
    onSuccess: usuario =>
    {
        Debug.Log($"Registrado: {usuario.username} (id: {usuario.id})");
        // Redirigir al menú de login
    },
    onError: err => Debug.LogError($"Error al registrar: {err}")
);
```

**Devuelve:** `UsuarioPublico`

| Campo | Tipo | Descripción |
|---|---|---|
| `id` | string | ID del usuario en MongoDB |
| `username` | string | Nombre de usuario |
| `email` | string | Correo electrónico |
| `fecha_registro` | string | Fecha ISO 8601 |
| `partidas` | List\<string\> | IDs de las partidas del usuario |

---

### Login

```csharp
ApiSetup.Auth.Login(
    username: "david",
    password: "miPassword123",
    onSuccess: token =>
    {
        Debug.Log("Login correcto, token guardado automáticamente");
        // El token JWT queda guardado en ApiManager.
        // Ya puedes llamar a endpoints protegidos.
        CargarMenuPrincipal();
    },
    onError: err => Debug.LogError($"Credenciales incorrectas: {err}")
);
```

> El token JWT se guarda automáticamente en `ApiManager`. No necesitas manejarlo manualmente.

---

### Obtener el usuario autenticado

Útil para mostrar el nombre del usuario en la UI.

```csharp
ApiSetup.Auth.ObtenerUsuarioActual(
    onSuccess: usuario => txtNombre.text = usuario.username,
    onError: err => Debug.LogError(err)
);
```

---

### Logout

Borra el token localmente (no hay endpoint de logout en la API).

```csharp
ApiSetup.Auth.Logout();
// Redirigir a la pantalla de login
```

---

## 3. PartidaService — Gestión de partidas

### Crear una partida nueva

Se llama al elegir personaje y starter al inicio del juego.

```csharp
ApiSetup.Partida.CrearPartida(
    personajeElegido: "chico",
    starterElegido: 1,          // número del ibermon starter
    onSuccess: partida =>
    {
        Debug.Log($"Partida creada: {partida.id}");
        PlayerPrefs.SetString("partida_id", partida.id);
        IniciarJuego(partida);
    },
    onError: err => Debug.LogError(err)
);
```

---

### Listar las partidas del usuario

Para la pantalla de "Cargar partida".

```csharp
ApiSetup.Partida.ListarPartidas(
    onSuccess: lista =>
    {
        foreach (var resumen in lista)
        {
            Debug.Log($"[{resumen.id}] Mapa: {resumen.mapa_actual} " +
                      $"| Medallas: {resumen.medallas.Count} " +
                      $"| Tiempo: {resumen.tiempo_jugado}s");
        }
    },
    onError: err => Debug.LogError(err)
);
```

**`PartidaResumen`** contiene: `id`, `personaje_elegido`, `mapa_actual`, `tiempo_jugado`, `medallas`, `combates_ganados`, `combates_perdidos`.

---

### Cargar una partida completa

```csharp
string partidaId = PlayerPrefs.GetString("partida_id");

ApiSetup.Partida.ObtenerPartida(
    partidaId: partidaId,
    onSuccess: partida =>
    {
        // Restaurar estado del juego
        MapaManager.CargarMapa(partida.mapa_actual);
        Jugador.transform.position = new Vector2(partida.posicion.x, partida.posicion.y);
        InventarioManager.Dinero = partida.dinero;
    },
    onError: err => Debug.LogError(err)
);
```

---

### Guardar la partida

Llámalo al guardar en un Centro Ibermon o al salir del juego.

```csharp
var datos = new GuardarPartidaRequest
{
    mapa_actual       = MapaManager.MapaActual,
    posicion          = new Posicion { x = jugador.x, y = jugador.y },
    dinero            = InventarioManager.Dinero,
    tiempo_jugado     = TiempoJugado,
    pokedex_visto     = Pokedex.Vistos,
    pokedex_capturado = Pokedex.Capturados,
    medallas          = MedallaManager.Medallas,
    logros            = LogroManager.Logros,
    combates_ganados  = EstadisticasManager.Ganados,
    combates_perdidos = EstadisticasManager.Perdidos,
};
datos.flags.Set("intro_vista", true);
datos.flags.Set("rival_derrotado", false);

ApiSetup.Partida.GuardarPartida(
    partidaId: _partidaId,
    datos: datos,
    onSuccess: partida => Debug.Log("Partida guardada"),
    onError: err => Debug.LogError(err)
);
```

---

### Actualizar posición (guardado rápido)

Para actualizaciones frecuentes de posición sin guardar todo el estado.

```csharp
ApiSetup.Partida.ActualizarPosicion(
    partidaId: _partidaId,
    mapaActual: "mapa_ciudad_1",
    x: jugador.transform.position.x,
    y: jugador.transform.position.y,
    onSuccess: _ => { },    // ignorar respuesta si no se necesita
    onError: err => Debug.LogWarning(err)
);
```

---

### Eliminar una partida

```csharp
ApiSetup.Partida.EliminarPartida(
    partidaId: _partidaId,
    onSuccess: () => Debug.Log("Partida eliminada"),
    onError: err => Debug.LogError(err)
);
```

---

## 4. IbermonJugadorService — Equipo e ibermon

### Ver el equipo

```csharp
ApiSetup.IbermonJugador.ObtenerEquipo(
    partidaId: _partidaId,
    onSuccess: equipo =>
    {
        foreach (var ib in equipo)
            Debug.Log($"{ib.nickname ?? "Sin nombre"} | Nv.{ib.nivel} | HP:{ib.hp_actual}");
    },
    onError: err => Debug.LogError(err)
);
```

---

### Ver el centro Ibermon (PC)

```csharp
ApiSetup.IbermonJugador.ObtenerCentro(
    partidaId: _partidaId,
    onSuccess: centro => Debug.Log($"Ibermon en el centro: {centro.Count}"),
    onError: err => Debug.LogError(err)
);
```

---

### Añadir un ibermon capturado

Se llama al capturar un ibermon en combate.

```csharp
var datos = new IbermonJugadorCrearRequest
{
    ibermon_catalogo_id = 4,          // número del ibermon en el catálogo
    nickname            = null,       // sin apodo de momento
    nivel               = 5,
    hp_actual           = 35,
    ubicacion           = "equipo",   // "equipo" o "centro"
};

ApiSetup.IbermonJugador.AnadirIbermon(
    partidaId: _partidaId,
    datos: datos,
    onSuccess: ibermon => Debug.Log($"Capturado: ID {ibermon.id}"),
    onError: err => Debug.LogError(err)
);
```

---

### Actualizar ibermon tras un combate

```csharp
var datos = new IbermonJugadorActualizarRequest
{
    nivel       = 12,
    experiencia = 340,
    hp_actual   = 28,
};

ApiSetup.IbermonJugador.ActualizarIbermon(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    datos: datos,
    onSuccess: ib => Debug.Log($"Nv.{ib.nivel} | XP:{ib.experiencia}"),
    onError: err => Debug.LogError(err)
);
```

---

### Mover un ibermon entre equipo y centro

```csharp
// Mandar al centro
ApiSetup.IbermonJugador.MoverIbermon(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    ubicacion: "centro",
    onSuccess: ib => Debug.Log($"{ib.id} movido a {ib.ubicacion}"),
    onError: err => Debug.LogError(err)
);
```

---

### Gestionar movimientos

**Aprender un movimiento:**
```csharp
ApiSetup.IbermonJugador.AprenderMovimiento(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    numeroMovimiento: 15,
    onSuccess: ib => Debug.Log("Movimiento aprendido"),
    onError: err => Debug.LogError(err)   // error si ya tiene 4 movimientos
);
```

**Olvidar un movimiento:**
```csharp
ApiSetup.IbermonJugador.OlvidarMovimiento(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    numeroMovimiento: 15,
    onSuccess: () => Debug.Log("Movimiento olvidado"),
    onError: err => Debug.LogError(err)
);
```

**Reemplazar todos los movimientos a la vez:**
```csharp
var movimientos = new List<int> { 10, 15, 22, 8 };  // máximo 4

ApiSetup.IbermonJugador.ActualizarMovimientos(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    movimientos: movimientos,
    onSuccess: ib => Debug.Log("Movimientos actualizados"),
    onError: err => Debug.LogError(err)
);
```

---

### Eliminar un ibermon

```csharp
ApiSetup.IbermonJugador.EliminarIbermon(
    partidaId: _partidaId,
    ibermonId: _ibermonId,
    onSuccess: () => Debug.Log("Ibermon liberado"),
    onError: err => Debug.LogError(err)
);
```

---

## 5. ItemJugadorService — Inventario

### Ver el inventario

```csharp
ApiSetup.ItemJugador.ObtenerInventario(
    partidaId: _partidaId,
    onSuccess: items =>
    {
        foreach (var item in items)
            Debug.Log($"Item #{item.item_catalogo_id} x{item.cantidad}");
    },
    onError: err => Debug.LogError(err)
);
```

---

### Añadir un item

Se llama al comprar en tienda o recoger del suelo.

```csharp
ApiSetup.ItemJugador.AnadirItem(
    partidaId: _partidaId,
    itemCatalogoId: 3,      // número del item en el catálogo
    cantidad: 5,
    onSuccess: item => Debug.Log($"Item añadido: id {item.id}"),
    onError: err => Debug.LogError(err)
);
```

---

### Actualizar cantidad de un item

Se llama al usar un item (reducir cantidad).

```csharp
ApiSetup.ItemJugador.ActualizarItem(
    partidaId: _partidaId,
    itemId: _itemId,
    nuevaCantidad: 3,
    onSuccess: item => Debug.Log($"Cantidad actualizada: {item.cantidad}"),
    onError: err => Debug.LogError(err)
);
```

---

### Eliminar un item del inventario

Cuando la cantidad llega a 0.

```csharp
ApiSetup.ItemJugador.EliminarItem(
    partidaId: _partidaId,
    itemId: _itemId,
    onSuccess: () => Debug.Log("Item eliminado"),
    onError: err => Debug.LogError(err)
);
```

---

## 6. CatalogoService — Datos del juego

Los catálogos son **públicos** (no requieren login). Úsalos para cargar datos del juego al inicio o bajo demanda.

### Catálogo de Ibermon (Pokédex)

```csharp
// Lista completa
ApiSetup.Catalogo.ListarIbermon(
    onSuccess: lista => Debug.Log($"Total ibermon: {lista.Count}"),
    onError: err => Debug.LogError(err)
);

// Detalle de uno concreto
ApiSetup.Catalogo.ObtenerIbermon(
    numero: 1,
    onSuccess: ib =>
    {
        Debug.Log($"{ib.nombre} | {ib.tipo1}/{ib.tipo2}");
        Debug.Log($"HP:{ib.hp_base} ATK:{ib.ataque_base} DEF:{ib.defensa_base}");
    },
    onError: err => Debug.LogError(err)
);
```

---

### Catálogo de movimientos

```csharp
ApiSetup.Catalogo.ListarMovimientos(
    onSuccess: lista => Debug.Log($"Total movimientos: {lista.Count}"),
    onError: err => Debug.LogError(err)
);

ApiSetup.Catalogo.ObtenerMovimiento(
    numero: 15,
    onSuccess: mov => Debug.Log($"{mov.nombre} | {mov.tipo} | {mov.potencia} daño | {mov.pp} PP"),
    onError: err => Debug.LogError(err)
);
```

---

### Catálogo de items

```csharp
ApiSetup.Catalogo.ListarItems(
    onSuccess: lista => Debug.Log($"Total items: {lista.Count}"),
    onError: err => Debug.LogError(err)
);

ApiSetup.Catalogo.ObtenerItem(
    numero: 3,
    onSuccess: item => Debug.Log($"{item.nombre} | {item.tipo} | {item.precio} monedas"),
    onError: err => Debug.LogError(err)
);
```

---

### Catálogo de logros

```csharp
ApiSetup.Catalogo.ListarLogros(
    onSuccess: lista => Debug.Log($"Total logros: {lista.Count}"),
    onError: err => Debug.LogError(err)
);

ApiSetup.Catalogo.ObtenerLogro(
    codigo: "PRIMERA_MEDALLA",
    onSuccess: logro => Debug.Log($"{logro.nombre}: {logro.descripcion}"),
    onError: err => Debug.LogError(err)
);
```

---

## 7. Gestión de errores

El callback `onError` recibe un string con el formato:

```
[CÓDIGO_HTTP] mensaje_del_servidor
```

Ejemplos:
```
[401] Credenciales incorrectas
[404] Partida no encontrada
[400] El ibermon ya tiene 4 movimientos
[422] Validation error: campo requerido
```

### Patrón recomendado

```csharp
ApiSetup.Partida.ObtenerPartida(_partidaId,
    onSuccess: partida => AplicarEstado(partida),
    onError: err =>
    {
        if (err.StartsWith("[401]"))
        {
            // Token expirado — mandar al login
            ApiSetup.Auth.Logout();
            SceneManager.LoadScene("Login");
        }
        else if (err.StartsWith("[404]"))
        {
            Debug.LogWarning("Partida no encontrada");
            SceneManager.LoadScene("MenuPrincipal");
        }
        else
        {
            UIManager.MostrarError("Error de conexión. Inténtalo de nuevo.");
            Debug.LogError(err);
        }
    }
);
```

---

## 8. Flujo completo de ejemplo

El siguiente ejemplo cubre el flujo típico: login → cargar partida → guardar al llegar a un Centro Ibermon.

```csharp
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private string _partidaId;

    // --- Paso 1: Login ---
    public void OnClickLogin(string usuario, string password)
    {
        ApiSetup.Auth.Login(usuario, password,
            onSuccess: token =>
            {
                Debug.Log("Login correcto");
                CargarListaPartidas();
            },
            onError: err => UIManager.MostrarError("Usuario o contraseña incorrectos")
        );
    }

    // --- Paso 2: Listar y elegir partida ---
    private void CargarListaPartidas()
    {
        ApiSetup.Partida.ListarPartidas(
            onSuccess: lista =>
            {
                if (lista.Count == 0)
                    SceneManager.LoadScene("NuevaPartida");
                else
                    UIManager.MostrarSelectorPartidas(lista);
            },
            onError: err => Debug.LogError(err)
        );
    }

    // --- Paso 3: Cargar partida seleccionada ---
    public void OnPartidaSeleccionada(string partidaId)
    {
        _partidaId = partidaId;

        ApiSetup.Partida.ObtenerPartida(partidaId,
            onSuccess: partida =>
            {
                // Restaurar estado completo del juego
                MapaManager.CargarMapa(partida.mapa_actual);
                Jugador.SetPosicion(partida.posicion.x, partida.posicion.y);
                InventarioManager.Dinero = partida.dinero;
                Pokedex.Restaurar(partida.pokedex_visto, partida.pokedex_capturado);

                // Cargar equipo
                ApiSetup.IbermonJugador.ObtenerEquipo(partidaId,
                    equipo => EquipoManager.Cargar(equipo),
                    err => Debug.LogError(err)
                );

                // Cargar inventario
                ApiSetup.ItemJugador.ObtenerInventario(partidaId,
                    items => InventarioManager.Cargar(items),
                    err => Debug.LogError(err)
                );
            },
            onError: err => Debug.LogError(err)
        );
    }

    // --- Paso 4: Guardar en Centro Ibermon ---
    public void GuardarPartida()
    {
        var datos = new GuardarPartidaRequest
        {
            mapa_actual       = MapaManager.MapaActual,
            posicion          = new Posicion { x = Jugador.X, y = Jugador.Y },
            dinero            = InventarioManager.Dinero,
            tiempo_jugado     = TiempoJugado.Segundos,
            pokedex_visto     = Pokedex.Vistos,
            pokedex_capturado = Pokedex.Capturados,
            medallas          = MedallaManager.Lista,
            logros            = LogroManager.Lista,
            combates_ganados  = Estadisticas.Ganados,
            combates_perdidos = Estadisticas.Perdidos,
        };

        ApiSetup.Partida.GuardarPartida(_partidaId, datos,
            onSuccess: _ => UIManager.MostrarMensaje("¡Partida guardada!"),
            onError: err => UIManager.MostrarError("No se pudo guardar")
        );
    }
}
```

---

## Referencia rápida de endpoints

| Servicio | Método | Endpoint API |
|---|---|---|
| `Auth.Registrar` | POST | `/auth/registro` |
| `Auth.Login` | POST | `/auth/login` |
| `Auth.ObtenerUsuarioActual` | GET | `/auth/yo` |
| `Partida.CrearPartida` | POST | `/partidas/` |
| `Partida.ListarPartidas` | GET | `/partidas/` |
| `Partida.ObtenerPartida` | GET | `/partidas/{id}` |
| `Partida.GuardarPartida` | PUT | `/partidas/{id}/guardar` |
| `Partida.ActualizarPosicion` | PATCH | `/partidas/{id}/posicion` |
| `Partida.EliminarPartida` | DELETE | `/partidas/{id}` |
| `IbermonJugador.ObtenerEquipo` | GET | `/partidas/{id}/ibermon/equipo` |
| `IbermonJugador.ObtenerCentro` | GET | `/partidas/{id}/ibermon/centro` |
| `IbermonJugador.AnadirIbermon` | POST | `/partidas/{id}/ibermon/` |
| `IbermonJugador.MoverIbermon` | PATCH | `/partidas/{id}/ibermon/{ibId}/mover` |
| `IbermonJugador.ActualizarIbermon` | PATCH | `/partidas/{id}/ibermon/{ibId}` |
| `IbermonJugador.EliminarIbermon` | DELETE | `/partidas/{id}/ibermon/{ibId}` |
| `IbermonJugador.AprenderMovimiento` | POST | `/partidas/{id}/ibermon/{ibId}/movimientos/{num}` |
| `IbermonJugador.OlvidarMovimiento` | DELETE | `/partidas/{id}/ibermon/{ibId}/movimientos/{num}` |
| `IbermonJugador.ActualizarMovimientos` | PUT | `/partidas/{id}/ibermon/{ibId}/movimientos` |
| `ItemJugador.ObtenerInventario` | GET | `/partidas/{id}/items/` |
| `ItemJugador.AnadirItem` | POST | `/partidas/{id}/items/` |
| `ItemJugador.ActualizarItem` | PATCH | `/partidas/{id}/items/{itemId}` |
| `ItemJugador.EliminarItem` | DELETE | `/partidas/{id}/items/{itemId}` |
| `Catalogo.ListarIbermon` | GET | `/catalogo/ibermon` |
| `Catalogo.ObtenerIbermon` | GET | `/catalogo/ibermon/{num}` |
| `Catalogo.ListarMovimientos` | GET | `/catalogo/movimientos` |
| `Catalogo.ObtenerMovimiento` | GET | `/catalogo/movimientos/{num}` |
| `Catalogo.ListarItems` | GET | `/catalogo/items` |
| `Catalogo.ObtenerItem` | GET | `/catalogo/items/{num}` |
| `Catalogo.ListarLogros` | GET | `/catalogo/logros` |
| `Catalogo.ObtenerLogro` | GET | `/catalogo/logros/{codigo}` |
