using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class JSONDeserializeTestMini : MonoBehaviour {

    public string urlSchedule;
    public string urlGame;
    public string urlPlayByPlay;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(TestSuite());
	}

    IEnumerator TestSuite()
    {
        yield return DownloadGumboDataSchedule(urlSchedule);
    }

    private IEnumerator DownloadGumboDataSchedule(string url)
    {
        //Debug.Log(dateScheduleData);
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;

        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("jeremy.pope@mlb.com:dov3sCry"));

        byte[] rawData = form.data;
        //string url = string.Join("", new string[3] { "http://statsapi.mlb.com/api/v1/schedule?date=", dateScheduleData.ToShortDateString(), "&sportId=1" });
        //Debug.Log(url);
        WWW www = new WWW(url, null, headers);

        yield return www;
        if (www.error == null)
        {
            //StartCoroutine(ProcessScheduleData(dateScheduleData, www.text, callback));
            //Process JSON
            //Process MiniJSON
            //Process 
        }
        else
        {
            //callback(false);
        }
    }

    private IEnumerator DownloadGumboDataGame(string url,string gameId, Action<bool> callback)
    {

        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;

        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("jeremy.pope@mlb.com:dov3sCry"));

        byte[] rawData = form.data;
        //string url = "http://statsapi.mlb.com/api/v1/game/" + gameId + "/feed/live";

        WWW www = new WWW(url, null, headers);

        yield return www;
        if (www.error == null)
        {
            StartCoroutine(ProcessGumboData(gameId, www.text, callback));
        }
        else
        {
            callback(false);
        }
    }

    private IEnumerator ProcessPlayByPlayGumbo(ScheduleData toAdd, string playbyplayJSON)
    {
        yield return new WaitForEndOfFrame();
        //SimpleJSON.JSONNode Json = SimpleJSON.JSON.Parse(playbyplayJSON);
        Debug.Log("JSON parsed");
    }

    private IEnumerator ProcessGumboData(string gameId, string gumboDataText, Action<bool> callback)
    {
        yield return new WaitForEndOfFrame();
        GumboData gumboData = new GumboData(gameId, gumboDataText);
        if (callback != null)
        {
            callback(true);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
