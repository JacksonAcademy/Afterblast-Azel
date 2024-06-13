using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public LeaderboardItem leaderboardItem;

    public static LeaderboardManager instance;
    public void Awake()
    {
        instance = this;
    }
}
