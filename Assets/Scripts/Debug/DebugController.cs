using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DebugController : MonoBehaviour {

    private bool showConsole = false;
    private string input;

    public static DebugController Instance { get; private set; }

    public enum CommandType {
        ShakeCamera,
        ReviveAll,
        KillEnemies,
        KillHeroes,
        GiveCard,
        Restart
    }

    private Dictionary<string, CommandType> commandDictionary = new() {
        {"shakecam", CommandType.ShakeCamera },
        {"reviveall", CommandType.ReviveAll},
        {"killenemies", CommandType.KillEnemies },
        {"killheroes", CommandType.KillHeroes },
        {"givecard", CommandType.GiveCard},
        {"restart", CommandType.Restart},
    };

    public void OnToggleDebug() {
        showConsole = !showConsole;
    }

    private void Awake() {
        input = "";
        Instance = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F12))
            OnToggleDebug();
    }

    private void OnGUI() {
        if (!showConsole)
            return;

        Event e = Event.current;
        if (e.keyCode == KeyCode.Return && showConsole) {
            HandleInput();
            input = "";
        }

        float y = 0f;
        GUI.SetNextControlName("console");
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 50, 50), input);
        GUI.Box(new Rect(0, y, Screen.width, 60), "");
        GUI.FocusControl("console");
    }

    private void HandleInput() {
        string[] inputParts = input.Split(' ');

        if (inputParts.Length > 0) {
            string command = inputParts[0].ToLower();

            if (commandDictionary.ContainsKey(command)) {
                CommandType type = commandDictionary[command];
                showConsole = false;

                switch (type) {
                    case CommandType.ShakeCamera:
                        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, 1);
                    break;

                    case CommandType.ReviveAll:
                        foreach (UnitController unit in UnitStaticManager.DeadUnitsInPlay)
                            EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitRevive, unit);

                        for (int i = UnitStaticManager.DeadUnitsInPlay.Count - 1; i >= 0; i--)
                            UnitStaticManager.ReviveUnit(UnitStaticManager.DeadUnitsInPlay[i], 0);
                    break;
                    case CommandType.Restart:
                        SceneManager.LoadScene(0);
                        break;
                    case CommandType.KillEnemies:
                        foreach(UnitController unit in UnitStaticManager.GetEnemies(0)) {
                            UnitStaticManager.UnitDeath(unit);
                        }
                        break;
                    case CommandType.KillHeroes:
                        foreach(UnitController unit in UnitStaticManager.GetEnemies(1)) {
                            UnitStaticManager.UnitDeath(unit);
                        }
                        break;
                    case CommandType.GiveCard:
                        EventManager<UIEvents, int>.Invoke(UIEvents.GiveCard, 0);
                        break;

                    default:
                        Debug.Log("Unknown command: " + command);
                        break;
                }
            }
            else
                Debug.Log("Unknown command: " + command);
        }
    }
}
