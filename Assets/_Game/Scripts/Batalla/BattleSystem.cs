using Assets.Scripts.Batalla;
using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Estados posibles para controlar el flujo de la máquina de estados del combate
public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, RUNNINGTURN, BUSY, PARTYSCREEN, BATTLEOVER,ABOUTTOUSE,MOVETOFORGET }
public enum BattleAction {MOVE,SWITCHPOKEMON,USEITEM,RUN }

public class BattleSystem : MonoBehaviour
{
    [Header("Referencias de Unidades")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] BattleDialogBox dialogBox;

    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    // Índices para la navegación en los menús
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;
    bool pokeballFallo = false;


    BattleState state;
    
    PokemonParty playerParty;
    Pokemon wildPokemon;
    PokemonParty trainerParty;

    bool esTrainerBattle =false;

    int escapeAttempts; // Contador de intentos para escapar
    MoveBase moveToLearn; // Movimiento que el Pokémon quiere aprender al subir de nivel

    // --- VARIABLES CONTROL MOVIL ---
    private bool esMovil;
    private float tiempoSiguienteInput = 0f; // Cooldown para que el joystick no se mueva demasiado rápido
    private float intervaloInput = 0.2f; // Tiempo de espera entre movimientos del cursor

    private void Awake()
    {
        ConditionsDB.Init();
        // Detectar si estamos en móvil
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }

