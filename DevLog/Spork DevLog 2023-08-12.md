# Spork DevLog 2023-08-12
#spork	#devlog

## ![](Spork%20DevLog%202023-08-12/CleanShot%202023-08-10%20at%2021.19.43.gif)<!-- {"width":1001} -->

I showed my kids how I could now play cards to kill the enemy. The first thing they noticed was how the hand of cards looked janky after playing a card because it left a gap. Can't stand for that. (The first thing *I* noticed was that the cards went underneath the health bar. Super easy fix. Just needed to move the Canvas for the health bar to the `Ships` sorting layer.)

## Reposition the hand after playing a card
To fix the position of the cards in the hand after playing a card I'll need the HandController to know when a card has left the hand so it can tell the rest of the cards to move to their new position. Should be no problem to reuse the fan positioning code after playing a card.

First the Card gets a reference to the HandController script and when the card is used for an attack it tells the HandController it's being played.
```cs
public class Card : MonoBehaviour
{
  public HandController handControllerScript;
  // ...

  void OnMouseUp()
  {
    isHolding = false;
    if (heldOverObject && heldOverObject.CompareTag("Enemy"))
    {
      // attack the enemy
      EnemyShip enemy = heldOverObject.GetComponent<EnemyShip>();
      enemy.ReceiveAttack(8);

      // play the card out of the hand
      handControllerScript.PlayCard(gameObject);

      // despawn the card
      Destroy(gameObject, 0.1f);
    }
    // ...
  }
```

Then refactor the HandController to separate the card spawning (DealHand) from the fan positioning (FanCards) and add a PlayCard function that removes the card and repositions the remaining cards.

```cs
public class HandController : MonoBehaviour
{
  // ...
  public void PlayCard(GameObject card)
  {
    // get rid of the card
    cardsInHand.Remove(card);
    Destroy(card, 0.1f);

    // reposition the hand
    FanCards();
  }
  // ...

  void DealHand(int numCards)
  {
    float fanTotalAngle = fanAngleIncrement * (numCards - 1);
    float fanInitialAngle = -fanTotalAngle / 2;

    for (int i = 0; i < numCards; i++)
    {
      GameObject card = Instantiate(cardGameObject,
        cardSpawnLocation, Quaternion.identity);
      cardsInHand.Add(card);
      card.GetComponent<Card>().handControllerScript = this;
    }
    FanCards();
  }

  void FanCards()
  {
	// set the position, rotation, depth of the cardsInHand
    // ...
  }
}
```

This all works and was quite easy to add, but I do worry about this pattern leading to unmaintainable spaghetti code. I suppose the worst sin here is the Card needing to know about the HandController. It makes sense for the HandController to know about the Cards but not vice versa. I should try to emit some event from the Card when it's played and leave it up to the HandController to react to that event. Alas I am on a plane and don't have any good C# documentation so it'll have to wait.

## Player health
I could go further with adding mechanics to support Attack Cards, but I feel like jumping to a couple other game design elements first. 1) The enemy attacks us on their turn and 2) we can play Shield cards to reduce the damage we take. 

Let's start by giving the player a health bar. We'll make it a HUD based health bar to see how that works. I'll try to copy the existing HealthBar object tree from the enemy and see how easy it is to reuse that in a different Canvas that's set to `Render Mode: Screen Space - Overlay`.

![](Spork%20DevLog%202023-08-12/CleanShot%202023-08-13%20at%2008.53.50%202.gif)

Ok, the reuse worked great. I made the HealthBar its own Prefab. The enemy puts its health bar in it's own World Space Canvas while the hero health bar goes in a separate Overlay Canvas that will act as the HUD. I was able to differentiate the size and position of the hero HealthBar without making any changes to the HealthBar Prefab. And I was able to attach this health bar to the HeroShip script which is nearly identical to the EnemyShip script, thus far.

