using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Card : MonoBehaviour
{
  public Vector3 positionInFan;
  public Quaternion rotationInFan;
  public float moveForceMagnitude = 10000f;
  public bool isMoving = false;
  public Vector2 moveToPosition;

  float cameraMinY;
  Vector3 cardSize;
  Collider2D cardCollider;
  Rigidbody2D cardRigidBody;
  SpriteRenderer cardSpriteRenderer;

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
