using UnityEngine;
using System.Collections;

public class ScoreUIPanelManager : MonoBehaviour {

    public InProgressManager inprogress;
    public PrePostUIPanelManager prePostText;

    //public delegate

	// Use this for initialization
	void Awake()
    {
        //Initialize();
	}

    public void Initialize()
    {
        if (inprogress == null)
            inprogress = GetComponentInChildren<InProgressManager>();
        if (prePostText == null)
            prePostText = GetComponentInChildren<PrePostUIPanelManager>();
        inprogress.Initialize();
    }

    public void ToggleHighlightsGraphics(bool onOff)
    {

    }

    public void SetInning(int inning, bool upDown)
    {
        inprogress.SetInning(inning, upDown);
    }

    public void SetBasesLoaded(bool[] baseaLoaded)
    {
        inprogress.SetBasesLoaded(baseaLoaded);
    }

    public void SetBSO(int balls, int strikes, int outs)
    {
        inprogress.SetBSO(balls, strikes, outs);
    }

    public void SetHighlighted(bool onOff)
    {
        inprogress.SetHighlighted(onOff);
    }
}


public enum eScorecardState
{
    None = -1,
    Regular,
    Highlighted
}