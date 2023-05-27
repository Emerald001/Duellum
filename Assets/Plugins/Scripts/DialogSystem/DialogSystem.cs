using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour {
    private static readonly Dictionary<string, bool> conditions = new();

    [Header("References")]
    [SerializeField] private GameObject dialogueSystemObject;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portrait;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject buttonPrefab;

    [Header("CommandSettings")]
    [SerializeField] private char commandChar;
    [SerializeField] private char optionChar;
    [SerializeField] private char sectionChar;
    [SerializeField] private char autoNextChar;

    [Header("Visual Settings")]
    [SerializeField] private float timeBetweenChars;

    [Header("Conditions")]
    [SerializeField] private List<string> conditionNames = new();

    public float CurrentTimeBetweenChars { get; set; }

    private readonly Dictionary<string, string[]> Files = new();
    private readonly DialogFunctionality funcs = new();

    private string[] currentDialog;
    private int index;
    private bool IsWriting;

    private void Awake() {
        foreach (var item in conditionNames)
            conditions.Add(item, false);

        funcs.Owner = this;
        funcs.SetEvents();

        CurrentTimeBetweenChars = timeBetweenChars;

        var tmp = Resources.LoadAll<TextAsset>("Files/");

        foreach (var item in tmp)
            Files.Add(item.name, item.ToString().Replace("\n\r\n", "\n").Split("\n"));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) 
            SetDialog("Test 1");

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
            NextLine();
    }

    public static void SetCondition(string settingName) {
        if (conditions.ContainsKey(settingName)) 
            conditions[settingName] = true;
    }

    private void SetDialog(string DialogName) {
        if (Files.ContainsKey(DialogName)) {
            index = 0;
            currentDialog = Files[DialogName];
            dialogueSystemObject.SetActive(true);
            NextLine();
            return;
        }

        Debug.LogError("No File named " + DialogName + " found!");
    }

    private void StopDialog() {
        StopAllCoroutines();
        RemoveOptions();
        
        mainText.text = "";
        nameText.text = "";
        
        currentDialog = null;
        IsWriting = false;
        
        index = 0;

        dialogueSystemObject.SetActive(false);
    }

    private void NextLine() {
        if (currentDialog == null)
            return;

        if (IsWriting) {
            FullLine(currentDialog[index - 1].Trim());
            return;
        }

        if (index >= currentDialog.Length) {
            StopDialog();
            return;
        }

        CurrentTimeBetweenChars = timeBetweenChars;

        if (TryCheckCommand(currentDialog[index], commandChar, out string[] product)) {
            CallCommand(product);
            index++;
            NextLine();
            return;
        }

        if (TryCheckCommand(currentDialog[index], optionChar, out string[] optionProduct)) {
            DisplayOptions(currentDialog);
            return;
        }

        if (TryCheckCommand(currentDialog[index], sectionChar, out string[] sectionProduct)) {
            switch (sectionProduct[1].Trim()) {
                case "jump":
                    JumpToSection(ConditionChecker(sectionProduct));
                    return;
                case "stop":
                    StopDialog();
                    return;
                
                case "end":
                case "wait":
                    return;
                
                default:
                    index++;
                    NextLine();
                    return;
            }
        }

        DisplayLine();
        index++;
    }

    private void DisplayLine() {
        StopAllCoroutines();
        StartCoroutine(DisplayText(currentDialog[index].Trim()));
    }

    #region Commands
    private bool TryCheckCommand(string line, char commandChar, out string[] product) {
        product = null;
        char[] tmp = line.Trim().ToCharArray();

        if (tmp.Length < 1)
            return false;

        if (tmp[0] == commandChar) {
            product = line.Split(" ");
            return true;
        }
        
        return false;
    }

    private void CallCommand(string[] command) {
        if (command.Length < 3)
            EventManager<DialogEvents>.Invoke(ParseEnum<DialogEvents>(command[1].ToUpper()));
        else if(float.TryParse(command[2], out var floatParse))
            EventManager<DialogEvents, float>.Invoke(ParseEnum<DialogEvents>(command[1]), floatParse);
        else if (bool.TryParse(command[2], out var boolParse))
            EventManager<DialogEvents, bool>.Invoke(ParseEnum<DialogEvents>(command[1]), boolParse);
        else 
            EventManager<DialogEvents, string>.Invoke(ParseEnum<DialogEvents>(command[1]), command[2]);
    }

    private T ParseEnum<T>(string value) {
        return (T)Enum.Parse(typeof(T), value, true);
    }
    #endregion

    #region Options
    private void DisplayOptions(string[] file) {
        buttonPanel.SetActive(true);

        List<GameObject> buttons = new();

        while (TryCheckCommand(file[index], optionChar, out string[] optionProduct)) {
            var tmpButton = Instantiate(buttonPrefab, buttonContainer.transform);
            tmpButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = file[index].Trim().Split(" ", 2)[1];
            tmpButton.GetComponent<Button>().onClick.AddListener(RemoveOptions);

            index++;

            if (TryCheckCommand(file[index], sectionChar, out string[] product)) {
                if (product[1] == "jump") {
                    string sectionName = ConditionChecker(product);
                    if (!string.IsNullOrEmpty(sectionName))
                        tmpButton.GetComponent<Button>().onClick.AddListener(() => JumpToSection(sectionName));
                }
                else if (product[1].Trim() == "stop") 
                    tmpButton.GetComponent<Button>().onClick.AddListener(StopDialog);
            }

            index++;
            buttons.Add(tmpButton);
        }

        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        for (int i = 0; i < buttons.Count; i++) {
            var rect = buttons[i].GetComponent<RectTransform>();
            float yOffset = (buttons.Count % 2 != 0) ?
                containerRect.sizeDelta.y / buttons.Count * (buttons.Count - 1 - i - buttons.Count / 2) :
                containerRect.sizeDelta.y / buttons.Count * (buttons.Count - 1 - i - buttons.Count / 2) + containerRect.sizeDelta.y / 2 / buttons.Count;

            rect.localPosition = new Vector2(0, yOffset);
            rect.sizeDelta = new Vector2(containerRect.sizeDelta.x, (containerRect.sizeDelta.y / buttons.Count) - 1);
        }
    }

    private string ConditionChecker(string[] product) {
        string sectionName = product[2];

        if (product[2] == "if") {
            if (conditions.ContainsKey(product[3]))
                sectionName = conditions[product[3]] ? product[4] : product[6];
            else {
                Debug.LogError("Condition: " + product[3] + " does not Exist. " +
                    "Make sure the condition is typed exactly the same as in the inspector, " +
                    "or add it in the inspector.");
                sectionName = product[6];
            }
        }

        return sectionName;
    }

    private void RemoveOptions() {
        buttonPanel.SetActive(false);

        for (int i = buttonContainer.transform.childCount - 1; i >= 0; i--) 
            Destroy(buttonContainer.transform.GetChild(i).gameObject);
    }
    #endregion

    private void JumpToSection(string sectionName) {
        for (int i = 0; i < currentDialog.Length; i++) {
            if (TryCheckCommand(currentDialog[i], sectionChar, out string[] command))
                if (command[1] == "start" && command[2] == sectionName) {
                    index = i;
                    NextLine();
                    return;
                }
        }
    }

    private IEnumerator DisplayText(string text) {
        IsWriting = true;

        yield return new WaitForEndOfFrame();

        List<char> charList = new();

        string sentence = SetupLine(text);
        for (int i = 0; i < sentence.Length; i++) {
            if (sentence[i] == commandChar) 
                HandleInlineCommand(sentence, ref i);

            if (sentence[i] == '<') {
                List<char> stylePartOne = ReadUntilChar(sentence, ref i, '>', true);
                List<char> textBetween = ReadUntilChar(sentence, ref i, '<');
                List<char> stylePartTwo = ReadUntilChar(sentence, ref i, '>', true);

                List<char> tmp = new();
                for (int j = 0; j < textBetween.Count; j++) {
                    tmp.Add(textBetween[j]);

                    var Final = new List<char>(charList);
                    Final.AddRange(stylePartOne);
                    Final.AddRange(tmp);
                    Final.AddRange(stylePartTwo);

                    mainText.text = new string(Final.ToArray());
                    //Do Typewriter Noise

                    yield return new WaitForSeconds(CurrentTimeBetweenChars);
                }

                charList.AddRange(stylePartOne);
                charList.AddRange(textBetween);
                charList.AddRange(stylePartTwo);
            }

            if (i >= sentence.Length)
                continue;

            charList.Add(sentence[i]);
            mainText.text = new string(charList.ToArray());
            //Do Typewriter Noise

            yield return new WaitForSeconds(CurrentTimeBetweenChars);
        }

        CheckForAutoSkip();
        IsWriting = false;
    }

    private void FullLine(string text) {
        StopAllCoroutines();

        List<char> charList = new();

        string sentence = SetupLine(text);
        for (int i = 0; i < sentence.Length; i++) {
            if (sentence[i] == commandChar)
                HandleInlineCommand(sentence, ref i);
            
            charList.Add(sentence[i]);
        }

        mainText.text = new string(charList.ToArray());

        CheckForAutoSkip();
        IsWriting = false;
    }

    #region DisplayMethods
    private void HandleInlineCommand(string sentence, ref int index) {
        List<char> command = new() {
            sentence[index]
        };
        index++;
        command.AddRange(ReadUntilChar(sentence, ref index, commandChar, true));
        index++;

        CallCommand(new string(command.ToArray()).Split(" "));
    }

    private List<char> ReadUntilChar(string sentence, ref int index, char target, bool includeTarget = false) {
        List<char> result = new();

        while (index < sentence.Length && sentence[index] != target) {
            result.Add(sentence[index]);
            index++;
        }
        if (includeTarget) {
            result.Add(sentence[index]);
            index++;
        }

        return result;
    }

    private string SetupLine(string text) {
        string[] frontAndBack = text.Split(" ", 2);
        string name = frontAndBack[0];
        nameText.text = name;

        return frontAndBack[1];
    }

    private void CheckForAutoSkip() {
        if (TryCheckCommand(currentDialog[index], autoNextChar, out string[] autoSkip)) {
            index++;
            IsWriting = false;
            NextLine();
        }
    }
    #endregion
}