using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

public class HandController : MonoBehaviour
{
  public GameObject cardGameObject;
  public Vector2 cardSpawnLocation = new Vector2(0f, 8f);
  public Vector2 fanCenter;
  public float fanRadiusMultiplier;
  public float fanAngleIncrement;

  float cardHeight;
  float cardWidth;
  float fanRadius;
  readonly List<GameObject> cardsInHand = new List<GameObject>();

  void Update()
  {
    // wasteful to compute these each update
    // but for now I want to see live updates as I tune the constants in the editor
    cardHeight = cardGameObject.transform.localScale.y;
    cardWidth = cardGameObject.transform.localScale.x;
    fanRadius = cardHeight * fanRadiusMultiplier;

    // might want to set a fixed pitch between the corners of the cards
    // then use that pitch, card width, and fan radius to determine the angle
    // for now, just use a fixed fan angle increment

    if (Input.GetKeyDown(KeyCode.Space))
    {
      cardsInHand.ForEach(card => Destroy(card));
      cardsInHand.Clear();
      DealHand(Random.Range(1, 11));
    }
  }

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

  Vector2 CardPositionInFan(float angleInDegrees)
  {
    float angle = angleInDegrees * Mathf.Deg2Rad;
    Vector2 cardOffset = new(
      fanRadius * Mathf.Sin(angle),
      fanRadius * Mathf.Cos(angle));
    return fanCenter + cardOffset;
  }

  Quaternion CardRotationInFan(float angleInDegress)
  {
    return Quaternion.Euler(0f, 0f, -angleInDegress);
  }
}
