using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<PlayerManager> players;
    public int playerCount;
    public List<Color> playerColors;
    public List<Transform> spawnPositions;
    public RelayMatchmaker matchmaker;
    public static GameManager instance;
    void Awake()
    {
        instance = this;
    }
}
