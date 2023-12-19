using System.Collections.Generic;
using UnityEngine;

public static class AbilityManager {
    public static void PerformAbility(AbilityCard card, int id, params Vector2Int[] positions) {
        List<UnitController> controllerList = new();
        foreach (Vector2Int position in positions) {
            if (UnitStaticManager.TryGetUnitFromGridPos(position, out var unit))
                controllerList.Add(unit);
        }

        switch (card.abilityType) {
            case AbilityCardType.ApplyEffect:
                foreach (var unit in controllerList)
                    unit.AddEffect(card.effectToApply);
                break;

            case AbilityCardType.PlaceBoulder:
                GridStaticFunctions.ReplaceTile(card.hexPrefab, positions);
                break;

            case AbilityCardType.Revive:
                foreach (var unit in controllerList)
                    unit.Values.RemoveEffect(card.effectToApply.type);
                break;

            case AbilityCardType.SkipOpponentsTurn:
                // Select All Enemy units
                break;

            case AbilityCardType.MoveUnit:
                foreach (var unit in controllerList) {
                    List<Vector2Int> openPositions = GridStaticFunctions.GetAllOpenGridPositions();
                    unit.ChangeUnitPosition(openPositions[Random.Range(0, openPositions.Count)]);
                }
                break;
                
            case AbilityCardType.SpinUnit:
                foreach (var unit in controllerList)
                    unit.ChangeUnitRotation(-unit.LookDirection);
                break;

            case AbilityCardType.Summon:
                foreach (Vector2Int position in positions) {
                    //var spawnedUnit = GameObject.Instantiate()
                    //BattleManager.Instance.CurrentPlayer.Units.Add();
                }
                break;

            case AbilityCardType.ApplyTileEffect:
                foreach (var item in positions)
                    GridStaticFunctions.TileEffectPositions.Add(item, card.tileEffect);
                break;

            case AbilityCardType.SmokeBomb:
                var pos = positions[0];
                var enemyUnit = controllerList[0];
                var backPos = pos - enemyUnit.LookDirection;
                var newLookDir = backPos - pos;

                UnitController attackingUnit = null;
                GridStaticFunctions.RippleThroughFullGridPositions(pos, 2, (pos, i) => {
                    if (UnitStaticManager.TryGetUnitFromGridPos(pos, out var unit)) {
                        attackingUnit ??= unit;
                        if (unit.Values.currentStats.Attack > attackingUnit.Values.currentStats.Attack)
                            attackingUnit = unit;
                    }
                });

                attackingUnit.ChangeUnitPosition(backPos);
                attackingUnit.ChangeUnitRotation(newLookDir);
                break;

            case AbilityCardType.Grapple:
                foreach (var item in controllerList)
                    item.AddEffect(card.effectToApply);
                break;

            case AbilityCardType.Charm:
                break;

            default:
                throw new System.NotImplementedException($"{card.abilityType} Not Yet Implemented");
        }
    }
}
