using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealth : NetworkBehaviour
{
    public int health;

    public int startingHealth;
    [HideInInspector] public PlayerAnimator animator;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public GameObject deathParticle;
    public PlayerManager player;
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        startingHealth = 100;
        SetHealth(startingHealth);
    }
    private void Awake()
    {
        startingHealth = 100;
        animator = GetComponent<PlayerAnimator>();
        SetHealth(startingHealth);
    }
    public void SetHealth(int amount)
    {
        health = amount;
        healthText.text = amount.ToString();
        healthSlider.value = amount;
        healthSlider.maxValue = startingHealth;
    }
    public void Death(PlayerManager whoKilledMe)
    {
        Die();
        CMDdie();
        Respawn();


    }
    [ServerRpc]
    public void CMDdie()
    {
        ObserversDie();
    }
    [ObserversRpc]
    public void ObserversDie()
    {
        Die();
    }
    public void Die(NetworkObject whoKilledMe)
    {
        PlayerManager playerWhoKilledMe = whoKilledMe.GetComponent<PlayerManager>();
        if(base.IsOwner)
        {

        }
        player.Die(playerWhoKilledMe);
        GameObject effect = Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(effect, 10);

    }
    public void Respawn()
    {
        SetHealth(startingHealth);
    }
    public void TakeDamage(int amount)
    {
        SetHealth(health - amount);
    }
}
