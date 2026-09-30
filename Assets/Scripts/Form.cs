using UnityEngine;
using UnityEngine.UI;
using ZKTris.Core;

// Next / Hold piece previews.
public class Form : MonoBehaviour
{
    public Image preview;
    public Image holdPreview;
    public Sprite L;
    public Sprite Z;
    public Sprite T;
    public Sprite O;
    public Sprite J;
    public Sprite S;
    public Sprite I;

    public void DisplayNextForm(PieceType next)
    {
        preview.sprite = SpriteFor(next);
    }

    public void DisplayHold(PieceType? held, bool available)
    {
        if (holdPreview == null) return;
        holdPreview.enabled = held.HasValue;
        if (held.HasValue) holdPreview.sprite = SpriteFor(held.Value);
        // Dimmed while hold can't be used again for this piece.
        holdPreview.color = available ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    Sprite SpriteFor(PieceType type)
    {
        switch (type)
        {
            case PieceType.L: return L;
            case PieceType.Z: return Z;
            case PieceType.T: return T;
            case PieceType.O: return O;
            case PieceType.J: return J;
            case PieceType.S: return S;
            default: return I;
        }
    }
}
