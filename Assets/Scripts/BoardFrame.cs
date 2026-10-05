using UnityEngine;

// Keeps this RectTransform exactly over the board (border included), so
// the HUD can hang off the board's edges at any screen aspect.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class BoardFrame : MonoBehaviour
{
    public Renderer board;
    public Camera worldCamera;

    void LateUpdate() => Refresh();

    public void Refresh()
    {
        if (board == null || worldCamera == null) return;
        var rt = (RectTransform)transform;
        var parent = (RectTransform)rt.parent;
        var bounds = board.bounds;
        Vector2 min = ToLocal(parent, bounds.min);
        Vector2 max = ToLocal(parent, bounds.max);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (min + max) / 2f - parent.rect.center;
        rt.sizeDelta = max - min;
    }

    Vector2 ToLocal(RectTransform parent, Vector3 world)
    {
        var screen = worldCamera.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, worldCamera, out var local);
        return local;
    }
}
