using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Card : MonoBehaviour
{
  public Vector3 positionInFan;
  public Quaternion rotationInFan;
  public float moveForceMagnitude;

  float cameraMinY;
  Vector3 cardSize;
  Collider2D cardCollider;
  Rigidbody2D cardRigidBody;
  SpriteRenderer cardSpriteRenderer;

  // movement
  bool isMoving = false;
  Vector2 moveOrigin;
  Vector2 moveDestination;

  // tell the card to start a movement towards the destination
  // so far the only thing telling the card to move is itself
  // but I'm expecting that the change
  public void StartMovement(Vector2 destination)
  {
    isMoving = true;
    moveOrigin = transform.localPosition;
    moveDestination = destination;
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
  }

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
      Vector2 force = direction * forcePolarity * moveForceMagnitude * Time.deltaTime;
      cardRigidBody.AddForce(force);

      // keep moving until we're close to the destination
      isMoving = (distanceToDestination > 0.1);
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
    cardSpriteRenderer.sortingOrder = 0;
  }
}
