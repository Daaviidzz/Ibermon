using Assets.Scripts.Batalla;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Estados posibles para controlar el flujo de la máquina de estados del combate
public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, PERFORMMOVE, BUSY, PARTYSCREEN, BATTLEOVER }

public class BattleSystem : MonoBehaviour
{
    [Header("Referencias de Unidades")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] BattleDialogBox dialogBox;

    // Índices para la navegación en los menús
    int currentAction;
    int currentMove;
    int currentMember;

    BattleState state;
    PokemonParty playerParty;
    Pokemon wildPokemon;

    private void Start()
    {
        // Recuperamos el equipo del jugador mediante el Tag "Player"
        var playerParty = GameObject.FindWithTag("Player").GetComponent<PokemonParty>();

        if (playerParty != null && BattleData.WildPokemon != null)
        {
            StartBattle(playerParty, BattleData.WildPokemon);
        }
        else
        {
            Debug.LogError("Error: Faltan datos para iniciar la batalla.");
        }
    }

    private void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        // Inicializa unidades: saca al primer Pokémon sano y al enemigo
        playerUnit.Setup(playerParty.GetHealtyPokemon());
        enemyUnit.Setup(wildPokemon);
        partyScreen.Init();
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido!");

        ActionSelection(); // Comienza el turno del jugador
    }

    // Cambia al estado de elegir Luchar, Mochila, etc.
    void ActionSelection()
    {
        state = BattleState.ACTIONSELECTION;
        dialogBox.SetDialog("Elije una opción");
        dialogBox.EnableActionSelector(true);
    }

    // Abre el menú de cambio de Pokémon
    void OpenPartyScreen()
    {
        state = BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    // Ejecuta el movimiento elegido por el jugador y luego cede el turno al enemigo
    IEnumerator PlayerMove()
    {
        state = BattleState.PERFORMMOVE;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PERFORMMOVE)
            StartCoroutine(EnemyMove());
    }

    // IA básica: El enemigo elige un movimiento al azar y lo ejecuta
    IEnumerator EnemyMove()
    {
        state = BattleState.PERFORMMOVE;
        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PERFORMMOVE)
            ActionSelection();
    }

    // Finaliza la lógica de batalla
    void BattleOver(bool won)
    {
        state = BattleState.BATTLEOVER;
        // ERROR ANTERIOR: EndBattle(won); 
        StartCoroutine(EndBattle(won)); // SOLUCIÓN: Usar StartCoroutine
    }

    // Método core: Maneja la animación, el daño y la reducción de PP de cualquier movimiento
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.Pp--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} usó {move.Base.Name}!");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} se ha debilitado!");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckBattleOver(targetUnit);
        }
    }

    // Verifica si alguien se quedó sin Pokémon para seguir luchando
    void CheckBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealtyPokemon();
            if (nextPokemon != null) OpenPartyScreen();
            else BattleOver(false); // Derrota
        }
        else BattleOver(true); // Victoria
    }

    // Gestiona la salida de la escena de combate
    IEnumerator EndBattle(bool won)
    {
        if (won) yield return dialogBox.TypeDialog("¡Has ganado la batalla!");
        else yield return dialogBox.TypeDialog("Has sido derrotado...");

        yield return new WaitForSeconds(1.5f);

        // Retorno a la escena previa o por defecto
        string sceneToLoad = !string.IsNullOrEmpty(JugadorSpawn.escenaAnterior) ? JugadorSpawn.escenaAnterior : "PuebloFuenlabrada";
        SceneManager.LoadScene(sceneToLoad);
    }

    // Pasa del menú principal al menú de ataques
    void MoveSelection()
    {
        state = BattleState.MOVESELECTION;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EneableMoveSelector(true);
    }

    private void Update()
    {
        // Delegamos el input según el estado actual
        if (state == BattleState.ACTIONSELECTION) HandleActionSelection();
        else if (state == BattleState.MOVESELECTION) HandleMoveSelection();
        else if (state == BattleState.PARTYSCREEN) HandlePartyScreenSelection();
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("¡Un golpe crítico!");

        if (damageDetails.TypeEffectiveness > 1f) yield return dialogBox.TypeDialog("¡Es muy efectivo!");
        else if (damageDetails.TypeEffectiveness < 1f) yield return dialogBox.TypeDialog("No es muy efectivo...");
    }

    // Control de navegación en el menú de ataques
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentMove -= 2;

        currentMove = Math.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EneableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EneableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    // Control de navegación en el menú de acciones principales
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentAction -= 2;

        currentAction = Math.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0) MoveSelection(); // Luchar
            else if (currentAction == 1) { /* Mochila */ }
            else if (currentAction == 2) OpenPartyScreen(); // Pokémon
            else if (currentAction == 3) StartCoroutine(dialogBox.TypeDialog("No puedes huir de una batalla salvaje!")); // Huir
        }
    }

    // Control de selección en la pantalla de equipo
    void HandlePartyScreenSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentMember -= 2;

        currentMember = Math.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes elegir un pokemon derrotado");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("No puedes elegir al mismo pokemon");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.BUSY;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    // Intercambia el Pokémon actual por uno nuevo
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Vuelve {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Tu turno {newPokemon.Base.Name}!");

        StartCoroutine(EnemyMove()); // Tras cambiar, el enemigo ataca
    }
}