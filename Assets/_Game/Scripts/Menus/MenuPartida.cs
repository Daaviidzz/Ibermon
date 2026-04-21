using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script controla la escena donde el jugador elige que partida cargar
// o crea una nueva eligiendo nombre y personaje
// Flujo general:
//   1. Al entrar pide al servidor la lista de partidas
//   2. Si hay partidas muestra la lista, si no va directo a crear una nueva
//   3. Para crear una nueva pasa por el panel de nombre y luego por el de eleccion de personaje
public class MenuPartidas : MonoBehaviour
{
    // Paneles que se activan o desactivan segun lo que vaya haciendo el jugador
    [Header("Paneles")]
    public GameObject panelPartidas;
    public GameObject panelPartida;
    public GameObject panelEleccionDePersonaje;
    public GameObject panelCarga;

    // Componentes necesarios para mostrar la lista de partidas del jugador
    [Header("Lista de partidas")]
    public Transform contenedorPartidas;
    public GameObject prefabPartida;

    // Texto que aparece en el panel de carga con el mensaje actual
    [Header("Texto de carga")]
    public TextMeshProUGUI textoCarga;

    // Nombre que el jugador escribe en el panel de partida
    // Lo guarda el InputField llamando a GuardarNombre
    private string nombrePartidaActual = "Mi Partida";

    // Componente que se encarga de instanciar al personaje y cambiar de escena
    [Header("Personaje")]
    public CrearYPosicionarPlayer creadorPersonaje;

    // Se ejecuta automaticamente al arrancar la escena
    private void Start()
    {
        // Liberamos el cursor por si venia bloqueado desde el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarCarga("Cargando partidas...");
        CargarPartidas();
    }

    // Pide al servidor la lista de partidas del usuario y monta la interfaz segun el resultado
    private void CargarPartidas()
    {
        ApiSetup.Partida.ListarPartidas(ManejarListaPartidasRecibida, ManejarErrorListaPartidas);

        // Funcion local que se ejecuta cuando el servidor devuelve la lista de partidas
        // Si esta vacia pasa directo al panel de creacion de partida
        // Si tiene partidas crea una entrada por cada una y muestra la lista
        void ManejarListaPartidasRecibida(List<PartidaResumen> listaRecibida)
        {
            LimpiarLista();

            bool noTienePartidas = listaRecibida.Count == 0;
            if (noTienePartidas)
            {
                MostrarPanel(panelPartida);
                return;
            }

            foreach (PartidaResumen partida in listaRecibida)
            {
                CrearEntradaEnLista(partida);
            }

            MostrarPanel(panelPartidas);
        }

        // Funcion local que se ejecuta si hubo un error al pedir la lista al servidor
        // Si el token ha caducado volvemos al login, si no mostramos la lista vacia
        void ManejarErrorListaPartidas(string mensajeError)
        {
            Debug.LogError($"Error al cargar partidas: {mensajeError}");

            bool tokenExpirado = mensajeError.Contains("401");
            if (tokenExpirado)
            {
                SceneManager.LoadScene("Login");
                return;
            }

            MostrarPanel(panelPartidas);
        }
    }

    // Instancia el prefab de una partida en el contenedor y le pasa los callbacks
    private void CrearEntradaEnLista(PartidaResumen partida)
    {
        GameObject entradaInstanciada = Instantiate(prefabPartida, contenedorPartidas);
        PartidaEntryUI controladorEntrada = entradaInstanciada.GetComponent<PartidaEntryUI>();

        // Al pulsar la entrada se llama a CargarPartida
        // Al pulsar el boton de la X se llama a EliminarPartida
        controladorEntrada.Inicializar(partida, CargarPartida, EliminarPartida);
    }

    // Destruye todos los hijos del contenedor de la lista
    // Se usa para vaciar la lista antes de volver a rellenarla
    private void LimpiarLista()
    {
        foreach (Transform hijo in contenedorPartidas)
        {
            Destroy(hijo.gameObject);
        }
    }

