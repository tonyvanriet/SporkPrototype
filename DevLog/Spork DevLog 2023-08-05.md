# # Spork DevLog 2023-08-05
#spork

## Card Motion

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-04%20at%2001.28.22.gif)

The hovered card comes to the front, but of course we'll need some smoother motion. I'm sure I can go find some ways that this is commonly done, but I wanna try my idea first. I think this movement process can work generally for both this tiny move on hover and larger moves like from the draw pile and to the discard pile.

When the HandController (or some other future controller) wants to move a card, it sets a new desiredPosition on the card. Then when the card's Update loop sees that it has a new desiredPosition, it starts a movement process.
1. Save the current position as `moveStartingPosition`
2. Calculate the `moveDistance`
3. Move towards the `desiredPosition` accelerating rapidly until the card reaches the halfway point
4. Decelerate rapidly during the second half of the move

Actually, it just sounds like I'm rewriting a basic part of a physics engine. I get the same result by applying a force towards the desiredPosition continuously until the halfway point and then applying the opposite force. Probably need a slow minimum speed as well to avoid bugs where the card slows to 0 before it gets to the desiredPosition. Let's give it a go.

### Moving with Force
Need to add a RigidBody to use the physics engine which has me worried because the card already has a Collider. With both of them, the cards are gonna start colliding with each other.

``` cs
  public float moveForceMagnitude = 10f;
  Rigidbody2D cardRigidBody;

  void Start()
  {
    cardRigidBody = GetComponent<Rigidbody2D>();
  }

  void OnMouseEnter()
  {
    // on mouse hover, apply a force up
    Vector2 moveForce = new(0f, moveForceMagnitude);
    cardRigidBody.AddForce(moveForce);
  }
```

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2000.46.24.gif)

Sure enough, the collision causes the cards to move away from each other. But at least the force is working.

Oh, if you enable `Is Trigger` on the Collider, you can use it to trigger mouse events without any physics based collisions.

Now on hover the card is setting its desired `moveToPosition` instead of immediately setting its new `transform.position` directly. (Also, turns out it's way better to use "Order in Layer", aka `sortingOrder`, on the SpriteRenderer instead of `z - 2`  like I did last time to bring the card to the front.)

```cs
  void Update()
  {
    if (isMoving)
    {
      // find the direction to the moveToPosition
      Vector2 currentPosition = this.transform.localPosition;
      Vector2 direction = (moveToPosition - currentPosition).normalized;
      // push the card	
      Vector2 force = direction * moveForceMagnitude * Time.deltaTime;
      cardRigidBody.AddForce(force);
    }
  }

  void OnMouseEnter()
  {
    // rotate to level
    this.transform.localRotation = Quaternion.identity;
    // bring the card to the front
    cardSpriteRenderer.sortingOrder = 1;
    // move above the bottom of the view
    cardSize = cardCollider.bounds.size; // TODO fix for rotated cards
    float moveToX = transform.localPosition.x;
    float moveToY = cameraMinY + cardSize.y / 2f;
    StartMovement(new Vector2(moveToX, moveToY));
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    StartMovement(positionInFan);
    this.transform.localRotation = rotationInFan;
    cardSpriteRenderer.sortingOrder = 0;
  }

  void StartMovement(Vector2 destination)
  {
    isMoving = true;
    moveToPosition = destination;
  }
}
```


![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2011.58.10.gif)

The cards are wiggling because I haven't turned off the force yet. Aside from that, the movement seems pretty close. Tuning the magnitude of the force vs. the `Linear Drag` on the Rigidbody.

Also, realized that fragile thing where I get the height of the card *after* rotating it back to level was more fragile than I thought. The rotation doesn't take effect immediately so the height of the rotated cards is still coming back larger than desired. That's why the cards on the ends are lifting up higher than the middle ones.

Decided to reorient the scene so that 0,0 is the bottom middle. I'm just guessing that'll be easier for my brain to work with. We'll see.

### Putting on the Brakes
Let's get back to applying an opposite force after the halfway point of the movement.
```cs
  void Update()
  {
    if (isMoving)
    {
      // push the card with a force toward the destination
      // invert the force if we're past the halfway point
      Vector2 currentPosition = this.transform.localPosition;
      Vector2 direction = (moveDestination - currentPosition).normalized;
      float distanceToDestination =
        Vector2.Distance(moveDestination, transform.localPosition);
      float distanceToOrigin =
        Vector2.Distance(moveOrigin, transform.localPosition);
      int forcePolarity = distanceToDestination < distanceToOrigin ? -1 : 1;
      Vector2 force = 
        direction * forcePolarity * moveForceMagnitude * Time.deltaTime;
      cardRigidBody.AddForce(force);

      // keep moving until we're close to the destination
      isMoving = (distanceToDestination > 0.1);
    }
  }
```

It's just not working out. Relying on pushing backwards to slow down makes it too hard to smoothly reach the destination. I'm starting to think I need to apply only a forward force that's proportional to the distance remaining, then rely on a strong drag force to slow the card down. That or maybe using the physics engine in this way is misguided since these cards aren't really trying to be physical entities freely moving around the scene. I'm gonna give the new idea a try and then go searching for established ways to do this.

```cs
  void Update()
  {
    if (isMoving)
    {
      // push the card towards the destination
      // with a force that's proportional to the distance
      Vector2 currentPosition = this.transform.localPosition;
      Vector2 direction = (moveDestination - currentPosition).normalized;
      float distanceToDestination =
        Vector2.Distance(moveDestination, transform.localPosition);
      float forceMagnitude =
        distanceToDestination * moveForceScalingFactor * Time.deltaTime;
      Vector2 force = direction * forceMagnitude;
      cardRigidBody.AddForce(force);

      // keep moving until we're slowly approaching the dest
      bool landing = (distanceToDestination < 0.01
        && cardRigidBody.velocity.magnitude < 0.01);
      if (landing) CompleteMovement();
    }
  }

  // stop moving and explicitly set the position to the destination
  public void CompleteMovement()
  {
    isMoving = false;
    transform.localPosition = moveDestination;
  }
```


![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2014.56.11.gif)

That's actually looking pretty decent. Seems like a delicate balance of the force scaling factor and the drag. Tinkering with the mass doesn't seem to effect the behavior any differently. It's as if `F = ma`. 

The cards aren't returning to the correct depth because my movement code is throwing away the original z value. I ended up changing any remaining Vector3 position variables to Vector2 and I'm using the `SpriteRenderer.sortingOrder` exclusively for depth. The depths in the fan are a sequence of negative numbers and it's set to 1 on hover.

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2015.29.23.gif)

