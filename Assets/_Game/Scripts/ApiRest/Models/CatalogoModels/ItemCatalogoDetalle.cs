using System;

namespace ApiRest.Models
{
    // Version completa de un item con todos sus datos
    [Serializable]
    public class ItemCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public string descripcion;
        public string tipo;
        public EfectoItem efecto;
        public int precio;
    }
}
