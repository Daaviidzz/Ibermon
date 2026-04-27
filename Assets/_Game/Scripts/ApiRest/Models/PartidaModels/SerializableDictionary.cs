using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Diccionario serializable que Unity si puede guardar y cargar
    // Se usa porque Dictionary normal no se puede serializar con JsonUtility
    // Guarda las claves y los valores en dos listas paralelas
    [Serializable]
    public class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<bool> values = new List<bool>();

        // Intenta obtener el valor asociado a una clave
        // Devuelve true si la clave existe y escribe el valor en el parametro de salida
        public bool TryGetValue(string key, out bool value)
        {
            int posicionClave = keys.IndexOf(key);
            if (posicionClave >= 0)
            {
                value = values[posicionClave];
                return true;
            }
            value = false;
            return false;
        }

        // Guarda un valor para una clave
        // Si la clave ya existe actualiza el valor, si no existe la anade
        public void Set(string key, bool value)
        {
            int posicionClave = keys.IndexOf(key);
            if (posicionClave >= 0)
            {
                values[posicionClave] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }
    }
}
