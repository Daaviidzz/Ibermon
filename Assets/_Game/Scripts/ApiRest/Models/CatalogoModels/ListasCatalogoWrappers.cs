using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Clases envoltorio para listas del catalogo
    // Se usan porque JsonUtility de Unity no puede deserializar una lista en la raiz del JSON
    // Por eso se envuelven dentro de un objeto con un campo items

    [Serializable]
    public class IbermonCatalogoResumenList
    {
        public List<IbermonCatalogoResumen> items;
    }

    [Serializable]
    public class MovimientoCatalogoResumenList
    {
        public List<MovimientoCatalogoResumen> items;
    }

    [Serializable]
    public class ItemCatalogoResumenList
    {
        public List<ItemCatalogoResumen> items;
    }

    [Serializable]
    public class LogroCatalogoList
    {
        public List<LogroCatalogo> items;
    }
}
