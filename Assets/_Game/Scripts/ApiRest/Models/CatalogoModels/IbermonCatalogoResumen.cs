using System;

namespace ApiRest.Models
{
    // Version resumida de un ibermon del catalogo
    // Se usa para listar todos los ibermon sin pedir tantos datos al servidor
    [Serializable]
    public class IbermonCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo1;
        // El segundo tipo puede ser nulo si el ibermon solo tiene un tipo
        public string tipo2;
        public string sprite;
    }
}
