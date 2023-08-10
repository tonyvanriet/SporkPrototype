# Spork DevLog 2023-08-06
#spork #devlog

## Cleaning up basic card movement
When I started this I wanted to see if I could build whatever code was needed to leaf through your hand of cards and have that feel satisfying. Not the most important, but... important, and I was drawn to it. I think I've just about pulled that off. Just two problems that I wanna take care of before moving off of feel and moving on to gameplay features that enable game design.

- [x] card OnMouseEnter sometimes triggers on a card that appears below the currently selected card

I thought it would be better to just use `sortingOrder` instead of `transform.z` to control the card depth, but `sortingOrder` only influences visual depth. The `OnMouseEnter` is triggered by the collider so the depth of the collider determines which card the mouse hits if they're overlapping. For now I'm setting both.

``` cs
  // set sortingOrder and transform.z to the given depth
  // larger value is closer to the camera
  void SetDepth(int depth)
  {
    cardSpriteRenderer.sortingOrder = depth;
    Vector3 currentPosition = transform.localPosition;
    currentPosition.z = (float)depth / -10;
    transform.localPosition = currentPosition;
  }
````

Seems likely that there’s a better way to uniformly manage visual and physical depth but I’m gonna live with this for now.

- [x] hovered position of cards is higher for the cards on the outside of the fan

I was getting the card size from its `Collider.bounds.size` which comes back larger when the card is rotated. The quick fix here was to use the `transform.localScale`but that doesn’t seem great being dependent on it scaling something that’s size 1,1. I don’t have a great sense of how the size and scaling work for a sprite object. Maybe a better solution will present itself when I spend more time with it later.
## Starting on gameplay mechanics
I wanna make this minimally playable so that I can start tinkering with game design. By game design here I mean the abilities and numbers on the cards and how that’s balanced with the enemies abilities. I imagine this cycle to be:
1. Pick a game design element to try out.
2. Implement the gameplay mechanics needed for that game design element.
3. Go play the game and tune that game design element.
4. Goto 1.

An obvious example of a starting point would be:
1. Attack Cards - The player has basic attack cards that damage an enemy when played.
2. Implement game mechanics to support attack cards.
   * The player is dealt attack cards in their hand.
   * The player can play an attack card by dragging it onto an enemy.
   * Attacks are visible on the screen.
   * Attacks take away health from the enemy.
   * The enemy’s health is visible on the screen.
   * The enemy dies when they run out of health.
   * A basic attack card goes to the discard pile after it’s played.
   * The draw pile is replenished from the discard pile.
3. Go play the game. Kill the enemy with basic attacks.

Of course there will be a lot more fundamental mechanics needed in the early iterations and there won’t be much to do in terms of tuning game design elements until there are more available. But it might be a good idea to still go through the motions of this cycle to make sure I stay connected to the higher level game design rather than just churning through implementation of game mechanics that I think I might want.

## Playing an attack card
Let's start with dragging a card onto an enemy to play that card.

Dragging is easy enough.
``` cs
  bool isHolding = false;
  Vector2 holdOffset;

  void OnMouseDown()
  {
    if (Input.GetMouseButton(0))
    {
      // Calculate the offset between the click location
      // and the card's position
      Vector2 mousePosition =
        Camera.main.ScreenToWorldPoint(Input.mousePosition);

      switch (holdPosition)
      {
        case HoldPosition.Initial:
          holdOffset = (Vector2)transform.position - mousePosition;
          break;
        case HoldPosition.Center:
          holdOffset = Vector2.zero;
          break;
        default:
          holdOffset = Vector2.zero;
          break;
      }

      isHolding = true;
      isMoving = false;
    }
  }

  private void OnMouseUp()
  {
    isHolding = false;
    // get back in your fan!!
    StartMovement(positionInFan, depthInFan);
    this.transform.localRotation = rotationInFan;
  }

  void Update()
  {

    if (isHolding)
    {
      // update the position of the card to follow the mouse
      // and include the original offset of the mouse click
      Vector2 mousePosition =
        Camera.main.ScreenToWorldPoint(Input.mousePosition);
      Vector2 heldPosition = mousePosition + holdOffset;
      transform.position = heldPosition;
      SetDepth(holdDepth);
    }
  }
```

Pretty fun to see the card return to the hand.

![](Spork%20DevLog%202023-08-06/CleanShot%202023-08-06%20at%2017.38.04.gif)<!-- {"width":739} -->

Gotta decide if the card should be held at the point where it was initially clicked or at the center. Not sure which is better UX. I think a center hold or a bottom hold is a good affordance for touch input which I'd like to eventually do with this prototype.

Did a little more work to detect when the card is dropped onto an enemy. If the card collides with another object while being held, hold a reference to that object. Throw away that reference if the collision ends. If the card is dropped while while colliding with an enemy object, 💥. For now just destroying the card object.

```cs
  GameObject heldOverObject;

  void OnTriggerEnter2D(Collider2D other)
  {
    if (isHolding)
      heldOverObject = other.gameObject;
  }

  void OnTriggerExit2D(Collider2D other)
  {
    heldOverObject = null;
  }

  void OnMouseUp()
  {
    isHolding = false;
    if (heldOverObject && heldOverObject.CompareTag("Enemy"))
    {
      Destroy(gameObject, 0.1f);
    }
    else
    {
      // get back in your fan!!
      StartMovement(positionInFan, depthInFan);
      this.transform.localRotation = rotationInFan;
    }
  }
```

![](Spork%20DevLog%202023-08-06/CleanShot%202023-08-06%20at%2023.54.35.gif)<!-- {"width":738} -->

