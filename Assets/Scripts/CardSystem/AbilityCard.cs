using UnityEngine;

[CreateAssetMenu(menuName = "Abilty Card", fileName = "Card")]
public class AbilityCard : ScriptableObject
{
    [Header("Visuals")]
    public string Name;
    public string Discription;

    public Sprite Icon;
    public Sprite Background;
    public Sprite Border;

    [Header("Card Abilities")]
    public int ManaCost;

    public Selector selector;
}
