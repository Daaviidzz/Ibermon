using System;

namespace ApiRest.Models
{
    // Version resumida de un item del catalogo
    [Serializable]
    public class ItemCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int precio;
    }
}
