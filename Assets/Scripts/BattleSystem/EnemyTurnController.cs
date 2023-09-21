﻿using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : TurnController {
    public EnemyTurnController(List<UnitController> units) : base(units) { }

    public override void OnEnter() {
        base.OnEnter();

        int unit = Random.Range(0, units.Count);
        PickUnit(UnitStaticManager.UnitPositions[units[unit]]);
    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnUpdate() {
        base.OnUpdate();
    }
}
