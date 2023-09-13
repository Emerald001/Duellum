using System.Collections.Generic;

public class UnitAttackModule {
    public UnitAttack PickedAttack => pickedAttack;

    private readonly List<UnitAttack> attacks;
    private UnitAttack pickedAttack;

    public UnitAttackModule(List<UnitAttack> attacks) {
        this.attacks = attacks;

        EventManager<BattleEvents>.Subscribe(BattleEvents.NewTurn, LowerAbilityCooldowns);
    }

    private void LowerAbilityCooldowns() {
        foreach (var ability in attacks) {
            if (ability.CurrentCooldown > 0)
                ability.CurrentCooldown--;
        }
    }

    public virtual void SelectAttack(int index) {
        if (pickedAttack == attacks[index]) {
            pickedAttack.OnExit();
            pickedAttack = null;
        }
        else {
            if (pickedAttack != null) {
                pickedAttack.OnExit();
            }
            pickedAttack = attacks[index];

            //if (pickedAttack.targetAnything)
            //    pickedAttack.OnEnter(this, turnManager.LivingUnitsInPlay);
            //else if (pickedAttack.targetEnemy)
            //    pickedAttack.OnEnter(this, EnemyList);
            //else
            //    pickedAttack.OnEnter(this, OwnList);
        }
    }

    private void Reset() {
        pickedAttack?.OnExit();

        pickedAttack = null;
    }
}