using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    public TextMeshProUGUI playerPosition, playerName, playerScore;

    public void UpdatePlayerItem(int position, string name, int score)
    {
        playerPosition.text = position.ToString();
        playerName.text = name.ToString();
        playerScore.text = score.ToString();
    }
}
