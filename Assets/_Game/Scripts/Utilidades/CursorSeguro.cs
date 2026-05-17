using UnityEngine;

public static class CursorSeguro
{
    public static void Bloquear()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    public static void Mostrar()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
