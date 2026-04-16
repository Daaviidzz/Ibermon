using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // --- SUBMODELOS ---

    [Serializable]
    public class Posicion
    {
        public float x;
        public float y;
    }

    // --- REQUESTS ---

    [Serializable]
    public class PartidaNuevaRequest
    {
        public string personaje_elegido;
        public int starter_elegido;
    }

    [Serializable]
    public class GuardarPartidaRequest
    {
        public string mapa_actual;
        public Posicion posicion;
        public int dinero;
        public int tiempo_jugado;
        public List<int> pokedex_visto = new List<int>();
        public List<int> pokedex_capturado = new List<int>();
        public List<string> medallas = new List<string>();
        public List<string> logros = new List<string>();
        public int combates_ganados;
        public int combates_perdidos;
        public SerializableDictionary flags = new SerializableDictionary();
    }

    [Serializable]
    public class ActualizarPosicionRequest
    {
        public string mapa_actual;
        public Posicion posicion;
    }

    // --- RESPONSES ---

    [Serializable]
    public class PartidaResumen
    {
        public string id;
        public string personaje_elegido;
        public string mapa_actual;
        public int tiempo_jugado;     // lo usamos como contador de entradas
        public int combates_ganados;
        public int combates_perdidos;
        // medallas puedes dejarlo o quitarlo, no lo usamos ahora
    }

    [Serializable]
    public class PartidaCompleta
    {
        public string id;
        public string usuario_id;
        public string personaje_elegido;
        public int starter_elegido;
        public string mapa_actual;
        public Posicion posicion;
        public int dinero;
        public int tiempo_jugado;
        public List<string> equipo = new List<string>();
        public List<string> centro_ibermon = new List<string>();
        public List<int> pokedex_visto = new List<int>();
        public List<int> pokedex_capturado = new List<int>();
        public List<string> medallas = new List<string>();
        public List<string> logros = new List<string>();
        public int combates_ganados;
        public int combates_perdidos;
        public SerializableDictionary flags = new SerializableDictionary();
    }

    // Diccionario serializable para flags (Dictionary<string,bool> no es serializable por Unity)
    [Serializable]
    public class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<bool> values = new List<bool>();

        public bool TryGetValue(string key, out bool value)
        {
            int index = keys.IndexOf(key);
            if (index >= 0) { value = values[index]; return true; }
            value = false;
            return false;
        }

        public void Set(string key, bool value)
        {
            int index = keys.IndexOf(key);
            if (index >= 0) values[index] = value;
            else { keys.Add(key); values.Add(value); }
        }
    }
}
