using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { START, IP_PLAYER_TURN, MONSTER_TURN, WON, LOST }

public class GameLogic : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    public Transform playerPlatform;
    public Transform monsterPlatform;

    Unit playerUnit;
    Unit monsterUnit;

    public HUD playerHUD;
    public HUD monsterHUD;

    public Text turnText;
    public Text dialogueText;

    public GameState state;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle() 
    {
        turnText.text = "Start";
        dialogueText.text = "Game loading...";

        // Create player and monster object
        GameObject playerGO = Instantiate(playerPrefab, playerPlatform);
        playerUnit = playerGO.GetComponent<Unit>();

        GameObject monsterGO = Instantiate(monsterPrefab, monsterPlatform);
        monsterUnit = monsterGO.GetComponent<Unit>();

        playerHUD.SetHUD(playerUnit);
        monsterHUD.SetHUD(monsterUnit);

        yield return new WaitForSeconds(2f);

        state = GameState.IP_PLAYER_TURN;
        IP_PlayerTurn();
    }

    IEnumerator IP_PlayerAttack()
    {
        dialogueText.text = "IP_Player attacked!";
        bool isDead = monsterUnit.TakeDamage(playerUnit.damage);

        monsterHUD.SetHP(monsterUnit);

        state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(2f);

        if (isDead) 
        {
            state = GameState.WON;
            EndBattle();
        } else 
        {
            StartCoroutine(MonsterTurn());
        }
    }

    IEnumerator MonsterTurn()
    {
        turnText.text = "Monster";
        dialogueText.text = "Monster attacked!";

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(monsterUnit.damage);

        playerHUD.SetHP(playerUnit);

        state = GameState.IP_PLAYER_TURN;

        yield return new WaitForSeconds(1f);

        if (isDead) 
        {
            state = GameState.LOST;
            EndBattle();
        } else 
        {
            IP_PlayerTurn();
        }
    }

    void EndBattle()
    {
        if (state == GameState.WON) 
        {
            turnText.text = "WON";
            dialogueText.text = "Congrats!";
        } else if (state == GameState.LOST) {
            turnText.text = "LOST";
            dialogueText.text = "You died!";
        }
    }

    void IP_PlayerTurn()
    {
        turnText.text = "IP_Player";
        dialogueText.text = "Choose an action:";
    }

    IEnumerator IP_PlayerHeal()
    {
        playerUnit.Heal(5);
        playerHUD.SetHP(playerUnit);
        dialogueText.text = "IP_Player healed!";

        state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(2f);

        StartCoroutine(MonsterTurn());
    }

    public void OnAttackButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        StartCoroutine(IP_PlayerAttack());
    }

    public void OnHealButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        StartCoroutine(IP_PlayerHeal());
    }
}
