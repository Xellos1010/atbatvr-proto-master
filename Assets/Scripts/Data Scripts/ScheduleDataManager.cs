using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScheduleDataManager : MonoBehaviour {

	public List<ScheduleData> allScheduleData;
	public static ScheduleData currentScheduleData;
	public ScheduleGameData currentScheduleGameData;

	public DateTime currentDate;

    public List<GameData> allGameData;
    public GameData currentGameData;

	public List<GumboData> allGumboData;
	public GumboData currentGumboData;

    public List<PlayData> allPlayData;
    public PlayData currentPlayData;

	public List<PlayerData> allPlayerData;

	public List<TeamDataObject> allTeamData;

	public string playDataError;

    private static ScheduleDataManager _instance;
    public static ScheduleDataManager instance
    {
        
        get {
            if (_instance == null)
                _instance = GameObject.FindGameObjectWithTag("DataManager").GetComponent<ScheduleDataManager>();
            return _instance;
        }
    }

	public bool smoothData = true;

    #region Event Info

    public delegate void ScheduleDownload();
    public static event ScheduleDownload scheduleDownloaded;
    public static void ScheduleDownloaded()
    {
        scheduleDownloaded();
    }
    #endregion

    #region Initialize

    void OnEnable()
    {
        _instance = this;
    }

    void Awake()
    {
        _instance = this;
        allScheduleData = new List<ScheduleData>();
        allGameData = new List<GameData>();
        allGumboData = new List<GumboData>();
        allPlayData = new List<PlayData>();
        allPlayerData = new List<PlayerData>();
        currentDate = new DateTime(2016, 11, 2); // TODO Change to get the closest game data
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(InitializeFirstScoreboardDate(0.1f, ScheduleDataLoaded));
	}

    #endregion

    public DateTime ReturnFirstScoreboardDate()
    {
        //TODO pull scores from the website and sort for months with game data
        return currentDate;
    }

#region Schedule Data Handling

	public static void UpdateCurrentDate (DateTime dateTime, bool loadSchedule = true) {
		_instance.currentDate = dateTime;
		if (loadSchedule) {
			LoadSchedule (dateTime, _instance.ScheduleDataLoaded);
		}
	}

    public static bool incDecDateUpDown = false;
    public static bool loadScheduleAfterDateChange;
    public static void AddMinusMonth(bool upDown, bool loadSchedule = true)
    {
        incDecDateUpDown = upDown;
        loadScheduleAfterDateChange = loadSchedule;
        //Check if date has schedule and keep adding until there is a schedule
        AddMinusMonth();
    }

    private static void AddMinusMonth()
    {
        //Check if date has schedule and keep adding until there is a schedule
        if (incDecDateUpDown)
        {
            _instance.currentDate = ScheduleDataManager.instance.currentDate.AddMonths(1);
        }
        else
            _instance.currentDate = ScheduleDataManager.instance.currentDate.AddMonths(-1);
        if (loadScheduleAfterDateChange)
        {
            LoadSchedule(_instance.currentDate, _instance.ScheduleIncrementMonthCallback);
        }
    }

    public static void AddMinusDay(bool upDown, bool loadSchedule = true)
    {
        incDecDateUpDown = upDown;
        loadScheduleAfterDateChange = loadSchedule;
        //Check if date has schedule and keep adding until there is a schedule
        AddMinusDay();
    }

    private static void AddMinusDay()
    {
        //Check if date has schedule and keep adding until there is a schedule
        if (incDecDateUpDown)
            _instance.currentDate = ScheduleDataManager.instance.currentDate.AddDays(1);
        else
            _instance.currentDate = ScheduleDataManager.instance.currentDate.AddDays(-1);

        
        if (loadScheduleAfterDateChange)
        {
            LoadSchedule(_instance.currentDate, _instance.ScheduleIncrementDayCallback);
        }
    }

    private void ScheduleIncrementMonthCallback(bool yesNo)
    {
        if (yesNo)
            MenuManager.ScheduleDownloaded(yesNo);
        else
            AddMinusMonth();
    }

    private void ScheduleIncrementDayCallback(bool yesNo)
    {
        if (yesNo)
            MenuManager.ScheduleDownloaded(yesNo);
        else
            AddMinusDay();
    }

    private void ScheduleDataLoaded(bool success)
    {
        MenuManager.ScheduleDownloaded(success);
    }

    private IEnumerator InitializeFirstScoreboardDate(float delay, Action<bool> callback)
    {
        yield return new WaitForSeconds(delay);
        //TODO Create a way to find active games and filter
        //Load Schedule November 2nd - End of the season at time of creation - TODO Remove for final build
        LoadSchedule(new DateTime(2016, 11, 2),callback);
    }

    private IEnumerator LoadScheduleAfterDelay(float delay, DateTime dateTime, Action<bool> callback) {
		yield return new WaitForSeconds (delay);
		LoadSchedule (dateTime, callback);
	}

    public static void LoadSchedule(DateTime dateTime, Action<bool> callback) {
        //_instance.StartCoroutine(_instance.DownloadScheduleDataLegacy(dateTime, callback));
        currentScheduleData = instance.ReturnScheduleIfExists(dateTime);
        if (currentScheduleData == null)
            _instance.StartCoroutine(instance.DownloadGumboDataSchedule(dateTime, callback));
        else
        {
            instance.CheckCurrentDataForGames(callback);
        }
    }
    #endregion

    #region Gumbo Data Handling

    public static void LoadGumbo(string gameId, Action<bool> callback) {
		_instance.StartCoroutine(_instance.DownloadGumboDataGame(gameId, callback));
	}

    private IEnumerator DownloadGumboDataSchedule(DateTime dateScheduleData, Action<bool> callback)
    {
        //Debug.Log(dateScheduleData);
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        
        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("jeremy.pope@mlb.com:dov3sCry"));

        byte[] rawData = form.data;
        string url = string.Join("",new string[3] { "http://statsapi.mlb.com/api/v1/schedule?date=",dateScheduleData.ToShortDateString(),"&sportId=1" });
        //Debug.Log(url);
        WWW www = new WWW(url, null, headers);

        yield return www;
        if (www.error == null)
        {
            StartCoroutine(ProcessScheduleData(dateScheduleData, www.text, callback));
        }
        else
        {
            callback(false);
        }
    }

    private ScheduleData ReturnScheduleIfExists(DateTime date)
    {
        for (int i = 0; i < allScheduleData.Count; i++)
        {
            if (allScheduleData[i].scheduleDate == date)
            {
                return allScheduleData[i];
            }
        }
        return null;
    }

    private IEnumerator ProcessScheduleData(DateTime date, string scheduleDataText, Action<bool> callback)
    {
        yield return new WaitForEndOfFrame();
        currentScheduleData = new ScheduleData(date, scheduleDataText);
        allScheduleData.Add(currentScheduleData);
        CheckCurrentDataForGames(callback);
    }

    private void CheckCurrentDataForGames(Action<bool> callback)
    {
        if (!currentScheduleData.noGamesFound)
        {
            //TODO Setup Download of Play by Play based on which game was currently selected
            //yield return DownloadPlayByPlay(currentScheduleData.allGames[0].gameId, currentScheduleData, callback);
            //Setup InProgress Data through Corutine
            //Check to see if already has schedule Data

            callback(true);
        }
        else
        {
            callback(false);
        }
        //TODO: Added to hook in menu Manager - maybe use callback
        //Debug.Log("Schedule Downloaded");
        //DataManager.scheduleDownloaded();
    }

    private IEnumerator DownloadPlayByPlay(string gamePK,ScheduleData toAdd, Action<bool> callback)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;

        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("jeremy.pope@mlb.com:dov3sCry"));

        byte[] rawData = form.data;
        string url = "https://statsapi.mlb.com/api/v1/game/" + gamePK + "/playByPlay";

        WWW www = new WWW(url, null, headers);

        yield return www;
        if (www.error == null)
        {
            StartCoroutine(ProcessPlayByPlayGumbo(toAdd , www.text));
        }
        else
        {
            Debug.Log("www.error: " + www.error + ", www.text: " + www.text + ", url: " + url);
            callback(false);
        }
    }

    private IEnumerator DownloadGumboDataGame(string gameId, Action<bool> callback) {

		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;

		headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("jeremy.pope@mlb.com:dov3sCry"));

		byte[] rawData = form.data;
		string url = "http://statsapi.mlb.com/api/v1/game/" + gameId + "/feed/live";

		WWW www = new WWW(url, null, headers);

		yield return www;
		if (www.error == null) {
			Debug.Log("url: " + url + ", www.text: " + www.text);
            /*string filename = "MLB Stats Link";
            var sr = File.CreateText(filename);
            sr.WriteLine(url);
            sr.WriteLine(www.text);
            sr.Close();*/
           // DebugLogger.Log("url: " + url + ", www.text: " + www.text);
			StartCoroutine(ProcessGumboData(gameId, www.text, callback));
		} else {
			Debug.Log("www.error: " + www.error + ", www.text: " + www.text + ", url: " + url);
            //DebugLogger.Log("www.error: " + www.error + ", www.text: " + www.text + ", url: " + url);
			callback(false);
		}
	}

    private IEnumerator ProcessPlayByPlayGumbo(ScheduleData toAdd,string playbyplayJSON)
    {
        yield return new WaitForEndOfFrame();
        
        //TODO Setup the Processing of Play by Play Data
        //SimpleJSON.JSONNode Json = SimpleJSON.JSON.Parse(playbyplayJSON);
        Debug.Log("JSON parsed");
    }

	private IEnumerator ProcessGumboData(string gameId, string gumboDataText, Action<bool> callback) {
		yield return new WaitForEndOfFrame();
		GumboData gumboData = new GumboData (gameId, gumboDataText);
		currentGumboData = gumboData;
		allGumboData.Add (currentGumboData);
		if (callback != null) {
			callback (true);
		}
	}
#endregion  
}