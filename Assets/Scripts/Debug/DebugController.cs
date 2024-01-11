using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugController : MonoBehaviour {

    private bool showConsole = false;
    private string input;

    public static DebugController Instance { get; private set; }

    public enum CommandType {
        ShakeCamera,
        ReviveAll,
        KillAll,
        KillEnemies,
        KillHeroes,
        GiveCard
    }

    private Dictionary<string, CommandType> commandDictionary = new() {
        {"shakecam", CommandType.ShakeCamera },
        {"reviveall", CommandType.ReviveAll},
        {"killall", CommandType.KillAll },
        {"killenemies", CommandType.KillEnemies },
        {"killheroes", CommandType.KillHeroes },
        {"givecard", CommandType.GiveCard},
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
                            UnitStaticManager.ReviveUnit(UnitStaticManager.DeadUnitsInPlay[i]);
                    break;

                    case CommandType.KillAll:
                        for (int i = UnitStaticManager.LivingUnitsInPlay.Count - 1; i >= 0; i--)
                            UnitStaticManager.UnitDeath(UnitStaticManager.LivingUnitsInPlay[i]);
                    break;
                    case CommandType.KillEnemies:
                        for (int i = UnitStaticManager.EnemyUnitsInPlay.Count - 1; i >= 0; i--)
                            UnitStaticManager.UnitDeath(UnitStaticManager.EnemyUnitsInPlay[i]);
                    break;

                    case CommandType.KillHeroes:
                        for (int i = UnitStaticManager.PlayerUnitsInPlay.Count - 1; i >= 0; i--)
                            UnitStaticManager.UnitDeath(UnitStaticManager.PlayerUnitsInPlay[i]);
                    break;


                    case CommandType.GiveCard:
                        if (inputParts.Length >= 3) {
                            string cardName = inputParts[1];
                            string[] parameterValues = inputParts.Skip(2).ToArray();

                            EventManager<BattleEvents, string>.Invoke(BattleEvents.GiveCard, cardName);
                        }
                        else {
                            Debug.Log("GiveCard command requires at least one parameter: Card Name.");
                        }
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
