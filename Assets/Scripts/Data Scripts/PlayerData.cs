using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using System.Linq;
using System;

public class PlayerData {

	public string playerId;
	public string playerName;
	public string playerNameFirst;
	public string playerNameLast;

	public int jerseyNumber;

	public ePlayPositionIdentifier positionIdentifier;	//Where they normally play while on defense, not where they are on the field (e.g. Batter, Catcher, Runner on 3rd, etc) 
	public ePlayPositionIdentifier primaryPositionIdentifier;

	public eTeamId teamId;

	public bool leftHandedThrower = false;

	public bool dataLoaded;

    public string alternatePosition;
    public string alternateNumber;

	public PlayerData(string playerIdToUse, string jsonString, ePlayPositionIdentifier positionIdentifierToUse) {

		Dictionary<string,object> dict = MiniJson.Deserialize(jsonString) as Dictionary<string,object>;

		List<object> playerDigest = dict ["player_digest"] as List<object>;
		Dictionary<string,object> playerDigestDict = dict["player_digest"] as Dictionary<string,object>;
		Dictionary<string,object> playerDict = playerDigestDict["player"] as Dictionary<string,object>;
		Dictionary<string,object> queryResultsDict = playerDict["queryResults"] as Dictionary<string,object>;
		Dictionary<string,object> rowDict = queryResultsDict["row"] as Dictionary<string,object>;

		playerId = playerIdToUse;
		playerName = rowDict ["name_first_last"].ToString();
		playerNameFirst = rowDict ["name_use"].ToString();
		playerNameLast = rowDict ["name_last"].ToString();

        alternateNumber = "";
        alternatePosition = "";

		if (rowDict ["primary_position"] != null && rowDict ["primary_position"].ToString().Length > 0) {
			short posId = 0;
			Int16.TryParse (rowDict ["primary_position"].ToString (), out posId);
			primaryPositionIdentifier = (ePlayPositionIdentifier)posId;
		}
			
		string throwsStr = "R";
		if (rowDict.ContainsKey ("throws") && rowDict ["throws"] != null) {
			throwsStr = rowDict ["throws"].ToString ();
			if (throwsStr.ToUpper ().Equals ("L")) {
				leftHandedThrower = true;
			}
		}

		string jerseyStr = rowDict ["jersey_number"].ToString ();

		//Empty jersey string, really?
		//if (jerseyStr.Length > 0) {
		//	jerseyNumber = Int32.Parse (rowDict ["jersey_number"].ToString ());
		//} else {

			Dictionary<string,object> playerTeamsDict = playerDigestDict["player_teams"] as Dictionary<string,object>;
			Dictionary<string,object> queryResultsTeamDict = playerTeamsDict["queryResults"] as Dictionary<string,object>;

			if (queryResultsTeamDict.ContainsKey ("row") && queryResultsTeamDict ["row"] != null) {
				List<object> rowList = queryResultsTeamDict ["row"] as List<object>;

				if (rowList != null) {
					foreach (Dictionary<string,object> rowListItem in rowList) {
						if (rowListItem.ContainsKey ("hitting_season") && rowListItem ["hitting_season"] != null) {
							string hittingSeasonStr = rowListItem ["hitting_season"].ToString ();
							if (hittingSeasonStr.Equals ("2016") || hittingSeasonStr.Equals ("2015")) {
								if (rowListItem ["jersey_number"] != null && rowListItem ["jersey_number"].ToString ().Length > 0) {
									jerseyStr = rowListItem ["jersey_number"].ToString ();
									Int32.TryParse (jerseyStr, out jerseyNumber);
									break;
								}
							}
						}
					}
				}
			}
		//}
			
		positionIdentifier = positionIdentifierToUse;

		//If the player isn't a fielder (i.e., the batter currently at the plate), use their primary position identifier
		if (!IsFielder()) {
			positionIdentifier = primaryPositionIdentifier;
		}

        dataLoaded = true;
    }

	public string GetPlayPositionIdentifierAbbrv(ePlayPositionIdentifier playPositionIdentifier) {
		string str = "";

		if (playPositionIdentifier == ePlayPositionIdentifier.Pitcher) {
			str = "P";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.Catcher) {
			str = "C";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.FirstBaseman) {
			str = "1B";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.SecondBaseman) {
			str = "2B";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.ThirdBaseman) {
			str = "3B";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.Shortstop) {
			str = "SS";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.LeftFielder) {
			str = "LF";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.CenterFielder) {
			str = "CF";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.RightFielder) {
			str = "RF";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.Batter) {
			str = "Batter";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.FirstBaseRunner) {
			str = "Runner (1st)";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.SecondBaseRunner) {
			str = "Runner (2nd)";
		} else if (playPositionIdentifier == ePlayPositionIdentifier.ThirdBaseRunner) {
			str = "Runner (3rd)";
		}

		return str;
	}

