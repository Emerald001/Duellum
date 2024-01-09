using System.Collections.Generic;
using UnityEngine;

public class CardHandStateMachine {
    public static System.Action OnDismiss;
    public static System.Action<Card> OnUse;

    private readonly List<List<Vector2Int>> tilesPerState = new();
    private List<Vector2Int> currentAvailableTiles = new();

    private List<CardState> statesLeft;
    private AbilityCard currentCard;
    private CardState currentState = null;
    private int stateIndex = 0;

    private readonly int ownerId;

    public CardHandStateMachine(int ownerId) {
        this.ownerId = ownerId;
    }

    public void OnUpdate() {

        if (currentState == null)
            return;


        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            PreviousState();
        }

        if (!currentAvailableTiles.Contains(MouseToWorldView.HoverTileGridPos))
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            stateIndex++;
            NextState();
        }
    }

    public void SetMachine(AbilityCard abilityCard) {
        currentCard = abilityCard;
        statesLeft = new(abilityCard.cardStates);
        NextState();
    }

    private void NextState() {
        if (currentState != null)
            tilesPerState.Add(GridStaticSelectors.GetPositions(currentState.mouseArea, MouseToWorldView.HoverTileGridPos, ownerId));

        if (stateIndex >= statesLeft.Count) {

            AbilityManager.PerformAbility(currentCard, ownerId, tilesPerState);
            OnUse.Invoke(currentCard);
            OnDismiss.Invoke();
            ResetMachine();
            return;
        }

        currentState = statesLeft[stateIndex];
        currentAvailableTiles = GridStaticSelectors.GetPositions(currentState.areaOfSelection, GridStaticFunctions.CONST_EMPTY, ownerId);

        if (currentAvailableTiles.Count < 1) {
            OnDismiss?.Invoke();
            ResetMachine();
            return;
        }

        GridStaticFunctions.ResetBattleTileColors();
        GridStaticFunctions.HighlightTiles(currentAvailableTiles, HighlightType.MovementHighlight);

        EventManager<CameraEventType, Selector>.Invoke(CameraEventType.CHANGE_CAM_SELECTOR, currentState.mouseArea);
    }

    private void PreviousState() {
        stateIndex--;

        if (stateIndex < 0) {
            OnDismiss?.Invoke();
            ResetMachine();
            return;
        }

        tilesPerState.RemoveAt(tilesPerState.Count - 1);
        NextState();
    }

    private void ResetMachine() {
        GridStaticFunctions.ResetBattleTileColors();
        EventManager<UIEvents>.Invoke(UIEvents.ReleasedAbilityCard);
        EventManager<CameraEventType, Selector>.Invoke(CameraEventType.CHANGE_CAM_SELECTOR, null);

        currentCard = null;
        currentState = null;
        stateIndex = 0;
        statesLeft.Clear();
        tilesPerState.Clear();
    }
}
