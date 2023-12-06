using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private TextMeshProUGUI unitDescription;
    [SerializeField] private TextMeshProUGUI unitAttackValue;
    [SerializeField] private TextMeshProUGUI unitDefenceValue;
    [SerializeField] private Image background;

    private UnitData unitData;

    public void SetCardData(UnitData data) {
        Debug.Log(data);

        unitData = data;

        unitName.text = data ? data.name : "";
        unitDescription.text = data ? data.Description : "";

        unitAttackValue.text = data ? data.BaseStatBlock.Attack.ToString() : "";
        unitDefenceValue.text = data ? data.BaseStatBlock.Defence.ToString() : "";

        background.sprite = data ? data.Icon : null;
    }
}
