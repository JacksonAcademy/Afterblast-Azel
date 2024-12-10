using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public LeaderboardItem item;
    public Transform spawnTransform;
    public static LeaderboardManager instance;

    public List<LeaderboardItem> items;

    private GameManager gameManager;
    
    public void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        gameManager = GameManager.instance;
    }
    public void UpdatePlayer(PlayerData playerData)
    {
        bool playerExists = false;
        for(int i =0; i < items.Count; i++) 
        {
            if (items[i].playerData.playerClientID == playerData.playerClientID)
            {
                items[i].playerData = playerData;
                items[i].text.text = "<b> " + playerData.playerName + " </b> " + playerData.playerKills + " kills - " + playerData.playerDeaths + " deaths";
                playerExists = true;
                break;
            }
        }
        if (!playerExists)
        {
            LeaderboardItem leaderboardItem = null;
            leaderboardItem = Instantiate(item.gameObject, spawnTransform).GetComponent<LeaderboardItem>();
            leaderboardItem.playerData = playerData;
            leaderboardItem.text.text = "<b> " + playerData.playerName + " </b> " + playerData.playerKills + " kills - " + playerData.playerDeaths + " deaths";
            items.Add(leaderboardItem);
        }
    }
    public void RemovePlayer(PlayerData playerData)
    {
        LeaderboardItem temp = null;
        int count = items.Count;
        for(int i= 0; i < count; i++)
        {
            temp = items[0];
            items.Remove(items[0]);
            Destroy(temp.gameObject);
        }
    }
}
