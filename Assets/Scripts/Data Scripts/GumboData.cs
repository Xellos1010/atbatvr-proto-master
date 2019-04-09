using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using System.Linq;
using System;

public enum eInningFrame {
	Top,
	Bottom
}

public class GumboData {
    public string m_gameId;

	public List<GumboPlayData> allPlays;
	public List<GumboPlayData> allLivePlays;
    private DateTime epoch;

    public bool dataLoaded;

    public GumboData(string gameId, string jsonString) {

		allPlays = new List<GumboPlayData> ();
		allLivePlays = new List<GumboPlayData> ();

        string dateFormat = "yyyy-MM-ddThh:mm:ss.fff";
        epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime();

        m_gameId = gameId;

		Dictionary<string,object> dict = MiniJson.Deserialize(jsonString) as Dictionary<string,object>;
		Dictionary<string,object> liveData = dict["liveData"] as Dictionary<string,object>;
		Dictionary<string,object> plays = liveData["plays"] as Dictionary<string,object>;
		List<object> allPlaysList = plays["allPlays"] as List<object>;

		if (allPlaysList != null) {
			for (int i = 0; i < allPlaysList.Count; i++) {
				Dictionary<string,object> gumboDataItem = allPlaysList [i] as Dictionary<string,object>;
				Dictionary<string,object> gumboDataItemAbout = gumboDataItem ["about"] as Dictionary<string,object>;
				string halfInningString = gumboDataItemAbout ["halfInning"] as string;
				string inningString = gumboDataItemAbout ["inning"] as string;

				Dictionary<string,object> gumboDataItemResult = gumboDataItem ["result"] as Dictionary<string,object>;
				string descriptionString = gumboDataItemResult ["description"] as string;

				string playEventsKey = "playEvents";

				if (!gumboDataItem.ContainsKey (playEventsKey)) {
					playEventsKey = "pitchEvents";
				}

				Dictionary<string,object> gumboDataItemPitchEvents = gumboDataItem [playEventsKey] as Dictionary<string,object>;
				List<object> gumboDataItemPitchEventsList = null;
				Dictionary<string,object> gumboDataItemPitchEvent = null;

				if (gumboDataItemPitchEvents == null) {
					gumboDataItemPitchEventsList = gumboDataItem [playEventsKey] as List<object>;
				}

                for (int j = 0; j < gumboDataItemPitchEventsList.Count; j++) {
                    Dictionary<string,object> pitchEvent = gumboDataItemPitchEventsList[j] as Dictionary<string,object>;

                    GumboPlayData gumboPlayData = new GumboPlayData ();
                    gumboPlayData.gameId = m_gameId;
                    gumboPlayData.inning = int.Parse(inningString);
                    gumboPlayData.inningFrame = halfInningString.Equals ("bottom") ? eInningFrame.Bottom : eInningFrame.Top;

                    if (pitchEvent != null && pitchEvent.ContainsKey("play_guid")) { 
                        gumboPlayData.playId = pitchEvent ["play_guid"] as string;

                        string pitchDescriptionString = "";
                        if (pitchEvent.ContainsKey("pitchNum")) {
                            pitchDescriptionString = "(Pitch " + pitchEvent ["pitchNum"] as string;
                        }

                        if (pitchEvent.ContainsKey("details")) {
                            Dictionary<string,object> pitchDetails = pitchEvent ["details"] as Dictionary<string,object>;
                            if (pitchDetails != null) {
                                if (pitchDetails.ContainsKey("displayName")) {
                                    pitchDescriptionString += " - " + pitchDetails ["displayName"] as string;
                                }
                                if (pitchDetails.ContainsKey("description")) {
                                    pitchDescriptionString += " - " + pitchDetails ["description"] as string; 
                                }
                            }
                        }

                        pitchDescriptionString += ")";

                        gumboPlayData.description = descriptionString + pitchDescriptionString;
                    }

                    if (j == gumboDataItemPitchEventsList.Count - 1) {
                        gumboPlayData.finalPitchOfAtBat = true;
                    }

                    allPlays.Add (gumboPlayData);
                    allLivePlays.Add (gumboPlayData);
                }
			}
		}
			
        dataLoaded = true;
    }

	public GumboPlayData GetGumboPlayData(string playId) {
		GumboPlayData gumboPlayDataFound = null;

		foreach (GumboPlayData gumboPlayData in allPlays) {
			if (gumboPlayData.playId != null && gumboPlayData.playId.Equals(playId)) {
				gumboPlayDataFound = gumboPlayData;
				break;
			}
		}
		return gumboPlayDataFound;
	}
}


