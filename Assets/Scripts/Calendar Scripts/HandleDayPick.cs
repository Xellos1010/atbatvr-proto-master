using UnityEngine;
using System.Collections;

public class HandleDayPick : MonoBehaviour {
    public bool enabled = false;
    public int iDay;
    private UnityEngine.UI.Text dayDisplay;

    public void Reset()
    {
        iDay = -1;
        UpdateTextOnDay();
        enabled = false;
        gameObject.SetActive(false);
    }

    public void SetDay(int day)
    {
        gameObject.SetActive(true);
        enabled = true;
        iDay = day;
        UpdateTextOnDay();
    }

    private void UpdateTextOnDay()
    {
        if (dayDisplay == null)
            dayDisplay = GetComponentInChildren<UnityEngine.UI.Text>();
        if (iDay > 0)
            dayDisplay.text = iDay.ToString();
        else
            dayDisplay.text = "";
    }

    public void HandleDaySelected()
    {
        if (enabled)
            CalendarPopup.instance.HandleDaySelection(iDay);
    }
}
