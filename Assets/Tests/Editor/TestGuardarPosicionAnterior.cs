using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-13 — Tests para GuardarPosicionAnterior (Bloque 3 - Jugador y Transiciones)
//
//  GuardarPosicionAnterior es una clase estática muy simple que actúa de "canal
//  estático" entre escenas. Almacena dónde estaba el jugador antes de abrir el
//  menú Opciones para restaurarlo al volver. Aunque la clase es trivial, los
//  bugs en estado global suelen ser difíciles de cazar, así que validamos
//  que se comporta como se espera.
// =====================================================================================
public class TestGuardarPosicionAnterior
{
    [SetUp]
    public void Setup()
    {
        // Reseteamos a valores conocidos antes de cada test para evitar contaminación
        GuardarPosicionAnterior.escenaAnterior = "";
        GuardarPosicionAnterior.posicionAnterior = Vector3.zero;
    }

    // -----------------------------------------------------------------------------------
    //  PR-13.1 - Almacena nombre de escena correctamente
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR13_01_EscenaAnterior_SeAlmacena()
    {
        GuardarPosicionAnterior.escenaAnterior = "PuebloFuenlabrada";

        Assert.AreEqual("PuebloFuenlabrada", GuardarPosicionAnterior.escenaAnterior);
    }

    // -----------------------------------------------------------------------------------
    //  PR-13.2 - Almacena Vector3 de posición sin perder componentes
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR13_02_PosicionAnterior_ConservaXYZ()
    {
        var pos = new Vector3(15.5f, -3.2f, 0f);
        GuardarPosicionAnterior.posicionAnterior = pos;

        Assert.AreEqual(15.5f, GuardarPosicionAnterior.posicionAnterior.x, 0.001f);
        Assert.AreEqual(-3.2f, GuardarPosicionAnterior.posicionAnterior.y, 0.001f);
        Assert.AreEqual(0f, GuardarPosicionAnterior.posicionAnterior.z, 0.001f);
    }

    // -----------------------------------------------------------------------------------
    //  PR-13.3 - Estado inicial tras reset es escena vacía y Vector3.zero
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR13_03_ResetAValoresVaciosPorDefecto()
    {
        // Setup() ya hace el reset; aquí solo lo verificamos
        Assert.AreEqual("", GuardarPosicionAnterior.escenaAnterior);
        Assert.AreEqual(Vector3.zero, GuardarPosicionAnterior.posicionAnterior);
    }
}