Oh yeah. That's it. Now let's go see if the internet will tell how you're *supposed* to do this...

### Moving with Interpolation

Word on the street is that you wanna use `Rigidbody2D.AddForce` or transform the position using `Lerp` or `SmoothDamp`. The latter does sound a bit more straightforward given that I don't need the card to be a full blown physical entity.

``` cs
  void Update()
  {
    if (isMoving)
      switch (moveUsing)
      {
        case MoveMethod.Force: MoveWithForce(); break;
        case MoveMethod.Lerp: MoveWithLerp(); break;
        case MoveMethod.SmoothDamp: MoveWithSmoothDamp(); break;
      }
  }

  void MoveWithForce()
  {
    // push the card towards the destination
    // with a force that's proportional to the distance
  	// ...
  }

  void MoveWithLerp()
  {
    transform.position = Vector2.Lerp(
      transform.position,
      moveDestination,
      moveLerpTime);
  }

  void MoveWithSmoothDamp()
  {
    transform.position = Vector2.SmoothDamp(
      transform.position,
      moveDestination,
      ref moveSmoothDampCurrentVelocity,
      moveSmoothTime);
  }
```

Well, Lerp and SmoothDamp win on ease of implementation but they sure are boring. Unity's having all the fun. 

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2019.06.42%202.gif)

They all look good. If I want the cards to have a little bounce at the end of the movement, I need to stick with Force. Hard to see a big different between Lerp and SmoothDamp for this small move.

### Dealing the Hand

I changed the HandController to spawn the cards in from a central location and then use the card's `StartMovement` to send them each off to their initial location in the fan. This makes the Force movement more pleasing to me. The bounce is nice, just can't go overboard with it.

``` cs
  void DealHand(int numCards)
  {
    float fanTotalAngle = fanAngleIncrement * (numCards - 1);
    float fanInitialAngle = -fanTotalAngle / 2;

    for (int i = 0; i < numCards; i++)
    {
      float angle = fanInitialAngle + (fanAngleIncrement * i);
      int depth = i - numCards;

      GameObject card = Instantiate(cardGameObject,
        cardSpawnLocation, CardRotationInFan(angle));

      Card cardScript = card.GetComponent<Card>();
      cardScript.positionInFan = CardPositionInFan(angle);
      cardScript.rotationInFan = CardRotationInFan(angle);
      cardScript.depthInFan = depth;
      cardScript.StartMovement(CardPositionInFan(angle));

      cardsInHand.Add(card);
    }
  }
```

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2019.45.02.gif)

I'm seeing a bug now where the mouse is still over the raised card but a card underneath still gets an OnMouseEnter trigger. I'm worried this throws a wrench in the plan to ignore z and use sortOrdering for depth.

Anyway, that's gonna have to be it for now.

![](Spork%20DevLog%202023-08-05/CleanShot%202023-08-05%20at%2019.51.57.gif)









