//using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
//using System.Linq;
using System;

[Serializable]
public class ScheduleData {
    public ScheduleGameData[] allGames;

    private DateTime epoch;

    public int? totalGames;
    public bool dataLoaded;
	public bool noGamesFound;

    public DateTime scheduleDate;

    public ScheduleData(DateTime date, string jsonString) {

        //Initialize the allGamesStatic array and populate each array item
        //for each game in the JSON grab 
        string dateFormat = "yyyy-MM-ddThh:mm:ss.fff";
        epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime();
        scheduleDate = date;
        //Dictionary<string,object> dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
        //allGamesStatic 
        //TODO refactor so only deserialize what you need
        //Dictionary<string, object> dict = Json.Deserialize(jsonString) as Dictionary<string, object>;
        //SimpleJSON.JSONNode JSONNode = SimpleJSON.JSON.Parse(jsonString);
        Dictionary<string, object> JSONNode = MiniJson.Deserialize(jsonString) as Dictionary<string, object>;
        totalGames = (int)((long)JSONNode["totalGames"]);
        //List<object> dates = (List<object>)JSONNode["dates"];// as List<Dictionary<string, object>>;
        if (totalGames > 0)
        {
            List<object> games = (((List<object>)JSONNode["dates"])[0] as Dictionary<string, object>)["games"] as List<object>;
            noGamesFound = false;
            allGames = new ScheduleGameData[(int)totalGames];
            for (int i = 0; i < allGames.Length; i++)
            {
                allGames[i] = new ScheduleGameData();

                
                Dictionary<string, object> gamesinfo = games[i] as Dictionary<string, object>;
                Dictionary<string, object> gameStatus = gamesinfo["status"] as Dictionary<string, object>;
                allGames[i].gameCompleted = ((gameStatus["detailedState"] as string) == "Final") ? true : false;
                #region Setup Teams
                Dictionary<string, object> teams = (games[i] as Dictionary<string, object>) ["teams"] as Dictionary<string, object>;
                allGames[i].SetAwayTeamInfo(
                    (eTeamId)Convert.ToInt16((((teams["away"] as Dictionary<string, object>)["team"] as Dictionary<string, object>)["id"])),
                    Convert.ToInt16(((teams["away"] as Dictionary<string, object>)["leagueRecord"] as Dictionary<string, object>)["wins"]),
                   Convert.ToInt16(((teams["away"] as Dictionary<string, object>)["leagueRecord"] as Dictionary<string, object>)["losses"])//,
                    //allGames[i].awayTeamScore = Convert.ToInt16((teams["away"] as Dictionary<string, object>)["score"])
                    );

                allGames[i].SetHomeTeamInfo(
                    (eTeamId)Convert.ToInt16((((teams["home"] as Dictionary<string, object>)["team"] as Dictionary<string, object>)["id"])),
                    Convert.ToInt16(((teams["home"] as Dictionary<string, object>)["leagueRecord"] as Dictionary<string, object>)["wins"]),
                   Convert.ToInt16(((teams["home"] as Dictionary<string, object>)["leagueRecord"] as Dictionary<string, object>)["losses"])//,
                    //allGames[i].homeTeamScore = Convert.ToInt16((teams["home"] as Dictionary<string, object>)["score"])
                    );


                #endregion
                allGames[i].liveFeedLink = (games[i] as Dictionary<string, object>)["link"] as string;
                allGames[i].gameId = (games[i] as Dictionary<string, object>)["gamePk"] as string;
                allGames[i].gameState = ((games[i] as Dictionary<string, object>)["status"] as Dictionary<string,object>)["detailedState"] as string;
                allGames[i].venueId = (eTeamVenueId)Convert.ToInt16((((games[i] as Dictionary<string, object>)["venue"] as Dictionary<string,object>)["id"])
                    );
                
                
            }
        }
        else
        {
            totalGames = 0;
            noGamesFound = true;
            Debug.Log("There are no games to activate");
        }
        dataLoaded = true;
    }


	public ScheduleGameData GetScheduleGameData(string gameId) {
		ScheduleGameData scheduleGameDataFound = null;
        for (int i = 0; i < allGames.Length; i++)
        {
            ScheduleGameData scheduleGameData = allGames[i];
            if (scheduleGameData.gameId.Equals(gameId))
            {
                scheduleGameDataFound = scheduleGameData;
                break;
            }
        }
        return scheduleGameDataFound;
	}

