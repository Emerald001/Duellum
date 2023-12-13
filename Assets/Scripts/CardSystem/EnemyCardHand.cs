using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCardHand : CardHand {
    [SerializeField] private Transform shownPosition;
    [SerializeField] private Transform hiddenPosition;

    protected override void OnEnable() {
        EventManager<UIEvents>.Subscribe(UIEvents.GiveEnemyCard, GiveCard);

        EventManager<BattleEvents>.Subscribe(BattleEvents.StartBattle, SetupForBattle);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, ResetAfterBattle);
    }
    protected override void OnDisable() {
        EventManager<UIEvents>.Unsubscribe(UIEvents.GiveEnemyCard, GiveCard);

        EventManager<BattleEvents>.Unsubscribe(BattleEvents.StartBattle, SetupForBattle);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, ResetAfterBattle);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B))
            GiveCard();
    }

    // TODO: Make this arc the other way and show the back of the cards!
    protected override void LineOutCards() {
        int numObjects = cards.Count;
        float arcAngle = Mathf.Max(-((numObjects - 1) * spacing), -maxWidth);
        float angleIncrement = arcAngle == 0 ? 0 : arcAngle / (numObjects - 1);
        float startAngle = -arcAngle / 2f;

        for (int i = 0; i < cards.Count; i++) {
            CardAssetHolder card = cards[i];

            float angle = startAngle + i * angleIncrement;
            float radianAngle = Mathf.Deg2Rad * angle;
            float x = radius * Mathf.Sin(radianAngle);
            float y = radius * Mathf.Cos(radianAngle);

            Vector3 position = transform.position + new Vector3(x, y, i * .01f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position - transform.position);

            int index = i;
            card.cardBehaviour.ClearQueue();
            card.cardBehaviour.SetActionQueue(new List<Action>() {
                new DoMethodAction(() => card.cardBehaviour.CanInvoke = false),
                new ActionStack(
                    new MoveObjectAction(card.gameObject, cardSpawnMoveSpeed, position + new Vector3(0, -radius, 0)),
                    new RotateAction(card.gameObject, rotation.eulerAngles, cardRotationSpeed, .01f)
                ),
                new DoMethodAction(() => card.cardBehaviour?.SetValues(position + new Vector3(0, -radius, 0) + new Vector3(0, raisedAmount, 0), uiCam, index))
            });
        }
    }

    private void SetupForBattle() {
        StartCoroutine(ShowCardhand());
    }

    private void ResetAfterBattle() {
        StartCoroutine(HideCardhand());
    }

    private IEnumerator ShowCardhand() {
        yield return new WaitForSeconds(2f);

        while (transform.position != shownPosition.position) {
            transform.position = Vector3.MoveTowards(transform.position, shownPosition.position, Time.deltaTime);

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator HideCardhand() {
        yield return new WaitForSeconds(2f);

        while (transform.position != hiddenPosition.position) {
            transform.position = Vector3.MoveTowards(transform.position, hiddenPosition.position, Time.deltaTime);

            yield return new WaitForEndOfFrame();
        }

        for (int i = cards.Count - 1; i >= 0; i--)
            RemoveCard(i);

        cards.Clear();
        cardStack.ResetDeck();
    }
}
