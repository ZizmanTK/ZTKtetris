using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZKTris.Core;

// Next queue and Hold previews.
public class Form : MonoBehaviour
{
    // Next queue slots, nearest first.
    public Image[] nextPreviews;
    public Image holdPreview;
    public Sprite L;
    public Sprite Z;
    public Sprite T;
    public Sprite O;
    public Sprite J;
    public Sprite S;
    public Sprite I;

    public void DisplayNext(IReadOnlyList<PieceType> upcoming)
    {
        for (int i = 0; i < nextPreviews.Length; i++)
        {
            bool shown = i < upcoming.Count;
            nextPreviews[i].enabled = shown;
            if (shown) nextPreviews[i].sprite = SpriteFor(upcoming[i]);
        }
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
