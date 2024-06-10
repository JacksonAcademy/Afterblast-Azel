using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int playerCount;
    public List<Color> playerColors;
    public List<string> playerNames;
    public List<Transform> spawnPositions;

    public static GameManager instance;
    void Awake()
    {
        instance = this;
    }
}