    // Carga una partida existente a partir de su resumen
    // Hace dos llamadas al servidor, primero pide los datos completos y despues guarda
    // la partida para incrementar el contador de veces que se ha entrado
    private void CargarPartida(PartidaResumen resumenPartida)
    {
        MostrarCarga("Cargando partida...");

        ApiSetup.Partida.ObtenerPartida(resumenPartida.id,
            ManejarPartidaCompletaRecibida, ManejarErrorObtenerPartida);

        // Funcion local que se ejecuta cuando se reciben los datos completos de la partida
        // Prepara la peticion de guardado para incrementar el contador y llama a la API
        void ManejarPartidaCompletaRecibida(PartidaCompleta partidaRecibida)
        {
            MostrarCarga("Guardando progreso...");

            // Construimos la peticion de guardado copiando todos los datos actuales
            // y sumando uno al tiempo jugado como contador de entradas
            GuardarPartidaRequest datosGuardar = new GuardarPartidaRequest
            {
                mapa_actual = partidaRecibida.mapa_actual,
                posicion = partidaRecibida.posicion,
                dinero = partidaRecibida.dinero,
                tiempo_jugado = partidaRecibida.tiempo_jugado + 1,
                pokedex_visto = partidaRecibida.pokedex_visto,
                pokedex_capturado = partidaRecibida.pokedex_capturado,
                medallas = partidaRecibida.medallas,
                logros = partidaRecibida.logros,
                combates_ganados = partidaRecibida.combates_ganados,
                combates_perdidos = partidaRecibida.combates_perdidos,
                flags = partidaRecibida.flags,
            };

            ApiSetup.Partida.GuardarPartida(partidaRecibida.id, datosGuardar,
                ManejarGuardadoExitoso, ManejarGuardadoFallido);

            // Funcion local que se ejecuta cuando el guardado con el contador incrementado va bien
            // Iniciamos la sesion con la partida ya actualizada y cambiamos de escena
            void ManejarGuardadoExitoso(PartidaCompleta partidaActualizada)
            {
                SessionManager.Instance.IniciarConPartida(partidaActualizada, new List<IbermonJugador>());

                string escenaDestino = ElegirEscenaDestino(partidaActualizada.mapa_actual);
                creadorPersonaje.escenaDestino = escenaDestino;
                creadorPersonaje.crearEInstanciarPersonaje();
            }

            // Funcion local que se ejecuta si el guardado fallo
            // Aunque no se haya actualizado el contador, arrancamos la partida con los datos originales
            void ManejarGuardadoFallido(string mensajeError)
            {
                Debug.LogWarning($"No se pudo incrementar el contador: {mensajeError}");

                SessionManager.Instance.IniciarConPartida(partidaRecibida, new List<IbermonJugador>());

                string escenaDestino = ElegirEscenaDestino(partidaRecibida.mapa_actual);
                creadorPersonaje.escenaDestino = escenaDestino;
                creadorPersonaje.crearEInstanciarPersonaje();
            }
        }

        // Funcion local que se ejecuta si no se pudo obtener la partida del servidor
        // Volvemos a mostrar la lista de partidas
        void ManejarErrorObtenerPartida(string mensajeError)
        {
            Debug.LogError($"Error al cargar partida: {mensajeError}");
            MostrarPanel(panelPartidas);
        }
    }

    // Devuelve la escena a la que se debe ir segun el mapa guardado
    // Si no hay mapa guardado va a CasaPersonaje por defecto
    private string ElegirEscenaDestino(string mapaGuardado)
    {
        bool mapaVacio = string.IsNullOrEmpty(mapaGuardado);
        return mapaVacio ? "CasaPersonaje" : mapaGuardado;
    }

