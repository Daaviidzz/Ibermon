using TMPro;
using UnityEngine;

public class ControladorTextosUI : MonoBehaviour
{
    //Objetos que hacen referencia al cuadro de dialogo y al texto de su interior
    [SerializeField]
    private GameObject cajaTexto;
    [SerializeField]
    private TextMeshProUGUI textoDialogo;

    public bool estaActiva = false;

    //metodo para activar desactivar caja de textos
    public void activarDesactivarCajaDeTextos(bool activado)
    {
        cajaTexto.SetActive(activado);
        estaActiva = activado;
    }

    //Metodo para mostrar textos
    public void mostrarTextos(string texto)
    {
        textoDialogo.text = texto.ToString();
    }

}
