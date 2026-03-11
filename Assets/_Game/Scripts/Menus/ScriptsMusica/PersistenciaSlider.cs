using UnityEngine;
using UnityEngine.UI;
public class PersistenciaSlider : MonoBehaviour
{
    void Start()
    {
        // Esto busca el valor que guardamos en PlayerPrefs y mueve la barrita sola
        float valorGuardado = PlayerPrefs.GetFloat("VolumenGuardado", 0.75f);
        GetComponent<Slider>().value = valorGuardado;
    }
}
