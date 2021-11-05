using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum BattleState {START, PLAYERTURN,ENEMYTURN,WON,LOST }
public class BattleSystem : MonoBehaviour
{
    public GameObject playerGun;
    public GameObject playerSword;
    public GameObject playerShield;
    public Transform P1BattleStage;
    public Transform P2BattleStage;
    public Text dialogueText;
    public BattleHud playerHUD;
    public BattleHud enemyHUD;
    Unit playerUnit;
    Unit enemyUnit;
    public BattleState state;
    // Start is called before the first frame update
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(setupBattle());

    }
    IEnumerator setupBattle()
    {
       GameObject playerGO = Instantiate(playerGun, P1BattleStage);
       playerUnit = playerGO.GetComponent<Unit>();
       GameObject enemyGO = Instantiate(playerSword, P2BattleStage);
       enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "a wild " + enemyUnit.unitName + " approaches.....";

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(2f);
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator PlayerAttack()
    {
        // damaging the enemy
       bool isDead =  enemyUnit.TakeDamage(playerUnit.damage);

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "the attack was made successfully!!";
        yield return new WaitForSeconds(2f);

        //check if enemy is dead
        if (isDead)
        {
            //end the battle
            state = BattleState.WON;
            EndBattle();
        } else
        {
            //enemy turn
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());

        }
        
    }

    IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " attacks!";

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

        playerHUD.SetHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }

    }

    void EndBattle()
    {
        if(state == BattleState.WON)
        {
            dialogueText.text = "Congratulations you won the battle!!!";
        } else if (state == BattleState.LOST)
        {
            dialogueText.text = "You were slain, try next time!!";
        }
    }
    void PlayerTurn()
    {
        dialogueText.text = "Choose an action: ";
    }
    
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;
        StartCoroutine(PlayerAttack());
    }
}
