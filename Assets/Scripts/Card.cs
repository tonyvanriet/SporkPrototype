using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Card : MonoBehaviour
{

  Vector3 positionInFan;
  Quaternion rotationInFan;
  float cameraMinY;
  Vector3 cardSize;
  Collider2D cardCollider;

  void Start()
  {
    Camera mainCamera = Camera.main;
    float cameraHeight = 2f * mainCamera.orthographicSize;
    cameraMinY = mainCamera.transform.position.y - cameraHeight / 2f;
    // TODO get camera info from a shared resource
    // rather than computing it in every Card

    cardCollider = GetComponent<Collider2D>();
  }

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
      transform.position.z - 2);
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    this.transform.localPosition = positionInFan;
    this.transform.localRotation = rotationInFan;
  }
}