	public eTeamVenueId GetVenueIdForGameId(string gameId) {
		eTeamVenueId teamVenueId = eTeamVenueId.Unknown;
		ScheduleGameData scheduleGameData = GetScheduleGameData (gameId);
		if (scheduleGameData != null) {
			teamVenueId = scheduleGameData.venueId;
		}
		return teamVenueId;
	}
}
    
public enum eSchedulGameStatus
{
    None = -1,
    PreGame,
    InProgress,
    Final
}

public class ScheduleGameData {
    public string gameId;
    public string liveFeedLink;
    public string gameState;
    private Dictionary<string, eSchedulGameStatus> scheduleStatusReturn;
    public eTeamVenueId venueId;

    public TeamData awayTeam;
    public TeamData homeTeam;

    public int?[] score;

    public string gameDateString;
	public DateTime gameDate;

	public string gameTimeString;
	public DateTime gameTime;

	public bool gameCompleted = true;

    public void SetAwayTeamInfo(eTeamId teamID, int? win, int? loss)//, int? score)
    {
        awayTeam = new TeamData(
                   teamID,
                   win,
                   loss
                   );
        //awayTeamScore = score;
    }

    public void SetHomeTeamInfo(eTeamId teamID, int? win, int? loss)//, int? score)
    {
        homeTeam = new TeamData(
                   teamID,
                   win,
                   loss
                   );
        //homeTeamScore = score;
    }

    public eSchedulGameStatus ReturnScheduleGameState()
    {
        if(scheduleStatusReturn == null)
        {
            scheduleStatusReturn = new Dictionary<string, eSchedulGameStatus>();
            foreach(eSchedulGameStatus stateAvailable in Enum.GetValues(typeof(eSchedulGameStatus)))
            {
                scheduleStatusReturn[stateAvailable.ToString()] = stateAvailable;
            }
        }
        if (scheduleStatusReturn.ContainsKey(gameState))
            return scheduleStatusReturn[gameState];
        else
            Debug.Log("eSchedulGameStatus does not contain " + gameState + "returning None");
        return eSchedulGameStatus.None;
    }
}





/*
"game_type": "R",
"double_header_sw": "N",
"away_team_brief": "Angels",
"away_sport_id": "1",
"game_time_local": "01:35 PM",
"home_team_full": "Baltimore Orioles",
"sport_code": "mlb",
"away_team_long": "Los Angeles Angels of Anaheim",
"away_division_id": "200",
"away_team_abbrev": "LAA",
"description": "",
"away_team": "Los Angeles Angels",
"game_id": "2015/05/17/anamlb-balmlb-1",
"sport_id": "1",
"venue_id": "2",
"gameday_sw": "P",
"away_team_code": "ana",
"away_team_short": "LA Angels",
"away_team_id": "108",
"home_score": "3",
"day_night_code": "D",
"away_sport_code": "mlb",
"away_score": "0",
"game_time_local24": "13:35",
"away_league_id": "103",
"season": "2015",
"home_team_id": "110",
"home_sport_id": "1",
"game_time": "05:35 PM",
"game_date_month": "05",
"home_team_short": "Baltimore",
"game_nbr": "1",
"away_league": "AL",
"scheduled_innings": "9",
"venue_max_seats": "45971",
"game_date_day": "17",
"game_timezone": "0",
"away_team_full": "Los Angeles Angels",
"home_division": "E",
"game_date_year": "2015",
"venue_city": "Baltimore",
"game_time24": "17:35",
"home_team": "Baltimore Orioles",
"venue_state": "MD",
"game_type_desc": "Regular Season",
"venue_country": "USA",
"home_league": "AL",
"game_date": "2015-05-17T00:00:00",
"home_sport_code": "mlb",
"sport": "Major League Baseball",
"game_pk": "414205",
"venue": "Oriole Park at Camden Yards",
"home_league_id": "103",
"venue_turf_type": "G",
"game_status_ind": "F",
"home_division_id": "201",
"home_team_brief": "Orioles",
"game_timezone_local": "-4",
"home_team_code": "bal",
"home_team_abbrev": "BAL",
"home_team_long": "Baltimore Orioles",
"away_division": "W"
*/

    
