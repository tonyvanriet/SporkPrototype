# Spork DevLog 2023-08-03
#spork #devlog

## VS Code Please
Got VSCode Intellisense and debugging setup thanks to this [video](https://www.youtube.com/watch?v=3GVGyooZ8jk). No more bloated Visual Studio for Mac.

## Tuning the Fan
I feel like messing with the fan of cards so they're mostly visible, but still a pleasing arc. Certainly silly to spend time on this now, but this is the impulse I have.

All of these measurements depend on the aspect ratio of the cards being 2:3. Sad if that changes. Probably will.

Pretty good start
![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.02.20@2x.png)<!-- {"width":739} -->![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2021.59.20@2x.png)<!-- {"width":370} -->

let's see if we can waste less space in the bottom middle.
![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.04.08@2x.png)<!-- {"width":739} -->![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.04.20@2x.png)<!-- {"width":341} -->

yeah, I like this well enough. let's muck with card hover now

actually let's see what no fan looks like.
![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.11.38@2x.png)<!-- {"width":738} -->![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.11.55@2x.png)<!-- {"width":396} -->

not a fan. too... square.
only fans

but it does make me wanna try a very slight fan.
![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.14.53@2x.png)<!-- {"width":739} -->![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.15.11@2x.png)<!-- {"width":412} -->

not bad. still want more arc

![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.17.36@2x.png)<!-- {"width":740} -->![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.17.50@2x.png)<!-- {"width":416} -->

that's the one
... for now

Ended up being a great example of structuring the code to allow quick dynamic feedback. I modified the HandController to spawn 10 cards every frame. Then I was able to click and drag from the labels in the Inspector and see the cards adjust in real time.
![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2022.44.21.gif)<!-- {"width":740} -->

Also, this made me feel like I found a great data model for the card positioning. I was able to try out all of these variations quickly with just these 3 variables.

## Flipping through the cards
It's important to be able to scan through the cards and clearly see the full face of the card you're focusing on. As you mouse over the fan, the hovered card rotates to level and moves up so you can see the entire face.

I think the difficulty could be in handling transitions between cards as you hover away from one. I bet it causes all kinds of problems if you magnify the hovered card. That would mean when you cross out of its boundary you're already most of the way past the adjacent card. Would be interesting to test but I'm gonna avoid magnification for now.

I want to play this on an iPad as well, but I'll focus on mouse and ignore touch for now. (foreshadowing)

each card needs to handle mouse enter and exit. let's focus on rotation first since it's the same for every card so it's easier.

``` cs
  Quaternion rotationInFan;

  void OnMouseEnter()
  {
    // store the current position for later
    rotationInFan = this.transform.localRotation;
    // oh gawd, this is an OO language. I can't do this, right?

    // rotate to level
    this.transform.localRotation = Quaternion.identity;
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    this.transform.localRotation = rotationInFan;
  }
```
holy shit that worked!!

![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-03%20at%2023.46.21.gif)<!-- {"width":740} -->

ok, I can just copy Quaternions and Vector3s because they're data structures and not reference types. cool cool cool.

now we need to move the card up and bring it to the front which means we need to reference the bottom of the view somehow. to ChatGPT!!!

I need a reference to the main camera to get these boundaries. I wanted to make `mainCamera` a public property of the `Card` and set it by dragging in the Main Camera from the scene into the Inspector. but that doesn't make sense because that camera is scene-specific and the Card is a scene-agnostic prefab. ok fine, I'll just do what ChatGPT says and try getting my reference from `Camera.main`.

alright, it looks like the bottom of the view is -5 Y
``` cs
  Camera mainCamera;
  float cameraMinY;

  void Start()
  {
    mainCamera = Camera.main;
    float cameraHeight = 2f * mainCamera.orthographicSize;
    cameraMinY = mainCamera.transform.position.y - cameraHeight / 2f;
    Debug.Log(cameraMinY);
    // TODO get camera info from a shared resource
    // rather than computing it in every Card
  }
```

so now I need to know the size of the card and set the position so the bottom is above that -5 Y. it was handy to get the view size from the camera's `orthographicSize`, but there's nothing like that on the card object's transform. you have to refer to the bounds of the object's Collider.
ChatGPT says
> The (collider's) bounds represent an axis-aligned bounding box that encapsulates the collider's shape.

seems like the kind of sentence that's gonna have to make more sense to me at some point in the future. anyway, let's find the card's new Y position.

the `collider.bounds.size` appears to be an absolute x,y size that's larger when the card is rotated, judging from the output when I deal 5 cards that are each 2x3.

![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-04%20at%2000.57.02@2x.png)

so if we're gonna use the collider size, we have to rotate it to level first. seems pretty fragile, but let's keep going.
```cs
  void OnMouseEnter()
  {
    // store the current position and rotation for later
    positionInFan = this.transform.localPosition;
    rotationInFan = this.transform.localRotation;

    // rotate to level
    this.transform.localRotation = Quaternion.identity;

    // and move above the bottom of the view
    // this .size has to be *after* the rotate. fragile
    cardSize = cardCollider.bounds.size;
    float halfCardHeight = cardSize.y / 2f;
    transform.position = new Vector3(
      transform.position.x,
      cameraMinY + halfCardHeight,
      transform.position.z);
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    this.transform.localPosition = positionInFan;
    this.transform.localRotation = rotationInFan;
  }
```

there we go

![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-04%20at%2001.22.11.gif)

...sorta. the vertical height isn't consistent, and higher above the bottom of the screen than I expected, but pretty good.
I was ready to go to bed, but now we're so close. just gotta bring the hovered card to the front and that's easy.

and turns out "easy" was an understatement.  just need to subtract a bit from the new z position.
``` cs
    transform.position = new Vector3(
      transform.position.x,
      cameraMinY + halfCardHeight,
      transform.position.z - 2);
```

![](Spork%20DevLog%202023-08-03/CleanShot%202023-08-04%20at%2001.28.22.gif)

the `- 2` is cheesy, but good enough for now. I know the cards are all within 1 z of each other because of how the depth is calculated
``` cs
      float depth = (float)(numCards - i) / 10;
```
and I'm assuming a max of 10 cards. I'll make that less fragile some day, I'm sure.
