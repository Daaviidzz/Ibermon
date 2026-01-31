using System.Collections;
using System.Collections.Generic;
using TMPro; // Importante: Librería para manejar TextMeshPro (textos de alta calidad).
using UnityEngine;

public class BattleDialogBox : MonoBehaviour
{
    // --- REFERENCIAS DE UI Y CONFIGURACIÓN ---

    
    // Referencia al componente de texto principal donde se muestra el diálogo.
    [SerializeField] TextMeshProUGUI dialogText;
    
    [SerializeField] int lettersPerSecond;
    // Color para resaltar la opción seleccionada (Acción o Movimiento).
    [SerializeField] Color highlightedColor;

    
    // Panel que contiene las opciones principales: "Luchar", "Mochila", "Pokémon", "Huir".
    [SerializeField] GameObject actionSelector;
    // Panel que contiene la lista de movimientos (ataques) del Pokémon.
    [SerializeField] GameObject moveSelector;
    // Panel que muestra los detalles del movimiento seleccionado (PP y Tipo).
    [SerializeField] GameObject moveDetails;

    
    // Lista de textos de las acciones (Luchar, Mochila, etc.) para poder cambiarles el color.
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    // Lista de textos de los movimientos para mostrar sus nombres y cambiarles el color.
    [SerializeField] List<TextMeshProUGUI> moveTexts;

   
    // Referencias para mostrar los PP (Puntos de Poder) y el Tipo del ataque seleccionado.
    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    // --- MÉTODOS PÚBLICOS ---

    // Establece el texto del diálogo instantáneamente, sin animación.
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    // Muestra el texto del diálogo con un efecto de escritura (tipo máquina de escribir).
    // Es una Corrutina (IEnumerator) para poder usar tiempos de espera.
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = ""; // Limpiamos el texto actual.

        // Recorremos el string letra por letra.
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter; // Añadimos la letra.
            // Esperamos una fracción de segundo antes de la siguiente letra.
            // 1f / lettersPerSecond nos da el tiempo exacto por letra.
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        // Esperamos un segundo extra al finalizar para que el jugador pueda leer antes de continuar.
        yield return new WaitForSeconds(1f);
    }

    // Muestra u oculta el componente de texto del diálogo principal.
    // Útil si queremos ocultar el texto mientras se muestran menús.
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    // Muestra u oculta el selector de acciones (Luchar, Mochila, etc.).
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    // Muestra u oculta el selector de movimientos y sus detalles.
    public void EneableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    // Actualiza el color de los textos en el menú de Acciones para indicar cuál está seleccionado.
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            // Si el índice coincide con la acción seleccionada, le ponemos el color de resalte.
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black; // Si no, color negro estándar.
        }
    }

    // Actualiza el color de los textos en el menú de Movimientos y muestra los detalles (PP y Tipo).
    // Recibe el índice seleccionado y el objeto 'Move' correspondiente.
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        // 1. Actualizar colores de la lista de movimientos
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedMove)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;
        }

        // 2. Actualizar los detalles del panel (PP y Tipo)
        // Verificamos si el movimiento es nulo (por ejemplo, si el Pokémon tiene menos de 4 ataques).
        if (move == null)
        {
            ppText.text = $"PP: -/-";
            typeText.text = $"Tipo: -";
        }
        else
        {
            // Mostramos los PP actuales / PP máximos (Base).
            ppText.text = $"PP: {move.Pp}/{move.Base.Pp}";
            // Mostramos el tipo del movimiento.
            typeText.text = move.Base.Type.ToString();
        }
    }

    // Rellena los textos de la lista de movimientos con los nombres de los ataques del Pokémon.
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            // Si el índice 'i' está dentro del rango de movimientos que tiene el Pokémon...
            if (i < moves.Count)
            {
                // Asignamos el nombre del movimiento al texto.
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                // Si el Pokémon tiene menos de 4 movimientos, ponemos un guion en los huecos vacíos.
                moveTexts[i].text = "-";
            }
        }
    }
}