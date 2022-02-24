using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { START, IP_PLAYER_TURN, MONSTER_TURN, END }

public class GameLogic : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    public Transform playerPlatform;
    public Transform monsterPlatform;

    IP_Player playerUnit;
    Monster monsterUnit;

    public Text HPBarName;

    public GameState state;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.START;
        SetupBattle();
    }

    void SetupBattle() 
    {
        // Create player and monster object
        GameObject playerGO = Instantiate(playerPrefab, playerPlatform);
        playerUnit = playerGO.GetComponent<IP_Player>();

        GameObject monsterGO = Instantiate(monsterPrefab, monsterPlatform);
        monsterUnit = monsterGO.GetComponent<Monster>();

        HPBarName.text = playerUnit.unitName;
    }
}
