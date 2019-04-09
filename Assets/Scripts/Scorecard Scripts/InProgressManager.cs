using UnityEngine;
using System.Collections;

public class InProgressManager : MonoBehaviour {

    public UIRowFillManager basesLoaded;
    public BSOManager bsoManager;
    public InningManager inningManager;

    // Use this for initialization
    void Awake()
    {
        //Initialize();
    }

    public void Initialize()
    {
        if (basesLoaded == null)
            basesLoaded = GetComponentInChildren<UIRowFillManager>();
        if (bsoManager == null)
            bsoManager = GetComponentInChildren<BSOManager>();
        if (inningManager == null)
            inningManager = GetComponentInChildren<InningManager>();
        bsoManager.Initialize();
        inningManager.Initialize();
    }

    public void SetInning(int inning, bool upDown)
    {
        inningManager.SetInning(inning, upDown);
    }

    public void SetBasesLoaded(bool[] baseaLoaded)
    {
        basesLoaded.SetFillAmount(baseaLoaded);
    }

    public void SetBSO(int balls, int strikes, int outs)
    {
        bsoManager.SetBSO(balls, strikes, outs);
    }

    public void SetHighlighted(bool onOff)
    {
        basesLoaded.SetHighlighted(onOff);
        bsoManager.SetHighlighted(onOff);
        inningManager.SetHighlighted(onOff);
    }
}
