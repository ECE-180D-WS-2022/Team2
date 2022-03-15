using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum GameState { START, WAITING, IP_PLAYER_TURN, R_PLAYER_TURN, MONSTER_TURN, WON, LOST }

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
    public mqttReceiver remoteReceiver;

    int turns = 0;

    int standing_index = -1;
    int chosen_index = -1;

    float timeForSpeech = 2f;
    float maxTime = 5f;
    float currentTime = 0f;
    bool runningTime;

    // Start is called before the first frame update
    void Start()
    {
        commandReceiver.OnMessageArrived += OnMessageArrivedCommandHandler;
        positionReceiver.OnMessageArrived += OnMessageArrivedPositionHandler;
        remoteReceiver.OnMessageArrived += OnMessageArrivedRemoteHandler;

        // Turn off timer
        runningTime = false;

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
            TooSlow();
        }
    }

    void TooSlow()
    {
        StartCoroutine(TooSlowMessage());
    }

    IEnumerator TooSlowMessage()
    {
        dialogueText.text = "You're too slow!";
        if (state == GameState.R_PLAYER_TURN) {
            remoteReceiver.messagePublish = "You're too slow";
            remoteReceiver.Publish();
        }

        combatButtons.SetActive(false);
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }
        yield return new WaitForSeconds(timeForSpeech);
        if (state == GameState.IP_PLAYER_TURN) {
            state = GameState.R_PLAYER_TURN;
            R_PlayerTurn();
        } else {
            StartCoroutine(CheckEndGame());
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

    void SendPrompt()
    {
        remoteReceiver.messagePublish = "Please pick a monster to control: ";
        remoteReceiver.Publish();

        string messageToSend = "";
        bool first = false;

        for (int i = 0; i < 3; i++) {
            if (takenMonsterPlatforms[i] == true) {
                if (!first) {
                    first = true;
                    messageToSend += i.ToString();
                } else {
                    messageToSend += ", " + i.ToString();
                }
            }
        }

        remoteReceiver.messagePublish = messageToSend;
        remoteReceiver.Publish();
    }

    void R_PlayerTurn()
    {
        runningTime = false;
        // Disable combat buttons
        combatButtons.SetActive(false);
        // Disable direction buttons
        for (int i = 0; i < 3; i++) {
            directions[i].SetActive(false);
        }

        turnText.text = "R_Player";

        dialogueText.text = "Waiting for remote player to choose...";

        SendPrompt();

        state = GameState.R_PLAYER_TURN;
        runningTime = true;
    }

    IEnumerator MonsterTurn()
    {
        turnText.text = "Monster";

        bool isDead = false;
        int killer = -1;

        
        if (chosen_index != -1) {
            if (takenMonsterPlatforms[chosen_index] == true) {
                state = GameState.MONSTER_TURN;
                dialogueText.text = monsterUnit[chosen_index].unitName + " attacked!";

                yield return new WaitForSeconds(timeForSpeech);

                monsterUnit[chosen_index].damage = Random.Range(0, 250);
                if (playerUnit.isDefending == true) {
                    playerUnit.isDefending = false;
                    monsterUnit[chosen_index].damage /= 2;
                }

                dialogueText.text = monsterUnit[chosen_index].unitName + " dealt " + monsterUnit[chosen_index].damage + " HP!";

                yield return new WaitForSeconds(timeForSpeech);

                isDead = playerUnit.TakeDamage(monsterUnit[chosen_index].damage);

                Vector3 moveforward = new Vector3(0f, 0f, -25f);

                monsterUnit[chosen_index].transform.position += moveforward;
                monsterHUD[chosen_index].transform.position += moveforward;

                playerHUD.SetHP(playerUnit);

                if (isDead) {
                    killer = chosen_index;
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
            StartCoroutine(CheckEndGame());
        }
    }

    IEnumerator CheckEndGame()
    {
        turns += 1;

        bool keepGoing = true;

        for (int i = 0; ((i < 3) && keepGoing); i++) {
            if (takenMonsterPlatforms[i] == true) {
                float threshold = -300f;
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
                SpawnMonster();
                yield return new WaitForSeconds(timeForSpeech);
            }

            timeForSpeech -= 0.125f;
            maxTime -= 0.5f;
            if (timeForSpeech < 1.0f) {
                state = GameState.WON;
                EndBattle();
            } else {
                state = GameState.IP_PLAYER_TURN;
                IP_PlayerTurn();
            }
        }
    }

    void EndBattle()
    {
        if (state == GameState.WON) 
        {
            turnText.text = "WON";
            dialogueText.text = "You won with " + playerUnit.points.ToString() + " points!";
        
            remoteReceiver.messagePublish = "IP Player has won!";
            remoteReceiver.Publish();
            remoteReceiver.messagePublish = "You've lost!";
            remoteReceiver.Publish();
        
        } else if (state == GameState.LOST) {
            turnText.text = "LOST";
            dialogueText.text = "You died with " + playerUnit.points.ToString() + " points!";
        
            remoteReceiver.messagePublish = "IP player has lost!";
            remoteReceiver.Publish();
            remoteReceiver.messagePublish = "You've won!";
            remoteReceiver.Publish();
        }
    }

    void IP_PlayerTurn()
    {
        runningTime = false;
        turnText.text = "IP_Player";
        dialogueText.text = "Choose an action:";

        playerUnit.isDefending = false;

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

        // Switch states so player cannot do additional moves in between
        state = GameState.WAITING;
        
        if (standing_index != -1 && (takenMonsterPlatforms[standing_index] == true)) {

            playerUnit.damage = Random.Range(300, 500);

            dialogueText.text = "You're attacking " + monsterUnit[standing_index].unitName + "!";

            yield return new WaitForSeconds(timeForSpeech);
            
            dialogueText.text = "You've dealt " + playerUnit.damage.ToString() + " damage!";
            
            playerUnit.isDefending = false;

            bool isDead = monsterUnit[index].TakeDamage(playerUnit.damage);

            monsterHUD[index].SetHP(monsterUnit[index]);

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
                    SpawnMonster();

                    yield return new WaitForSeconds(timeForSpeech);
                }

                state = GameState.IP_PLAYER_TURN;
                IP_PlayerTurn();
            } else {
                //StartCoroutine(MonsterTurn());
                state = GameState.R_PLAYER_TURN;
                R_PlayerTurn();
            }
        } else {
            dialogueText.text = "You're standing on the wrong spot!";

            yield return new WaitForSeconds(timeForSpeech);

            dialogueText.text = "You've lost your turn!";

            //StartCoroutine(MonsterTurn());
            state = GameState.R_PLAYER_TURN;
            R_PlayerTurn();
        }

        
    }

    IEnumerator IP_PlayerDefend()
    {
        // Disable combat buttons
        combatButtons.SetActive(false);

        dialogueText.text = "IP_Player defended!";
        playerUnit.isDefending = true;

        state = GameState.WAITING;  

        yield return new WaitForSeconds(timeForSpeech);

        state = GameState.R_PLAYER_TURN;
        R_PlayerTurn();
    }

    IEnumerator IP_PlayerHeal()
    {
        // Disable combat buttons
        combatButtons.SetActive(false);

        int healAmount = Random.Range(300, 500);

        playerUnit.Heal(healAmount);
        playerHUD.SetHP(playerUnit);
        dialogueText.text = "IP_Player healed " + healAmount + " HP!";

        state = GameState.WAITING;

        yield return new WaitForSeconds(timeForSpeech);

        state = GameState.R_PLAYER_TURN;
        R_PlayerTurn();
    }

    public void OnAttackButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        runningTime = false;
        StartCoroutine(IP_PlayerAttack_Choose());
    }

    public void OnDefendButton()
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        runningTime = false;
        StartCoroutine(IP_PlayerDefend());
    }

    public void OnHealButton() 
    {
        if (state != GameState.IP_PLAYER_TURN)
            return;
        
        runningTime = false;
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
            dialogueText.text = "A monster has spawned!";

            // Pick random index from LIST
            randomIndex = Random.Range(0, indices.Count());
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
        } else {
            dialogueText.text = "No space for monster!";
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
        //Debug.Log("Command Event Fired. The message is = " + newMsg);
        //Debug.Log("Gamestate is: " + state);
        runningTime = false;
        if (state == GameState.IP_PLAYER_TURN) {
            if (newMsg=="attack")
            {
                StartCoroutine(IP_PlayerAttack(standing_index));
            }
            else if (newMsg=="heal")
            {
                StartCoroutine(IP_PlayerHeal());
            } else if (newMsg=="defend")
            {
                StartCoroutine(IP_PlayerDefend());
            }
        }
    } 

    private void OnMessageArrivedPositionHandler(string newMsg)
    {
        //Debug.Log("Position Event Fired. The message is = " + newMsg);
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

    private void OnMessageArrivedRemoteHandler(string newMsg)
    {
        if (state == GameState.R_PLAYER_TURN) {
            if (newMsg=="0")
            {
                runningTime = false;
                chosen_index = int.Parse(newMsg);
                Debug.Log("Chosen_index is: " + chosen_index.ToString());
                state = GameState.MONSTER_TURN;
                StartCoroutine(MonsterTurn());
            }
            else if (newMsg=="1")
            {
                runningTime = false;
                chosen_index = int.Parse(newMsg);
                Debug.Log("Chosen_index is: " + chosen_index.ToString());
                state = GameState.MONSTER_TURN;
                StartCoroutine(MonsterTurn());
            } else if (newMsg=="2")
            {
                runningTime = false;
                chosen_index = int.Parse(newMsg);
                Debug.Log("Chosen_index is: " + chosen_index.ToString());
                state = GameState.MONSTER_TURN;
                StartCoroutine(MonsterTurn());
            } else {
                remoteReceiver.messagePublish = "You've sent an invalid response! Try again...";
                remoteReceiver.Publish();

                SendPrompt();
            }
        }
    }
}