    // Borra una partida del servidor y recarga la lista
    private void EliminarPartida(PartidaResumen resumenPartida)
    {
        MostrarCarga("Eliminando partida...");

        ApiSetup.Partida.EliminarPartida(resumenPartida.id,
            ManejarEliminacionExitosa, ManejarEliminacionFallida);

        // Funcion local que se ejecuta cuando la partida se borra bien
        // Recargamos la lista para que el jugador vea el cambio
        void ManejarEliminacionExitosa()
        {
            CargarPartidas();
        }

        // Funcion local que se ejecuta si no se pudo borrar la partida
        void ManejarEliminacionFallida(string mensajeError)
        {
            Debug.LogError($"Error al eliminar partida: {mensajeError}");
            MostrarPanel(panelPartidas);
        }
    }

    // Metodo que llama el InputField del panel de partida cuando el jugador escribe el nombre
    // Si el jugador no escribe nada dejamos el nombre por defecto
    public void GuardarNombre(string nombreIntroducido)
    {
        bool nombreVacio = string.IsNullOrWhiteSpace(nombreIntroducido);
        nombrePartidaActual = nombreVacio ? "Mi Partida" : nombreIntroducido;
    }

    // Boton Continuar del panel de partida que lleva al panel de eleccion de personaje
    public void OnContinuar()
    {
        MostrarPanel(panelEleccionDePersonaje);
    }

    // Boton del panel de eleccion para elegir al personaje Torrente
    public void OnElegirTorrente()
    {
        CrearPartidaConPersonaje("torrente");
    }

    // Boton del panel de eleccion para elegir al otro personaje
    public void OnElegirPersonaje1()
    {
        CrearPartidaConPersonaje("personaje1");
    }

    // Llama a la API para crear la partida con el personaje elegido y arranca el juego
    private void CrearPartidaConPersonaje(string personajeElegido)
    {
        MostrarCarga("Creando partida...");

        // De momento el starter esta fijado al valor 1
        // En el futuro se hara una pantalla para elegirlo
        const int starterHardcodeado = 1;

        ApiSetup.Partida.CrearPartida(personajeElegido, starterHardcodeado,
            ManejarPartidaCreada, ManejarErrorCreacion);

        // Funcion local que se ejecuta cuando el servidor crea la partida correctamente
        // Inicia la sesion con los datos recibidos y carga la casa del personaje
        void ManejarPartidaCreada(PartidaCompleta partidaCreada)
        {
            SessionManager.Instance.IniciarConPartida(partidaCreada, new List<IbermonJugador>());
            creadorPersonaje.escenaDestino = "CasaPersonaje";
            creadorPersonaje.crearEInstanciarPersonaje();
        }

        // Funcion local que se ejecuta si el servidor devuelve error al crear la partida
        void ManejarErrorCreacion(string mensajeError)
        {
            Debug.LogError($"Error al crear partida: {mensajeError}");
            MostrarPanel(panelEleccionDePersonaje);
        }
    }

    // Boton Nueva del panel de partidas que lleva al panel donde se escribe el nombre
    public void OnNuevaPartida()
    {
        MostrarPanel(panelPartida);
    }

    // Boton Volver que devuelve al jugador al menu principal
    public void OnVolver()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    // Activa solo el panel indicado y desactiva los demas
    private void MostrarPanel(GameObject panelAMostrar)
    {
        panelPartidas.SetActive(panelAMostrar == panelPartidas);
        panelPartida.SetActive(panelAMostrar == panelPartida);
        panelEleccionDePersonaje.SetActive(panelAMostrar == panelEleccionDePersonaje);
        panelCarga.SetActive(false);
    }

    // Muestra el panel de carga con un mensaje concreto y oculta los demas
    private void MostrarCarga(string mensaje)
    {
        panelPartidas.SetActive(false);
        panelPartida.SetActive(false);
        panelEleccionDePersonaje.SetActive(false);
        panelCarga.SetActive(true);

        if (textoCarga != null)
        {
            textoCarga.text = mensaje;
        }
    }
}
