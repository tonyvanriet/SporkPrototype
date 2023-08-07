using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Card : MonoBehaviour
{
  public Vector2 positionInFan;
  public Quaternion rotationInFan;
  public int depthInFan;
  public int focusDepth = 1;
  public int holdDepth = 2;

  public float moveForceScalingFactor;
  public float moveLerpTime;
  public float moveSmoothTime;
  public enum MoveMethod { Force, Lerp, SmoothDamp }
  public MoveMethod moveUsing;
  public float landingDistance;

  public enum HoldPosition { Initial, Center }
  public HoldPosition holdPosition = HoldPosition.Initial;

  float cameraMinY;
  Vector3 cardSize;
  Collider2D cardCollider;
  Rigidbody2D cardRigidBody;
  SpriteRenderer cardSpriteRenderer;

  // movement
  bool isMoving = false;
  Vector2 moveOrigin;
  Vector2 moveDestination;
  int moveDepth;
  float distanceToDestination;
  Vector2 moveSmoothDampCurrentVelocity = Vector2.zero;

  // holding
  bool isHolding = false;
  Vector2 holdOffset;

  // tell the card to start a movement towards the destination
  // so far the only thing telling the card to move is itself
  // but I'm expecting that the change
  public void StartMovement(Vector2 destination, int depth)
  {
    isMoving = true;
    moveOrigin = transform.localPosition;
    moveDestination = destination;
    moveDepth = depth;
  }

  // stop moving and explicitly set the position to the destination
  public void CompleteMovement()
  {
    isMoving = false;
    transform.localPosition = moveDestination;
    SetDepth(moveDepth);
  }

  void Start()
  {
    Camera mainCamera = Camera.main;
    float cameraHeight = 2f * mainCamera.orthographicSize;
    cameraMinY = mainCamera.transform.localPosition.y - cameraHeight / 2f;
    // TODO get camera info from a shared resource
    // rather than computing it in every Card

    cardCollider = GetComponent<Collider2D>();
    cardRigidBody = GetComponent<Rigidbody2D>();
    cardSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

    cardSize = transform.localScale;
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

    if (isMoving)
    {
      distanceToDestination =
        Vector2.Distance(moveDestination, transform.localPosition);
      SetDepth(moveDepth);

      switch (moveUsing)
      {
        case MoveMethod.Force: MoveWithForce(); break;
        case MoveMethod.Lerp: MoveWithLerp(); break;
        case MoveMethod.SmoothDamp: MoveWithSmoothDamp(); break;
      }

      // keep moving until we're slowly approaching the dest
      bool landing = (distanceToDestination < landingDistance
        && cardRigidBody.velocity.magnitude < landingDistance);
      if (landing) CompleteMovement();
    }
  }

  void OnMouseEnter()
  {
    // rotate to level
    // TODO make rotation part of card movement
    this.transform.localRotation = Quaternion.identity;
    // start a movement above the bottom of the view
    // and bring to front
    float moveDestinationX = transform.localPosition.x;
    float moveDestinationY = cameraMinY + cardSize.y / 2f;
    StartMovement(
      new Vector2(moveDestinationX, moveDestinationY), focusDepth);
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    StartMovement(positionInFan, depthInFan);
    this.transform.localRotation = rotationInFan;
  }

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

  void MoveWithForce()
  {
    // push the card towards the destination
    // with a force that's proportional to the distance
    Vector2 currentPosition = this.transform.localPosition;
    Vector2 direction = (moveDestination - currentPosition).normalized;
    float forceMagnitude =
      distanceToDestination * moveForceScalingFactor * Time.deltaTime;
    Vector2 force = direction * forceMagnitude;
    cardRigidBody.AddForce(force);
    SetDepth(moveDepth);
  }

  void MoveWithLerp()
  {
    transform.position = Vector2.Lerp(
      transform.position,
      moveDestination,
      moveLerpTime);
    SetDepth(moveDepth);
  }

  void MoveWithSmoothDamp()
  {
    transform.position = Vector2.SmoothDamp(
      transform.position,
      moveDestination,
      ref moveSmoothDampCurrentVelocity,
      moveSmoothTime);
    SetDepth(moveDepth);
  }

  // set sortingOrder and transform.z to the given depth
  // larger value is closer to the camera
  void SetDepth(int depth)
  {
    cardSpriteRenderer.sortingOrder = depth;
    Vector3 currentPosition = transform.localPosition;
    currentPosition.z = (float)depth / -10;
    transform.localPosition = currentPosition;
  }
}
