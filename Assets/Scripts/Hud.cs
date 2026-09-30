using UnityEngine;
using UnityEngine.UI;

// Score panel, banners ("LEVEL 3", "ZKTRIS!"), pause hint and game over.
public class Hud : MonoBehaviour
{
    public static readonly Color AccentColor = new Color(1f, 0.78f, 0.25f);
    public static readonly Color HoleColor = new Color(0.86f, 0.25f, 0.27f);

    public Text score;
    public Text best;
    public Text level;
    public Text lines;
    public Text holes;
    public Text banner;
    public GameObject pausedPanel;
    public GameObject gameOverPanel;
    public Text gameOverScore;

    public float bannerDuration = 1.2f;

    float bannerTime = -1f;

    void Awake()
    {
        banner.gameObject.SetActive(false);
    }

    public void SetStats(int score, int best, int level, int lines, int holes)
    {
        this.score.text = score.ToString("N0");
        this.best.text = best.ToString("N0");
        this.level.text = level.ToString();
        this.lines.text = lines.ToString();
        this.holes.text = holes.ToString();
    }

    public void ShowBanner(string text, Color color)
    {
        banner.text = text;
        banner.color = color;
        banner.gameObject.SetActive(true);
        bannerTime = 0f;
    }

    public void ShowPaused(bool paused)
    {
        pausedPanel.SetActive(paused && !gameOverPanel.activeSelf);
    }

    public void ShowGameOver(int finalScore, int bestScore, bool newBest)
    {
        pausedPanel.SetActive(false);
        banner.gameObject.SetActive(false);
        bannerTime = -1f;
        gameOverScore.text = newBest
            ? $"NEW BEST  {finalScore:N0}"
            : $"SCORE  {finalScore:N0}\nBEST  {bestScore:N0}";
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
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
        // Pop in, hold, fade out.
        float scale = t < 0.15f ? Mathf.Lerp(1.6f, 1f, t / 0.15f) : 1f;
        banner.rectTransform.localScale = Vector3.one * scale;
        var c = banner.color;
        c.a = t > 0.7f ? Mathf.InverseLerp(1f, 0.7f, t) : 1f;
        banner.color = c;
    }
}
