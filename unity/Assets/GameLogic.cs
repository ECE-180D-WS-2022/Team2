using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { START, IP_PLAYER_TURN, MONSTER_TURN, WON, LOST }

public class GameLogic : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject[] monsterPrefab = new GameObject[4];

    public Transform playerPlatform;
    public Transform[] monsterPlatform = new Transform[4];

    IP_Player playerUnit;
    Monster[] monsterUnit = new Monster[4];

    bool[] takenMonsterPlatforms = new bool[4];
    bool[] selectMonster = new bool[4];

    public PlayerHUD playerHUD;
    public MonsterHUD[] monsterHUD = new MonsterHUD[4];

    public Text turnText;
    public Text dialogueText;

    public GameState state;

    public GameObject combatButtons;

    int turns = 0;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle() 
    {
        for (int i = 0; i < 4; i++) {
            takenMonsterPlatforms[i] = false;
        }

        turnText.text = "Start";
        dialogueText.text = "Game loading...";

        // Create player and monster object
        GameObject playerGO = Instantiate(playerPrefab, playerPlatform);
        playerUnit = playerGO.GetComponent<IP_Player>();

        // Set current and max HP
        playerUnit.maxHP = 1000;
        playerUnit.currentHP = playerUnit.maxHP;

        playerHUD.SetHUD(playerUnit);

        SpawnMonster();

        yield return new WaitForSeconds(2f);

        state = GameState.IP_PLAYER_TURN;
        IP_PlayerTurn();
    }

    IEnumerator MonsterTurn()
    {
        for (int i = 0; i < 4; i++) {
            bool isDead = false;
            
            if (takenMonsterPlatforms[i] == true) {
                state = GameState.MONSTER_TURN;
                turnText.text = "Monster";
                dialogueText.text = monsterUnit[i].unitName + " attacked!";

                yield return new WaitForSeconds(1f);

                monsterUnit[i].damage = Random.Range(0, 500);
                isDead = playerUnit.TakeDamage(monsterUnit[i].damage);

                playerHUD.SetHP(playerUnit);
            }

            yield return new WaitForSeconds(1f);

            if (isDead) 
            {
                state = GameState.LOST;
                EndBattle();
            } else 
            {
                state = GameState.IP_PLAYER_TURN;
                IP_PlayerTurn();
                turns += 1;
            }
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

        SpawnMonster();
    }

    IEnumerator IP_PlayerAttack()
    {
        // dialogueText.text = "IP_Player attacked!";
        // playerUnit.damage = 500;
        // //playerUnit.isDefending = false;

        // bool isDead = monsterUnit.TakeDamage(playerUnit.damage);

        // monsterHUD.SetHP(monsterUnit);

        // state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(2f);

        bool isDead = false;
        if (isDead) 
        {
            //playerUnit.points += monsterUnit.value;
            state = GameState.WON;
            EndBattle();
        } else 
        {
            StartCoroutine(MonsterTurn());
        }
    }

    IEnumerator IP_PlayerDefend()
    {
        dialogueText.text = "IP_Player defended!";
        //playerUnit.isDefending = true;

        state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(2f);
        StartCoroutine(MonsterTurn());
    }

    IEnumerator IP_PlayerHeal()
    {
        playerUnit.Heal(5);
        playerHUD.SetHP(playerUnit);
        dialogueText.text = "IP_Player healed!";
        //playerUnit.isDefending = false;

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

    public void OnDefendButton()
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        StartCoroutine(IP_PlayerDefend());
    }

    public void OnHealButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        StartCoroutine(IP_PlayerHeal());
    }

    void SpawnMonster() 
    {
        bool allTrue = false;
        for (int i = 0; i < 4; i++) {
            if (takenMonsterPlatforms[i] == false) {
                allTrue = false;
                break;
            } else {
                allTrue = true;
            }
        }

        // if there's an available spot
        if (!allTrue) {
            while (true) {
                // find random index
                int randomIndex = Random.Range(0, 3);
                // if it's free
                if (takenMonsterPlatforms[randomIndex] == false) {
                    // make it "taken"
                    takenMonsterPlatforms[randomIndex] = true;
                    // create the monster
                    GameObject monsterGO = Instantiate(monsterPrefab, monsterPlatform[randomIndex]);
                    monsterUnit[randomIndex] = monsterGO.GetComponent<Monster>();

                    // initialize the monster
                    monsterUnit[randomIndex].unitName = "Monster" + randomIndex.ToString();
                    monsterUnit[randomIndex].maxHP = 1000;
                    monsterUnit[randomIndex].currentHP = monsterUnit[randomIndex].maxHP;
                    monsterUnit[randomIndex].index = randomIndex;

                    //monsterUnit[randomIndex].SetHUD();

                    break;
                }
            }
            
        }
    }
}
