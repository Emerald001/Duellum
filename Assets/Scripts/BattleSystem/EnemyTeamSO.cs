using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Enemy Team", menuName = "Battle/EnemyTeam")]
public class EnemyTeamSO : ScriptableObject {
    public List<UnitData> Enemies = new();
}
