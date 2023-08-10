using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShip : MonoBehaviour
{
  public UnityEngine.UI.Image healthFillImage;
  public int maxHealth = 50;
  public int health = 50;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    float healthPercentage = (float)health / maxHealth;
    healthFillImage.fillAmount = healthPercentage;
  }
}
