using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyTeamSO : ScriptableObject {
    public List<UnitData> Enemies = new();
}
