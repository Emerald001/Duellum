using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CardHand : MonoBehaviour
{
    [SerializeField] private AbilityCard TMPCardToAdd;
    [SerializeField] private CardAssetHolder CardPrefab;

    [SerializeField] private Transform StackPos;

    [SerializeField] private float cardMoveSpeed;
    [SerializeField] private float cardRotationSpeed;

    [SerializeField] private float spacing;
    [SerializeField] private float maxWidth;
    [SerializeField] private float radius;

    private readonly List<CardAssetHolder> cards = new();
    private CardAssetHolder nextCard;

    private void Start() {
        nextCard = Instantiate(CardPrefab, StackPos.position, StackPos.rotation);
        nextCard.transform.parent = StackPos;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.V))
            AddCard(TMPCardToAdd);
    }

    public void AddCard(AbilityCard card) {
        CardAssetHolder cardObject = Instantiate(CardPrefab, StackPos.position, StackPos.rotation);

        //cardObject.Name.text = card.Name;
        //cardObject.Discription.text = card.Discription;
        //cardObject.Icon.sprite = card.Icon;
        //cardObject.Background.sprite = card.Background;
        //cardObject.ManaCost.text = card.ManaCost.ToString();

        nextCard.transform.parent = transform;
        cards.Add(nextCard);
        LineOutCards();

        nextCard = cardObject;
    }

    public void RemoveCard() {

    }

    private void LineOutCards() {
        int numObjects = cards.Count;
        float arcAngle = Mathf.Min((numObjects - 1) * spacing, maxWidth);
        float angleIncrement = arcAngle == 0 ? 0 : arcAngle / (numObjects - 1);
        float startAngle = -arcAngle / 2f;

        for (int i = 0; i < cards.Count; i++) {
            CardAssetHolder card = cards[i];

            float angle = startAngle + i * angleIncrement;
            float radianAngle = Mathf.Deg2Rad * angle;
            float x = radius * Mathf.Sin(radianAngle);
            float y = radius * Mathf.Cos(radianAngle);

            Vector3 position = transform.position + new Vector3(x, y, 0f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position - transform.position);

            card.ClearQueue();
            card.SetActionQueue(new List<Action>() {
                new ActionStack(
                    new MoveObjectAction(card.gameObject, cardMoveSpeed, position + new Vector3(0, -radius, 0)),
                    new RotateAction(card.gameObject, rotation.eulerAngles, cardRotationSpeed, .01f)
                )
            });
        }
    }
}
