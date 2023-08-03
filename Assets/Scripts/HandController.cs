using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject cardGameObject;
    public Vector3 fanCenter = new(0f, -12f, 0f);
    public float fanRadiusMultiplier = 3f;
    public float fanAngleIncrement = 10f;

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
            DealHand(Random.Range(1, 10));
        }

        // highlight the card on hover
    }

    void DealHand(int numCards)
    {
        float totalAngle = fanAngleIncrement * (numCards - 1);
        float initialAngle = -totalAngle / 2;

        for (int i = 0; i < numCards; i++)
        {
            float angle = initialAngle + (fanAngleIncrement * i);
            float depth = (float)(numCards - i) / 10;
            GameObject card = Instantiate(cardGameObject, CardPositionInFan(angle, depth), CardRotationInFan(angle));
            cardsInHand.Add(card);
        }
    }

    Vector3 CardPositionInFan(float angleInDegrees, float depth)
    {
        float angle = angleInDegrees * Mathf.Deg2Rad;
        Vector3 cardOffset = new(fanRadius * Mathf.Sin(angle), fanRadius * Mathf.Cos(angle), depth);
        return fanCenter + cardOffset;
    }

    Quaternion CardRotationInFan(float angleInDegress)
    {
        return Quaternion.Euler(0f, 0f, -angleInDegress);
    }
}
