using System.Collections.Generic;
using UnityEngine;

public static class AbilityManager {
    public static void PerformAbility(AbilityCard card, int id, List<List<Vector2Int>> allFoundTiles) {
        List<UnitController> controllerList = new();
        foreach (Vector2Int position in allFoundTiles[0]) {
            if (UnitStaticManager.TryGetUnitFromGridPos(position, out var unit))
                controllerList.Add(unit);
        }

        switch (card.abilityType) {
            case AbilityCardType.ApplyEffect:
                foreach (var unit in controllerList)
                    unit.AddEffect(card.effectToApply);
                break;

            case AbilityCardType.PlaceBoulder:
                GridStaticFunctions.ReplaceTile(card.hexPrefab, allFoundTiles[0].ToArray());
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
                foreach (Vector2Int position in allFoundTiles[0]) {
                    //var spawnedUnit = GameObject.Instantiate()
                    //BattleManager.Instance.CurrentPlayer.Units.Add();
                }
                break;

            case AbilityCardType.ApplyTileEffect:
                foreach (var item in allFoundTiles[0])
                    GridStaticFunctions.TileEffectPositions.Add(item, card.tileEffect);
                break;

            case AbilityCardType.SmokeBomb:
                var enemyPos = allFoundTiles[0][0];
                var enemyUnit = controllerList[0];
                var backPos = enemyPos - enemyUnit.LookDirection;
                var newLookDir = backPos - enemyPos;

                UnitController attackingUnit = null;
                GridStaticFunctions.RippleThroughFullGridPositions(enemyPos, 2, (pos, i) => {
                    if (UnitStaticManager.TryGetUnitFromGridPos(pos, out var unit)) {
                        if (pos == enemyPos)
                            return;

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
            case AbilityCardType.AreaOfEffectAttack:
                DamageManager.DealDamage(card.Damage, controllerList.ToArray());
                break;

            default:
                throw new System.NotImplementedException($"{card.abilityType} Not Yet Implemented");
        }
    }
}
