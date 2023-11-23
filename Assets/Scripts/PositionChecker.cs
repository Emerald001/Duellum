using System.Collections;
using UnityEngine;

public class PositionChecker : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(CheckPositions());
    }

    private IEnumerator CheckPositions() {
        for (int i = -100;  i < 100; i++) {
            for (int j = -100; j < 100; j++) {
                if (GridStaticFunctions.Grid.ContainsKey(new(i, j))) {
                    GridStaticFunctions.Grid[new(i, j)].SetHighlight(HighlightType.AttackHighlight);
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}
