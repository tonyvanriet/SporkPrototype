using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
  public HeroShip heroShip;
  public GameObject cardGameObject;
  public Vector2 cardSpawnLocation = new Vector2(0f, 8f);
  public Vector2 fanCenter;
  public float fanRadiusMultiplier;
  public float fanAngleIncrement;

  float cardHeight;
  float cardWidth;
  float fanRadius;
  readonly List<GameObject> cardsInHand = new List<GameObject>();

  public void PlayCard(GameObject card)
  {
    // get rid of the card
    cardsInHand.Remove(card);
    Destroy(card, 0.1f);

    // reposition the hand
    FanCards();
  }

  void Start()
  {
    cardHeight = cardGameObject.transform.localScale.y;
    cardWidth = cardGameObject.transform.localScale.x;
    fanRadius = cardHeight * fanRadiusMultiplier;
    // might want to set a fixed pitch between the corners of the cards
    // then use that pitch, card width, and fan radius to determine the angle
    // for now, just use a fixed fan angle increment

    DealHand(7);
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      cardsInHand.ForEach(card => Destroy(card));
      cardsInHand.Clear();
      heroShip.ReceiveAttack(12);
      DealHand(Random.Range(1, 11));
    }
  }

  void DealHand(int numCards)
  {
    float fanTotalAngle = fanAngleIncrement * (numCards - 1);
    float fanInitialAngle = -fanTotalAngle / 2;

    for (int i = 0; i < numCards; i++)
    {
      GameObject card = Instantiate(cardGameObject,
        cardSpawnLocation, Quaternion.identity);
      cardsInHand.Add(card);
      card.GetComponent<Card>().handControllerScript = this;
    }
    FanCards();
  }

  void FanCards()
  {
    int numCards = cardsInHand.Count;

    float fanTotalAngle = fanAngleIncrement * (numCards - 1);
    float fanInitialAngle = -fanTotalAngle / 2;

    for (int i = 0; i < numCards; i++)
    {
      float angle = fanInitialAngle + (fanAngleIncrement * i);
      int depth = i - numCards;

      Card cardScript = cardsInHand[i].GetComponent<Card>();
      cardScript.positionInFan = CardPositionInFan(angle);
      cardScript.rotationInFan = CardRotationInFan(angle);
      cardScript.depthInFan = depth;
      cardScript.StartMovement(CardPositionInFan(angle), depth);
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
