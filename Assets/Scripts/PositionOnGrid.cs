using UnityEngine;

// One block of the falling piece (or of its ghost).
[RequireComponent(typeof(SpriteRenderer))]
public class PositionOnGrid : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Show(Vector3 position, Sprite sprite)
    {
        transform.position = position;
        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }
}
