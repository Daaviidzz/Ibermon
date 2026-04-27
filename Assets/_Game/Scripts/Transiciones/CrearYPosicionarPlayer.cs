using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrearYPosicionarPlayer : MonoBehaviour
{
    // Escena a la que se navegara despues de instanciar al personaje
    public string escenaDestino;

    // Posicion donde aparecera el personaje al entrar en la escena
    public Vector2 posicionSpawn;

    // Lista de personajes disponibles, asignar desde el editor en el mismo orden
    // que los identificadores de personaje que devuelve la API
    // Ejemplo: indice 0 → "torrente", indice 1 → "personaje1"
    public List<GameObject> personajes;

    // Identificador del personaje activo, se asigna antes de llamar a crearEInstanciarPersonaje
    // Debe coincidir con el valor de personaje_elegido que devuelve la API
    [HideInInspector]
    public string personajeElegido;

    // Devuelve el prefab que corresponde al identificador dado
    // Si no se reconoce el identificador usa el primero de la lista como fallback
    private GameObject ObtenerPrefabPersonaje(string identificador)
    {
        // Mapeamos el nombre del personaje a su indice en la lista
        // El orden debe coincidir con el que hayas puesto en el inspector
        Dictionary<string, int> indicesPorNombre = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "torrente",    0 },
            { "personaje1",  1 },
        };

        bool encontrado = indicesPorNombre.TryGetValue(identificador, out int indice);

        if (!encontrado || indice >= personajes.Count)
        {
            Debug.LogWarning($"[CrearYPosicionarPlayer] Personaje '{identificador}' no reconocido, usando el primero de la lista");
            return personajes.Count > 0 ? personajes[0] : null;
        }

        return personajes[indice];
    }

    public void crearEInstanciarPersonaje()
    {
        GameObject prefab = ObtenerPrefabPersonaje(personajeElegido);

        if (prefab == null)
        {
            Debug.LogError("[CrearYPosicionarPlayer] No hay personajes en la lista, no se puede instanciar");
            return;
        }

        // Instanciamos el personaje solo si no existe ya en la escena
        if (GameObject.FindWithTag("Player") == null)
        {
            Instantiate(prefab);
        }

        // Solo sobreescribimos la posicion si NO hay una posicion guardada de la API esperando
        // Si usarPosicionGuardada es true significa que MenuPartidas ya asigno la posicion correcta
        if (!JugadorSpawn.usarPosicionGuardada)
        {
            JugadorSpawn.posicion = posicionSpawn;
        }

        SceneManager.LoadScene(escenaDestino);
    }
}