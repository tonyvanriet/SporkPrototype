# Spork DevLog 2023-08-10
#spork #devlog

From the last session, the current meta goal is to implement whatever gameplay mechanics are needed to support the game design element of [playing an attack card](Spork%20DevLog%202023-08-06.md#playing-an-attack-card). We can already drag a card around and when it's dropped on the enemy, the card despawns. Now to actually carry out the attack on the enemy.
## Attacking the enemy
I’ve spent some time trying to put a health bar on the enemy and it hasn't worked out. I started by adding 2D sprites for the health bar border and the red fill. Then when I hooked  the health value up to the Y scale of the fill, I realized that that would cause the fill to change size relative to the center of the health bar (not pictured here). Not what I want, and the idea of coordinating the X position and the Y scale to have a left justified health bar did not sound like a good time.

So, I asked ChatGPT how to do it and it led me to some patterns using a Canvas and adding UI Images to that. That still wasn't working out well for me though as the positioning and sizing of these child objects behave the same as with child GameObjects. The positioning and sizing of the UI Images was absolute for the whole scene and I wanted to work with them relative to the enemy GameObject. I just didn't understand that by default these Unity UI elements are meant to be used to implement the HUD of your game, as if it's in a static position relative to the camera, aka Render Mode: Screen Space - Overlay. If you want them to move around in the game world, that has to change to Render Mode: World Space.

Turns out this does appear to be *the* way to do it. I just needed a little more context to understand it. [This youtube video](https://www.youtube.com/watch?v=BLfNP4Sc_iA) seems to have explained it in a way that should apply to my use case, so let's go do what it says...

So far so good. I've got a Canvas with a HealthBar empty object that houses the Background Image and a Fill Image.

![](Spork%20DevLog%202023-08-10/CleanShot%202023-08-10%20at%2020.32.50@2x.png)<!-- {"width":192} -->

Then in the top level HealthBar we can:
1. control the size of the health bar graphic
2. add a `Slider` component and make it non-interactable
3. attach the Fill Image to the Fill Rect of the Slider
4. and then set min, max, and current value of the slider
5. from the script component

   ![](Spork%20DevLog%202023-08-10/CleanShot%202023-08-10%20at%2020.41.50@2x.png)<!-- {"width":277.99999999999977} -->

Now to the health bar needs to be anchored to the enemy sprite.
Set the Canvas to `Render Mode: World Space`, update the `Scale` to look appropriate, make the Canvas a child of the EnemyShip. Sweet! The health bar follows the enemy sprite!

![](Spork%20DevLog%202023-08-10/CleanShot%202023-08-10%20at%2020.57.17.gif)

Now to do damage to the enemy when we drop a card onto it. First set up the enemy script so it loses health when attacked and despawns when it loses all of its health.

``` cs
public class EnemyShip : MonoBehaviour
{
// ...
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
    }
  }
}
```

Then setup the card to attack when it's dropped on top of an enemy.

``` cs
  void OnMouseUp()
  {
    isHolding = false;
    if (heldOverObject && heldOverObject.CompareTag("Enemy"))
    {
      // attack the enemy
      EnemyShip enemy = heldOverObject.GetComponent<EnemyShip>();
      enemy.ReceiveAttack(8);

      // despawn the card
      Destroy(gameObject, 0.1f);
    }
	// ...
  }
```

And we’ve drawn first blood in Spork.

![](Spork%20DevLog%202023-08-10/CleanShot%202023-08-10%20at%2021.19.43.gif)<!-- {"width":1001} -->


