using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One line of the pause menu (not "MenuItem": that name is Unity's editor attribute). Hover selects it, click activates it.
public class PauseMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public Text label;
    // ON/OFF readout for settings; empty for actions.
    public Text value;
    public bool isSetting;

    [HideInInspector]
    public PauseMenu menu;

    public void SetValue(bool on)
    {
        if (value == null) return;
        value.text = on ? "ON" : "OFF";
        value.color = on ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }

    public void OnPointerEnter(PointerEventData eventData) => menu.Select(this);

    public void OnPointerClick(PointerEventData eventData) => menu.Activate(this);
}