    void Start()
    {
        var playerParty = GameObject.FindWithTag("Player").GetComponent<PokemonParty>();

        if (BattleData.EsEntrenador && BattleData.TrainerPokemons != null)
        {
            var trainerGO = new GameObject("TrainerPartyTemp");
            trainerGO.SetActive(false); // CRÍTICO: evita que Start se ejecute
            var tempParty = trainerGO.AddComponent<PokemonParty>();
            tempParty.SetPokemonsForBattle(BattleData.TrainerPokemons); // ahora llega antes que Start
            trainerGO.SetActive(true); // Start se ejecuta aquí, ya con esBatallaTemp = true
            StartTrainerBattle(playerParty, tempParty);
        }
        else if (playerParty != null && BattleData.WildPokemon != null)
        {
            StartBattle(playerParty, BattleData.WildPokemon);
        }
        else
        {
            Debug.LogError("Error: Faltan datos para iniciar la batalla.");
        }

        BattleData.EsEntrenador = false;
        BattleData.TrainerPokemons = null;
        BattleData.WildPokemon = null;
    }

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        esTrainerBattle = false;
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        esTrainerBattle=true;
        StartCoroutine(SetupBattle());
    }
    public IEnumerator SetupBattle()
    {

        playerUnit.Clear();
        enemyUnit.Clear();
        if (!esTrainerBattle)
        {
            playerUnit.Setup(playerParty.GetHealtyPokemon());
            // Batalla salvaje: el enemigo es el pokemon salvaje
            enemyUnit.Setup(wildPokemon);
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"¡Un {wildPokemon.Base.Name} salvaje ha aparecido!");
            escapeAttempts = 0;
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            yield return dialogBox.TypeDialog($"¡El entrenador {BattleData.NombreEntrenador} quiere combatir!");

            // Mandar el pokemon del enterenador a la pantalla de combate
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon=trainerParty.GetHealtyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"¡El entrenador ha enviado a {enemyPokemon.Base.Name}!");
            // Mandar el pokemon del personaje principal a la pantalla de combate
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealtyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"¡Ve {playerPokemon.Base.Name}!");

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);


        }
        partyScreen.Init();
        ActionSelection();
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
        partyScreen.CalledFrom = state;
        state = BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.BUSY;
        yield return dialogBox.TypeDialog($"El entrenador {BattleData.NombreEntrenador} va usar a {newPokemon.Base.Name}, quieres cambiar de Pokemon?");
        state = BattleState.ABOUTTOUSE;
        dialogBox.EnableChoiceBox(true);
    }
    IEnumerator ChooseMoveToForget(Pokemon pokemon,MoveBase newMove)
    {
        state = BattleState.BUSY;
        yield return dialogBox.TypeDialog($" Elige un movimiento para olvidar.");
        
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(m => m.Base).ToList(), newMove);
        moveToLearn= newMove;
        state = BattleState.MOVETOFORGET;
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state=BattleState.RUNNINGTURN;
        if (playerAction == BattleAction.MOVE)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
           enemyUnit.Pokemon.CurrentMove=enemyUnit.Pokemon.GetRandomMove();
            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //quien va primero
            bool playerGoesFirst = true;
            if(enemyMovePriority> playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if(enemyMovePriority==playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //primer turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BATTLEOVER) yield break;
            if(secondPokemon.HP> 0)
            { 
                //Segundo turno
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BATTLEOVER) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SWITCHPOKEMON)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.BUSY;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.USEITEM)
            {
                pokeballFallo = false; // resetear antes
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();

                if (pokeballFallo)
                {
                    // El pokémon escapó, ahora ataca el enemigo
                    var enemyMove = enemyUnit.Pokemon.GetRandomMove();
                    yield return RunMove(enemyUnit, playerUnit, enemyMove);
                    yield return RunAfterTurn(enemyUnit);
                    if (state == BattleState.BATTLEOVER) yield break;
                }
                else
                {
                    yield break; 
                }
            }
            else if(playerAction == BattleAction.RUN)
            {
                yield return TryToEscape();
                if (state == BattleState.BATTLEOVER) yield break;
            }

        }
        if (state != BattleState.BATTLEOVER)
        {
            ActionSelection();
        }
    }

   

    // IA básica: El enemigo elige un movimiento al azar y lo ejecuta
    IEnumerator EnemyMove()
    {
        state = BattleState.RUNNINGTURN;
        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.RUNNINGTURN)
            ActionSelection();
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
       bool canRunMove= sourceUnit.Pokemon.OnBeforeMove();
       if (!canRunMove)
       {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
       }
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} usó {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Estado)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon,move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }
            if(move.Base.Secondries !=null && move.Base.Secondries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Secondries)
                {
                    if (UnityEngine.Random.Range(1, 101) <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon,secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }

        }
        else
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} falló el ataque");

       

    }
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BATTLEOVER) yield break;
        yield return new WaitUntil(() => state == BattleState.RUNNINGTURN);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RUNNINGTURN);
        }
    }
    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target,MoveTarget moveTarget)
    {
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);

        }
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        if(effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHit)
            return true;
        float moveAccuracy = move.Base.Accuracy;
        int evasion = target.StatsBoosts[Stat.Evasion];
        int accuracy = target.StatsBoosts[Stat.Accuracy];
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];
        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
        else 
        { 
            if (!esTrainerBattle)
            {
                BattleOver(true); // Victoria en batalla salvaje

            }
            else
            {
                var nextPokemon = trainerParty.GetHealtyPokemon();
                if (nextPokemon != null)
                {
                   StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    BattleOver(true); // Victoria en batalla de entrenador
                }
            }
            
        }
    }

    // Gestiona la salida de la escena de combate
    IEnumerator EndBattle(bool won)
    {
        if (won)
        {
            yield return dialogBox.TypeDialog("¡Has ganado la batalla!");

            
            if (esTrainerBattle && !string.IsNullOrEmpty(BattleData.NombreEntrenador))
            {
                string clave = $"Entrenador_{BattleData.NombreEntrenador}_Derrotado";
                PlayerPrefs.SetInt(clave, 1);
                PlayerPrefs.Save();
                BattleData.NombreEntrenador = null;
            }
        }
        else
            yield return dialogBox.TypeDialog("Has sido derrotado...");

        yield return new WaitForSeconds(1.5f);
        string sceneToLoad = !string.IsNullOrEmpty(JugadorSpawn.escenaAnterior)
            ? JugadorSpawn.escenaAnterior
            : "PuebloFuenlabrada";
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
        else if (state == BattleState.ABOUTTOUSE) HandleAboutToUseSelection();
        else if (state == BattleState.MOVETOFORGET)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} no aprendió {moveToLearn.Name}."));
                }
                else
                {
                    var selectedMove=playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}!"));
                    playerUnit.Pokemon.Moves[moveIndex]=new Move(moveToLearn);


                }
                moveToLearn = null;
                state=BattleState.RUNNINGTURN;

            };
            moveSelectionUI.HandleMoveSelection(esMovil,onMoveSelected);
        }


    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("¡Un golpe crítico!");

        if (damageDetails.TypeEffectiveness > 1f) yield return dialogBox.TypeDialog("¡Es muy efectivo!");
        else if (damageDetails.TypeEffectiveness < 1f) yield return dialogBox.TypeDialog("No es muy efectivo...");
    }

    // --- FUNCIONES AUXILIARES PARA INPUTS ---

    // Detectar "Enter" o Botón Interacción (A)
    bool InputConfirmar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
        {
            return ControlesMoviles.Instance.botonInteraccion.SePresionoEsteFrame();
        }
        return Input.GetKeyDown(KeyCode.Return);
    }

    // Detectar "Escape" o Botón Correr (que usaremos como botón B/Atrás)
    bool InputCancelar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
        {
            // Usamos el botón de correr como "Atrás" en los menús
            return ControlesMoviles.Instance.botonCorrer.SePresionoEsteFrame();
        }
        return Input.GetKeyDown(KeyCode.Escape);
    }

    // Control de navegación en el menú de ataques
    void HandleMoveSelection()
    {
        // Input de movimiento con cooldown para el joystick
        if (Time.time >= tiempoSiguienteInput)
        {
            // Derecha
            if (Input.GetKeyDown(KeyCode.RightArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() > 0.5f))
            {
                ++currentMove;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            // Izquierda
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() < -0.5f))
            {
                --currentMove;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            // Abajo
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f))
            {
                currentMove += 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            // Arriba
            else if (Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
            {
                currentMove -= 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
        }

        currentMove = Math.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (InputConfirmar())
        {
            var move =playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return; // No se puede seleccionar un movimiento sin PP
            
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.MOVE));
        }
        else if (InputCancelar())
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    // Control de navegación en el menú de acciones principales
    void HandleActionSelection()
    {
        if (Time.time >= tiempoSiguienteInput)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() > 0.5f))
            {
                ++currentAction;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() < -0.5f))
            {
                --currentAction;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f))
            {
                currentAction += 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
            {
                currentAction -= 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
        }

        currentAction = Math.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (InputConfirmar())
        {
            if (currentAction == 0) MoveSelection(); // Luchar
            else if (currentAction == 1)
            {
                StartCoroutine(RunTurns(BattleAction.USEITEM));//Mochila
            }
            else if (currentAction == 2)
            {// Pokémons
                
                OpenPartyScreen(); 
            } 
            else if (currentAction == 3)
            {
                dialogBox.EnableActionSelector(false);
                StartCoroutine(RunTurns(BattleAction.RUN)); // Huir
            }
        }
    }

    // Control de selección en la pantalla de equipo
    void HandlePartyScreenSelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
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
            if (partyScreen.CalledFrom == BattleState.ACTIONSELECTION)
            {

                StartCoroutine(RunTurns(BattleAction.SWITCHPOKEMON));
            }
            else
            {
                state = BattleState.BUSY;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.ABOUTTOUSE;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("Debes elegir un pokemon para continuar");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if (partyScreen.CalledFrom == BattleState.ABOUTTOUSE)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };
        partyScreen.HandleUpdate(onSelected, onBack);
        
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} ha sido derrotado!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //Ganar Experiencia
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = esTrainerBattle ? 1.5f : 1f; // Bonus por ser batalla de entrenador
            //Formula real
            int expGain = Mathf.FloorToInt((enemyLevel * expYield*trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} ganó {expGain} de experiencia");
            yield return playerUnit.Hud.SetExpSmooth();
            //Chack Subida de nivel
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} subio a nivel {playerUnit.Pokemon.Level}");

                //Aprender movimientos nuevos
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if(playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} aprendió {newMove.MoveBase.Name}!");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} quiere aprender {newMove.MoveBase.Name}.");
                        yield return dialogBox.TypeDialog($"Pero no puede aprender más de {PokemonBase.MaxNumOfMoves} movimientos.");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.MoveBase);
                        yield return new WaitUntil(() => state != BattleState.MOVETOFORGET);
                        yield return new WaitForSeconds(2f);
                    }
                }


                yield return playerUnit.Hud.SetExpSmooth(true);
                playerUnit.Pokemon.ResetHealth();
            }
            yield return new WaitForSeconds(1f);
        }

        CheckBattleOver(faintedUnit);
        //yield return new WaitUntil(() => state == BattleState.RUNNINGTURN);
    }

    void HandleAboutToUseSelection()
    {
       //Primero ver si va de arriba a abjo entre SI y NO
       if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f) || Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }
        
        dialogBox.UpdateChoiceBox(aboutToUseChoice);
        if (InputConfirmar())
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice)
            {
                // Cambiar a nuestro pokemon
                
                OpenPartyScreen();
            }
            else
            {
                // Dejar que el entrenador use su pokemon
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (InputCancelar())
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }
    // Intercambia el Pokémon actual por uno nuevo
    IEnumerator SwitchPokemon(Pokemon newPokemon,bool isTrainerAboutToUse=false)
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

        if(isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state= BattleState.RUNNINGTURN;
        
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.BUSY;
        if (esTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"No puedes usar una pokeball contra un entrenador!");
            pokeballFallo = true; // Indicar que no se intentó capturar, para que el enemigo ataque normalmente
            state = BattleState.RUNNINGTURN;
            yield break;
        }

        yield return dialogBox.TypeDialog($"Has usado una pokeball");

        var pokeballObJ = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(0, 2, 1), Quaternion.identity);
        var pokeball = pokeballObJ.GetComponent<SpriteRenderer>();

        //Animaciones
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2, 1), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 3f, 0.5f).WaitForCompletion();

        int shakeCount = TryCatchPokemon(enemyUnit.Pokemon);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
        if (shakeCount == 4)
        {
            //Pokemon capturado
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha sido capturado");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            if (playerParty.AddPokemon(enemyUnit.Pokemon))
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha sido añadido a tu equipo");
            }
            else
                yield return dialogBox.TypeDialog($"Equipo lleno");

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
            
            pokeballFallo = true; // Indicar que el pokémon escapó

            state = BattleState.RUNNINGTURN;
        }

    }
    int TryCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
        if (a >= 255)
            return 4;
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b) break;
            ++shakeCount;
        }
        return shakeCount;
    }
    IEnumerator TryToEscape()
    {
        state = BattleState.BUSY;

        if(esTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"No puedes huir de un combate contra un entrenador!");
            state = BattleState.RUNNINGTURN;
            yield break;
        }

        ++escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Puedes huir del combate a salvo!");
            BattleOver(false);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Puedes huir a salvo!");
                BattleOver(false);
            }
            else
            {
                yield return dialogBox.TypeDialog($"No has podido huir!");
                state= BattleState.RUNNINGTURN;

            }
        }
    }
    IEnumerator SendNextTrainerPokemon()
    {
        state=BattleState.BUSY;
       
        var nextPokemon = trainerParty.GetHealtyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"¡El entrenador {BattleData.NombreEntrenador} ha enviado a {nextPokemon.Base.Name}!");
        state = BattleState.RUNNINGTURN;
        
        
    }

}