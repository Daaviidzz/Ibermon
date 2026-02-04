using UnityEngine;

public class ControlesMoviles : MonoBehaviour
{
    public static ControlesMoviles Instance;

    [Header("Referencias UI")]
    public JoystickVirtual joystick;
    public ActivacionBoton botonInteraccion;
    public ActivacionBoton botonCorrer;
    public ActivacionBoton botonMenuOpciones;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}