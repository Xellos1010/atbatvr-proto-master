using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Globalization;



public class CalendarPopup : MonoBehaviour
{

    public HandleDayPick[] days;         //Holds 35 labels
    public Transform dayHolder;
    public string[] Months;             //Holds the months
    public Text[] HeaderLabels;         //The label used to show the Month
    public Text DateLabel;           //Label which is set when date is chosen;
    //public GameObject overlay;
    public GameObject Calendar;

    public int monthCounter = DateTime.Now.Month - 1;
    public int yearCounter = 0;
    private DateTime dateTime;
    public DateTime curDisplay;
    public DateTime currentDateDisplay;

    public static CalendarPopup instance
    {
        get
        {
            return _instance;
        }
    }
    private static CalendarPopup _instance;

    void OnEnable()
    {
        _instance = this;
        days = GetAllDays();
        //SetupEventsOnDays();
        clearLabels();
        CreateMonths();
        if(ScheduleDataManager.instance != null)
            CreateCalendar(ScheduleDataManager.instance.currentDate);
        //CreateCalendar(ScheduleDataManager.instance.ReturnFirstScoreboardDate());// currentDate);
    }

    HandleDayPick[] GetAllDays()
    {
        return GetDaysFromRow(dayHolder);
    }

    HandleDayPick[] GetDaysFromRow(Transform transformToCheck)
    {
        List<HandleDayPick> returnValue = new List<HandleDayPick>();
        if (transformToCheck.gameObject.GetComponent<HandleDayPick>() == null)
            for (int i = 0; i < transformToCheck.childCount; i++)
            {
                returnValue.AddRange(GetDaysFromRow(transformToCheck.GetChild(i)));
            }
        else
            returnValue.Add(transformToCheck.GetComponent<HandleDayPick>());
        return returnValue.ToArray();
    }

    void SetupEventsOnDays()
    {
        //for (int i = 0; i < DayLabels.Length; i++)
        //    DayLabels[i].transform.parent.GetComponent<Button>().onClick.AddListener(() => HandleDaySelection(DayLabels[i].transform.parent.GetComponent<HandleDayPick>()));
    }
    /*Adds al the months to the Months Array and sets the current month
 
    in the header label*/

    void CreateMonths()

    {
        Months = new string[12];
        dateTime = new DateTime(2009, 1, 1);

        for (int i = 0; i < 12; ++i)
        {
            dateTime = new DateTime(2009, i + 1, 1);
            Months[i] = dateTime.ToString("MMMM");
        }

        //SetHeaderText(Months[DateTime.Now.Month - 1] + " " + DateTime.Now.Year);

    }

    void CreateMonths(int year)

    {
        Months = new string[12];

        for (int i = 0; i < 12; ++i)
        {
            dateTime = new DateTime(year, i + 1, 1);
            Months[i] = dateTime.ToString("MMMM");
        }
        //SetHeaderText(Months[DateTime.Now.Month - 1] + " " + DateTime.Now.Year);
    }

    /*Sets the days to their correct labels*/

    void CreateCalendar()
    {
        
        curDisplay = dateTime;
        ResetAllDays();
        int curDays = GetDays(curDisplay.DayOfWeek);
        int index = 0;

        if (curDays > 0)
            index = (curDays - 1);
        else
            index = curDays;

        while (curDisplay.Month == dateTime.Month)
        {
            days[index].SetDay(curDisplay.Day);
            curDisplay = curDisplay.AddDays(1);
            index++;
        }

    }

