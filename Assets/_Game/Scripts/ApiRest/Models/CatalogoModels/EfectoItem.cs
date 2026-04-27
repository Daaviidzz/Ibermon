using System;

namespace ApiRest.Models
{
    // Efecto que produce un item al usarlo
    [Serializable]
    public class EfectoItem
    {
        public string tipo_efecto;
        // El valor se guarda como string porque desde el JSON puede venir de varios tipos
        public string valor;
    }
}
