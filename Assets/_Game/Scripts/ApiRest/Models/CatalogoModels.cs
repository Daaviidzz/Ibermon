using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // --- IBERMON CATALOGO ---

    [Serializable]
    public class IbermonCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo1;
        public string tipo2;    // puede ser null
        public string sprite;
    }

    [Serializable]
    public class MovimientoPosible
    {
        public int numero;
        public int nivel;   // nivel mínimo al que el ibermon aprende el movimiento
    }

    [Serializable]
    public class IbermonCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public string tipo1;
        public string tipo2;
        public string descripcion;
        public int hp_base;
        public int ataque_base;
        public int defensa_base;
        public int ataque_especial_base;
        public int defensa_especial_base;
        public int velocidad_base;
        public List<MovimientoPosible> movimientos_posibles = new List<MovimientoPosible>();
        public int? evoluciona_a;
        public int? nivel_evolucion;
        public string sprite;
        public int catch_rate = 255;
        public int exp_yield = 100;
        public string growth_rate = "Medio";   // "Medio" o "Rapido"
    }

    // --- MOVIMIENTO CATALOGO ---

    [Serializable]
    public class MovimientoCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int potencia;
        public int pp;
    }

    [Serializable]
    public class MovimientoCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int potencia;
        public int precision;
        public int pp;
        public string descripcion;
        public string efecto;           // puede ser null
        public string categoria = "Fisico";     // "Fisico", "Especial" o "Estado"
        public string objetivo = "Foe";         // "Foe" o "Self"
        public bool siempre_acierta = false;
        public int prioridad = 0;
    }

    // --- ITEM CATALOGO ---

    [Serializable]
    public class EfectoItem
    {
        public string tipo_efecto;
        public string valor;    // simplificado como string desde JSON
    }

    [Serializable]
    public class ItemCatalogoResumen
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int precio;
    }

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

    // --- LOGRO CATALOGO ---

    [Serializable]
    public class LogroCatalogo
    {
        public string codigo;
        public string nombre;
        public string descripcion;
        public string condicion;
        public string icono;
    }

    // Wrappers de lista para JsonUtility (no soporta List<T> en la raíz)
    [Serializable] public class IbermonCatalogoResumenList { public List<IbermonCatalogoResumen> items; }
    [Serializable] public class MovimientoCatalogoResumenList { public List<MovimientoCatalogoResumen> items; }
    [Serializable] public class ItemCatalogoResumenList { public List<ItemCatalogoResumen> items; }
    [Serializable] public class LogroCatalogoList { public List<LogroCatalogo> items; }
}
