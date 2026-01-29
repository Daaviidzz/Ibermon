using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

//Define los posibles estados de la batalla
public enum BattleState { START, PLAYERACTION,PLAYERMOVE,ENEMYMOVE,BUSY }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;
    int currentAction;
    int currentMove;

    BattleState state;

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }
    //Configura la batalla inicializando las unidades y la interfaz de usuario
    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        //Muestra el mensaje de inicio de batalla
        yield return dialogBox.TypeDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido!");
    

        PlayerAction();
    }
    //Maneja la acción del jugador mostrando las opciones disponibles
    void PlayerAction()
    {
       state = BattleState.PLAYERACTION;
       StartCoroutine(dialogBox.TypeDialog("Elije una opción"));
        //Hanbilita huir o luchar
        dialogBox.EnableActionSelector(true);

    }
    //Realiza el movimiento seleccionado por el jugador
    IEnumerator PerformPlayerMove() 
    {
        //Cambia el estado a ocupado
        state = BattleState.BUSY;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} usó {move.Base.Name}!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        //Aplica el daño al enemigo y verifica si se ha debilitado
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move,playerUnit.Pokemon);
       yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} se ha debilitado!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f); // Esperamos 2 segundos para ver la animación

            // AQUÍ LLAMAMOS AL FIN DE LA BATALLA (VICTORIA)
            yield return EndBattle(true);
        }
        else
        {
           yield return EnemyMove();
        }

    }
    //Realiza el movimiento del enemigo
    IEnumerator EnemyMove()
    {
        state = BattleState.ENEMYMOVE;
        var move = enemyUnit.Pokemon.GetRandomMove();
        Debug.Log("Movimientos del enemigo: " + enemyUnit.Pokemon.Moves.Count);

        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} usó {move.Base.Name}!");
      
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        //Aplica el daño al jugador y verifica si se ha debilitado
        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
       yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} se ha debilitado!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f); // Tiempo para ver la animación de derrota

            // LLAMAMOS AL FIN DE BATALLA (DERROTA)
            yield return EndBattle(false);
        }
        else
        {
            PlayerAction();
        }
    }

    // true si ganamos, false si perdemos
    IEnumerator EndBattle(bool won)
    {
        if (won)
        {
            yield return dialogBox.TypeDialog("¡Has ganado la batalla!");
        }
        else
        {
            yield return dialogBox.TypeDialog("Has sido derrotado...");
        }

        yield return new WaitForSeconds(1.5f); // Pausa dramática antes de salir

        // Cargamos la escena de vuelta (PuebloFuenlabrada o la anterior)
        if (!string.IsNullOrEmpty(JugadorSpawn.escenaAnterior))
        {
            SceneManager.LoadScene(JugadorSpawn.escenaAnterior);
        }
        else
        {
            SceneManager.LoadScene("PuebloFuenlabrada");
        }
    }
    //Cuando el jugador selecciona "Luchar", muestra la selección de movimientos
    void PlayerMove()
    {
        state = BattleState.PLAYERMOVE;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EneableMoveSelector(true);
    }

    //Actualiza el estado de la batalla y maneja la entrada del jugador
    private void Update()
    {
        if (state == BattleState.PLAYERACTION)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PLAYERMOVE)
        {
            HandleMoveSelection();
        }
    }

    //Muestra los detalles del daño como crítico y efectividad
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("¡Un golpe crítico!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("¡Es muy efectivo!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("No es muy efectivo...");
        }
    }


    //Maneja la selección de movimientos del jugador
    void HandleMoveSelection()
    {
      if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(currentMove < playerUnit.Pokemon.Moves.Count -1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(currentMove > 0)
            {
                --currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currentMove < playerUnit.Pokemon.Moves.Count -2)
            {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currentMove > 1)
            {
                currentMove -= 2;
            }
        }
        //Actualiza la selección de movimiento en la interfaz
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
        
        if(Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EneableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }

    }

    //Maneja la selección de acciones del jugador
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currentAction > 0)
            {
                --currentAction;
            }
        }
        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Return)) 
        {
            if (currentAction == 0) //Luchar
            {
                PlayerMove();
            }
            else if (currentAction == 1) //Huir
            {
                StartCoroutine(dialogBox.TypeDialog("No puedes huir de una batalla salvaje!"));
            }
        }
    }
}
 
