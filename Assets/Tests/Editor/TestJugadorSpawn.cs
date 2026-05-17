using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-14 — Tests para JugadorSpawn (Bloque 3 - Transiciones)
//
//  JugadorSpawn es el componente clave para mantener al jugador con vida entre
//  escenas (DontDestroyOnLoad). Mantiene 3 variables estáticas:
//    · posicion : Vector2 — dónde aparecer al cargar la siguiente escena
//    · escenaAnterior : string — qué escena estaba antes del cambio
//    · usarPosicionGuardada : bool — flag para cargar posición exacta desde API
//
//  Y un HashSet privado estático _escenasOcultas que lista las escenas donde
//  el jugador debe estar invisible (menús, combate). Cualquier cambio en esa
//  lista podría romper la experiencia, así que la verificamos.
// =====================================================================================
public class TestJugadorSpawn
{
    [SetUp]
    public void Setup()
    {
        // Reset de las variables estáticas antes de cada test
        JugadorSpawn.posicion = Vector2.zero;
        JugadorSpawn.escenaAnterior = "";
        JugadorSpawn.usarPosicionGuardada = false;
    }

    // -----------------------------------------------------------------------------------
    //  PR-14.1 - posicion se almacena y recupera correctamente
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR14_01_Posicion_SeAlmacena()
    {
        var coordenada = new Vector2(7.5f, -2.3f);
        JugadorSpawn.posicion = coordenada;

        Assert.AreEqual(7.5f, JugadorSpawn.posicion.x, 0.001f);
        Assert.AreEqual(-2.3f, JugadorSpawn.posicion.y, 0.001f);
    }

    // -----------------------------------------------------------------------------------
    //  PR-14.2 - escenaAnterior se almacena (usado por AutoVolverEscena)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR14_02_EscenaAnterior_SeAlmacena()
    {
        JugadorSpawn.escenaAnterior = "Ruta1";

        Assert.AreEqual("Ruta1", JugadorSpawn.escenaAnterior);
    }

    // -----------------------------------------------------------------------------------
    //  PR-14.3 - usarPosicionGuardada es bool con default false
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR14_03_UsarPosicionGuardada_DefaultFalse()
    {
        Assert.IsFalse(JugadorSpawn.usarPosicionGuardada,
            "usarPosicionGuardada debe ser false al inicio para que el spawn por defecto funcione");

        JugadorSpawn.usarPosicionGuardada = true;
        Assert.IsTrue(JugadorSpawn.usarPosicionGuardada);
    }

    // -----------------------------------------------------------------------------------
    //  PR-14.4 - HashSet _escenasOcultas contiene las escenas de menú principales
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR14_04_EscenasOcultas_ContieneMenusYCombate()
    {
        // El HashSet es private static readonly, lo accedemos vía reflection
        var field = typeof(JugadorSpawn).GetField("_escenasOcultas",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(field, "No se encontró el campo _escenasOcultas");

        var escenas = (HashSet<string>)field.GetValue(null);

        // Estas escenas DEBEN estar en la lista o el jugador aparecerá visible donde no debe
        Assert.IsTrue(escenas.Contains("Portada"),    "Portada debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("Login"),       "Login debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("MenuPrincipal"), "MenuPrincipal debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("Partidas"),    "Partidas debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("Creditos"),    "Creditos debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("Opciones"),    "Opciones debe estar en _escenasOcultas");
        Assert.IsTrue(escenas.Contains("Combate"),     "Combate debe estar en _escenasOcultas");
    }

    // -----------------------------------------------------------------------------------
    //  PR-14.5 - Las escenas de mapa NO están en _escenasOcultas
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR14_05_EscenasOcultas_NoContieneEscenasDeMapa()
    {
        var field = typeof(JugadorSpawn).GetField("_escenasOcultas",
            BindingFlags.NonPublic | BindingFlags.Static);
        var escenas = (HashSet<string>)field.GetValue(null);

        Assert.IsFalse(escenas.Contains("PuebloFuenlabrada"),
            "Las escenas de mapa NO deben estar en _escenasOcultas");
        Assert.IsFalse(escenas.Contains("CasaPersonaje"),
            "CasaPersonaje (mapa) NO debe estar en _escenasOcultas");
    }
}
