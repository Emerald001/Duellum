using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleData {
    public Vector2Int PlayerPos;
    public Vector2Int EnemyPos;

    public List<UnitData> PlayerUnits;
    public List<UnitData> EnemyUnits;

    public BattleData(Vector2Int playerPos, Vector2Int enemyPos, List<UnitData> playerUnits, List<UnitData> enemyUnits) {
        PlayerPos = playerPos;
        EnemyPos = enemyPos;
        PlayerUnits = playerUnits;
        EnemyUnits = enemyUnits;
    }
}