	public string GetPlayPositionIdentifierAbbrv() {
		return GetPlayPositionIdentifierAbbrv(positionIdentifier);
	}

	public bool IsFielder() {
		return IsFielder(positionIdentifier);
	}

	public static bool IsFielder(ePlayPositionIdentifier posId) {
		bool toReturn = false;

		if (posId == ePlayPositionIdentifier.Pitcher) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.Catcher) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.FirstBaseman) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.SecondBaseman) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.ThirdBaseman) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.Shortstop) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.LeftFielder) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.CenterFielder) {
			toReturn = true;
		} else if (posId == ePlayPositionIdentifier.RightFielder) {
			toReturn = true;
		}

		return toReturn;
	}
}

/*

/*
{  
	"player_digest":{  
		"player_relation":{  
			"queryResults":{  
				"created":"2016-04-15T15:21:30",
				"totalSize":"0"
			}
		},
		"copyRight":" Copyright 2016 MLB Advanced Media, L.P.  Use of any content on this page acknowledges agreement to the terms posted here http://gdx.mlb.com/components/copyright.txt  ",
		"player":{  
			"queryResults":{  
				"created":"2016-04-15T14:02:51",
				"totalSize":"1",
				"row":{  
					"name_first_last":"Ross Stripling",
					"name_prefix":"",
					"birth_country":"USA",
					"weight":"210",
					"birth_state":"PA",
					"draft_round":"Round 5",
					"player":"Stripling, Ross",
					"draft_year":"2012",
					"last_played":"2016-04-14T00:00:00",
					"college":"Texas A&M",
					"height_inches":"3",
					"name_middle":"Ross",
					"name_last_first_html":"Stripling, Ross",
					"death_country":"",
					"jersey_number":"",
					"name_pronunciation":"",
					"death_state":"",
					"bats":"R",
					"name_first":"Thomas",
					"age":"26",
					"height_feet":"6",
					"gender":"M",
					"birth_city":"Bluebell",
					"pro_debut_date":"2016-04-08T00:00:00",
					"name_roster_html":"Stripling",
					"name_nick":"",
					"draft_team_abbrev":"LAD",
					"death_date":"",
					"primary_position":"1",
					"name_matrilineal":"",
					"birth_date":"1989-11-23T00:00:00",
					"throws":"R",
					"death_city":"",
					"name_first_last_html":"Ross Stripling",
					"name_roster":"Stripling",
					"primary_position_txt":"P",
					"twitter_id":"",
					"high_school":"Carroll, Southlake, TX",
					"name_use":"Ross",
					"name_title":"",
					"player_id":"548389",
					"name_last":"Stripling",
					"primary_stat_type":"pitching",
					"active_sw":"Y",
					"primary_sport_code":"min"
				}
			}
		},
		"player_teams":{  
			"queryResults":{  
				"created":"2016-04-15T15:21:30",
				"totalSize":"7",
				"row":[  
					{  
						"season_state":"inseason",
						"hitting_season":"2016",
						"sport_full":"Major League Baseball",
						"org":"Dodgers",
						"sport_code":"mlb",
						"org_short":"LA Dodgers",
						"jersey_number":"42",
						"end_date":"",
						"team_brief":"Dodgers",
						"forty_man_sw":"Y",
						"sport_id":"1",
						"league_short":"National",
						"org_full":"Los Angeles Dodgers",
						"status_code":"A",
						"league_full":"National League",
						"primary_position":"P",
						"team_abbrev":"LAD",
						"status":"Active",
						"org_abbrev":"LAD",
						"league_id":"104",
						"class":"MLB",
						"sport":"MLB",
						"team_short":"LA Dodgers",
						"team":"Los Angeles Dodgers",
						"league":"NL",
						"fielding_season":"2016",
						"org_id":"119",
						"class_id":"1",
						"league_season":"2016",
						"pitching_season":"2016",
						"sport_short":"",
						"status_date":"2016-04-03T00:00:00",
						"player_id":"548389",
						"current_sw":"Y",
						"primary_stat_type":"pitching",
						"team_id":"119",
						"start_date":"2015-11-20T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"",
						"sport_full":"Triple-A",
						"org":"Dodgers",
						"sport_code":"aaa",
						"org_short":"LA Dodgers",
						"jersey_number":"",
						"end_date":"2016-04-03T00:00:00",
						"team_brief":"Dodgers",
						"forty_man_sw":"N",
						"sport_id":"11",
						"league_short":"Pacific Coast",
						"org_full":"Los Angeles Dodgers",
						"status_code":"ASG",
						"league_full":"Pacific Coast League",
						"primary_position":"P",
						"team_abbrev":"OKC",
						"status":"Assigned to New Team/Level",
						"org_abbrev":"LAD",
						"league_id":"112",
						"class":"MiLB",
						"sport":"AAA",
						"team_short":"Okla. City",
						"team":"Oklahoma City Dodgers",
						"league":"PCL",
						"fielding_season":"",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2015",
						"pitching_season":"",
						"sport_short":"class AAA",
						"status_date":"2016-04-03T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"238",
						"start_date":"2016-03-17T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"2015",
						"sport_full":"Double-A",
						"org":"Dodgers",
						"sport_code":"aax",
						"org_short":"LA Dodgers",
						"jersey_number":"21",
						"end_date":"2015-11-20T00:00:00",
						"team_brief":"Drillers",
						"forty_man_sw":"N",
						"sport_id":"12",
						"league_short":"Texas",
						"org_full":"Los Angeles Dodgers",
						"status_code":"ASG",
						"league_full":"Texas League",
						"primary_position":"P",
						"team_abbrev":"TUL",
						"status":"Assigned to New Team/Level",
						"org_abbrev":"LAD",
						"league_id":"109",
						"class":"MiLB",
						"sport":"AA",
						"team_short":"Tulsa",
						"team":"Tulsa Drillers",
						"league":"TEX",
						"fielding_season":"2015",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2015",
						"pitching_season":"2015",
						"sport_short":"class AA",
						"status_date":"2015-11-20T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"260",
						"start_date":"2015-06-16T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"",
						"sport_full":"Class A",
						"org":"Dodgers",
						"sport_code":"afx",
						"org_short":"LA Dodgers",
						"jersey_number":"37",
						"end_date":"2015-06-16T00:00:00",
						"team_brief":"Loons",
						"forty_man_sw":"N",
						"sport_id":"14",
						"league_short":"Midwest",
						"org_full":"Los Angeles Dodgers",
						"status_code":"ASG",
						"league_full":"Midwest League",
						"primary_position":"P",
						"team_abbrev":"GL",
						"status":"Assigned to New Team/Level",
						"org_abbrev":"LAD",
						"league_id":"118",
						"class":"MiLB",
						"sport":"A(Full)",
						"team_short":"Great Lakes",
						"team":"Great Lakes Loons",
						"league":"MID",
						"fielding_season":"2015",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2015",
						"pitching_season":"2015",
						"sport_short":"class A (Full)",
						"status_date":"2015-06-16T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"456",
						"start_date":"2015-06-10T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"2013",
						"sport_full":"Double-A",
						"org":"Dodgers",
						"sport_code":"aax",
						"org_short":"LA Dodgers",
						"jersey_number":"44",
						"end_date":"2014-09-17T00:00:00",
						"team_brief":"Lookouts",
						"forty_man_sw":"Y",
						"sport_id":"12",
						"league_short":"Southern",
						"org_full":"Los Angeles Dodgers",
						"status_code":"A",
						"league_full":"Southern League",
						"primary_position":"P",
						"team_abbrev":"CHA",
						"status":"Active",
						"org_abbrev":"LAD",
						"league_id":"111",
						"class":"MiLB",
						"sport":"AA",
						"team_short":"Chattanooga",
						"team":"Chattanooga Lookouts",
						"league":"SOU",
						"fielding_season":"2013",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2014",
						"pitching_season":"2013",
						"sport_short":"class AA",
						"status_date":"2014-09-17T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"498",
						"start_date":"2013-05-07T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"",
						"sport_full":"Class A Advanced",
						"org":"Dodgers",
						"sport_code":"afa",
						"org_short":"LA Dodgers",
						"jersey_number":"17",
						"end_date":"2013-05-07T00:00:00",
						"team_brief":"Quakes",
						"forty_man_sw":"N",
						"sport_id":"13",
						"league_short":"California",
						"org_full":"Los Angeles Dodgers",
						"status_code":"ASG",
						"league_full":"California League",
						"primary_position":"P",
						"team_abbrev":"RC",
						"status":"Assigned to New Team/Level",
						"org_abbrev":"LAD",
						"league_id":"110",
						"class":"MiLB",
						"sport":"A(Adv)",
						"team_short":"Rancho Cucamonga",
						"team":"Rancho Cucamonga Quakes",
						"league":"CAL",
						"fielding_season":"2013",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2013",
						"pitching_season":"2013",
						"sport_short":"class A (Adv)",
						"status_date":"2013-05-07T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"526",
						"start_date":"2013-03-28T00:00:00"
					},
					{  
						"season_state":"offseason",
						"hitting_season":"",
						"sport_full":"Rookie",
						"org":"Dodgers",
						"sport_code":"rok",
						"org_short":"LA Dodgers",
						"jersey_number":"36",
						"end_date":"2013-03-28T00:00:00",
						"team_brief":"Raptors",
						"forty_man_sw":"N",
						"sport_id":"16",
						"league_short":"Pioneer",
						"org_full":"Los Angeles Dodgers",
						"status_code":"ASG",
						"league_full":"Pioneer League",
						"primary_position":"P",
						"team_abbrev":"OGD",
						"status":"Assigned to New Team/Level",
						"org_abbrev":"LAD",
						"league_id":"128",
						"class":"MiLB",
						"sport":"ROK",
						"team_short":"Ogden",
						"team":"Ogden Raptors",
						"league":"PIO",
						"fielding_season":"2012",
						"org_id":"119",
						"class_id":"7",
						"league_season":"2012",
						"pitching_season":"2012",
						"sport_short":"class ROK",
						"status_date":"2013-03-28T00:00:00",
						"player_id":"548389",
						"current_sw":"N",
						"primary_stat_type":"pitching",
						"team_id":"530",
						"start_date":"2012-06-19T00:00:00"
					}
				]
			}
		}
	}
}

{
   "player_digest": {
      "player_relation": {
         "queryResults": {
            "created": "2016-02-06T21:50:34",
            "totalSize": "1",
            "row": {
               "player": "Trout, Mike",
               "relative_first_last": "Jeff Trout",
               "relative_has_stats": "N",
               "relation": "son of",
               "player_first_last": "Mike Trout",
               "relative_id": "565367",
               "player_id": "545361",
               "relative": "Trout, Jeff"
            }
         }
      },
      "copyRight": " Copyright 2016 MLB Advanced Media, L.P.  Use of any content on this page acknowledges agreement to the terms posted here http://gdx.mlb.com/components/copyright.txt  ",
      "player": {
         "queryResults": {
            "created": "2016-02-06T20:24:50",
            "totalSize": "1",
            "row": {
               "name_first_last": "Mike Trout",
               "name_prefix": "",
               "birth_country": "USA",
               "weight": "235",
               "birth_state": "NJ",
               "draft_round": "Round 1",
               "player": "Trout, Mike",
               "draft_year": "2009",
               "last_played": "2015-10-04T00:00:00",
               "college": "",
               "height_inches": "2",
               "name_middle": "Nelson",
               "name_last_first_html": "Trout, Mike",
               "death_country": "",
               "jersey_number": "27",
               "name_pronunciation": "",
               "death_state": "",
               "bats": "R",
               "name_first": "Michael",
               "age": "24",
               "height_feet": "6",
               "gender": "M",
               "birth_city": "Vineland",
               "pro_debut_date": "2011-07-08T00:00:00",
               "name_roster_html": "Trout",
               "name_nick": "The Millville Meteor",
               "draft_team_abbrev": "LAA",
               "death_date": "",
               "primary_position": "8",
               "name_matrilineal": "",
               "birth_date": "1991-08-07T00:00:00",
               "throws": "R",
               "death_city": "",
               "name_first_last_html": "Mike Trout",
               "name_roster": "Trout",
               "primary_position_txt": "CF",
               "twitter_id": "@MikeTrout",
               "high_school": "Millville, NJ",
               "name_use": "Mike",
               "name_title": "",
               "player_id": "545361",
               "name_last": "Trout",
               "primary_stat_type": "hitting",
               "active_sw": "Y",
               "primary_sport_code": "mlb"
            }
         }
      },
      "player_teams": {
         "queryResults": {
            "created": "2016-02-06T21:50:34",
            "totalSize": "8",
            "row": [
               {
                  "season_state": "postseason",
                  "hitting_season": "2015",
                  "sport_full": "Major League Baseball",
                  "org": "Angels",
                  "sport_code": "mlb",
                  "org_short": "LA Angels",
                  "jersey_number": "27",
                  "end_date": "",
                  "team_brief": "Angels",
                  "forty_man_sw": "Y",
                  "sport_id": "1",
                  "league_short": "American",
                  "org_full": "Los Angeles Angels",
                  "status_code": "A",
                  "league_full": "American League",
                  "primary_position": "CF",
                  "team_abbrev": "LAA",
                  "status": "Active",
                  "org_abbrev": "LAA",
                  "league_id": "103",
                  "sport": "MLB",
                  "team_short": "LA Angels",
                  "team": "Los Angeles Angels",
                  "league": "AL",
                  "fielding_season": "2015",
                  "org_id": "108",
                  "class_id": "1",
                  "league_season": "2015",
                  "pitching_season": "",
                  "sport_short": "",
                  "status_date": "2012-04-28T00:00:00",
                  "player_id": "545361",
                  "current_sw": "Y",
                  "primary_stat_type": "hitting",
                  "team_id": "108",
                  "start_date": "2011-07-08T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2012",
                  "sport_full": "Triple-A",
                  "org": "Angels",
                  "sport_code": "aaa",
                  "org_short": "LA Angels",
                  "jersey_number": "23",
                  "end_date": "2012-04-28T00:00:00",
                  "team_brief": "Bees",
                  "forty_man_sw": "N",
                  "sport_id": "11",
                  "league_short": "Pacific Coast",
                  "org_full": "Los Angeles Angels",
                  "status_code": "ASG",
                  "league_full": "Pacific Coast League",
                  "primary_position": "CF",
                  "team_abbrev": "SLC",
                  "status": "Assigned to New Team/Level",
                  "org_abbrev": "LAA",
                  "league_id": "112",
                  "sport": "AAA",
                  "team_short": "Salt Lake",
                  "team": "Salt Lake Bees",
                  "league": "PCL",
                  "fielding_season": "2012",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2012",
                  "pitching_season": "",
                  "sport_short": "class AAA",
                  "status_date": "2012-04-28T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "561",
                  "start_date": "2012-03-30T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2011",
                  "sport_full": "Offseason Leagues",
                  "org": "Angels",
                  "sport_code": "win",
                  "org_short": "LA Angels",
                  "jersey_number": "27",
                  "end_date": "2011-11-20T00:00:00",
                  "team_brief": "Scorpions",
                  "forty_man_sw": "Y",
                  "sport_id": "17",
                  "league_short": "Arizona Fall",
                  "org_full": "Los Angeles Angels",
                  "status_code": "A",
                  "league_full": "Arizona Fall League",
                  "primary_position": "OF",
                  "team_abbrev": "SCO",
                  "status": "Active",
                  "org_abbrev": "LAA",
                  "league_id": "119",
                  "sport": "WIN",
                  "team_short": "Scottsdale",
                  "team": "Scottsdale Scorpions",
                  "league": "AFL",
                  "fielding_season": "2011",
                  "org_id": "108",
                  "class_id": "17",
                  "league_season": "2011",
                  "pitching_season": "",
                  "sport_short": "",
                  "status_date": "2011-08-30T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "544",
                  "start_date": "2011-08-30T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2011",
                  "sport_full": "Double-A",
                  "org": "Angels",
                  "sport_code": "aax",
                  "org_short": "LA Angels",
                  "jersey_number": "23",
                  "end_date": "2011-08-19T00:00:00",
                  "team_brief": "Travelers",
                  "forty_man_sw": "N",
                  "sport_id": "12",
                  "league_short": "Texas",
                  "org_full": "Los Angeles Angels",
                  "status_code": "ASG",
                  "league_full": "Texas League",
                  "primary_position": "CF",
                  "team_abbrev": "ARK",
                  "status": "Assigned to New Team/Level",
                  "org_abbrev": "LAA",
                  "league_id": "109",
                  "sport": "AA",
                  "team_short": "Arkansas",
                  "team": "Arkansas Travelers",
                  "league": "TEX",
                  "fielding_season": "2011",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2011",
                  "pitching_season": "",
                  "sport_short": "class AA",
                  "status_date": "2011-08-19T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "574",
                  "start_date": "2011-08-01T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "",
                  "sport_full": "Class A Advanced",
                  "org": "Angels",
                  "sport_code": "afa",
                  "org_short": "LA Angels",
                  "jersey_number": "23",
                  "end_date": "2011-04-03T00:00:00",
                  "team_brief": "66ers",
                  "forty_man_sw": "N",
                  "sport_id": "13",
                  "league_short": "California",
                  "org_full": "Los Angeles Angels",
                  "status_code": "ASG",
                  "league_full": "California League",
                  "primary_position": "CF",
                  "team_abbrev": "IE",
                  "status": "Assigned to New Team/Level",
                  "org_abbrev": "LAA",
                  "league_id": "110",
                  "sport": "A(Adv)",
                  "team_short": "Inland Empire",
                  "team": "Inland Empire 66ers",
                  "league": "CAL",
                  "fielding_season": "",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2010",
                  "pitching_season": "",
                  "sport_short": "class A (Adv)",
                  "status_date": "2011-04-03T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "401",
                  "start_date": "2010-09-30T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2010",
                  "sport_full": "Class A Advanced",
                  "org": "Angels",
                  "sport_code": "afa",
                  "org_short": "LA Angels",
                  "jersey_number": "23",
                  "end_date": "2010-09-27T00:00:00",
                  "team_brief": "Quakes",
                  "forty_man_sw": "Y",
                  "sport_id": "13",
                  "league_short": "California",
                  "org_full": "Los Angeles Angels",
                  "status_code": "A",
                  "league_full": "California League",
                  "primary_position": "CF",
                  "team_abbrev": "RC",
                  "status": "Active",
                  "org_abbrev": "LAA",
                  "league_id": "110",
                  "sport": "A(Adv)",
                  "team_short": "Rancho Cucamonga",
                  "team": "Rancho Cucamonga Quakes",
                  "league": "CAL",
                  "fielding_season": "2010",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2010",
                  "pitching_season": "",
                  "sport_short": "class A (Adv)",
                  "status_date": "2010-07-13T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "526",
                  "start_date": "2010-07-13T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2010",
                  "sport_full": "Class A",
                  "org": "Angels",
                  "sport_code": "afx",
                  "org_short": "LA Angels",
                  "jersey_number": "20",
                  "end_date": "2010-07-12T00:00:00",
                  "team_brief": "Kernels",
                  "forty_man_sw": "N",
                  "sport_id": "14",
                  "league_short": "Midwest",
                  "org_full": "Los Angeles Angels",
                  "status_code": "ASG",
                  "league_full": "Midwest League",
                  "primary_position": "CF",
                  "team_abbrev": "CR",
                  "status": "Assigned to New Team/Level",
                  "org_abbrev": "LAA",
                  "league_id": "118",
                  "sport": "A(Full)",
                  "team_short": "Cedar Rapids",
                  "team": "Cedar Rapids Kernels",
                  "league": "MID",
                  "fielding_season": "2010",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2010",
                  "pitching_season": "",
                  "sport_short": "class A (Full)",
                  "status_date": "2010-07-12T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "492",
                  "start_date": "2009-09-01T00:00:00"
               },
               {
                  "season_state": "offseason",
                  "hitting_season": "2009",
                  "sport_full": "Rookie",
                  "org": "Angels",
                  "sport_code": "rok",
                  "org_short": "LA Angels",
                  "jersey_number": "68",
                  "end_date": "2009-08-31T00:00:00",
                  "team_brief": "AZL Angels",
                  "forty_man_sw": "N",
                  "sport_id": "16",
                  "league_short": "Arizona",
                  "org_full": "Los Angeles Angels",
                  "status_code": "ASG",
                  "league_full": "Arizona League",
                  "primary_position": "CF",
                  "team_abbrev": "ANG",
                  "status": "Assigned to New Team/Level",
                  "org_abbrev": "LAA",
                  "league_id": "121",
                  "sport": "ROK",
                  "team_short": "AZL Angels",
                  "team": "AZL Angels",
                  "league": "AZL",
                  "fielding_season": "2009",
                  "org_id": "108",
                  "class_id": "7",
                  "league_season": "2009",
                  "pitching_season": "",
                  "sport_short": "class ROK",
                  "status_date": "2009-08-31T00:00:00",
                  "player_id": "545361",
                  "current_sw": "N",
                  "primary_stat_type": "hitting",
                  "team_id": "404",
                  "start_date": "2009-07-05T00:00:00"
               }
            ]
         }
      }
   }
}

*/


    