    public void CreateCalendar(DateTime dateTime)
    {
        //Set curDisplay to the first of the month
        curDisplay = new DateTime(dateTime.Year,dateTime.Month,1);
        //Debug.Log("currentDateDisplay updated");
        //currentDateDisplay = new DateTime(DateTime.Now.Year + yearCounter, monthCounter+1, 1);
        //currentDateDisplay = new DateTime(dateTime.Year, dateTime.Month, 1);
        ResetAllDays();
        int curDays = GetDays(curDisplay.DayOfWeek);
        int index = 0;

        if (curDays > 0)
            index = (curDays - 1);
        else
            index = curDays;

        monthCounter = dateTime.Month;

        while (curDisplay.Month == dateTime.Month)
        {
            days[index].SetDay(curDisplay.Day);
            curDisplay = curDisplay.AddDays(1);
            index++;
        }
        DateLabel.text = Months[dateTime.Month-1] + " " + (dateTime.Year);
    }

    void ResetAllDays()
    {
        for (int i = 0; i < days.Length; i++)
        {
            days[i].Reset();
        }
    }

    private int GetDays(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Sunday: return 1;
            case DayOfWeek.Monday: return 2;
            case DayOfWeek.Tuesday: return 3;
            case DayOfWeek.Wednesday: return 4;
            case DayOfWeek.Thursday: return 5;
            case DayOfWeek.Friday: return 6;
            case DayOfWeek.Saturday: return 7;
            //case DayOfWeek.Sunday: return 7;
            default: throw new Exception("Unexpected DayOfWeek: " + day);
        }
    }

    /*when left arrow clicked go to previous month */

    public void SetHeaderText(string text)
    {
        for (int i = 0; i < HeaderLabels.Length; i++)
        {
            HeaderLabels[i].text = text;
        }
    }
    /*when right arrow clicked go to next month */

    public void nextMonth()
    {
        monthCounter++;
        if (monthCounter > 11)
        {
            monthCounter = 0;
            yearCounter++;
        }

        //SetHeaderText(Months[monthCounter] + " " + (DateTime.Now.Year + yearCounter));
        clearLabels();
        dateTime = new DateTime(DateTime.Now.Year + yearCounter, monthCounter, 1);//curDisplay.AddMonths(1);
        currentDateDisplay = dateTime;
        //CreateCalendar();
        CreateCalendar(dateTime);
    }

    public void previousMonth()
    {
        monthCounter--;
        if (monthCounter < 0)
        {
            monthCounter = 11;
            yearCounter--;
        }
        //SetHeaderText(Months[monthCounter] + " " + (DateTime.Now.Year + yearCounter));
        clearLabels();
        dateTime = new DateTime(DateTime.Now.Year + yearCounter, monthCounter, 1);//curDisplay.AddMonths(-1);
        curDisplay = dateTime;
        currentDateDisplay = dateTime;
        //CreateCalendar();
        CreateCalendar(dateTime);
    }

    public void ShowPopup()
    {
        //overlay.SetActive(true);
        Calendar.SetActive(true);
        LeanTween.scale(Calendar, new Vector3(1, 1, 1), .5f);
        //HOTween.To(Calendar.transform, 0.5f, "localScale", new Vector3(1, 1, 1), false);
    }

    public void ClosePopup()
    {
        LeanTween.scale(Calendar, new Vector3(.01f, .01f, .01f), .5f).setOnComplete(TurnOff);
        //HOTween.To(Calendar.transform, 0.5f, new TweenParms().Prop("localScale", new Vector3(0.01f, 0.01f, 0.01f), false).OnComplete(TurnOff));
    }

    /*clears all the day labels*/

    void clearLabels()
    {
        for (int x = 0; x < days.Length; x++)
        {
            days[x].Reset();
        }
    }

    void TurnOff()
    {
        Calendar.SetActive(false);
        //overlay.SetActive(false);
    }

    public void HandleDaySelection(HandleDayPick dayPicked)
    {
        if (dayPicked.enabled)
        {
            Debug.Log("Process day picked " + dayPicked.iDay);
        }
    }

    public void HandleDaySelection(int iDayPicked)
    {
        //TODO Call Menu Manager to Load Games from website
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            previousMonth();
        if (Input.GetKeyDown(KeyCode.RightArrow))
            nextMonth();
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ClosePopup();
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ShowPopup();
    }
}