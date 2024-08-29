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
    private int previousCount;
    private float lastChange;
    
    public void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        gameManager = GameManager.instance;
    }

    private void Update()
    {
        if(gameManager.players.Count !=  previousCount)
        {
            UpdateLeaderboard();
            previousCount = items.Count;
        }
        else
        { //UPdate leaderboard every second
            if (Time.time > lastChange)
            {
                lastChange = Time.time + 1;
                UpdateLeaderboard();
            }
        }

    }
    public void UpdatePlayer(PlayerData playerData)
    {
        LeaderboardItem leaderboardItem = null;
        leaderboardItem = Instantiate(item.gameObject, spawnTransform).GetComponent<LeaderboardItem>();
        leaderboardItem.playerData = playerData;
        leaderboardItem.text.text = playerData.playerName.ToString() + " - " + playerData.playerKills.ToString();
        items.Add(leaderboardItem);
    }
    public void DeleteLeaderboards()
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
    public void UpdateLeaderboard()
    {
        DeleteLeaderboards();
        for(int i = 0; i < gameManager.players.Count; i++)
        {
            UpdatePlayer(gameManager.players[i]);
        }
    }
}
