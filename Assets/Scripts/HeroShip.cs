using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class HeroShip : MonoBehaviour
{
  public int maxHealth = 100;
  public int currentHealth;
  public HealthBar healthBar;

  void Start()
  {
    currentHealth = maxHealth;
    healthBar.SetMaxHealth(maxHealth);
    healthBar.SetHealth(currentHealth);
  }

  public void ReceiveAttack(int damage)
  {
    currentHealth -= damage;
  }

  void Update()
  {
    healthBar.SetHealth(currentHealth);
    if (currentHealth <= 0)
    {
      Destroy(gameObject, 0.1f);
      // game over man
    }
  }
}
