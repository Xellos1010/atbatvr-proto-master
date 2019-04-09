using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

public class MenuManager : MonoBehaviour {
    public CalendarPopup calendarPopup;
    public GameObject scoreboardPopup;
    
    public ScorecardManager scorecardManager;

    public PageScrollRectModifiedSchedule modifiedPageScroll;
    public CustomScroll customScroll;

    public Text[] dateDisplays;

    private static MenuManager instance;
    public static MenuManager Instance
    {
        get { return instance; }
    }


	public float autoRefreshTime = 8.0f;

	public LoadingIndicator loadingIndicator;

    #region Initialize

    void Awake() {
		gameObject.SetActive (true);
        instance = this;
    }

	// Use this for initialization
	void Start () {
		PlayerPrefs.DeleteAll ();

        //TODO Setup for At Bat
        //Activate Scoreboard deactivate calendar
        scoreboardPopup.SetActive(true);
        calendarPopup.gameObject.SetActive(false);

        //Set the Active Date for all holders
        try
        {
            SetDateDisplays(ScheduleDataManager.instance.currentDate.ToShortDateString());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    void OnEnable()
    {
        ScheduleDataManager.scheduleDownloaded += LoadScheduleData;
    }

    void OnDisable()
    {
        //We cache then -= because on application close the datamanger instance goes null and gameobjkect can't be found
        ScheduleDataManager.scheduleDownloaded -= LoadScheduleData;
    }

    #endregion Functions

    #region DataLoading
    /// <summary>
    /// Event is fired from DataManager upon sucessful schedule Download. Load scoreboards.
    /// </summary>
    public void LoadScheduleData()
    {
        //Debug.Log("Loading Schedule data");
        //Activate Slots
        scorecardManager.ActivateSlots((int)ScheduleDataManager.currentScheduleData.totalGames);
        //Load Data Per Slot
        for (int i = 0; i < scorecardManager.activeSlots; i++)
        {
            scorecardManager.SetScorecardData(i, ScheduleDataManager.currentScheduleData.allGames[i]);
            //Set Slot Main UI
            //scorecardManager.score[i]
        }
    }
    void SetDateDisplays(string date)
    {
        for (int i = 0; i < dateDisplays.Length; i++)
        {
            dateDisplays[i].text = date;
        }
    }
    #endregion

    #region UI Date Update Functions
    public void OpenDatePicker()
    {
        datePickerActive = true;
        calendarPopup.gameObject.SetActive(datePickerActive);
        Debug.Log(ScheduleDataManager.instance.currentDate);
        calendarPopup.CreateCalendar(ScheduleDataManager.instance.currentDate);
    }

    public CustomToggle[] customToggle;

    public void ProcessDaySelected(HandleDayPick dayPicked)
    {
        //UpdateDatePicker(new DateTime(calendarPopup.curDisplay));
        if (dayPicked.enabled)
        {
            Debug.Log(calendarPopup.curDisplay);
            scorecardManager.ActivateSlots(0);
            ScheduleDataManager.UpdateCurrentDate(new DateTime(DateTime.Now.Year + calendarPopup.yearCounter, calendarPopup.monthCounter, dayPicked.iDay), true);//new DateTime(calendarPopup.curDisplay.Year, calendarPopup.curDisplay.Month,dayPicked.iDay),true);
            UpdateCurrentDateFields();
            if (modifiedPageScroll != null)
                modifiedPageScroll.BringUpSchedules();
            ToggleDatePicker();
            for (int i = 0; i < customToggle.Length; i++)
            {
                customToggle[i].ToggleActive();
            }
            //calendarPopup.gameObject.SetActive(false);
        }
    }

    bool datePickerActive = false;
    public void ToggleDatePicker()
    {
        if (datePickerActive)
            CloseDatePicker();
        else
            OpenDatePicker();
    }

    public void UpdateDatePicker()
    {
        UpdateDatePicker(ScheduleDataManager.instance.currentDate);
    }

    public void UpdateDatePicker(DateTime currentDate)
    {

        //ScrollToStartOfGameList ();
        //TODO check if calendar is active to create new date
        calendarPopup.CreateCalendar(currentDate);
    }

    public void ScorecardDateLeftArrowSelected()
    {
        ScheduleDataManager.AddMinusDay(false, true);//ScheduleDataManager.UpdateCurrentDate(ScheduleDataManager.instance.currentDate.AddDays(-1), true);
        //Add callback to update current date after correct game was found
        UpdateCurrentDateFields();
    }

    public void ScorecardDateRightArrowSelected()
    {
        ScheduleDataManager.AddMinusDay(true, true);//ScheduleDataManager.UpdateCurrentDate(ScheduleDataManager.instance.currentDate.AddDays(1), true);
        UpdateCurrentDateFields();
    }
    #endregion

    #region Date Picker / Calendar / Scoredcard Date

    void UpdateCurrentDateFields()
    {
        SetDateDisplays(ScheduleDataManager.instance.currentDate.ToLongDateString());
    }

    public void DatePickerLeftArrowSelected() {
        ScheduleDataManager.AddMinusMonth(false, true); //ScheduleDataManager.UpdateCurrentDate(ScheduleDataManager.instance.currentDate.AddMonths (-1), true);
		UpdateDatePicker (ScheduleDataManager.instance.currentDate);
	}

	public void DatePickerRightArrowSelected() {
        ScheduleDataManager.AddMinusMonth(true, true);//ScheduleDataManager.UpdateCurrentDate(ScheduleDataManager.instance.currentDate.AddMonths (1), true);

        UpdateDatePicker (ScheduleDataManager.instance.currentDate);
	}

	public void DayItemSelected(int dayNumber) {
		//loadingScreen.SetActive (true);
		ScheduleDataManager.UpdateCurrentDate(new DateTime (ScheduleDataManager.instance.currentDate.Year, ScheduleDataManager.instance.currentDate.Month, dayNumber));
        //TODO Decide whether to close date picker
        CloseDatePicker();
        //UpdateDatePicker (DataManager.instance.currentDate);
	}

    private void CloseDatePicker() {
        //TODO Implement Date Picker
        datePickerActive = false;
        calendarPopup.gameObject.SetActive(datePickerActive);
    }

    private void DisplayDatePicker() {
        //TODO Implement Date Picker
        /*datePickerDisplayed = true;
        datePicker.gameObject.SetActive (true);
        datePickerBackdrop.gameObject.SetActive (true);*/
        //UpdateDatePicker (DataManager.Instance.currentDate);
    }

    #endregion

    #region Schedule Functions
    public static void ScheduleDownloaded(bool yesNo)
    {
        //TODO Implement Data Manager Schedule Loaded
        if (yesNo)
        {
            ScheduleDataManager.ScheduleDownloaded();
            instance.UpdateCurrentDateFields();
        }

    }
    #endregion
}
