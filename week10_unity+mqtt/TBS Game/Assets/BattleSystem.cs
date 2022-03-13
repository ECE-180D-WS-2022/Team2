using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum BattleState {START, PLAYERTURN,ENEMYTURN , WON, LOST}


public class BattleSystem : MonoBehaviour
{  
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab1;
    public GameObject EnemyPrefab2;
    public GameObject EnemyPrefab3;
    public Transform PlayerBattleStation;
    public Transform EnemyBattleStation1;
    public Transform EnemyBattleStation2;
    public Transform EnemyBattleStation3;

    unit playerUnit;
    unit enemyUnit1;
    unit enemyUnit2;
    unit enemyUnit3;

    public BattleHud playerHud;
    public BattleHud enemyHud1;
    public BattleHud enemyHud2;
    public BattleHud enemyHud3;
    
    public Text dialogueText;
    public mqttReceiver _eventSender;
    public BattleState state;
    // setups
    void Start()
    {
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;
        state=BattleState.START;
        StartCoroutine(SetupBattle());

    }

    IEnumerator SetupBattle()
    {
        GameObject playerGo = Instantiate(PlayerPrefab,PlayerBattleStation);
        playerUnit = playerGo.GetComponent<unit>();
        GameObject enemyGo1 = Instantiate(EnemyPrefab1,EnemyBattleStation1);
        enemyUnit1 = enemyGo1.GetComponent<unit>();
        GameObject enemyGo2 = Instantiate(EnemyPrefab2,EnemyBattleStation2);
        enemyUnit2 = enemyGo2.GetComponent<unit>();
        GameObject enemyGo3 = Instantiate(EnemyPrefab3,EnemyBattleStation3);
        enemyUnit3 = enemyGo3.GetComponent<unit>();

        dialogueText.text="Wild " + enemyUnit1.name + ", " + enemyUnit2.name + ", and " + enemyUnit3.name + " approach.";

        playerHud.SetHUD(playerUnit);
        enemyHud1.SetHUD(enemyUnit1);
        enemyHud2.SetHUD(enemyUnit2);
        enemyHud3.SetHUD(enemyUnit3);

        yield return new WaitForSeconds(2f);
        state=BattleState.PLAYERTURN;
        PlayerTurn();

    }
    
    //player's turn
    IEnumerator PlayerAttack()
    {
        bool isDead1=enemyUnit1.TakeDamage(playerUnit.damage);
        bool isDead2=enemyUnit2.TakeDamage(playerUnit.damage);
        bool isDead3=enemyUnit3.TakeDamage(playerUnit.damage);
        enemyUnit1.Dead=isDead1;
        enemyUnit2.Dead=isDead2;
        enemyUnit3.Dead=isDead3;

        enemyHud1.SetHP(enemyUnit1.currentHP);
        enemyHud2.SetHP(enemyUnit2.currentHP);
        enemyHud3.SetHP(enemyUnit3.currentHP);

        dialogueText.text="you've dealt "+playerUnit.damage+" damage to all enemies";
        yield return new WaitForSeconds(2f);

        if(isDead1&&isDead2&&isDead3)
        {
            state=BattleState.WON;
            EndBattle();
        }
        else{
            state=BattleState.ENEMYTURN;
            StartCoroutine(EnemyAttack());
        }

    }

    IEnumerator PlayerHeal()
    {
        playerUnit.FullyHeal();
        playerHud.SetHP(playerUnit.maxHP);
        dialogueText.text="you are fully healed";
        yield return new WaitForSeconds(2f);
        StartCoroutine(EnemyAttack());
    }

    //enmey's turn
    IEnumerator EnemyAttack()
    {
        if(!(enemyUnit1.Dead))
        {
            int num=Random.Range(1,1+enemyUnit1.damage);
            dialogueText.text=enemyUnit1.name+ " dealt "+ num + " damage to you";
            yield return new WaitForSeconds(1f);
            bool isDead=playerUnit.TakeDamage(num);
            playerHud.SetHP(playerUnit.currentHP);

            if(isDead)
            {
                state=BattleState.LOST;
                EndBattle();
            }
            else{
                state=BattleState.PLAYERTURN;
                PlayerTurn();
            }
        }

        if(!(enemyUnit2.Dead))
        {
            int num=Random.Range(1,1+enemyUnit2.damage);
            dialogueText.text=enemyUnit2.name+ " dealt "+ num + " damage to you";
            yield return new WaitForSeconds(1f);
            bool isDead=playerUnit.TakeDamage(num);
            playerHud.SetHP(playerUnit.currentHP);

            if(isDead)
            {
                state=BattleState.LOST;
                EndBattle();
            }
            else{
                state=BattleState.PLAYERTURN;
                PlayerTurn();
            }
        }

        if(!(enemyUnit3.Dead))
        {
            int num=Random.Range(1,1+enemyUnit3.damage);
            dialogueText.text=enemyUnit3.name+ " dealt "+ num + " damage to you";
            yield return new WaitForSeconds(1f);
            bool isDead=playerUnit.TakeDamage(num);
            playerHud.SetHP(playerUnit.currentHP);

            if(isDead)
            {
                state=BattleState.LOST;
                EndBattle();
            }
            else{
                state=BattleState.PLAYERTURN;
                PlayerTurn();
            }
        }
        
        
        
    }

    //end the game
    void EndBattle()
    {
        if(state==BattleState.WON)
        {
            dialogueText.text="You've won the battle";
        }
        else if (state==BattleState.LOST)
        {
            dialogueText.text="Defeated";
        }
    }






    //player's operation
    void PlayerTurn()
    {
        dialogueText.text="Make your action";
    }

    private void OnMessageArrivedHandler(string newMsg)
    {

        Debug.Log("Event Fired. The message is = " + newMsg);
        if(newMsg=="attack")
        {
            if(state!=BattleState.PLAYERTURN)
            return;
        
            StartCoroutine(PlayerAttack());
        }
        else if(newMsg=="heal")
        {
            if(state!=BattleState.PLAYERTURN)
            return;
        
            StartCoroutine(PlayerHeal());
        }

        
    } 

}
