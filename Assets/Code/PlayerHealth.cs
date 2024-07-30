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
    [ObserversRpc]
    public void ObserversDieEffects(NetworkObject whoKilledMe)
    {
        player.Hide(false);
        if (base.IsOwner)
            player.DieScreen(whoKilledMe.GetComponent<PlayerManager>());
        SetHealth(startingHealth);
        DieEffects(whoKilledMe);
        player.Die();
    }
    public void DieEffects(NetworkObject whoKilledMe)
    {
        GameObject effect = Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(effect, 10);
    }
    public void TakeDamage(int amount)
    {
        SetHealth(health - amount);
    }
}
