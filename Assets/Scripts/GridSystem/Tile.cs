using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    [SerializeField] private new Renderer renderer;

    [Header("Colors")]
    [SerializeField] private Material standardColor;
    [SerializeField] private Material HoverColor;
    [SerializeField] private Material movementColor;
    [SerializeField] private Material attackColor;
    [SerializeField] private Material ownPositionColor;
    [SerializeField] private Material transparent;

    [SerializeField] private TileType type;

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardWorldPosition { get; set; }
    public float Height { get; set; }
    public TileType Type => type;

    private readonly List<Vector3> ripplePositions = new();

    private HighlightType currentType = HighlightType.None;
    private readonly ActionQueue queue = new();

    private void Update() {
        queue.OnUpdate();
    }

    public void SetHighlight(HighlightType type) {
        if (renderer != null) {
            renderer.material = type switch {
                HighlightType.None => standardColor,
                HighlightType.MovementHighlight => movementColor,
                HighlightType.AttackHighlight => attackColor,
                HighlightType.OwnPositionHighlight => ownPositionColor,
                HighlightType.Transparent => transparent,
                _ => throw new System.NotImplementedException(),
            };
        }

        currentType = type;
    }

    public void SetHover(bool hover) {
        if (hover)
            renderer.material = HoverColor;
        else
            SetHighlight(currentType);
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public IEnumerator DoRipple(float waitTime, float rippleStrength, bool setToFalse = false) {
        yield return new WaitForSeconds(waitTime);

        ripplePositions.Add(StandardWorldPosition - Vector3.up * rippleStrength);
        ripplePositions.Add(StandardWorldPosition + Vector3.up * (rippleStrength / 3));
        ripplePositions.Add(StandardWorldPosition - Vector3.up * (rippleStrength / 6));
        ripplePositions.Add(StandardWorldPosition + Vector3.up * (rippleStrength / 15));
        ripplePositions.Add(StandardWorldPosition);

        foreach (var item in ripplePositions) {
            while (transform.position != item) {
                transform.position = Vector3.MoveTowards(transform.position, item, 30 * Time.deltaTime);
                yield return null;
            }
        }

        if (setToFalse) {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);
        }

        ripplePositions.Clear();
    }
    
    public void ClearQueue() => queue.Clear();
}

public enum HighlightType {
    None,
    MovementHighlight,
    AttackHighlight,
    OwnPositionHighlight,
    Transparent,
}

public enum TileType {
    Normal,
    Lava,
    Cover,
    HalfCover,
    Spawn,
    Special,
    EnemySpawn,
    BossSpawn,
}

public enum TileEffect {
    Card,
    OnFire,
    Trapped,
}