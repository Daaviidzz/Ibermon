using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Datos que se envian al servidor para guardar el estado completo de la partida
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
}
