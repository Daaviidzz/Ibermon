using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Datos completos de una partida tal como los devuelve el servidor
    // Contiene toda la informacion necesaria para cargar el juego
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
}
