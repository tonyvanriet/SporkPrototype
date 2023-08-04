using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
  public Vector3 positionInFan;
  public Quaternion rotationInFan;

  void OnMouseEnter()
  {
    // store the current position for later
    rotationInFan = this.transform.localRotation;
    // oh gawd, this is an OO language. I can't do this, right?

    // rotate to level
    this.transform.localRotation = Quaternion.identity;
    // and move up for full visibility
  }

  void OnMouseExit()
  {
    // get back in your fan!!
    this.transform.localRotation = rotationInFan;
  }
}
