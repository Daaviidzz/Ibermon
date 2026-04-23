using UnityEngine;

// Guarda en memoria la posicion y escena actuales del jugador
// Se actualiza continuamente desde Movimiento.cs
// Se usa al guardar la partida desde el menu de opciones
public static class PosicionActual
{
    public static string escena = "";
    public static Vector2 posicion = Vector2.zero;
}