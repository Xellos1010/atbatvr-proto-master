using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using System.Linq;
using System;

public class GameData {
    public string m_gameId;

    public List<GamePlayData> allPlays;
    public List<GamePlayData> allLivePlays;
    private DateTime epoch;

    public bool dataLoaded;

	public eTeamId teamIdAway;
	public eTeamId teamIdHome;

    public GameData(string gameId, string jsonString) {

        List<GamePlayData> allPlaysUnsorted = new List<GamePlayData>();

        string dateFormat = "yyyy-MM-ddThh:mm:ss.fff";
        epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime();

        m_gameId = gameId;

        var objectList = MiniJson.Deserialize(jsonString) as List<object>;

        for (int i=0; i<objectList.Count; i++) {
            Dictionary<string,object> gamePlayDataItem = objectList[i] as Dictionary<string,object>;;

            GamePlayData playData = new GamePlayData();

            playData.gameId = gamePlayDataItem["gamePk"].ToString();
            playData.timeCode = (double.Parse(gamePlayDataItem["timeCode"].ToString()));
            playData.playId = gamePlayDataItem["guid"].ToString();
            playData.atBatNumber = int.Parse(gamePlayDataItem["atBatNumber"].ToString());
            playData.pitchNumber = int.Parse(gamePlayDataItem["pitchNumber"].ToString());

            Dictionary<string,object> gameModeDict = gamePlayDataItem["gameMode"] as Dictionary<string,object>;

            if (gameModeDict != null) {
                playData.gameMode = (eGameMode)(int.Parse(gameModeDict["id"].ToString()));
            }

            playData.inning = int.Parse(gamePlayDataItem["inning"].ToString());
            playData.isTopInning = bool.Parse(gamePlayDataItem["isTopInning"].ToString());
            playData.isPitch = bool.Parse(gamePlayDataItem["isPitch"].ToString());
            playData.isPickoff = bool.Parse(gamePlayDataItem["isPickoff"].ToString());
            playData.isHit = bool.Parse(gamePlayDataItem["isHit"].ToString());

            playData.rawFile = gamePlayDataItem["rawFile"].ToString();
            playData.parsedFile = gamePlayDataItem["parsedFile"].ToString();

            playData.timeStampString = gamePlayDataItem["time"].ToString();

            DateTime timeStampDateTime = DateTime.Parse(playData.timeStampString);
            TimeSpan timeStampSpan = (timeStampDateTime.ToUniversalTime() - epoch);
            playData.timeStampSeconds = timeStampSpan.TotalSeconds;

            playData.createdAt = gamePlayDataItem["createdAt"].ToString();
            playData.updatedAt = gamePlayDataItem["updatedAt"].ToString();
            playData.hasUpdates = bool.Parse(gamePlayDataItem["hasUpdates"].ToString());

            allPlaysUnsorted.Add(playData);
        }
        allPlays = allPlaysUnsorted.OrderBy(o=>o.timeStampSeconds).ToList();

        allLivePlays = new List<GamePlayData>();

        for (int i=0; i<allPlays.Count; i++) {
            if (allPlays[i].gameMode == eGameMode.Live) {
                allLivePlays.Add(allPlays[i]);
            }
        }

        dataLoaded = true;
    }
}

/*
"gamePk": "414205",
"timeCode": "354055810",
"guid": "9165c489-9184-4ecd-bc1b-ae76210c3793",
"atBatNumber": 1,
"pitchNumber": 0,
"gameMode": {
    "id": 1,
    "name": "Warm up"
},
"inning": 1,
"isTopInning": true,
"isPitch": true,
"isPickoff": false,
"isHit": false,
"rawFile": "https://statsapi.mlb.com/api/v1/game/414205/9165c489-9184-4ecd-bc1b-ae76210c3793/analytics/raw",
"parsedFile": "https://statsapi.mlb.com/api/v1/game/414205/9165c489-9184-4ecd-bc1b-ae76210c3793/analytics/parsed",
"time": "2015-05-17T17:34:22.2461712Z",
"createdAt": "2015-05-17T05:34:48.000069Z",
"updatedAt": "2015-05-17T05:34:48.000069Z",
"hasUpdates": false
*/

public enum eGameMode {
    BattingPractice,
    WarmpUps,
    Live
}

public class GamePlayData {
    public string gameId;
    public double timeCode;
    public string playId;

    public int atBatNumber;
    public int pitchNumber;

    public eGameMode gameMode;
    public int inning;
    public bool isTopInning;
    public bool isPitch;
    public bool isPickoff;
    public bool isHit;

	public bool isHomeRun;

	public bool isBallInPlay;
	public bool isActualHit;	//isHit (in the json) incudes foul ball hits, this narrows down to actual hits
	public bool isDoublePlay;
	public bool isSteal;

    public string rawFile;
    public string parsedFile;

    public string timeStampString;
    public double timeStampSeconds;

    public string createdAt;
    public string updatedAt;
    public bool hasUpdates;

	public List<ePlayPositionIdentifier> fielders;

	public void ParseFieldersFromDescription(string description) {
		fielders = new List<ePlayPositionIdentifier> ();

		string descriptionLower = description.ToLower ();

		if (descriptionLower.Contains ("pitcher")) {
			fielders.Add (ePlayPositionIdentifier.Pitcher);
		} else if (descriptionLower.Contains ("catcher")) {
			fielders.Add (ePlayPositionIdentifier.Catcher);
		} else if (descriptionLower.Contains ("first baseman")) {
			fielders.Add (ePlayPositionIdentifier.FirstBaseman);
		} else if (descriptionLower.Contains ("second baseman")) {
			fielders.Add (ePlayPositionIdentifier.SecondBaseman);
		} else if (descriptionLower.Contains ("third baseman")) {
			fielders.Add (ePlayPositionIdentifier.ThirdBaseman);
		} else if (descriptionLower.Contains ("shortstop")) {
			fielders.Add (ePlayPositionIdentifier.Shortstop);
		} else if (descriptionLower.Contains ("left fielder")) {
			fielders.Add (ePlayPositionIdentifier.LeftFielder);
		} else if (descriptionLower.Contains ("center fielder")) {
			fielders.Add (ePlayPositionIdentifier.CenterFielder);
		} else if (descriptionLower.Contains ("right fielder")) {
			fielders.Add (ePlayPositionIdentifier.RightFielder);
		}
	}
}

    
