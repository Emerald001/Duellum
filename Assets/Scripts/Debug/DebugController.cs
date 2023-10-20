using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class DebugController : MonoBehaviour {
    private bool showConsole = false;
    string input;
    //public GUIStyle myStyle;

    public enum CommandType {
        ShakeCamera,
        KillAllUnits
    }
    private Dictionary<string, CommandType> commandDictionary = new Dictionary<string, CommandType>
{
    { "shakecam", CommandType.ShakeCamera },
        {"killall", CommandType.KillAllUnits }
};
    

    public void OnToggleDebug() {
        showConsole = !showConsole;
    }

    private void Awake() {
        input = "";
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F12)) OnToggleDebug();
    }
        
    private void OnGUI() {
        if (!showConsole) return;

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
                    case CommandType.KillAllUnits:
                        
                        break;
                    default:
                        Debug.Log("Unknown command: " + command);
                        break;
                }
            }
            else {
                Debug.Log("Unknown command: " + command);
            }
        }
    }
}
