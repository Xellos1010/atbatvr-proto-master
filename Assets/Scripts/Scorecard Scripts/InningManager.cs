using UnityEngine;

using System;

public class InningManager : MonoBehaviour {

    [SerializeField]
    UnityEngine.UI.Text inningText;
    [SerializeField]
    UnityEngine.UI.Text inningTextHighlighted;

    bool highlighted = false;

    // Use this for initialization
    void Awake()
    {
        //Initialize();
    }

    public void Initialize()
    {
        Debug.Log("Initializing Inning Manager");
        for (int i = 0; i < transform.childCount; i++)
        {
            if (inningText == null || (transform.GetChild(i).name.ToUpper().Contains("NORMAL") && inningText != transform.GetChild(i).GetComponent<UnityEngine.UI.Text>()))
                if (transform.GetChild(i).name.ToUpper().Contains("NORMAL"))
                    inningText = transform.GetChild(i).GetComponent<UnityEngine.UI.Text>();
            if (inningTextHighlighted == null)
                if (transform.GetChild(i).name.ToUpper().Contains("HIGHLIGHT"))
                    inningTextHighlighted = transform.GetChild(i).GetComponent<UnityEngine.UI.Text>();
        }
        
    }

    //TODO Add way to inject highlighted Arrow - possibly activate seperate text box

    public void SetHighlighted(bool onOff)
    {
        highlighted = onOff;
        inningTextHighlighted.gameObject.SetActive(onOff);
    }

    public void SetInning(int inningNumber, bool upDown)
    {
        if (upDown)
            inningText.text = string.Join(" ", new String[2] { "▲", inningNumber.ToString() });
        else
            inningText.text = string.Join(" ", new String[2] { "▼", inningNumber.ToString() });
    }
}
