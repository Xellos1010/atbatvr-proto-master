using UnityEngine;
using System.Collections;

public class BSOManager : MonoBehaviour
{
    [SerializeField]
    private Sprite empty;
    [SerializeField]
    private Sprite fill;
    [SerializeField]
    private Sprite highlight;

    [SerializeField]
    UIRowFillManager[] fillRows;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        fillRows = GetComponentsInChildren<UIRowFillManager>();
        for (int i = 0; i < fillRows.Length; i++)
        {
            InitializeFillRow(i);
            InitializeFillRowIcons(i);
        }
    }

    void InitializeFillRow(int iRow)
    {
        fillRows[iRow].Initialize();
    }

    void InitializeFillRowIcons(int iRow)
    {
        fillRows[iRow].SetSprites(empty,fill,highlight);
    }
    
    public void SetHighlighted(bool onOff)
    {
        for (int i = 0; i < fillRows.Length; i++)
        {
            fillRows[i].SetHighlighted(onOff);
        }
    }
	
    public void SetBSO(int ball, int strike, int outs)
    {
        Debug.Log("Setting BSO");
        for (int i = 0; i < fillRows.Length; i++)
        {
            if (fillRows[i].gameObject.name.Contains("B"))
            {
                Debug.Log("Setting B");
                fillRows[i].SetFillAmount(ball);
                continue;
            }
            if (fillRows[i].gameObject.name.Contains("S"))
            {
                Debug.Log("Setting S");
                fillRows[i].SetFillAmount(strike);
                continue;
            }
            if (fillRows[i].gameObject.name.Contains("O"))
            {
                Debug.Log("Setting O");
                fillRows[i].SetFillAmount(outs);
                continue;
            }
        }
    }
}
