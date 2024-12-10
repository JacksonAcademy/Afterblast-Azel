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
    public Slider healthSlider, healthRed;
    public TextMeshProUGUI healthText;
    public GameObject deathParticle;
    public PlayerManager player;
    public float redSmoothness;
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
        healthRed.maxValue = startingHealth;
        healthRed.value = startingHealth;
    }
    private void FixedUpdate()
    {
        if(base.IsOwner)
        {
            healthRed.value = Mathf.Lerp(healthRed.value, healthSlider.value, redSmoothness * Time.deltaTime);
        }
    }
    [ObserversRpc]
    public void ObserversDieEffects(NetworkObject whoKilledMe)
    {
        DieEffect();
        if (base.IsOwner)
            player.DieScreen(whoKilledMe.GetComponent<PlayerManager>());
        SetHealth(startingHealth);
        DieEffects(whoKilledMe);
        player.Die();
    }
    public IEnumerator DieEffect()
    {
        animator._animator.SetTrigger("Death");
        yield return new WaitForSeconds(3);
        player.HideModel();
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
