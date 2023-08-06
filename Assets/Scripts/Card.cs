using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Card : MonoBehaviour
{
  public Vector2 positionInFan;
  public Quaternion rotationInFan;
  public int depthInFan;

  public float moveForceScalingFactor;
  public float moveLerpTime = 0.3f;
  public float moveSmoothTime = 0.3f;
  public enum MoveMethod { Force, Lerp, SmoothDamp }
  public MoveMethod moveUsing = MoveMethod.Lerp;

  float cameraMinY;
  Vector3 cardSize;
  Collider2D cardCollider;
  Rigidbody2D cardRigidBody;
  SpriteRenderer cardSpriteRenderer;

  // movement
  bool isMoving = false;
  Vector2 moveOrigin;
  Vector2 moveDestination;
  Vector2 moveSmoothDampCurrentVelocity = Vector2.zero;

  // tell the card to start a movement towards the destination
  // so far the only thing telling the card to move is itself
  // but I'm expecting that the change
  public void StartMovement(Vector2 destination)
  {
    isMoving = true;
    moveOrigin = transform.localPosition;
    moveDestination = destination;
  }

  // stop moving and explicitly set the position to the destination
  public void CompleteMovement()
  {
    isMoving = false;
    transform.localPosition = moveDestination;
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

    cardSpriteRenderer.sortingOrder = depthInFan;
  }

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

  void OnMouseEnter()
  {
    // rotate to level
    this.transform.localRotation = Quaternion.identity;
    // bring the card to the front
    cardSpriteRenderer.sortingOrder = 1;
    // move above the bottom of the view
    cardSize = cardCollider.bounds.size; // TODO fix for rotated cards
    float moveDestinationX = transform.localPosition.x;
    float moveDestinationY = cameraMinY + cardSize.y / 2f;
    StartMovement(new Vector2(moveDestinationX, moveDestinationY));
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    StartMovement(positionInFan);
    this.transform.localRotation = rotationInFan;
    cardSpriteRenderer.sortingOrder = depthInFan;
  }

  void MoveWithForce()
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
}
