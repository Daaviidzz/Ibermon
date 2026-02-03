using Assets.Scripts.Batalla;
using DG.Tweening;
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

    [SerializeField] GameObject pokeballSprite;

    // Índices para la navegación en los menús
    int currentAction;
    int currentMove;
    int currentMember;

    BattleState state;
    PokemonParty playerParty;
    Pokemon wildPokemon;

    int escapeAttempts; // Contador de intentos para escapar

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
        escapeAttempts = 0; 
        ChooseFirstTurn();
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
    //Elegir quien ataca primero
    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }


    // Finaliza la lógica de batalla
    void BattleOver(bool won)
    {
        state = BattleState.BATTLEOVER;
        //Resetear estados al acabar la batalla
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        StartCoroutine(EndBattle(won)); // Usar StartCoroutine para evitar errores
    }

    // Método core: Maneja la animación, el daño y la reducción de PP de cualquier movimiento
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.Pp--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} usó {move.Base.Name}!");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Estado)
        {
          yield return RunMoveEffects(move,sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }


        if (targetUnit.Pokemon.HP<=0)
        {
          yield return HandlePokemonFainted(targetUnit);
        }
    }
    IEnumerator RunMoveEffects(Move move,Pokemon source,Pokemon target)
    {
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);

        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message=pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
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
        dialogBox.EnableMoveSelector(true);
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
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableMoveSelector(false);
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
            else if (currentAction == 1)
            {
                dialogBox.EnableActionSelector(false);
                StartCoroutine(ThrowPokeball());
            }//Mochila
            else if (currentAction == 2) OpenPartyScreen(); // Pokémon
            else if (currentAction == 3)
            {
                dialogBox.EnableActionSelector(false);
                StartCoroutine(TryToEscape()); // Huir
            }
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

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} ha sido derrotado!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //Ganar Experiencia
            int expYield=faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            //Formula real
            int expGain=Mathf.FloorToInt((enemyLevel * expYield)/7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} ganó {expGain} de experiencia");
            yield return playerUnit.Hud.SetExpSmooth();
            //Chack Subida de nivel

        }

        CheckBattleOver(faintedUnit);
    }


    // Intercambia el Pokémon actual por uno nuevo
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted=false;
            yield return dialogBox.TypeDialog($"Vuelve {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Tu turno {newPokemon.Base.Name}!");

        //Si nuestro pokemon es derrotado, se vuelve a elegir quien ataca
        if (currentPokemonFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove()); // Tras cambiar, el enemigo ataca, 
    }

    IEnumerator ThrowPokeball()
    {
        state=BattleState.BUSY;

       yield return dialogBox.TypeDialog($"Has usado una pokeball");

       var pokeballObJ=Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(0, 2, 1), Quaternion.identity);
        var pokeball = pokeballObJ.GetComponent<SpriteRenderer>();

        //Animaciones
       yield return pokeball.transform.DOJump(enemyUnit.transform.position+ new Vector3(0,2,1),2f,1,1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y-3f,0.5f).WaitForCompletion();

        int shakeCount = TryCatchPokemon(enemyUnit.Pokemon);

        for (int i = 0; i < Mathf.Min(shakeCount,3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0,0,10f),0.8f).WaitForCompletion() ;
        }
        if (shakeCount == 4)
        {
            //Pokemon capturado
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha sido capturado");
            yield return pokeball.DOFade(0,1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha sido añadido a tu equipo");

            Destroy(pokeballObJ);
            BattleOver(true);
        }
        else
        {
            // Pokémon se escapa
            yield return new WaitForSeconds(1f);

            // Desvanecemos la pokeball antes de que el pokemon salga
            yield return pokeball.DOFade(0, 0.2f).WaitForCompletion();

            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha escapado de la PokeBall!");
            else
                yield return dialogBox.TypeDialog($"¡Casi lo atrapas!");

            // IMPORTANTE: Destruir el objeto completo, no solo el componente
            Destroy(pokeballObJ);

            StartCoroutine(EnemyMove());
        }

    }
    int TryCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate / (3 * pokemon.MaxHp);
        if (a >= 255)
            return 4;
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b) break;
            ++shakeCount;
        }
        return shakeCount;
    }
    IEnumerator TryToEscape()
    {
        state=BattleState.BUSY;

        ++escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed=enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Puedes huir del combate a salvo!");
            BattleOver(false);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0,256)<f)
            {
                yield return dialogBox.TypeDialog($"Puedes huir a salvo!");
                BattleOver(false);
            }
            else
            {
                yield return dialogBox.TypeDialog($"No has podido huir!");
                StartCoroutine(EnemyMove());

            }
        }
    }
   
}