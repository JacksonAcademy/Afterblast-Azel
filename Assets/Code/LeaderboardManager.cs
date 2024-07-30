using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Progress;

public class LeaderboardManager : MonoBehaviour
{
    public LeaderboardItem item;
    public Transform spawnTransform;
    public static LeaderboardManager instance;

    public List<LeaderboardItem> items;
    public void Awake()
    {
        instance = this;
    }
    public void UpdatePlayer(PlayerData playerData)
    {
        print("updating leaderboard player: " + playerData.playerName);
        LeaderboardItem leaderboardItem = null;
        if(items.Count > 0)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].playerData == playerData)
                {
                    leaderboardItem = items[i];
                    break;
                }
            }
        }
        else if(items.Count == 0)
        {
            leaderboardItem = Instantiate(item, spawnTransform);
            items.Add(item);
        }
        leaderboardItem.playerData = playerData;
        leaderboardItem.text.text = playerData.playerName.ToString() + " - " + playerData.playerKills.ToString();

    }
}
