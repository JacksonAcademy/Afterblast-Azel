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
        player.Die(whoKilledMe);

    }
    [ServerRpc]
    public void CMDdie()
    {
        if (!base.IsOwner)
            Die();
        ObserversDie();
    }
    [ObserversRpc]
    public void ObserversDie()
    {
        if (base.IsOwner || base.IsServerInitialized)
            return;
        Die();
    }
    public void Die()
    {
        GameObject effect = Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(effect, 10);
    }
    public void Respawn()
    {
        SetHealth(startingHealth);
    }
    public void TakeDamage(int amount, PlayerManager whoDamagedMe)
    {
        SetHealth(health - amount);
        if(health <= 0)
        {
            Death(whoDamagedMe);
            if(base.IsOwner)
            {
                player.Die(whoDamagedMe);
            }
        }
    }
}
