using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum GameState { START, WAITING, IP_PLAYER_TURN, MONSTER_TURN, WON, LOST }

public class GameLogic : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject monsterPrefab;
    public GameObject monsterHUDPrefab;

    public Transform playerPlatform;
    public Transform[] monsterPlatform = new Transform[3];

    IP_Player playerUnit;
    Monster[] monsterUnit = new Monster[3];

    bool[] takenMonsterPlatforms = new bool[3];
    bool[] selectMonster = new bool[3];

    public PlayerHUD playerHUD;
    MonsterHUD[] monsterHUD = new MonsterHUD[3];

    public Text turnText;
    public Text dialogueText;

    public GameState state;

    public GameObject combatButtons;
    public GameObject[] directions = new GameObject[3];

    public mqttReceiver commandReceiver;
    public mqttReceiver positionReceiver;

    int turns = 0;

    int standing_index = -1;

    float timeForSpeech = 2f;
    float maxTime = 10f;
    float currentTime = 0f;
    bool runningTime;
    bool skipped;

    // Start is called before the first frame update
    void Start()
    {
        commandReceiver.OnMessageArrived += OnMessageArrivedCommandHandler;
        positionReceiver.OnMessageArrived += OnMessageArrivedPositionHandler;

        // Turn off timer
        runningTime = false;
        // Initialize
        skipped = false;

        state = GameState.START;
        StartCoroutine(SetupBattle());
    }

    void Update()
    {
        if (runningTime == false) {
            currentTime = 0f;
        } else {
            currentTime += Time.deltaTime;
        }

        if (currentTime >= maxTime) {
            runningTime = false;
            skipped = true;
            StartCoroutine(MonsterTurn());
        }
    }

    IEnumerator SetupBattle() 
    {
        combatButtons.SetActive(false);
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }

        // Make all platforms available
        for (int i = 0; i < 3; i++) {
            takenMonsterPlatforms[i] = false;
        }

        // Set up text
        turnText.text = "Start";
        dialogueText.text = "Game loading...";

        // Create player and monster object
        GameObject playerGO = Instantiate(playerPrefab, playerPlatform);
        playerUnit = playerGO.GetComponent<IP_Player>();

        // Set current and max HP
        playerUnit.maxHP = 1000;
        playerUnit.currentHP = playerUnit.maxHP;

        // Set up hp bar
        playerHUD.SetHUD(playerUnit);

        // Spawn a monster
        SpawnMonster();

        yield return new WaitForSeconds(timeForSpeech);

        state = GameState.IP_PLAYER_TURN;
        IP_PlayerTurn();
    }

    IEnumerator MonsterTurn()
    {
        // Disable combat buttons
        combatButtons.SetActive(false);
        // Disable direction buttons
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }

        turnText.text = "Monster";

        if (skipped) {
            skipped = false;
            dialogueText.text = "You're too slow!";
            yield return new WaitForSeconds(timeForSpeech);
        }

        bool isDead = false;
        int killer = -1;

        for (int i = 0; i < 3; i++) {
            if (takenMonsterPlatforms[i] == true) {
                state = GameState.MONSTER_TURN;
                dialogueText.text = monsterUnit[i].unitName + " attacked!";

                yield return new WaitForSeconds(timeForSpeech);

                monsterUnit[i].damage = Random.Range(0, 250);
                if (playerUnit.isDefending == true) {
                    playerUnit.isDefending = false;
                    monsterUnit[i].damage /= 2;
                }

                dialogueText.text = monsterUnit[i].unitName + " dealt " + monsterUnit[i].damage + " HP!";

                yield return new WaitForSeconds(timeForSpeech);

                isDead = playerUnit.TakeDamage(monsterUnit[i].damage);

                Vector3 moveforward = new Vector3(0f, 0f, -100f);

                monsterUnit[i].transform.position += moveforward;
                monsterHUD[i].transform.position += moveforward;

                playerHUD.SetHP(playerUnit);

                if (isDead) {
                    killer = i;
                    break;
                }
            }
        }

        if (isDead) 
        {
            dialogueText.text = dialogueText.text = monsterUnit[killer].unitName + " has killed the player!";
            yield return new WaitForSeconds(2f);

            state = GameState.LOST;
            EndBattle();
        } else {
            turns += 1;

            bool keepGoing = true;

            for (int i = 0; ((i < 3) && keepGoing); i++) {
                if (takenMonsterPlatforms[i] == true) {
                    float threshold = 0f;
                    if (i == 0 || i == 2) 
                        threshold = -200f;
                    else
                        threshold = -300f;
                    if (monsterUnit[i].transform.position.z <= threshold) {
                        keepGoing = false;
                        dialogueText.text = monsterUnit[i].unitName + " has killed the player!";
                        playerUnit.TakeDamage(playerUnit.currentHP);
                        playerHUD.SetHP(playerUnit);
                        yield return new WaitForSeconds(2f);

                        state = GameState.LOST;
                        EndBattle();
                    }
                }
            }

            if (keepGoing) {
                if (turns % 2 == 0) {
                    dialogueText.text = "A monster has spawned!";

                    SpawnMonster();
                    yield return new WaitForSeconds(timeForSpeech);
                }

                timeForSpeech -= 0.125f;
                maxTime -= 0.5f;
                if (timeForSpeech < 1.0f) {
                    state = GameState.WON;
                    EndBattle();
                } else {
                    state = GameState.WAITING;
                    IP_PlayerTurn();
                }
            }
        }
    }

    void EndBattle()
    {
        if (state == GameState.WON) 
        {
            turnText.text = "WON";
            dialogueText.text = "You won with " + playerUnit.points.ToString() + " points!";
        } else if (state == GameState.LOST) {
            turnText.text = "LOST";
            dialogueText.text = "You died with " + playerUnit.points.ToString() + " points!";
        }
    }

    void IP_PlayerTurn()
    {
        state = GameState.WAITING;

        turnText.text = "IP_Player";
        dialogueText.text = "Choose an action:";

        // Enable combat buttons
        combatButtons.SetActive(true);
        // Disable direction buttons
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }

        runningTime = true;
    }

    IEnumerator IP_PlayerAttack_Choose()
    {
        runningTime = false;

        turnText.text = "IP_Player";
        dialogueText.text = "You've chosen attack!";

        // Disable combat buttons
        combatButtons.SetActive(false);

        yield return new WaitForSeconds(timeForSpeech);

        dialogueText.text = "Choose a monster to attack:";

        // Enable direction buttons
        for (int i = 0; i < 3; i++) {
            if (takenMonsterPlatforms[i] == true) {
                directions[i].SetActive(true);
            } else {
                directions[i].SetActive(false);
            }
        }

        runningTime = true;
    }

    public void Choose0()
    {
        runningTime = false;
        StartCoroutine(IP_PlayerAttack(0));
    }

    public void Choose1()
    {
        runningTime = false;
        StartCoroutine(IP_PlayerAttack(1));
    }

    public void Choose2()
    {
        runningTime = false;
        StartCoroutine(IP_PlayerAttack(2));
    }

    IEnumerator IP_PlayerAttack(int index)
    {
        // Disable combat buttons
        combatButtons.SetActive(false);
        // Disable direction buttons
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }
        
        if (standing_index != -1 && (takenMonsterPlatforms[standing_index] == true)) {
            state = GameState.MONSTER_TURN;

            playerUnit.damage = Random.Range(0, 500);

            dialogueText.text = "You're attacking " + monsterUnit[standing_index].unitName + "!";

            yield return new WaitForSeconds(timeForSpeech);
            
            dialogueText.text = "You've dealt " + playerUnit.damage.ToString() + " damage!";
            
            playerUnit.isDefending = false;

            bool isDead = monsterUnit[index].TakeDamage(playerUnit.damage);

            monsterHUD[index].SetHP(monsterUnit[index]);

            state = GameState.MONSTER_TURN;

            yield return new WaitForSeconds(timeForSpeech);

            if (isDead) {
                dialogueText.text = "You've killed a monster!";

                playerUnit.points += monsterUnit[index].value;
                DespawnMonster(index);

                yield return new WaitForSeconds(timeForSpeech);

                dialogueText.text = "You've gained " + monsterUnit[index].value.ToString() + " points!";

                playerHUD.SetPoints(playerUnit);

                yield return new WaitForSeconds(timeForSpeech);

                bool allFalse = true;
                for (int i = 0; i < 3; i++) {
                    if (takenMonsterPlatforms[i] == true) {
                        allFalse = false;
                        break;
                    } 
                }

                if (allFalse) {
                    dialogueText.text = "A monster has spawned!";
                    SpawnMonster();

                    yield return new WaitForSeconds(timeForSpeech);
                }

                state = GameState.IP_PLAYER_TURN;
                IP_PlayerTurn();
            } else {
                StartCoroutine(MonsterTurn());
            }
        } else {
            dialogueText.text = "You're standing on the wrong spot!";

            yield return new WaitForSeconds(timeForSpeech);

            dialogueText.text = "You've lost your turn!";

            state = GameState.MONSTER_TURN;
            StartCoroutine(MonsterTurn());
        }

        
    }

    IEnumerator IP_PlayerDefend()
    {
        // Disable combat buttons
        combatButtons.SetActive(false);

        dialogueText.text = "IP_Player defended!";
        playerUnit.isDefending = true;

        state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(timeForSpeech);
        StartCoroutine(MonsterTurn());
    }

    IEnumerator IP_PlayerHeal()
    {
        // Disable combat buttons
        combatButtons.SetActive(false);

        int healAmount = Random.Range(0, 500);

        playerUnit.Heal(healAmount);
        playerHUD.SetHP(playerUnit);
        dialogueText.text = "IP_Player healed " + healAmount + " HP!";

        state = GameState.MONSTER_TURN;

        yield return new WaitForSeconds(timeForSpeech);

        StartCoroutine(MonsterTurn());
    }

    public void OnAttackButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        StartCoroutine(IP_PlayerAttack_Choose());
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
        // Create list which will track which indices are available
        List<int> indices = new List<int>();

        // If an index is available, add it to the list
        for (int i = 0; i < 3; i++) {
            if (takenMonsterPlatforms[i] == false) {
                indices.Add(i);
            }
        }

        // for (int i = 0; i < 4; i++) {
        //     GameObject monsterGO = Instantiate(monsterPrefab, monsterPlatform[i]);
        //     monsterUnit[i] = monsterGO.GetComponent<Monster>();

        //     Vector3 offset = new Vector3(0f, 75f, 0f);

        //     GameObject monsterHUDGO = Instantiate(monsterHUDPrefab, monsterPlatform[i]);
        //     monsterHUDGO.transform.position += offset;
        //     monsterHUD[i] = monsterHUDGO.GetComponent<MonsterHUD>();
        // }
        
        // If there is an index that is available, pick a random one to use
        // Then, take the spot and spawn a monster
        int randomIndex = -1;
        if (indices.Any()) {
            // Pick random index from LIST
            randomIndex = Random.Range(0, indices.Count() - 1);
            // Actual index to use
            int i_to_use = indices[randomIndex];

            // Set platform to true to not reuse
            takenMonsterPlatforms[i_to_use] = true;
            // Instantiate monster
            GameObject monsterGO = Instantiate(monsterPrefab, monsterPlatform[i_to_use]);
            monsterUnit[i_to_use] = monsterGO.GetComponent<Monster>();

            // Offset for hp bar
            Vector3 offset = new Vector3(0f, 100f, 0f);

            // Instantiate hp bar
            GameObject monsterHUDGO = Instantiate(monsterHUDPrefab, monsterPlatform[i_to_use]);
            monsterHUDGO.transform.position += offset;
            monsterHUD[i_to_use] = monsterHUDGO.GetComponent<MonsterHUD>();

            // Initialize monster settings
            monsterUnit[i_to_use].unitName = "Monster " + i_to_use.ToString();
            monsterUnit[i_to_use].maxHP = 500;
            monsterUnit[i_to_use].currentHP = monsterUnit[i_to_use].maxHP;
            monsterUnit[i_to_use].value = 1000;

            // Set up hp bar
            monsterHUD[i_to_use].SetHUD(monsterUnit[i_to_use]);
        }
    }

    void DespawnMonster(int index) 
    {
        Destroy(monsterUnit[index].gameObject);
        Destroy(monsterHUD[index].gameObject);
        takenMonsterPlatforms[index] = false;
    }

    private void OnMessageArrivedCommandHandler(string newMsg)
    {
        Debug.Log("Command Event Fired. The message is = " + newMsg);
        Debug.Log("Gamestate is: " + state);
        runningTime = false;
        if (state == GameState.WAITING) {
            if(newMsg=="attack")
            {
                Debug.Log("attack");
                state = GameState.IP_PLAYER_TURN;
                StartCoroutine(IP_PlayerAttack(standing_index));
            }
            else if(newMsg=="heal")
            {
                state = GameState.IP_PLAYER_TURN;
                StartCoroutine(IP_PlayerHeal());
            } else if(newMsg=="defend")
            {
                state = GameState.IP_PLAYER_TURN;
                StartCoroutine(IP_PlayerDefend());
            }
        }
    } 

    private void OnMessageArrivedPositionHandler(string newMsg)
    {
        Debug.Log("Position Event Fired. The message is = " + newMsg);
        if (newMsg == "L") {
            standing_index = 0;
        } else if (newMsg == "R") {
            standing_index = 2;
        } else if (newMsg == "M") {
            standing_index = 1;
        } else {
            standing_index = -1;
        }
        
    } 
}
