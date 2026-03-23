using Assets.Scripts.Batalla;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCharacterController : MonoBehaviour
{
    // Mueve aquí todo lo de ChequearHierba de Movimiento.cs
    public LayerMask grassLayer;
    public float probabilidad = 60f;
    private float cronometroPasos;
    public float tiempoEntreChequeos = 0.5f;

    private PokemonParty party;

    private void Awake()
    {
        party = GetComponent<PokemonParty>();
    }

    // Movimiento.cs llama a este método en lugar de tener la lógica dentro
    public void ChequearHierba(Vector2 posicion)
    {
        if (Physics2D.OverlapCircle(posicion, 0.2f, grassLayer))
        {
            cronometroPasos += Time.deltaTime;
            if (cronometroPasos >= tiempoEntreChequeos)
            {
                cronometroPasos = 0;
                if (party.GetHealtyPokemon() == null) return;

                if (Random.Range(0f, 100f) < probabilidad)
                {
                    var area = Physics2D.OverlapCircle(posicion, 0.2f, grassLayer).GetComponent<MapArea>();
                    if (area != null)
                    {
                        IniciarBatallaSalvaje(posicion, area.GetRandomWildPokemon());
                    }
                }
            }
        }
    }

    private void IniciarBatallaSalvaje(Vector2 posicion, Pokemon wildPokemon)
    {
        JugadorSpawn.posicion = posicion;
        JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;
        BattleData.WildPokemon = wildPokemon;
        BattleData.EsEntrenador = false;
        SceneManager.LoadScene("Combate");
    }
}