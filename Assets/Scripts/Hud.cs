using UnityEngine;
using UnityEngine.UI;
using ZKTris.Core;

// Floating stats next to the board and the clear/level messages that slide
// in beside it.
public class Hud : MonoBehaviour
{
    // Palette: cream labels over the orange/grey background, white values,
    // light red for holes; salmon is the board border.
    public static readonly Color AccentColor = Color.white;
    public static readonly Color HoleColor = new Color(1f, 0.72f, 0.72f);
    public static readonly Color TextColor = new Color(0.96f, 0.9f, 0.8f);
    public static readonly Color Salmon = new Color(0.808f, 0.42f, 0.45f);

    public Text score;
    public Text best;
    public Text level;
    public Text lines;
    public Text holes;
    // Fill of the thin line under LEVEL: progress to the next level.
    public RectTransform levelProgress;
    public Text banner;

    public float bannerDuration = 1.4f;
    public float bannerSlide = 40f;

    float bannerTime = -1f;
    Vector2 bannerHome;

    void Awake()
    {
        bannerHome = banner.rectTransform.anchoredPosition;
        banner.gameObject.SetActive(false);
    }

    public void SetStats(int score, int best, int level, int lines, int holes)
    {
        this.score.text = score.ToString("N0");
        this.best.text = "BEST " + best.ToString("N0");
        this.level.text = "LEVEL " + level;
        this.lines.text = lines.ToString();
        this.holes.text = holes.ToString();
        float progress = (lines % ScoreKeeper.LinesPerLevel) / (float)ScoreKeeper.LinesPerLevel;
        levelProgress.anchorMax = new Vector2(progress, 1f);
    }

    public void ShowBanner(string text, Color color)
    {
        banner.text = text;
        banner.color = color;
        banner.gameObject.SetActive(true);
        bannerTime = 0f;
    }

    void Update()
    {
        if (bannerTime < 0f) return;
        bannerTime += Time.unscaledDeltaTime;
        float t = bannerTime / bannerDuration;
        if (t >= 1f)
        {
            banner.gameObject.SetActive(false);
            bannerTime = -1f;
            return;
        }
        // Slides out from the board edge, holds, then fades.
        float slide = 1f - Mathf.Clamp01(t / 0.18f);
        banner.rectTransform.anchoredPosition = bannerHome + new Vector2(slide * slide * bannerSlide, 0f);
        var c = banner.color;
        c.a = t < 0.18f ? t / 0.18f : t > 0.7f ? Mathf.InverseLerp(1f, 0.7f, t) : 1f;
        banner.color = c;
    }
}
