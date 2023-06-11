using UnityEngine;

[CreateAssetMenu(menuName = "Abilty Card", fileName = "Card")]
public class AbilityCard : ScriptableObject
{
    [Header("Visuals")]
    public string Name;
    public Sprite Icon;


    public string Discription;

    [Header("Card Abilities")]
    public int ManaCost;

    public Selector selector;
}
