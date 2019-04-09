using UnityEngine;
using System.Collections;

public class UIRowFillManager : MonoBehaviour {

    public Sprite empty;
    public Sprite fill;
    public Sprite highlight;

    private bool highlighted = false;
    [SerializeField]
    private int filled;

    [SerializeField]
    UnityEngine.UI.Image[] fillIcons;
	// Use this for initialization
	void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        fillIcons = GetComponentsInChildren<UnityEngine.UI.Image>();
    }
	
    public void SetSprites(Sprite emptyIcon, Sprite filledIcon, Sprite highlightedIcon)
    {
        empty = emptyIcon;
        fill = filledIcon;
        highlighted = highlightedIcon;
    }

	public void SetFillAmount(int ifillAmount)
    {
        filled = ifillAmount;
        for (int i = 0; i < fillIcons.Length; i++)
            if (i < ifillAmount)
                if (highlighted)
                    fillIcons[i].sprite = highlight;
                else
                    fillIcons[i].sprite = fill;
            else
                fillIcons[i].sprite = empty;
    }

    public void SetFillAmount(bool[] iFillSlots)
    {
        for (int i = 0; i < fillIcons.Length; i++)
            if (i < iFillSlots.Length)
                if(iFillSlots[i])
                    if (highlighted)
                        fillIcons[i].sprite = highlight;
                    else
                        fillIcons[i].sprite = fill;
                else
                    fillIcons[i].sprite = empty;
    }

    public void SetHighlighted(bool onOff)
    {
        if (highlighted != onOff)
            highlighted = onOff;
        SetFillAmount(filled);
    }
}
