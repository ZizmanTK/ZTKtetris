using UnityEngine;
using UnityEngine.UI;

// On/off switch visual for a Toggle: slides the knob and tints the track.
// Follows isOn every frame, so it also tracks changes made from code.
[RequireComponent(typeof(Toggle))]
public class ToggleSwitch : MonoBehaviour
{
    public Image track;
    public RectTransform knob;
    public Color onColor = new Color(1f, 0.78f, 0.25f);
    public Color offColor = new Color(0.3f, 0.3f, 0.3f);
    public float travel = 20f;

    Toggle toggle;
    float position;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        position = toggle.isOn ? 1f : 0f;
        Apply();
    }

    void Update()
    {
        float target = toggle.isOn ? 1f : 0f;
        if (Mathf.Approximately(position, target)) return;
        position = Mathf.MoveTowards(position, target, Time.unscaledDeltaTime * 8f);
        Apply();
    }

    void Apply()
    {
        track.color = Color.Lerp(offColor, onColor, position);
        knob.anchoredPosition = new Vector2(Mathf.Lerp(-travel, travel, position), 0f);
    }
}
