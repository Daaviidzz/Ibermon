using System;

namespace ApiRest.Models
{
    // Version resumida de un movimiento del catalogo
    // Solo trae los datos basicos para mostrarlos en listas
    [Serializable]
    public class MovimientoCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int potencia;
        public int pp;
    }
}