/*

{
   "about": {
	  "atBatIndex": "10",
	  "halfInning": "bottom",
	  "inning": "2",
	  "startTfs": "20150517_180022",
	  "endTfs": "20150517_180234",
	  "isComplete": true,
	  "isScoringPlay": false
   },
   "count": {
	  "balls": "2",
	  "strikes": "2",
	  "outs": "1"
   },
   "matchup": {
	  "batter": "448801",
	  "pitcher": "572070",
	  "batterHotColdZones": null,
	  "splits": null
   },
   "result": {
	  "type": "atBat",
	  "event": "Lineout",
	  "eventEs": "Línea de Out",
	  "brief": "",
	  "description": "Chris Davis lines out to right fielder Kole Calhoun.  ",
	  "descriptionEs": "Chris Davis batea línea de out a jardinero derecho Kole Calhoun.  ",
	  "rbi": 0
   },
   "runnerIndex": null,
   "runners": [
	  {
		 "movement": {
			"start": "1B",
			"end": "2B"
		 },
		 "details": {
			"event": "Wild Pitch",
			"eventEs": "Lanzamiento Descontrolado",
			"runner": "ID430321",
			"isScoringEvent": false,
			"rbi": false,
			"earned": false
		 }
	  }
   ],
   "pitchEvents": [
	  {
		 "count": {
			"strikes": 1
		 },
		 "details": {
			"call": "S",
			"description": "Foul",
			"ballColor": "rgba(170, 21, 11, 1.0)",
			"isInPlay": false,
			"trailColor": "rgba(152, 0, 101, 1.0)",
			"type": "S"
		 },
		 "stats": {
			"startSpeed": "95.4",
			"endSpeed": "88.4",
			"nastyFactor": "63",
			"strikezoneTop": "3.64",
			"strikezoneBottom": "1.66",
			"coordinates": {
			   "aX": "2.394",
			   "aY": "29.506",
			   "aZ": "-15.963",
			   "pfxX": "1.21",
			   "pfxZ": "8.12",
			   "pX": "0.669",
			   "pZ": "3.077",
			   "vX0": "6.817",
			   "vY0": "-139.595",
			   "vZ0": "-5.571",
			   "x": "91.5",
			   "y": "155.7",
			   "x0": "-1.955",
			   "y0": "50.0",
			   "z0": "6.139"
			},
			"breaks": {
			   "breakAngle": "-12.4",
			   "breakLength": "3.6",
			   "breakX": null,
			   "breakY": "23.8",
			   "breakZ": null
			}
		 },
		 "id": "83",
		 "index": 1,
		 "sportvisionID": "150517_140110",
		 "play_guid": "971e3776-6d04-4c0e-8489-6c29e6137d2a",
		 "tfs": "20150517_180031",
		 "endTfs": "20150517_180055",
		 "isPitch": true
	  },
	  {
		 "count": {
			"strikes": 2
		 },
		 "details": {
			"call": "S",
			"description": "Foul",
			"ballColor": "rgba(170, 21, 11, 1.0)",
			"isInPlay": false,
			"trailColor": "rgba(152, 0, 101, 1.0)",
			"type": "S"
		 },
		 "stats": {
			"startSpeed": "94.6",
			"endSpeed": "88.2",
			"nastyFactor": "37",
			"strikezoneTop": "3.64",
			"strikezoneBottom": "1.66",
			"coordinates": {
			   "aX": "5.861",
			   "aY": "26.917",
			   "aZ": "-15.836",
			   "pfxX": "2.98",
			   "pfxZ": "8.26",
			   "pX": "0.353",
			   "pZ": "3.147",
			   "vX0": "4.951",
			   "vY0": "-138.523",
			   "vZ0": "-5.29",
			   "x": "103.54",
			   "y": "153.81",
			   "x0": "-1.835",
			   "y0": "50.0",
			   "z0": "6.117"
			},
			"breaks": {
			   "breakAngle": "-21.9",
			   "breakLength": "3.7",
			   "breakX": null,
			   "breakY": "23.9",
			   "breakZ": null
			}
		 },
		 "id": "84",
		 "index": 2,
		 "sportvisionID": "150517_140133",
		 "play_guid": "29ab8168-f15e-40f7-808b-d9881e3bea67",
		 "tfs": "20150517_180055",
		 "endTfs": "20150517_180122",
		 "isPitch": true
	  },
	  {
		 "count": {
			"balls": 1,
			"strikes": 2
		 },
		 "details": {
			"call": "B",
			"description": "Ball",
			"ballColor": "rgba(39, 161, 39, 1.0)",
			"isInPlay": false,
			"trailColor": "rgba( 0, 0, 254, 1.0)",
			"type": "B"
		 },
		 "stats": {
			"startSpeed": "86.7",
			"endSpeed": "80.8",
			"nastyFactor": "18",
			"strikezoneTop": "3.64",
			"strikezoneBottom": "1.66",
			"coordinates": {
			   "aX": "5.947",
			   "aY": "26.291",
			   "aZ": "-36.648",
			   "pfxX": "3.66",
			   "pfxZ": "-2.8",
			   "pX": "1.497",
			   "pZ": "0.825",
			   "vX0": "7.349",
			   "vY0": "-126.844",
			   "vZ0": "-6.095",
			   "x": "59.94",
			   "y": "216.51",
			   "x0": "-1.914",
			   "y0": "50.0",
			   "z0": "6.186"
			},
			"breaks": {
			   "breakAngle": "-10.8",
			   "breakLength": "9.6",
			   "breakX": null,
			   "breakY": "23.9",
			   "breakZ": null
			}
		 },
		 "id": "85",
		 "index": 3,
		 "sportvisionID": "150517_140201",
		 "play_guid": "32d065e7-a58a-4582-92f6-506f4371fb46",
		 "tfs": "20150517_180122",
		 "endTfs": "20150517_180156",
		 "isPitch": true
	  },
	  {
		 "count": {
			"balls": 2,
			"strikes": 2
		 },
		 "details": {
			"call": "B",
			"description": "Ball",
			"ballColor": "rgba(39, 161, 39, 1.0)",
			"isInPlay": false,
			"trailColor": "rgba( 0, 0, 254, 1.0)",
			"type": "B"
		 },
		 "stats": {
			"startSpeed": "86.5",
			"endSpeed": "81.7",
			"nastyFactor": "16",
			"strikezoneTop": "3.64",
			"strikezoneBottom": "1.66",
			"coordinates": {
			   "aX": "4.711",
			   "aY": "24.135",
			   "aZ": "-36.935",
			   "pfxX": "2.9",
			   "pfxZ": "-2.98",
			   "pX": "1.424",
			   "pZ": "-1.506",
			   "vX0": "7.075",
			   "vY0": "-126.211",
			   "vZ0": "-11.278",
			   "x": "62.72",
			   "y": "0.00",
			   "x0": "-1.785",
			   "y0": "50.0",
			   "z0": "5.967"
			},
			"breaks": {
			   "breakAngle": "-8.3",
			   "breakLength": "9.8",
			   "breakX": null,
			   "breakY": "23.9",
			   "breakZ": null
			}
		 },
		 "id": "86",
		 "index": 4,
		 "sportvisionID": "150517_140229",
		 "play_guid": "a2397410-ca83-41fa-9e93-5de81c56b96b",
		 "tfs": "20150517_180156",
		 "endTfs": "20150517_180206",
		 "isPitch": true
	  },
	  {
		 "count": {
			"balls": "2",
			"strikes": "2",
			"outs": "0",
			"pitchNum": "4",
			"colorCommentary": ""
		 },
		 "details": {
			"description": "With Chris Davis batting, wild pitch by Garrett Richards, Delmon Young to 2nd.  ",
			"descriptionEs": "Con Chris Davis bateando, lanzamiento desviado de Garrett Richards, Delmon Young a 2da.  ",
			"event": "Wild Pitch",
			"eventEs": "Lanzamiento Descontrolado",
			"player": "430321",
			"scored": false,
			"homeScore": "0",
			"awayScore": "0"
		 },
		 "runnerIndex": [
			0
		 ],
		 "name": "action",
		 "isPitch": false,
		 "tfs": "20150517_180206",
		 "endTfs": "20150517_180234"
	  },
	  {
		 "count": {
			"balls": 2,
			"strikes": 3
		 },
		 "details": {
			"call": "X",
			"description": "In play, out(s)",
			"ballColor": "rgba(26, 86, 190, 1.0)",
			"isInPlay": true,
			"trailColor": "rgba(152, 0, 101, 1.0)",
			"type": "X"
		 },
		 "stats": {
			"startSpeed": "95.8",
			"endSpeed": "89.4",
			"nastyFactor": "37",
			"strikezoneTop": "3.64",
			"strikezoneBottom": "1.66",
			"coordinates": {
			   "aX": "4.199",
			   "aY": "27.391",
			   "aZ": "-16.946",
			   "pfxX": "2.08",
			   "pfxZ": "7.5",
			   "pX": "0.193",
			   "pZ": "3.045",
			   "vX0": "5.455",
			   "vY0": "-140.28",
			   "vZ0": "-5.25",
			   "x": "109.64",
			   "y": "156.57",
			   "x0": "-2.035",
			   "y0": "50.0",
			   "z0": "6.021"
			},
			"breaks": {
			   "breakAngle": "-16.2",
			   "breakLength": "3.7",
			   "breakX": null,
			   "breakY": "23.9",
			   "breakZ": null
			}
		 },
		 "id": "90",
		 "index": 5,
		 "sportvisionID": "150517_140308",
		 "play_guid": "a09cb48e-e0fe-4267-9a8c-e9f7bf805a8d",
		 "tfs": "20150517_180234",
		 "endTfs": null,
		 "isPitch": true
	  }
   ]
},

*/

public class GumboPlayData {
    public string gameId;
    public double timeCode;
    public string playId;
	public string description;

	public int inning;
	public eInningFrame inningFrame;

    public bool finalPitchOfAtBat = false;

    /*
    "type": "atBat",
    "event": "Pop Out",
    "description": "Kole Calhoun pops out softly to second baseman Steve Pearce.  ",
     */
}

    
