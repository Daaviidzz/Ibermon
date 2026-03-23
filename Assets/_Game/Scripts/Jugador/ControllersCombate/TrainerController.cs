using Assets.Scripts.Batalla;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DireccionEntrenador { Arriba, Abajo, Izquierda, Derecha }

public class TrainerController : MonoBehaviour
{
    [Header("Campo de visión")]
    [SerializeField]  GameObject campoDeVision;
    [SerializeField]  DireccionEntrenador direccion = DireccionEntrenador.Abajo;
    [SerializeField]  GameObject exclamacion; 

    [Header("Movimiento")]
    [SerializeField]  float velocidadMovimiento = 3f;

     PokemonParty trainerParty;
     Rigidbody2D rb;
     bool batallaYaIniciada = false; // Para que solo se dispare una vez

     void Awake()
    {
        trainerParty = GetComponent<PokemonParty>();
        rb = GetComponent<Rigidbody2D>();
    }

     void Start()
    {
        AplicarRotacionFOV();
        if (exclamacion != null) exclamacion.SetActive(false);
    }

    // Rota el hijo CampoDeVision según la dirección elegida en el Inspector
    private void AplicarRotacionFOV()
    {
        if (campoDeVision == null) return;
        float angulo = direccion switch
        {
            DireccionEntrenador.Arriba => 0f,
            DireccionEntrenador.Derecha => 90f,
            DireccionEntrenador.Abajo => 180f,
            DireccionEntrenador.Izquierda => 270f,
            _ => 0f
        };
        campoDeVision.transform.eulerAngles = new Vector3(0f, 0f, angulo);
    }

    // El collider del hijo llama a esto cuando detecta al jugador
    public void JugadorDetectado(Transform jugador)
    {
        if (batallaYaIniciada) return;
        batallaYaIniciada = true;
        StartCoroutine(SecuenciaEntrenador(jugador));
    }

    private IEnumerator MoverHaciaJugador(Transform jugador)
    {
        while (true)
        {
            Vector2 dirHaciaJugador = (jugador.position - transform.position);
            float distancia = dirHaciaJugador.magnitude;

            if (distancia <= 0.6f)
            {
                rb.linearVelocity = Vector2.zero;
                // NUEVO: congelar posición para que no empuje al jugador
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                yield break;
            }

            rb.linearVelocity = dirHaciaJugador.normalized * velocidadMovimiento;
            yield return null;
        }
    }

    private IEnumerator SecuenciaEntrenador(Transform jugador)
    {
        var movJugador = jugador.GetComponent<Movimiento>();
        if (movJugador != null) movJugador.estaEnInteraccion = true;

        if (exclamacion != null)
        {
            exclamacion.SetActive(true);
            yield return new WaitForSeconds(0.8f);
            exclamacion.SetActive(false);
        }

        yield return MoverHaciaJugador(jugador);
        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.3f);

        
        var interactuable = GetComponentInChildren<Interactuable>();
        Debug.Log($"Interactuable encontrado: {interactuable}");

        if (interactuable != null)
            interactuable.IniciarDialogoDesdeEntrenador();
        else
            IniciarBatallaEntrenador(jugador);
    }

    public void IniciarBatallaEntrenador(Transform jugador)
    {
        if (trainerParty == null)
        {
            Debug.LogError($"[{gameObject.name}] No tiene PokemonParty asignada");
            return;
        }

        JugadorSpawn.posicion = jugador.position;
        JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;
        BattleData.TrainerPokemons = new List<Pokemon>(trainerParty.Pokemons); // Copia la lista
        BattleData.EsEntrenador = true;
        BattleData.WildPokemon = null;
        SceneManager.LoadScene("Combate");
    }
}