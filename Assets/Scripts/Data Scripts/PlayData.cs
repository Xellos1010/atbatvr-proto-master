using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using System.Linq;
using System;

public class PlayData {
    public string m_gameId;
    public string m_playId;

    public List<PlayEvent> playEventsData;
	public List<SegmentEvent> segmentEventsData;
	public List<StatEvent> statEventsData;
	public List<StatOverlay> statOverlayData;

	public List<string> keyPlayerMlbIds;

	public List<Player> playerLineupData;
	public List<ePlayPositionIdentifier> playerPositionsToDisplay;

    public List<PositionalData> mergedPositionalData;
    public BallPositionalData ballPositionalData;

	public List<DataPoint> ballDataCH;
	public List<DataPoint> ballDataTM;
	public List<DataPoint> ballDataPoly;	//Polynomial equation from play data - hit only 
	public List<DataPoint> combinedBallPositions;

    public List<string> reducedConfidenceData;
    public bool reducedConfidence;

	public bool batterIsRightHanded = true;
    public bool hitToRightField = true;
	public bool rotateCameraClockwise = false;

    private PlayEvent beginOfPlayEvent;
    private PlayEvent ballWasPitchedEvent;
    private PlayEvent ballWasHitEvent;
	private PlayEvent ballHitApexEvent;
    private PlayEvent ballWasFieldedEvent;
	private PlayEvent ballWasReleasedEvent;
	public PlayEvent endOfPlayEvent;

	public SegmentEvent hitSegment;

    private double beginOfPlayTimeStampSeconds = 0;

	private List<float> hitTrajectoryX;
	private List<float> hitTrajectoryY;
	private List<float> hitTrajectoryZ;

	PositionalData firstPositionalData;
	PositionalData lastPositionalData;

    private DateTime epoch;

    public bool dataLoaded;

	private static float FEET_TO_METERS = 0.3047999902464f;
	private static Vector3 FEET_TO_METERS_VECTOR;

	public bool cameraEventsSetup = false;

	private GumboPlayData gumboPlayData;
	private GamePlayData gamePlayData;

	public float spinRate = 1800f;
	public float launchAngle = 15f;
	public float launchVector = 45f;

	public string errorNote;

	public float playStartTime;
	public float playDuration;

	public PlayData(string gameId, string playId, string jsonString, GumboPlayData gumboPlayDataToUse, GamePlayData gamePlayDataToUse) {

        FEET_TO_METERS_VECTOR = new Vector3(FEET_TO_METERS, FEET_TO_METERS, FEET_TO_METERS);
        epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime();

        m_gameId = gameId;
        m_playId = playId;

		gumboPlayData = gumboPlayDataToUse;
		gamePlayData = gamePlayDataToUse;

		errorNote = "";

        Dictionary<string,object> dict = MiniJson.Deserialize(jsonString) as Dictionary<string,object>;

		ProcessPositionalData((List<object>)dict["mergedPositionalData"]);
        ProcessPlayEventsData((List<object>)dict["playEvents"]);

		//We can't rely on the BEGIN_OF_PLAY event to be present...
		//so we wait until we've processed CH data and play events and then update the time using the correct beginOfPlayTimeStampSeconds value
		UpdatePositionalDataTime ();

		Dictionary<string,object> gameEventData = dict["gameEvent"] as Dictionary<string,object>;
		ProcessPlayerLineup(gameEventData);

        Dictionary<string,object> ballData = dict["ballPositionalData"] as Dictionary<string,object>;
        ProcessBallPositionalData((List<object>)ballData["BallPositions"]);

		ProcessSegmentEventsData((List<object>)ballData["Segments"]);

		Dictionary<string,object> measurementsData = dict["measurements"] as Dictionary<string,object>;

		CombineBallPositionData ();

		ProcessStatEvents (measurementsData);

		//playStartTime = SimulationManager.Instance.playStartTime;
		playDuration = endOfPlayEvent.time - beginOfPlayEvent.time;
    }

    private void ProcessPlayEventsData(List<object> data) {

        List<PlayEvent> playEventDataUnsorted = new List<PlayEvent>();

        int count = 0;

        //TODO: This is an unholy mess, will need to be cleaned up later.
        foreach (Dictionary<string,object> playEventItem in data) {

            PlayEvent playEvent = new PlayEvent();
            playEvent.timeStampString = playEventItem["timeStamp"].ToString();

            DateTime timeStampDateTime = DateTime.Parse(playEvent.timeStampString);
            TimeSpan timeStampSpan = (timeStampDateTime.ToUniversalTime() - epoch);
            playEvent.timeStampSeconds = timeStampSpan.TotalSeconds;

            playEvent.playId = playEventItem["playID"].ToString(); //Not sure that this would ever differ from m_playId?
            playEvent.playEventId = playEventItem["playEventID"].ToString();
            playEvent.playEventType = (ePlayEventType)(int.Parse(playEventItem["playEventType"].ToString()));
           
            playEvent.playPositionIdentifier = (ePlayPositionIdentifier)(int.Parse(playEventItem["positionId"].ToString()));

			if (playEvent.playEventType == ePlayEventType.BALL_WAS_RELEASED) {
				PlayEvent customPlayEvent = new PlayEvent ();
				customPlayEvent.playEventType = ePlayEventType.CUSTOM_BEFORE_THROW;
				customPlayEvent.playPositionIdentifier = playEvent.playPositionIdentifier;
				customPlayEvent.timeStampSeconds = playEvent.timeStampSeconds - 0.25f;
				playEventDataUnsorted.Add (customPlayEvent);
			}

            playEventDataUnsorted.Add(playEvent);
        }


		//BEGIN_OF_PLAY
		beginOfPlayEvent = playEventDataUnsorted.Find(x => x.playEventType == ePlayEventType.BEGIN_OF_PLAY);
		if (beginOfPlayEvent == null) {
			string errorString = "BEGIN_OF_PLAY event was not found";
			ScheduleDataManager.instance.playDataError = errorString;
			Debug.LogError (errorString);

			beginOfPlayEvent = new PlayEvent ();
			beginOfPlayEvent.playEventType = ePlayEventType.BEGIN_OF_PLAY;
			beginOfPlayEvent.time = 0;
			beginOfPlayEvent.timeStampSeconds = beginOfPlayTimeStampSeconds; //Use the time from the first CH data point
			playEventDataUnsorted.Insert (0, beginOfPlayEvent);
		} else {
			beginOfPlayTimeStampSeconds = beginOfPlayEvent.timeStampSeconds;
		}
		//Debug.Log ("beginOfPlayTimeStampSeconds: " + beginOfPlayTimeStampSeconds);

		//END_OF_PLAY
		endOfPlayEvent = playEventDataUnsorted.Find(x => x.playEventType == ePlayEventType.END_OF_PLAY);
		if (endOfPlayEvent == null) {
			errorNote += "Missing END_OF_PLAY play event; ";
			string errorString = "END_OF_PLAY event was not found";
			ScheduleDataManager.instance.playDataError = errorString;
			Debug.LogError (errorString);

			endOfPlayEvent = new PlayEvent ();
			endOfPlayEvent.playEventType = ePlayEventType.END_OF_PLAY;
			endOfPlayEvent.time = lastPositionalData.time + 3.0f;
			endOfPlayEvent.timeStampSeconds = lastPositionalData.timeStampSeconds + 3.0f; //Use the time from the last CH data point
			playEventDataUnsorted.Insert (0, endOfPlayEvent);
		}

		//BALL_WAS_PITCHED
		ballWasPitchedEvent = playEventDataUnsorted.Find(x => x.playEventType == ePlayEventType.BALL_WAS_PITCHED);
		if (ballWasPitchedEvent == null) {
			errorNote += "Missing BALL_WAS_PITCHED play event; ";
			string errorString = "BALL_WAS_PITCHED event was not found";
			ScheduleDataManager.instance.playDataError = errorString;
			Debug.LogError (errorString);

			ballWasPitchedEvent = new PlayEvent ();
			ballWasPitchedEvent.playEventType = ePlayEventType.BALL_WAS_PITCHED;
			ballWasPitchedEvent.playPositionIdentifier = ePlayPositionIdentifier.Unknown;
			ballWasPitchedEvent.time = 3.0f;

			if (beginOfPlayEvent != null) {
				ballWasPitchedEvent.timeStampSeconds = beginOfPlayEvent.timeStampSeconds + 3.0f;
			}

			playEventDataUnsorted.Add (ballWasPitchedEvent);
		}

		PlayEvent playEventAboutToBePitched = new PlayEvent ();
		playEventAboutToBePitched.timeStampSeconds = ballWasPitchedEvent.timeStampSeconds - 0.1f;
		playEventAboutToBePitched.playEventType = ePlayEventType.CUSTOM_BEFORE_PITCH;
		playEventAboutToBePitched.playPositionIdentifier = ePlayPositionIdentifier.Pitcher;
		playEventDataUnsorted.Add (playEventAboutToBePitched);

        playEventsData = playEventDataUnsorted.OrderBy(o=>o.timeStampSeconds).ToList();

		for (int i = 0; i < playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			playEvent.time = (float)(playEvent.timeStampSeconds - beginOfPlayTimeStampSeconds);

			if (playEventsData [i].playEventType == ePlayEventType.BALL_WAS_PITCHED) {
				ballWasPitchedEvent = playEvent;
				ballWasPitchedEvent.playPositionIdentifier = ePlayPositionIdentifier.Unknown;
			} else if (playEventsData [i].playEventType == ePlayEventType.BALL_WAS_HIT) {
				ballWasHitEvent = playEvent;
				ballWasHitEvent.playPositionIdentifier = ePlayPositionIdentifier.Unknown;
			} else if (playEventsData [i].playEventType == ePlayEventType.BALL_WAS_FIELDED && ballWasFieldedEvent == null) {
				ballWasFieldedEvent = playEvent;
			} else if (playEventsData [i].playEventType == ePlayEventType.BALL_WAS_RELEASED && ballWasReleasedEvent == null) {	//We only want the first instance of this event
				ballWasReleasedEvent = playEvent;
			} else if (playEventsData [i].playEventType == ePlayEventType.END_OF_PLAY) {
				endOfPlayEvent = playEvent;
			}
			
			//Debug.Log ("playEvent: " + playEvent.playEventType + ", playEvent.time: " + playEvent.time);
		}
			
		//First, try to associate the event with the ball being caught for an out...
		if (gamePlayData.isHit && !gamePlayData.isHomeRun && ballWasFieldedEvent == null) {
			Debug.LogWarning ("gamePlayData.isHit && !gamePlayData.isHomeRun && ballWasFieldedEvent == null");
			ballWasFieldedEvent = playEventDataUnsorted.Find(x => x.playEventType == ePlayEventType.BALL_WAS_CAUGHT_OUT);
			if (ballWasFieldedEvent != null) {
				Debug.LogWarning ("ballWasFieldedEvent now refers to playEvent: " + ballWasFieldedEvent.playEventType);
			}
		}

		//Next, try to tie the event to a throw...
		if (ballWasFieldedEvent == null && ballWasReleasedEvent != null) {
			Debug.LogWarning ("ballWasFieldedEvent == null && ballWasReleasedEvent != null - creating a custom fielded event");
			ballWasFieldedEvent = new PlayEvent ();
			ballWasFieldedEvent.playEventType = ePlayEventType.CUSTOM_BALL_WAS_FIELDED;
			ballWasFieldedEvent.playPositionIdentifier = ballWasReleasedEvent.playPositionIdentifier;
			ballWasFieldedEvent.timeStampSeconds = ballWasReleasedEvent.timeStampSeconds - 2.0f;
			ballWasFieldedEvent.time = ballWasReleasedEvent.time - 2.0f;
			if (ballWasHitEvent != null) {
				if (ballWasFieldedEvent.timeStampSeconds < ballWasHitEvent.timeStampSeconds) {
					ballWasFieldedEvent.timeStampSeconds = ballWasReleasedEvent.timeStampSeconds;
					ballWasFieldedEvent.time = ballWasReleasedEvent.time;
				}
			}
			playEventsData.Add (ballWasFieldedEvent);
			playEventsData = playEventsData.OrderBy (o => o.timeStampSeconds).ToList ();
		}

		//As a last resort, tie the event to the ball being caught...
		if (gamePlayData.isHit && !gamePlayData.isHomeRun && ballWasFieldedEvent == null) {
			Debug.LogWarning ("gamePlayData.isHit && !gamePlayData.isHomeRun && ballWasFieldedEvent == null");
			ballWasFieldedEvent = playEventDataUnsorted.Find(x => x.playEventType == ePlayEventType.BALL_WAS_CAUGHT);
			if (ballWasFieldedEvent != null) {
				Debug.LogWarning ("ballWasFieldedEvent now refers to playEvent: " + ballWasFieldedEvent.playEventType);
			}
		}

		if ((gamePlayData.isHit || gamePlayData.isActualHit) && ballWasHitEvent == null) {
			PlayEvent playEventDefelected = playEventsData.Find (x => x.playEventType == ePlayEventType.TRACKMAN_BALL_WAS_DEFLECTED);
			if (playEventDefelected != null && Mathf.Abs (playEventDefelected.time - 3.4f) <= 0.1f) {
				ballWasHitEvent = playEventDefelected;
				Debug.Log ("BALL_WAS_HIT is missing, found TRACKMAN_BALL_WAS_DEFLECTED event that seems a suitable replacement.");
			}

			if (ballWasHitEvent == null) {
				errorNote += "Missing BALL_WAS_HIT play event; ";
				Debug.LogError ("BALL_WAS_HIT is missing!");
				ballWasHitEvent = new PlayEvent ();
				ballWasHitEvent.playEventType = ePlayEventType.BALL_WAS_HIT;
				ballWasHitEvent.playPositionIdentifier = ePlayPositionIdentifier.Unknown;
				ballWasHitEvent.time = ballWasPitchedEvent.time + 0.4f;
				ballWasHitEvent.timeStampSeconds = ballWasPitchedEvent.timeStampSeconds + 0.4f;
				playEventsData.Add (ballWasHitEvent);
				playEventsData = playEventsData.OrderBy (o => o.timeStampSeconds).ToList ();
			}
		}
    }

	private void ProcessStatEvents(Dictionary<string,object> measurementData) {

		List<object> statisticsList = (List<object>)measurementData ["statistics"] as List<object>;

		List<StatEvent> statEventsDataUnsorted = new List<StatEvent> ();

		//We want to get a list of position identifiers so that we can narrow focus down to the key players
		Dictionary<int,int> fieldingPositionIdentiferDict = new Dictionary<int, int> ();
		Dictionary<int,int> baserunningPositionIdentiferDict = new Dictionary<int, int> ();

		foreach (Dictionary<string,object> statItem in statisticsList) {
			StatEvent statEvent = new StatEvent ();

			long timeStamp = 0;
			string timeString = statItem ["time"].ToString ();
			timeString = timeString.Substring (0, 13);
			long.TryParse (timeString, out timeStamp);
			double timeStampDivided = timeStamp / 1000L;
			float statEventTime = (float)(timeStampDivided - beginOfPlayTimeStampSeconds);

			//Strange things can happen... DST switches, etc.
			//See "gamePK": 469551, "playId": "d6af04a7-8bf5-46bc-a87d-66402950339e", BEGIN_OF_PLAY as an exmaple
			if (statEventTime > 1000f) {
				statEventTime = 3.5f;
			}

			statEvent.time = statEventTime;

			statEvent.nameString = statItem ["name"].ToString();
			statEvent.unit = statItem ["unit"].ToString();
			statEvent.UpdateUnitStrings ();

			statEvent.statEventCategoryString = statItem ["category"].ToString();
			if (statItem.ContainsKey ("description")) {
				statEvent.description = statItem ["description"].ToString ();
			}

			Int32.TryParse (statItem ["target_id"].ToString(), out statEvent.targetId);
			if (statItem.ContainsKey ("target_mlb_id")) {
				Int32.TryParse (statItem ["target_mlb_id"].ToString(), out statEvent.targetMlbId);
			}

			statEvent.valueString = statItem ["value"].ToString();
			Int32.TryParse (statItem ["type_id"].ToString(), out statEvent.statEventTypeInt);

			/*
			if (statEvent.statEventTypeInt != 0 && statEvent.statEventTypeInt < 1000) {
				statEvent.statEventTypeInt += 1000;
			}
			*/

			statEvent.statEventType = (eStatEventType)statEvent.statEventTypeInt;

			//Ugh
			if (statEvent.statEventTypeInt == 45 || statEvent.statEventTypeInt == 31) {
				statEvent.statEventType = eStatEventType.Hit_Travel_Time;
			}
			if (statEvent.statEventTypeInt == 46 || statEvent.statEventTypeInt == 32) {
				statEvent.statEventType = eStatEventType.Hit_Travel_Distance;
			}

			statEvent.DetermineStatEventCategory ();

			if (statEvent.statEventCategory == eStatEventCategory.Fielders_C_P_IF_OF ||
				statEvent.statEventCategory == eStatEventCategory.Fielders_IF_OF ||
				statEvent.statEventCategory == eStatEventCategory.Fielders_OF ||
				statEvent.statEventCategory == eStatEventCategory.Catcher_Fielding) {

				if (fieldingPositionIdentiferDict.ContainsKey (statEvent.targetId)) {
					fieldingPositionIdentiferDict [statEvent.targetId]++;
				} else {
					fieldingPositionIdentiferDict.Add (statEvent.targetId, 1);
				}
			}

			if (statEvent.statEventCategory == eStatEventCategory.Baserunning) {
				if (baserunningPositionIdentiferDict.ContainsKey (statEvent.targetId)) {
					baserunningPositionIdentiferDict [statEvent.targetId]++;
				} else {
					baserunningPositionIdentiferDict.Add (statEvent.targetId, 1);
				}
			}

			if (statEvent.statEventType == eStatEventType.Spin_Rate) {
				float.TryParse(statEvent.valueString, out spinRate);
			}

			if (statEvent.statEventType == eStatEventType.Launch_Angle) {
				float.TryParse(statEvent.valueString, out launchAngle);
			}

			if (statEvent.statEventType == eStatEventType.Launch_Vector) {
				float.TryParse(statEvent.valueString, out launchVector);
			}

			statEventsDataUnsorted.Add (statEvent);
		}
			
		statEventsData = statEventsDataUnsorted.OrderBy(o=>o.time).ToList();

		//The idea here is that for fielding & baserunning, there's the possibility of highlighting multiple players in a single play
		//We'll default to only showing an overlay for what appears to be the primary fielder/runner
		ePlayPositionIdentifier fielderPrimary = ePlayPositionIdentifier.Unknown;
		ePlayPositionIdentifier baserunnerPrimary = ePlayPositionIdentifier.Unknown;

		if (fieldingPositionIdentiferDict.Count > 0) {
			fielderPrimary = (ePlayPositionIdentifier)fieldingPositionIdentiferDict.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		if (baserunningPositionIdentiferDict.Count > 0) {
			baserunnerPrimary = (ePlayPositionIdentifier)baserunningPositionIdentiferDict.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		StatOverlay statOverlayPitching = new StatOverlay (eStatOverlayCategory.Pitching, this);
		StatOverlay statOverlayBatting = new StatOverlay (eStatOverlayCategory.Hitting_General, this);
		StatOverlay statOverlayBattingApex = new StatOverlay (eStatOverlayCategory.Hitting_Apex, this);
		StatOverlay statOverlayBaserunningPrimary = new StatOverlay (eStatOverlayCategory.Baserunning_General, this);
		StatOverlay statOverlayFieldingPrimary = new StatOverlay (eStatOverlayCategory.Fielding_General, this);

		statOverlayData = new List<StatOverlay> ();

		//Now that we know who to focus on, we can build the overlays by only selecting stat events relevant to these players 
		for (int i = 0; i < statEventsData.Count; i++) {
			StatEvent statEvent = statEventsData[i];
			if (statEvent.statEventCategory == eStatEventCategory.Pitching) {
				statOverlayPitching.statEventsAll.Add (statEvent);
				statOverlayPitching.targetId = statEvent.targetId;
				statOverlayPitching.positionIdentifier = (ePlayPositionIdentifier)statEvent.targetId;
				statOverlayPitching.targetMlbId = statEvent.targetMlbId;
			} else if (statEvent.statEventCategory == eStatEventCategory.Batting) {
				statOverlayBatting.statEventsAll.Add (statEvent);
				statOverlayBatting.targetId = statEvent.targetId;
				statOverlayBatting.positionIdentifier = (ePlayPositionIdentifier)statEvent.targetId;
				statOverlayBatting.targetMlbId = statEvent.targetMlbId;

				statOverlayBattingApex.statEventsAll.Add (statEvent);
				statOverlayBattingApex.targetId = statEvent.targetId;
				statOverlayBattingApex.positionIdentifier = (ePlayPositionIdentifier)statEvent.targetId;
				statOverlayBattingApex.targetMlbId = statEvent.targetMlbId;

			} else if (statEvent.statEventCategory == eStatEventCategory.Baserunning) {
				if (statEvent.targetId == (int)baserunnerPrimary) {
					statOverlayBaserunningPrimary.statEventsAll.Add (statEvent);
					statOverlayBaserunningPrimary.targetId = statEvent.targetId;
					statOverlayBaserunningPrimary.positionIdentifier = (ePlayPositionIdentifier)statEvent.targetId;
					statOverlayBaserunningPrimary.targetMlbId = statEvent.targetMlbId;
				}
			} else if (statEvent.statEventCategory == eStatEventCategory.Fielders_C_P_IF_OF || statEvent.statEventCategory == eStatEventCategory.Fielders_IF_OF || statEvent.statEventCategory == eStatEventCategory.Fielders_OF) {
				if (statEvent.targetId == (int)fielderPrimary) {
					statOverlayFieldingPrimary.statEventsAll.Add (statEvent);
					statOverlayFieldingPrimary.targetId = statEvent.targetId;
					statOverlayFieldingPrimary.positionIdentifier = (ePlayPositionIdentifier)statEvent.targetId;
					statOverlayFieldingPrimary.targetMlbId = statEvent.targetMlbId;
				}
			}
		}

		statOverlayData.Add (statOverlayPitching);
		statOverlayData.Add (statOverlayBatting);

		if (IsHit() && launchAngle >= 12f) {
			statOverlayData.Add (statOverlayBattingApex);
		}
		//statOverlayData.Add (statOverlayFieldingPrimary);
		//statOverlayData.Add (statOverlayBaserunningPrimary);

		foreach (StatOverlay statOverlay in statOverlayData) {
			eStatOverlayCategory statEventCategory = statOverlay.statOverlayCategory;

			if (statEventCategory == eStatOverlayCategory.Fielding_General || statEventCategory == eStatOverlayCategory.Fielding_Throw || statEventCategory == eStatOverlayCategory.Fielding_Throw_Tag_Up) {
				if (ballWasFieldedEvent != null) {
					statOverlay.time = ballWasFieldedEvent.time;
				} else if (ballWasReleasedEvent != null) {
					statOverlay.time = ballWasReleasedEvent.time;
				}
			} else if (statEventCategory == eStatOverlayCategory.Pitching) {
				if (ballWasPitchedEvent != null) {
					statOverlay.time = ballWasPitchedEvent.time;
				}
			} else if (statEventCategory == eStatOverlayCategory.Hitting_General || statEventCategory == eStatOverlayCategory.Hitting_HomeRun) {
				if (ballWasHitEvent != null) {
					statOverlay.time = ballWasHitEvent.time;
				}
			} else if (statEventCategory == eStatOverlayCategory.Hitting_Apex) {

				statOverlay.time = 7.0f;

				if (statOverlay.playData != null && statOverlay.playData.ballHitApexEvent != null) {
					statOverlay.time = ballHitApexEvent.time - 0.25f;
				} else if (ballWasHitEvent != null) {
					statOverlay.time = ballWasHitEvent.time + 1.5f;
				}

			} else if (statEventCategory == eStatOverlayCategory.Baserunning_General) {
				statOverlay.time = 7.0f;
			} else if (statEventCategory == eStatOverlayCategory.Baserunning_Steal) {
				statOverlay.time = 4.0f;
			}

			statOverlay.statEvents = new List<StatEvent> ();
			statOverlay.DetermineStatOverlayCategory ();
			statOverlay.PopulateStatEvents ();
			statOverlay.DetermineStatEventsDisplayTime ();
		}

		if (statOverlayPitching.statEvents.Count == 0) {
			statOverlayData.Remove (statOverlayPitching);
		}
		if (statOverlayBatting.statEvents.Count == 0) {
			statOverlayData.Remove (statOverlayBatting);
		}
		if (statOverlayFieldingPrimary.statEvents.Count == 0) {
			statOverlayData.Remove (statOverlayFieldingPrimary);
		}
		if (statOverlayBaserunningPrimary.statEvents.Count == 0) {
			statOverlayData.Remove (statOverlayBaserunningPrimary);
		}

		statOverlayData = statOverlayData.OrderBy (x => x.time).ToList();
	}

    private void ProcessPositionalData(List<object> data) {

        List<PositionalData> mergedPositionalDataUnsorted = new List<PositionalData>();

        int count = 0;

        //TODO: This is an unholy mess, will need to be cleaned up later.
        foreach (Dictionary<string,object> positionalDataItem in data) {

            PositionalData positionalData = new PositionalData();
            positionalData.timeStampString = positionalDataItem["timeStamp"].ToString();

            DateTime timeStampDateTime = DateTime.Parse(positionalData.timeStampString);
            TimeSpan timeStampSpan = (timeStampDateTime.ToUniversalTime() - epoch);
            positionalData.timeStampSeconds = timeStampSpan.TotalSeconds;
            //positionalData.time = (float)(positionalData.timeStampSeconds - beginOfPlayTimeStampSeconds);

            positionalData.positions = new List<PlayPosition>();

            List<object> positionItemsData = (List<object>)positionalDataItem["positions"];
            foreach (Dictionary<string,object> positionItems in positionItemsData) {
                PlayPosition playPosition = new PlayPosition();

                if (positionItems.ContainsKey("typ")) {
                    playPosition.playPositionType = (ePlayPositionType)(int.Parse(positionItems["typ"].ToString()));
                }
                if (positionItems.ContainsKey("id")) {
                    playPosition.playPositionIdentifier = (ePlayPositionIdentifier)(int.Parse(positionItems["id"].ToString()));
                }

                if (playPosition.playPositionType == ePlayPositionType.Ball) {
                    playPosition.position = new Vector3(float.Parse(positionItems["x"].ToString()), float.Parse(positionItems["y"].ToString()), float.Parse(positionItems["z"].ToString()));
                } else {
                    playPosition.position = new Vector3(float.Parse(positionItems["x"].ToString()), float.Parse(positionItems["y"].ToString()), 0);
                }
                playPosition.position = Vector3.Scale(playPosition.position, FEET_TO_METERS_VECTOR);

                positionalData.positions.Add(playPosition);
            }

            mergedPositionalDataUnsorted.Add(positionalData);

            count++;

            //Debug.Log("timeStamp: " + positionalDataItem["timeStamp"] + ", count: " + count);  

        }

        mergedPositionalData = mergedPositionalDataUnsorted.OrderBy(o=>o.timeStampSeconds).ToList();

		if (mergedPositionalData.Count > 0) {
			//Determine batter handedness
			firstPositionalData = mergedPositionalData [0];
			lastPositionalData = mergedPositionalData [mergedPositionalData.Count - 1];

			PlayPosition batter = firstPositionalData.GetPlayPositionForPositionIdentifier (ePlayPositionIdentifier.Batter);
			if (batter != null) {
				if (batter.position.x > 0) {
					batterIsRightHanded = false;
				}
				//Debug.Log ("batterIsRightHanded: " + batterIsRightHanded);
			}

			if (firstPositionalData != null) {
				beginOfPlayTimeStampSeconds = firstPositionalData.timeStampSeconds;
				//Debug.Log ("firstPositionalData.time: " + firstPositionalData.time + ", firstPositionalData.timeStampSeconds: " + firstPositionalData.timeStampSeconds + ", beginOfPlayTimeStampSeconds: " + beginOfPlayTimeStampSeconds + ", lastPositionalData.timeStampSeconds: " + lastPositionalData.timeStampSeconds);
			}
		}
    }

	private void UpdatePositionalDataTime() {
		//Debug.Log ("UpdatePositionalDataTime called, beginOfPlayTimeStampSeconds: " + beginOfPlayTimeStampSeconds);
			
		for (int i=0; i<mergedPositionalData.Count; i++) {
			PositionalData positionalData = mergedPositionalData[i];
			positionalData.time = (float)(positionalData.timeStampSeconds - beginOfPlayTimeStampSeconds);
		}

		Debug.Log ("UpdatePositionalDataTime called, firstPositionalData.time: " + firstPositionalData.time + ", firstPositionalData.timeStampSeconds: " + firstPositionalData.timeStampSeconds + ", beginOfPlayTimeStampSeconds: " + beginOfPlayTimeStampSeconds + ", lastPositionalData.time: " + lastPositionalData.time);
	}

	private void ProcessPlayerLineup(Dictionary<string,object> gameEventData) {

		List<object> playerLineupList = (List<object>)gameEventData ["lineup"] as List<object>;
		playerLineupData = new List<Player>();
		playerPositionsToDisplay = new List<ePlayPositionIdentifier>();

		foreach (Dictionary<string,object> playerLineupItem in playerLineupList) {

			if (playerLineupItem.ContainsKey ("id") && playerLineupItem ["id"] != null) {

				string playerIdStr = playerLineupItem ["id"].ToString ();

				ePlayPositionIdentifier positionIdentifier = (ePlayPositionIdentifier)(Int32.Parse (playerLineupItem ["pos"].ToString ()));

				/*Player player = GameManager.Instance.GetPlayerByPositionIdentifier (positionIdentifier);

				if (player != null) {
					player.positionIdentifier = positionIdentifier;
					player.playerId = playerIdStr;
					playerLineupData.Add (player);
					playerPositionsToDisplay.Add (positionIdentifier);
				}*/
			}
		}
			
		/*

		"gameEvent": {
			"gpk": 414205,
			"guid": "a09cb48e-e0fe-4267-9a8c-e9f7bf805a8d",
			"lineup": [
				{
					"pos": 1,
					"id": 572070
				},
				{
					"pos": 2,
					"id": 542208
				},
				{
					"pos": 3,
					"id": 571868
				},
				{
					"pos": 4,
					"id": 543213
				},
				{
					"pos": 5,
					"id": 501896
				},
				{
					"pos": 6,
					"id": 430947
				},
				{
					"pos": 7,
					"id": 459964
				},
				{
					"pos": 8,
					"id": 545361
				},
				{
					"pos": 9,
					"id": 594777
				},
				{
					"pos": 10,
					"id": 448801
				},
				{
					"pos": 11,
					"id": null
				},
				{
					"pos": 12,
					"id": 430321
				},
				{
					"pos": 13,
					"id": null
				}
			],
			"umpires": [
				{
					"pos": 14,
					"id": 427248
				},
				{
					"pos": 15,
					"id": 427013
				},
				{
					"pos": 16,
					"id": 427081
				},
				{
					"pos": 17,
					"id": 503077
				},
				{
					"pos": 18,
					"id": null
				},
				{
					"pos": 19,
					"id": null
				}
			],
			"event": [
				{
					"pos": 10,
					"details": [
						{
							"pos": 9,
							"typ": "f_putout",
							"id": 594777
						},
						{
							"pos": 1,
							"typ": "p_out_line_drive",
							"id": 572070
						}
					],
					"typ": "field_out",
					"id": 448801
				}
			],
			"runners": [],
			"sit": {
				"outs": 1,
				"balls": 2,
				"top_inning": 0,
				"strikes": 2,
				"inning": 2
			}
		},
		*/

	}

    private void ProcessBallPositionalData(List<object> data) {

        ballPositionalData = new BallPositionalData();
        List<BallPosition> ballPositionsUnsorted = new List<BallPosition>();

        int count = 0;
		BallPosition ballPositionPrevious = null;

        //TODO: This is an unholy mess, will need to be cleaned up later.
        foreach (Dictionary<string,object> dataItem in data) {

            Dictionary<string,object> ballPositionalDataItem = dataItem["BallPosition"] as Dictionary<string,object>;

            BallPosition ballPosition = new BallPosition();
            ballPosition.timeCodeOffset = float.Parse(ballPositionalDataItem["TimeCodeOffset"].ToString());

            string typeString = ballPositionalDataItem["TimeCodeOffset"].ToString();

            switch (typeString) {
                case "Measured":
                    ballPosition.ballMeasurementType = eBallMeasurementType.Measured;
                    break;
                case "Estimated":
                    ballPosition.ballMeasurementType = eBallMeasurementType.Estimated;
                    break;
                default:
                    ballPosition.ballMeasurementType = eBallMeasurementType.None;
                    break;
            }
                    
            Dictionary<string,object> positionItemsData = ballPositionalDataItem["Position"] as Dictionary<string,object>;
            ballPosition.position = new Vector3(float.Parse(positionItemsData["X"].ToString()), float.Parse(positionItemsData["Y"].ToString()), float.Parse(positionItemsData["Z"].ToString()));
            ballPosition.position = Vector3.Scale(ballPosition.position, FEET_TO_METERS_VECTOR);

            ballPosition.timeCode = double.Parse(ballPositionalDataItem["TimeCode"].ToString());
            ballPosition.time = float.Parse(ballPositionalDataItem["Time"].ToString()) + ballWasPitchedEvent.time;

            Dictionary<string,object> velocityItemsData = ballPositionalDataItem["Velocity"] as Dictionary<string,object>;
            ballPosition.velocity = new Vector3(float.Parse(velocityItemsData["X"].ToString()), float.Parse(velocityItemsData["Y"].ToString()), float.Parse(velocityItemsData["Z"].ToString()));
            ballPosition.velocity = Vector3.Scale(ballPosition.velocity, FEET_TO_METERS_VECTOR);
                
			//Skip duplicates
			if (count == 0 || (ballPositionPrevious != null && ballPosition.time != ballPositionPrevious.time)) {
				ballPositionsUnsorted.Add (ballPosition);
			}
			ballPositionPrevious = ballPosition;

            count++;
            //Debug.Log("timeStamp: " + positionalDataItem["timeStamp"] + ", count: " + count);  

        }

		//Cull dupes
		List<BallPosition> ballPositionsUnduped = new List<BallPosition>();
		BallPosition previousBall = null;
		for (int i=0; i<ballPositionsUnsorted.Count; i++) {
			BallPosition ballPosition = ballPositionsUnsorted[i];
			if (i == 0 || (ballPosition.time != previousBall.time)) {
				ballPositionsUnduped.Add (ballPosition);
				previousBall = ballPosition;
			}
		}

		ballPositionalData.ballPositions = ballPositionsUnduped.OrderBy(o=>o.time).ToList();			
    }

	private void ProcessSegmentEventsData(List<object> data) {

		if (data.Count == 0) {
			errorNote = "No segments found!";
			return;
		}

		segmentEventsData = new List<SegmentEvent> ();

        int count = 0;

        reducedConfidenceData = new List<string>();

        //TODO: This is an unholy mess, will need to be cleaned up later.
        foreach (Dictionary<string,object> dataItem in data) {

            Dictionary<string,object> segmentDataItem = dataItem["SegmentData"] as Dictionary<string,object>;

			SegmentEvent segmentEvent = new SegmentEvent ();

			if (segmentDataItem ["SegmentType"] != null) {
				string segmentTypeString = segmentDataItem ["SegmentType"] as string;
				segmentEvent.segmentType = (eSegmentType)Enum.Parse(typeof(eSegmentType), segmentTypeString);

				if (segmentEvent.segmentType == eSegmentType.BaseballHit) {
					hitSegment = segmentEvent;
				}
			}

			if (segmentDataItem ["StartData"] != null) {
				Dictionary<string,object> startDataItem = segmentDataItem["StartData"] as Dictionary<string,object>;
				segmentEvent.startData = new SegmentEventData (startDataItem);
				segmentEvent.time = segmentEvent.startData.time;
			}

			if (segmentDataItem ["EndData"] != null) {
				Dictionary<string,object> endDataItem = segmentDataItem["EndData"] as Dictionary<string,object>;
				segmentEvent.endData = new SegmentEventData (endDataItem);
			}

			segmentEvent.trajectoryPolynomialX = new List<float> ();
			segmentEvent.trajectoryPolynomialY = new List<float> ();
			segmentEvent.trajectoryPolynomialZ = new List<float> ();

			if (segmentDataItem.ContainsKey ("TrajectoryPolynomialX")) {
				ParseTrajectoryData (segmentDataItem ["TrajectoryPolynomialX"] as List<object>, segmentEvent.trajectoryPolynomialX);
			}
			if (segmentDataItem.ContainsKey ("TrajectoryPolynomialY")) {
				ParseTrajectoryData (segmentDataItem ["TrajectoryPolynomialY"] as List<object>, segmentEvent.trajectoryPolynomialY);
			}
			if (segmentDataItem.ContainsKey ("TrajectoryPolynomialZ")) {
				ParseTrajectoryData (segmentDataItem ["TrajectoryPolynomialZ"] as List<object>, segmentEvent.trajectoryPolynomialZ);
			}


			//Debug.Log ("segmentEvent: " + segmentEvent.segmentType + ", segmentEvent.startData.time: " + segmentEvent.startData.time + ", segmentEvent.endData.time: " + segmentEvent.endData.time);

            if (segmentDataItem["ReducedConfidence"] != null) {

				segmentEvent.reducedConfidence = new List<string> ();

				List<object> reducedConfidenceItems = segmentDataItem["ReducedConfidence"] as List<object>;

                foreach (object reducedConfidenceItem in reducedConfidenceItems) {
                    string reducedConfidenceString = reducedConfidenceItem.ToString();

                    if (reducedConfidenceString.Length > 0) {
                        //Debug.Log("reducedConfidenceString: " + reducedConfidenceString);  
						segmentEvent.reducedConfidence.Add (reducedConfidenceString);

						reducedConfidenceData.Add(segmentEvent.segmentType.ToString() + " - " + reducedConfidenceString);
                        reducedConfidence = true;
                    }
                }
            }

			segmentEventsData.Add (segmentEvent);
        }

		ballDataPoly = new List<DataPoint> ();

		segmentEventsData = segmentEventsData.OrderBy (x => x.time).ToList ();
		for (int i = 0; i < segmentEventsData.Count; i++) {
			SegmentEvent segmentEvent = segmentEventsData [i];
			bool evaluateUntilGroundHit = false;
			bool alignWithPreviousSegment = false;

			if (segmentEvent.segmentType == eSegmentType.BaseballHit || segmentEvent.segmentType == eSegmentType.BaseballBounce || segmentEvent.segmentType == eSegmentType.BaseballDeflection) {
				alignWithPreviousSegment = true;
			}

			if (segmentEvent.segmentType == eSegmentType.BaseballHit && gamePlayData.isHomeRun && segmentEvent.Duration () <= 3.5f) {
				evaluateUntilGroundHit = true;
			}

			ProcessSegmentData (segmentEvent, segmentEvent.startData.time, segmentEvent.endData.time, evaluateUntilGroundHit, alignWithPreviousSegment);
		}

		//Look for missing data segments
		for (int i = 0; i < playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			if (playEvent.playEventType == ePlayEventType.BALL_WAS_PITCHED) {
				SegmentEvent segmentEvent = segmentEventsData.Find (x => x.segmentType == eSegmentType.BaseballPitch);
				if (segmentEvent == null) {
					errorNote += "Missing pitch segment; ";
				}
			}
			if (playEvent.playEventType == ePlayEventType.BALL_WAS_HIT) {
				SegmentEvent segmentEvent = segmentEventsData.Find (x => x.segmentType == eSegmentType.BaseballHit);
				if (segmentEvent == null) {
					errorNote += "Missing hit segment; ";
				}
			}
			if (playEvent.playEventType == ePlayEventType.BALL_WAS_RELEASED) {
				SegmentEvent segmentEvent = segmentEventsData.Find (x => x.segmentType == eSegmentType.BaseballThrow);
				if (segmentEvent == null) {
					errorNote += "Missing throw segment; ";
				}
			}
		}

		if (errorNote.Length > 0) {
			errorNote = "NOTE: " + errorNote.ToUpper ();
		}

		if (hitSegment != null) {
			DataPoint highestBallOfHit = FindBallWithGreatestHeight (ballDataPoly, hitSegment.time, hitSegment.endData.time);
			ballHitApexEvent = new PlayEvent ();
			ballHitApexEvent.playEventType = ePlayEventType.CUSTOM_APEX_OF_HIT;
			ballHitApexEvent.time = highestBallOfHit.time;
			playEventsData.Add (ballHitApexEvent);
			playEventsData = playEventsData.OrderBy (x => x.time).ToList ();
		}

		float hitDirection = 0;
		int ballsSampled = 0;

		for (int i=0; i<ballDataPoly.Count; i++) {
			if (ballDataPoly[i].time >= 3.5f && ballDataPoly[i].time < 4.0f) {
				ballsSampled++;
				hitDirection += ballDataPoly[i].position.x;
			}
		}

		float hitDirectionAverage = hitDirection / (ballsSampled == 0 ? 1.0f : (float)ballsSampled);
		hitToRightField = hitDirectionAverage >= 0;
    }

	private void ProcessSegmentData(SegmentEvent segmentEvent, float startTime, float endTime, bool evaluateUntilGroundHit, bool alignWithPreviousSegment) {
		//Debug.Log ("ProcessSegmentData called, segmentEvent.segmentType: " + segmentEvent.segmentType + ", startTime: " + startTime + ", endTime: " + endTime + ", evaluateUntilGroundHit: " + evaluateUntilGroundHit + ", alignWithPreviousSegment: " + alignWithPreviousSegment);
		float duration = endTime - startTime;
		float timeOffset = 3.0f;
		float timeValue = startTime - timeOffset;

		float dataPointInterval = 0.01f;
		int numberOfDataPoints = (int)(duration / dataPointInterval);

		if (evaluateUntilGroundHit) {
			numberOfDataPoints = 2000;	//20 seconds should be plenty
		}

		int dataPointCount = 0;
		bool addPoints = true;

		DataPoint previousSegmentLastDataPoint = null;
		Vector3 positionOffset = Vector3.zero;

		bool offsetDetermined = !alignWithPreviousSegment;
		if (alignWithPreviousSegment && ballDataPoly.Count > 0) {
			previousSegmentLastDataPoint = ballDataPoly [ballDataPoly.Count - 1];
		}

		while (addPoints) {

			float posX = 0;
			float posY = 0;
			float posZ = 0;

			if (segmentEvent.segmentType == eSegmentType.BaseballHit) {
				posX = EvaluatePolynomialDegreeEight (timeValue, segmentEvent.trajectoryPolynomialX) * FEET_TO_METERS;
				posY = EvaluatePolynomialDegreeEight (timeValue, segmentEvent.trajectoryPolynomialY) * FEET_TO_METERS;
				posZ = EvaluatePolynomialDegreeEight (timeValue, segmentEvent.trajectoryPolynomialZ) * FEET_TO_METERS;
			} else {
				posX = EvaluatePolynomialDegreeThree (timeValue, segmentEvent.trajectoryPolynomialX) * FEET_TO_METERS;
				posY = EvaluatePolynomialDegreeThree (timeValue, segmentEvent.trajectoryPolynomialY) * FEET_TO_METERS;
				posZ = EvaluatePolynomialDegreeThree (timeValue, segmentEvent.trajectoryPolynomialZ) * FEET_TO_METERS;
			}

			Vector3 pos = new Vector3 (posX, posY, Mathf.Max (0, posZ));
			float simTime = timeValue + timeOffset;

			if (!offsetDetermined && previousSegmentLastDataPoint != null) {
				positionOffset = previousSegmentLastDataPoint.position - pos;
				offsetDetermined = true;
				simTime = previousSegmentLastDataPoint.time;
				//ebug.Log ("positionOffset: " + positionOffset);
			}

			DataPoint dataPoint = new DataPoint (pos + positionOffset, simTime);
			dataPoint.dataSource = eDataSource.Polynomial;

			if (evaluateUntilGroundHit && posZ <= 0) {
				addPoints = false;
				posZ = 0;
			} else if (dataPointCount >= numberOfDataPoints) {
				addPoints = false;
			}

			ballDataPoly.Add (dataPoint);

			timeValue += dataPointInterval;
			dataPointCount++;

			//Look at the final point of the hit segment, if it's floating in mid air... keep going
			if (!addPoints && !evaluateUntilGroundHit && segmentEvent.segmentType == eSegmentType.BaseballHit && posZ > 15.0f) {
				evaluateUntilGroundHit = true;
				addPoints = true;
				numberOfDataPoints = 2000;
				Debug.LogError ("Ball was lost by radar, continuing the hit trajectory until it reaches the ground plane.");
			}
		}
	}

	private float EvaluatePolynomialDegreeThree(float timeValue, List<float> valueList) {
		float finalValue = 0;

		float a = valueList[0];
		float b = valueList[1];
		float c = valueList[2];

		float x = timeValue;
		finalValue = a * Mathf.Pow (x, 2) + b*x + c;

		return finalValue;
	}

	private float EvaluatePolynomialDegreeEight(float timeValue, List<float> valueList) {
		float finalValue = 0;

		float a = valueList[0];
		float b = valueList[1];
		float c = valueList[2];
		float d = valueList[3];
		float f = valueList[4];
		float g = valueList[5];
		float h = valueList[6];
		float j = valueList[7];
		float k = valueList[8];

		float x = timeValue;
		finalValue = a * Mathf.Pow (x, 8) + b * Mathf.Pow (x, 7) + c * Mathf.Pow (x, 6) + d * Mathf.Pow (x, 5) + f * Mathf.Pow (x, 4) + g * Mathf.Pow (x, 3) + h * Mathf.Pow (x, 2) + j*x + k;

		return finalValue;
	}

	private void ParseTrajectoryData(List<object> dataValues, List<float> dataList) {
		foreach (object dataValueItem in dataValues) {
			float itemValue = 0;
			float.TryParse (dataValueItem.ToString (), out itemValue);
			dataList.Add (itemValue);
		}
		dataList.Reverse ();
	}

	private void CombineBallPositionData() {

		ballDataCH = new List<DataPoint> ();
		ballDataTM = new List<DataPoint> ();

		combinedBallPositions = new List<DataPoint> ();

		//Let's start by just isolating the CH ball data into a single list with only time and position info.
		//Later, this process should be folded into the intital CH data processing step.
		for (int i=0; i<mergedPositionalData.Count; i++) {
			PositionalData positionalData = mergedPositionalData[i];
			for (int j=0; j<positionalData.positions.Count; j++) {
				PlayPosition playPosition = positionalData.positions [j];
				if (playPosition.playPositionType == ePlayPositionType.Ball) {
					DataPoint combinedBallPosition = new DataPoint (playPosition.position, positionalData.time);
					combinedBallPosition.dataSource = eDataSource.ChyronHego;
					ballDataCH.Add (combinedBallPosition);
				}
			}
		}
		ballDataCH = ballDataCH.OrderBy (x => x.time).ToList ();

		//Grab the TM ball data.
		for (int i=0; i<ballPositionalData.ballPositions.Count; i++) {
			DataPoint combinedBallPosition = new DataPoint (ballPositionalData.ballPositions [i].position, ballPositionalData.ballPositions[i].time);
			combinedBallPosition.dataSource = eDataSource.Trackman;
			ballDataTM.Add (combinedBallPosition);
		}

		ballDataTM = ballDataTM.OrderBy (x => x.time).ToList ();
		bool bumTrackmanData = ballDataTM.Count == 0 || ballDataTM[0].time >= 3.1f;

		//Debug.Log ("Before RemoveErrantDataPoints, ballDataCH.Count: " + ballDataCH.Count);
		//Debug.Log ("Before RemoveErrantDataPoints, ballDataTM.Count: " + ballDataTM.Count);

		int errantBallDataCountCH = ballDataCH.Count;
		int errantBallDataCountTM = ballDataTM.Count;

		//Remove any data points that seem errant
		//We run this multiple times because occionally a series of errant balls will occur, and it's very difficult to discern the balls WITHIN these sequences
		//By running this recursively we can eliminate the more easily detected errant balls at the start and end of each series

		float searchStartTime = 0;
		if (ballWasHitEvent != null) {
			searchStartTime = ballWasHitEvent.time;
		}

		if (ballDataCH.Count > 0) {
			int ballsRemovedCH = int.MaxValue;
			while (ballsRemovedCH > 0) {
				ballsRemovedCH = RemoveErrantDataPoints (ballDataCH, searchStartTime, ballDataCH.Last ().time, 1.0f);
			}
		}

		if (ballDataTM.Count > 0) {
			int ballsRemovedTM = int.MaxValue;
			while (ballsRemovedTM > 0) {
				ballsRemovedTM = RemoveErrantDataPoints (ballDataTM, searchStartTime, ballDataTM.Last ().time, 0.5f);
			}
		}

		errantBallDataCountCH -= ballDataCH.Count;
		errantBallDataCountTM -= ballDataTM.Count;

		if (errantBallDataCountCH > 0) {
			Debug.LogWarning ("errantBallDataCountCH: " + errantBallDataCountCH);
		}
		if (errantBallDataCountTM > 0) {
			Debug.LogWarning ("errantBallDataCountTM: " + errantBallDataCountTM);
		}

		if (ScheduleDataManager.instance.smoothData) {
			CleanBallDataList (ballDataCH);
		}


		List<DataPoint> combinedBallPositionsUnsorted = new List<DataPoint> ();

		if (ballDataPoly != null) {
			foreach (DataPoint dataPoint in ballDataPoly) {
				combinedBallPositionsUnsorted.Add (dataPoint);
			}
		}

		//We want to fill-in any gaps in the trajectory data with CH data points
		//First, let's find stretches of time with missing trajectory data
		List<DataPoint> fallbackDataPoints = new List<DataPoint> ();

		float missingDataStartTime = 0;
		float missingDataEndTime = 0;

		if (segmentEventsData != null) {

			for (int i = 0; i < segmentEventsData.Count; i++) {
				float timeGap = 0;
				SegmentEvent segmentEventA = segmentEventsData [i];

				if (i < segmentEventsData.Count - 1) {
				
					SegmentEvent segmentEventB = segmentEventsData [i + 1];

					timeGap = segmentEventB.startData.time - segmentEventA.endData.time;
					//Debug.Log ("segmentEventA.segmentType: " + segmentEventA.segmentType + ", segmentEventB.segmentType: " + segmentEventB.segmentType + ", timeGap: " + timeGap);

					if (timeGap >= 0.1f) {
						missingDataStartTime = segmentEventA.endData.time;
						missingDataEndTime = segmentEventB.startData.time;
					}

				} else {

					timeGap = endOfPlayEvent.time - segmentEventA.endData.time;
					if (timeGap >= 0.1f) {
						missingDataStartTime = segmentEventA.endData.time;
						missingDataEndTime = endOfPlayEvent.time;
					}
				}

				if (missingDataEndTime > 0) {
					fallbackDataPoints = FindBallsInTimeRange (ballDataCH, missingDataStartTime - 0.02f, missingDataEndTime);
					if (fallbackDataPoints.Count > 1) {
						Debug.Log ("Adding " + fallbackDataPoints.Count + " CH balls from: " + fallbackDataPoints [0].time + " to " + fallbackDataPoints [fallbackDataPoints.Count - 1].time);
						foreach (DataPoint dataPoint in fallbackDataPoints) {
							combinedBallPositionsUnsorted.Add (dataPoint);
						}
					}
				}
			}
		} else {
			//No segment data, just use CH data
			for (int i = 0; i < ballDataCH.Count; i++) {
				combinedBallPositionsUnsorted.Add (ballDataCH [i]);
			}
		}
		combinedBallPositions = combinedBallPositionsUnsorted.OrderBy (x => x.time).ToList ();

		//Mark data points as outOfView
		for (int i = 0; i < combinedBallPositions.Count; i++) {
			if (combinedBallPositions [i].time < 3.0f) {
				combinedBallPositions [i].outOfView = true;
			} else {
				if (i < combinedBallPositions.Count - 1) {
					DataPoint nextDataPoint = combinedBallPositions [i + 1];
					if (nextDataPoint.time - combinedBallPositions [i].time > 0.25f) {
						combinedBallPositions [i].outOfView = true;
					}
				} else {
					combinedBallPositions [i].outOfView = !gamePlayData.isHomeRun;
				}
			}
		}

		bool searchCompleted = false;
		float startTime = 3.5f;


		if (segmentEventsData != null) {
			
			SegmentEvent currentSegment = GetSegmentEventForType (eSegmentType.BaseballPitch);
			float endTime = currentSegment.endData.time;

			if (hitSegment != null) {
				currentSegment = hitSegment;
				endTime = currentSegment.endData.time + 0.01f;
			}

			DataPoint currentDataPoint = null;

			while (!searchCompleted) {
				currentDataPoint = FindBallWithGreatestTimeDifferenceToNextBall (combinedBallPositions, startTime, endTime);

				if (currentDataPoint != null) {

					int currentDataPointIndex = combinedBallPositions.IndexOf (currentDataPoint);

					if (currentDataPointIndex < combinedBallPositions.Count - 1) {
						DataPoint nextDataPoint = combinedBallPositions [currentDataPointIndex + 1];

						//Debug.Log ("---------- Comparing currentDataPoint.time: " + currentDataPoint.time + " to nextDataPoint.time: " + nextDataPoint.time);

						float timeDifference = nextDataPoint.time - currentDataPoint.time;

						if (timeDifference > 0.25f) {
							//Debug.Log ("---------- Marking data point at " + currentDataPoint.time + " as outOfView");
							currentDataPoint.outOfView = true;
							nextDataPoint.outOfView = false;
						}

						//If we're not finding a large time gap, kill the search
						if (endTime == endOfPlayEvent.time && timeDifference < 0.05f) {
							searchCompleted = true;
						}

						startTime = nextDataPoint.time + 0.0001f;

						int segmentEventIndex = segmentEventsData.IndexOf (currentSegment);
						if (segmentEventIndex != segmentEventsData.Count - 1) {
							currentSegment = segmentEventsData [segmentEventIndex + 1];
							endTime = currentSegment.endData.time + 0.01f;
						} else {
							endTime = endOfPlayEvent.time;
						}

					}
				} else {
					searchCompleted = true;
				}
			}
		}

		for (int i = 0; i < playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			if (playEvent.playEventType == ePlayEventType.BALL_WAS_FIELDED || playEvent.playEventType == ePlayEventType.BALL_WAS_CAUGHT || playEvent.playEventType == ePlayEventType.BALL_WAS_CAUGHT_OUT || playEvent.playEventType == ePlayEventType.TAG_WAS_APPLIED) {
				DataPoint dataPoint = FindFirstBallAtTime (combinedBallPositions, playEvent.time - 0.24f);
				if (dataPoint != null) {
					Debug.Log ("ball outOfView == TRUE at time: " + dataPoint.time + ", for event: " + playEvent.playEventType);
					dataPoint.outOfView = true;
				}
			} else if (playEvent.playEventType == ePlayEventType.BALL_WAS_RELEASED) {
				DataPoint dataPoint = FindFirstBallAtTime (combinedBallPositions, playEvent.time + 0.05f);
				if (dataPoint != null) {
					Debug.Log ("ball outOfView == FALSE at time: " + dataPoint.time + ", for event: " + playEvent.playEventType);
					dataPoint.outOfView = false;
				}
			}
		}


		//rotateCameraClockwise = !hitToRightField;
		rotateCameraClockwise = !batterIsRightHanded;

		dataLoaded = true;
	}
		
#region Find Ball / DataPoint

	private DataPoint FindFirstBallWithDistanceFrom(List<DataPoint> ballDataList, float startTime, float endTime, Vector3 originPoint, float distanceThreshold) {
		DataPoint ballFound = null;
		DataPoint nextBall = null;
		float distanceFrom = 0;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				distanceFrom = Vector3.Distance (ball.position, Vector3.zero);
				if (distanceFrom >= distanceThreshold) {
					ballFound = ball;
					break;
				}
			}
		}

		if (ballFound != null) {
			//Debug.Log ("distanceFrom : " + distanceFrom + ", ballFound.time: " + ballFound.time);
		}     

		return ballFound;
	}

	private DataPoint FindBallWithGreatestTimeDifferenceToNextBall(List<DataPoint> ballDataList, float startTime, float endTime) {
		DataPoint ballFound = null;
		DataPoint nextBall = null;

		float maxTime = 0;
		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				if (i < ballDataList.Count - 1) {
					nextBall = ballDataList [i + 1];
				}
				if (nextBall != null) {
					float timeDifference = nextBall.time - ball.time;
					if (timeDifference > maxTime) {
						maxTime = timeDifference;
						ballFound = ball;
					}
				}
			}
		}

		if (ballFound != null) {
			//Debug.Log ("FindBallWithGreatestTimeDifferenceToNextBall called, maxTime : " + maxTime + ", ballFound.time: " + ballFound.time);
		}     

		return ballFound;
	}

	private DataPoint FindFirstBallOutOfView(List<DataPoint> ballDataList, float startTime) {
		DataPoint ballFound = null;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime) {
				if (ball.outOfView) {
					ballFound = ball;
					break;
				}
			}
		}
		return ballFound;
	}

	private DataPoint FindFirstBallNotOutOfView(List<DataPoint> ballDataList, float startTime) {
		DataPoint ballFound = null;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime) {
				if (!ball.outOfView) {
					ballFound = ball;
					break;
				}
			}
		}
		return ballFound;
	}

	private DataPoint FindBallWithGreatestDistanceToNextBall(List<DataPoint> ballDataList, float startTime, float endTime) {
		DataPoint ballFound = null;
		DataPoint nextBall = null;

		float maxDistance = 0;
		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				if (i < ballDataList.Count - 1) {
					nextBall = ballDataList [i + 1];
				}
				if (nextBall != null) {
					float distance = Vector3.Distance (ball.position, nextBall.position);
					if (distance > maxDistance) {
						maxDistance = distance;
						ballFound = ball;
					}
				}
			}
		}

		if (ballFound != null) {
			//Debug.Log ("maxDistance : " + maxDistance + ", ballFound.time: " + ballFound.time);
		}     

		return ballFound;
	}

	private DataPoint FindBallWithGreatestHeight(List<DataPoint> ballDataList, float startTime, float endTime) {
		DataPoint ballFound = null;

		float maxHeight = 0;
		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				if (ball.position.z >= maxHeight) {
					maxHeight = ball.position.z;
					ballFound = ball;
				}
			}
		}
		return ballFound;
	}

	private DataPoint FindBallWithGreatestChangeInDirection(List<DataPoint> ballDataList, float startTime, float endTime) {
		DataPoint ballFound = null;
		DataPoint previousBall = null;

		Vector3 previousDirection = Vector3.zero;
		float maxDirectionDistance = 0;
		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				if (previousBall != null) {
					previousDirection = ball.position - previousBall.position;
				}
				DataPoint nextBall = null;
				if (i < ballDataList.Count - 1) {
					nextBall = ballDataList [i + 1];
				}
				if (nextBall != null) {
					Vector3 nextDirection = nextBall.position - ball.position;
					float distance = Vector3.Distance (previousDirection, nextDirection);
					if (distance > maxDirectionDistance) {
						maxDirectionDistance = distance;
						ballFound = ball;
					}
				}
			}
			previousBall = ball;
		}

		if (ballFound != null) {
			//Debug.Log ("maxDirectionDistance : " + maxDirectionDistance + ", ballFound.time: " + ballFound.time);
		}     

		return ballFound;
	}

	private DataPoint FindFirstBallAtTime(List<DataPoint> ballDataList, float startTime) {
		DataPoint ballFound = null;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime) {
				ballFound = ball;
				break;
			}
		}
		return ballFound;
	}

	private DataPoint FindFirstBallAfterTime(List<DataPoint> ballDataList, float startTime) {
		DataPoint ballFound = null;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time > startTime) {
				ballFound = ball;
				break;
			}
		}
		return ballFound;
	}

	private DataPoint FindFirstBallBeforeTime(List<DataPoint> ballDataList, float startTime) {
		DataPoint ballFound = null;

		for (int i=ballDataList.Count - 1; i >= 0; i--) {
			DataPoint ball = ballDataList [i];
			if (ball.time <= startTime) {
				ballFound = ball;
				break;
			}
		}
		return ballFound;
	}

	private List<DataPoint> FindBallsInTimeRange(List<DataPoint> ballDataList, float startTime, float endTime) {
		List<DataPoint> ballsFound = new List<DataPoint> ();

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && ball.time <= endTime) {
				ballsFound.Add (ball);
			}
		}
		return ballsFound;
	}

#endregion

#region Clean Data

	private int RemoveErrantDataPoints(List<DataPoint> ballDataList, float startTime, float endTime, float distanceThreshold) {
		//Debug.Log ("RemoveErrantDataPoints called, startTime: " + startTime + ", endTime: " + endTime + ", distanceThreshold: " + distanceThreshold);

		//The idea here is to eliminate data points that are too far from their neighbors
		List<DataPoint> dataPointsToRemove = new List<DataPoint> ();

		DataPoint ballPrevious = null;
		DataPoint ballCurrent = null;
		DataPoint ballNext = null;
		Vector3 previousDirection = Vector3.zero;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time > startTime && ball.time <= endTime) {

				ballCurrent = ballDataList [i];

				if (i > 0) {
					ballPrevious = ballDataList [i-1];
				}

				if (i < ballDataList.Count - 1) {
					ballNext = ballDataList [i+1];
				}

				if (ballPrevious != null && ballNext != null) {
					float distanceFromPreviousBall = Vector3.Distance (ballCurrent.position, ballPrevious.position);
					float distanceToNextBall = Vector3.Distance (ballCurrent.position, ballNext.position);
					float timeSincePreviousBall = ballCurrent.time - ballPrevious.time;
					float timeUntilNextBall = ballNext.time - ballCurrent.time;

					float ballSpeed = distanceFromPreviousBall / timeSincePreviousBall;

					Vector3 currentDirection = ball.position - ballPrevious.position;
					float changeInDirection = Vector3.SqrMagnitude ( currentDirection);

					float realiabilityValue = ballCurrent.realiabilityFactor;

					if (ballSpeed > 70.0f) {
						realiabilityValue = Mathf.Max(0, realiabilityValue - (ballSpeed / 200.0f));
					}

					//Check for sudden, sharp changes in direction
					if (changeInDirection >= 1.0f && timeSincePreviousBall < 0.05f) {
						realiabilityValue = Mathf.Max(0, realiabilityValue - (changeInDirection / 10.0f));
					}

					if (distanceFromPreviousBall >= 1.0f || timeSincePreviousBall > 0.25f || changeInDirection >= 1.0f) {
						//Debug.Log ("time: " + ballCurrent.time + ", distanceFromPrevious: " + distanceFromPreviousBall + ", distanceToNext: " + distanceToNextBall + ", timeSincePrevious: " + timeSincePreviousBall + ", timeUntilNext: " + timeUntilNextBall + ", changeInDirection: " + changeInDirection + ", ballSpeed: " + ballSpeed + ", realiabilityValue: " + realiabilityValue + ", index: " + i);
					}

					previousDirection = currentDirection;
					ballCurrent.realiabilityFactor = realiabilityValue;

					if (ballCurrent.realiabilityFactor == 0) {
						//Debug.Log ("ballCurrent added to dataPointsToRemove, distanceFromPreviousBall: " + distanceFromPreviousBall + ", distanceToNextBall: " + distanceToNextBall + ", ballCurrent.time: " + ballCurrent.time);
						dataPointsToRemove.Add (ballCurrent);
					}
				}
			}
		}

		int numberOfBallsToRemove = dataPointsToRemove.Count;

		//Debug.Log ("RemoveErrantDataPoints called, distanceThreshold: " + distanceThreshold + ", dataPointsToRemove.Count: " + dataPointsToRemove.Count);
		ballDataList.RemoveAll(item => dataPointsToRemove.Contains(item));

		return numberOfBallsToRemove;
	}

	private void CleanBallDataList(List<DataPoint> ballDataList) {

		bool endOfListReached = false;

		int segmentIndex = 0;
		float timeAllowance = 0.1f;

		if (segmentEventsData != null) {

			SegmentEvent segmentEvent = segmentEventsData [segmentIndex];

			while (!endOfListReached) {
		
				DataPoint startOfSmoothingDataPoint = FindFirstBallAtTime (ballDataList, segmentEvent.time);
				DataPoint endOfSmoothingDataPoint = FindBallWithGreatestChangeInDirection (ballDataList, segmentEvent.endData.time - timeAllowance, segmentEvent.endData.time + timeAllowance);

				if (endOfSmoothingDataPoint == null) {
					endOfSmoothingDataPoint = FindFirstBallBeforeTime (ballDataList, segmentEvent.endData.time);
				}

				if (startOfSmoothingDataPoint == null && endOfSmoothingDataPoint == null) {
					endOfListReached = true;
				} else {

					if (segmentIndex < segmentEventsData.Count - 1) {
						segmentIndex++;
						segmentEvent = segmentEventsData [segmentIndex];
					} else {
						endOfListReached = true;
					}
					
					int range = 7;

					int startingIndex = ballDataList.IndexOf (startOfSmoothingDataPoint);
					int endingIndex = ballDataList.IndexOf (endOfSmoothingDataPoint);
					int numDataPoints = endingIndex - startingIndex;

					if (startOfSmoothingDataPoint == null || endOfSmoothingDataPoint == null) {
						endOfListReached = true;
					} else if (numDataPoints > range) {
						range = Math.Min (numDataPoints / 2 - 1, 7);
						//Debug.Log ("Smoothing ball data between startOfSmoothingDataPoint.time: " + startOfSmoothingDataPoint.time + ", and endOfSmoothingDataPoint.time: " + endOfSmoothingDataPoint.time + ", numDataPoints: " + numDataPoints + ", range: " + range);

						CleanBallData (ballDataList, startOfSmoothingDataPoint.time, endOfSmoothingDataPoint.time, range, 0.7f);
					}
				}
			}
		}
	}

	private void CleanBallData(List<DataPoint> ballDataList, float startTime, float endTime, int range, float decay) {
		//Range: Number of data points each side to sample.
		//Decay:  [0.0 - 1.0] How slowly to decay from raw value.

		List<DataPoint> combinedBallsToSmooth = new List<DataPoint> ();

		int smoothIndexStart = 0;
		int smoothIndexEnd = 0;
		bool startIndexFound = false;

		for (int i=0; i<ballDataList.Count; i++) {
			DataPoint ball = ballDataList [i];
			if (ball.time >= startTime && !startIndexFound) {
				combinedBallsToSmooth.Add(ball);
				smoothIndexStart = i;
				startIndexFound = true;
			} else if (ball.time >= startTime && ball.time <= endTime) {
				combinedBallsToSmooth.Add(ball);
				smoothIndexEnd = i;
			}
		}

		DataPoint firstPoint = combinedBallsToSmooth [0];
		DataPoint lastPoint = combinedBallsToSmooth [combinedBallsToSmooth.Count -1];

		//Debug.Log ("----- Smoothing, smoothIndexStart: " + smoothIndexStart + ", smoothIndexEnd: " + smoothIndexEnd + ", firstPoint.time: " + firstPoint.time + ", lastPoint.time: " + lastPoint.time);

		if (combinedBallsToSmooth.Count > range * 2) {

			try {

				range = Math.Min (combinedBallsToSmooth.Count, range);

				float[] cleanX = CleanData (combinedBallsToSmooth.Select (x => x.position.x).ToArray (), range - 2, decay - 0.2f);
				float[] cleanY = CleanData (combinedBallsToSmooth.Select (x => x.position.y).ToArray (), range, decay);
				float[] cleanZ = CleanData (combinedBallsToSmooth.Select (x => x.position.z).ToArray (), range, decay);

				List<DataPoint> combinedBallPositionsSmooth = new List<DataPoint> ();

				for (int i = 0; i < combinedBallsToSmooth.Count; i++) {
					DataPoint ball = combinedBallsToSmooth [i];
					ballDataList [i + smoothIndexStart].position = new Vector3 (cleanX [i], cleanY [i], Mathf.Max (0, cleanZ [i]));
				}
			} catch (Exception e) {
				Debug.Log ("Exception caught: " + e.Message);
			}
		}
	}

	private float[] CleanData(float[] noisy, int range, float decay, bool averageEdges = false) {
		if (averageEdges) {
			return CleanDataAndAverageEdges (noisy, range, decay);
		} else {
			return CleanDataAndKeepEdges (noisy, range, decay);
		}
	}

	private float[] CleanDataAndKeepEdges(float[] noisy, int range, float decay) {
		float[] clean = new float[noisy.Length];
		float[] coefficients = Coefficients(range, decay);

		int evenNumberedOffset = clean.Length % 2 == 0 ? 1 : 0;

		for (int i = 0; i < clean.Length; i++) {

			int halfLength = clean.Length / 2;
			int distanceFromEdge = halfLength - Math.Abs (i - halfLength);
			if (i >= halfLength) {
				distanceFromEdge -= evenNumberedOffset;
			}

			float divisor = 0;
			float valueSum = 0;
			int elementsToSample = Math.Min (distanceFromEdge, range);

			for (int j = -elementsToSample; j <= elementsToSample; j++) {
				divisor += coefficients[Math.Abs(j)];
				valueSum += noisy [i + j] * coefficients[Math.Abs(j)];
			}
			clean[i] = valueSum / divisor;
		}

		return clean;
	}
		
	//http://www.dynamicnotions.net/2014/01/cleaning-noisy-time-series-data-low.html
	private float[] CleanDataAndAverageEdges(float[] noisy, int range, float decay) {
		float[] clean = new float[noisy.Length];
		float[] coefficients = Coefficients(range, decay);

		// Calculate divisor value.
		float divisor = 0;
		for (int i = -range; i <= range; i++)
			divisor += coefficients[Math.Abs(i)];

		// Clean main data.
		for (int i = range; i < clean.Length - range; i++)
		{
			float temp = 0;
			for (int j = -range; j <= range; j++)
				temp += noisy[i + j] * coefficients[Math.Abs(j)];
			clean[i] = temp / divisor;
		}

		// Calculate leading and trailing slopes.
		float leadSum = 0;
		float trailSum = 0;
		int leadRef = range;
		int trailRef = clean.Length - range - 1;
		for (int i = 1; i <= range; i++)
		{
			leadSum += (clean[leadRef] - clean[leadRef + i]) / i;
			trailSum += (clean[trailRef] - clean[trailRef - i]) / i;
		}
		float leadSlope = leadSum / range;
		float trailSlope = trailSum / range;

		// Clean edges.
		for (int i = 1; i <= range; i++)
		{
			clean[leadRef - i] = clean[leadRef] + leadSlope * i;
			clean[trailRef + i] = clean[trailRef] + trailSlope * i;
		}
		return clean;
	}

	static private float[] Coefficients(int range, float decay)
	{
		// Precalculate coefficients.
		float[] coefficients = new float[range + 1];
		for (int i = 0; i <= range; i++)
			coefficients[i] = Mathf.Pow(decay, i);
		return coefficients;
	}

#endregion

	public PlayEvent GetPlayEventForType(ePlayEventType playEventType) {
		PlayEvent playEventFound = null;

		for (int i=0; i<playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			if (playEvent.playEventType == playEventType) {
				playEventFound = playEvent;
				break;
			}
		}

		return playEventFound;
	}

	public PlayEvent GetPlayEventForTypeAfterTime(ePlayEventType playEventType, float time) {
		PlayEvent playEventFound = null;

		for (int i=0; i<playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			if (playEvent.playEventType == playEventType && playEvent.time >= time) {
				playEventFound = playEvent;
				break;
			}
		}

		return playEventFound;
	}

	public PlayEvent GetPlayEventForSegmentEvent(SegmentEvent segmentEvent) {
		PlayEvent playEventFound = null;

		for (int i=0; i<playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			if (segmentEvent.segmentType == eSegmentType.BaseballPitch && playEvent.playEventType == ePlayEventType.BALL_WAS_PITCHED) {
				playEventFound = playEvent;
				break;
			} else if (segmentEvent.segmentType == eSegmentType.BaseballHit && playEvent.playEventType == ePlayEventType.BALL_WAS_HIT) {
				playEventFound = playEvent;
				break;
			}
		}

		return playEventFound;
	}

	public SegmentEvent GetSegmentEventForType(eSegmentType segmentType) {
		return GetSegmentEventForTypeAfterTime(segmentType, 0);
	}

	public SegmentEvent GetSegmentEventForTypeAfterTime(eSegmentType segmentType, float startTime) {
		SegmentEvent segmentEventFound = null;

		for (int i=0; i<segmentEventsData.Count; i++) {
			SegmentEvent segmentEvent = segmentEventsData [i];
			if (segmentEvent.segmentType == segmentType && segmentEvent.time >= startTime) {
				segmentEventFound = segmentEvent;
				break;
			}
		}

		return segmentEventFound;
	}

	public SegmentEvent GetSegmentEventForPlayEvent(PlayEvent playEvent) {
		SegmentEvent segmentEventFound = null;

		for (int i=0; i<segmentEventsData.Count; i++) {
			SegmentEvent segmentEvent = segmentEventsData [i];
			if (segmentEvent.segmentType == eSegmentType.BaseballPitch && playEvent.playEventType == ePlayEventType.BALL_WAS_PITCHED) {
				segmentEventFound = segmentEvent;
				break;
			} else if (segmentEvent.segmentType == eSegmentType.BaseballHit && playEvent.playEventType == ePlayEventType.BALL_WAS_HIT) {
				segmentEventFound = segmentEvent;
				break;
			}
		}

		return segmentEventFound;
	}

	public void ListSegmentEvents() {
		if (segmentEventsData == null) {
			return;
		}

		string segmentEventsString = "";
		for (int i=0; i<segmentEventsData.Count; i++) {
			SegmentEvent segmentEvent = segmentEventsData [i];
			string suffix = i < playEventsData.Count - 1 ? ", " : "";
			segmentEventsString += segmentEvent.segmentType.ToString () + " (" + segmentEvent.time.ToString ("F3") + " - " + segmentEvent.endData.time + ")" + suffix;
		}
		Debug.Log ("Segments: " + segmentEventsString);
	}

	public void ListPlayEvents() {
		string playEventsString = "";
		for (int i=0; i<playEventsData.Count; i++) {
			PlayEvent playEvent = playEventsData [i];
			string suffix = i < playEventsData.Count - 1 ? ", " : "";
			playEventsString += playEvent.playEventType.ToString () + " (" + playEvent.time.ToString ("F3") + ")" + suffix;
		}
		Debug.Log ("Play events: " + playEventsString);
	}

	public List<StatEvent> GetStatEventsForPositionId(ePlayPositionIdentifier positionIdentifier) {
		List<StatEvent> playerStatEvents = new List<StatEvent> ();

		for (int i = 0; i < statEventsData.Count; i++) {
			if (statEventsData [i].targetId == (int)positionIdentifier) {
				playerStatEvents.Add (statEventsData [i]);
			}
		}
		return playerStatEvents;
	}

	public List<ePlayPositionIdentifier> GetLineupPositionIds() {
		List<ePlayPositionIdentifier> playerPositionIds = new List<ePlayPositionIdentifier> ();
		for (int i = 0; i < playerLineupData.Count; i++) {
			playerPositionIds.Add (playerLineupData [i].positionIdentifier);
		}
		return playerPositionIds;
	}

	public List<string> GetLineupMLBIds() {
		List<string> playerIds = new List<string> ();
		for (int i = 0; i < playerLineupData.Count; i++) {
			playerIds.Add (playerLineupData [i].playerId);
		}
		return playerIds;
	}

	public int GetNumberOfStatEventsForPositionId(ePlayPositionIdentifier positionIdentifier) {
		return GetStatEventsForPositionId(positionIdentifier).Count;
	}

	public bool IsHomeRun() {
		return gamePlayData.isHomeRun;
	}

	public bool IsHit() {
		return gamePlayData.isHit || gamePlayData.isActualHit;
	}
}

public enum ePlayPositionType {
    Unassigned,
    Players,
    Umpires,
    Coaches,
    Ball
}

public enum ePlayPositionIdentifier {
    Unknown,
    Pitcher,
    Catcher,
    FirstBaseman,
    SecondBaseman,
    ThirdBaseman,
    Shortstop,
    LeftFielder,
    CenterFielder,
    RightFielder,
    Batter,
    FirstBaseRunner,
    SecondBaseRunner,
    ThirdBaseRunner,
    HomePlateUmpire,
    FirstBaseUmpire,
    SecondBaseUmpire,
    ThirdBaseUmpire,
    FirstBaseCoach,
    ThirdBaseCoach,
    LeftFieldFoulLineUmpire,
    RightFieldFoulLineUmpire,
    Manager,
    BattingCoach,
    PitchingCoach,
    BenchCoach
}

//Custom events are usually used to take action before a event, such as scaling the ball down/up as it enters and leaves a player's possession
public enum ePlayEventType {
    BEGIN_OF_PLAY,
    PITCHER_GOING_TO_WINDUP,
    BALL_WAS_PITCHED,
    BALL_WAS_HIT,
    BALL_WAS_CAUGHT,
    BALL_WAS_CAUGHT_OUT,
    BALL_WAS_RELEASED,
    BALL_WAS_DEFLECTED,
    TAG_WAS_APPLIED,
    PICK_OFF_BALL_RELEASED,
    END_OF_PLAY,
    REMOVED,
    TRACKMAN_BALL_WAS_RELEASED,
    TRACKMAN_BALL_WAS_DEFLECTED,
    TRACKMAN_BALL_WAS_CATCHER_RELEASED,
    TRACKMAN_PICK_OFF_BALL_RELEASED,
    TRACKMAN_BALL_BOUNCE,
    BALL_WAS_FIELDED,
	CUSTOM,
	CUSTOM_BEFORE_PITCH,
	CUSTOM_BEFORE_THROW,
	CUSTOM_BALL_WAS_FIELDED,
	CUSTOM_THROW_WAS_CAUGHT,
	CUSTOM_ACTUAL_HIT,
	CUSTOM_APEX_OF_HIT
}

public class PlayEvent {
    public string timeStampString;
    public Double timeStampSeconds;
    public float time;

	public bool processed = false;

    public string playId;
    public string playEventId;

    public ePlayEventType playEventType;                    //playEvent (from the json data) seems to be equivalent to playEventType, only spelled out
    public ePlayPositionIdentifier playPositionIdentifier;  //Player that has the ball during this event
}

public enum eStatEventCategory {
	Pitching,
	Batting,
	Baserunning,
	Catcher_Fielding,
	Fielders_OF,
	Fielders_IF_OF,
	Fielders_C_P_IF_OF
}

public enum eStatEventType {
	None,
	Spin_Rate = 1000,
	Extension = 1001,
	Regular_Speed = 1002,
	Exit_Velocity = 1003,
	Launch_Vector = 1004,
	Launch_Angle = 1005,
	Pitcher_Release = 1006,
	First_Step = 1007,
	Top_Speed = 1008,
	Lead_Distance = 1009,
	Secondary_Lead_Distance = 1010,
	Acceleration = 1011,
	Speed_Dig = 1012,
	Extra_Bases_Home_To_Second = 1013,
	Extra_Bases_Home_To_Third = 1014,
	Extra_Bases_First_To_Third = 1015,
	Extra_Bases_First_To_Home = 1016,
	Extra_Bases_Second_To_Home = 1017,
	Arm_Strength = 1018,
	Positioning = 1019,
	First_Step_Stealing = 1020,
	First_Step_Efficiency = 1021,
	Route_Efficiency = 1022,
	Distance_Covered = 1023,
	Exchange = 1024,
	Pick_Off_Steal_Exchange = 1025,
	Pop_Time = 1026,
	Hang_Time = 1027,
	Perceived_Pitch_Velocity = 1028,
	Home_Run_Trot = 1029,
	Hit_Distance = 1030,
	Projected_Hit_Distance = 1031,
	Top_Acceleration = 1032,
	Hit_Type = 1033,
	Throwing_Accuracy = 1034,
	Time_To_Next_Base_First_To_Second = 1035,
	Time_To_Next_Base_Second_To_Third = 1036,
	Time_To_Next_Base_Third_To_Home = 1037,
	Throw_Distance = 1038,
	Max_Height = 1039,
	Base_Steal_Time_First_To_Second = 1040,
	Base_Steal_Time_Second_To_Third = 1041,
	Base_Steal_Time_Third_To_Home = 1042,
	Pitching_Position = 1043,
	Generated_Velocity = 1044,
	Estimated_Swing_Speed = 1045,
	Throw_Fielder_Angle = 1046,
	Backspin_Rate = 1047,
	Sidespin_Rate = 1048,
	Hit_Travel_Time = 1049,
	Hit_Travel_Distance = 1050
}

public enum eStatOverlayCategory {
	None,
	Custom,
	Pitching,
	Hitting_General,
	Hitting_HomeRun,
	Hitting_Apex,
	Fielding_General,
	Fielding_Throw,
	Fielding_Throw_Tag_Up,
	Baserunning_General,
	Baserunning_Steal
}

public class StatOverlay {
	public float time;
	public float finalDisplayTime = 4.75f;

	public bool processed = false;
	public bool shouldDisplay = true;

	public int targetId;
	public int targetMlbId;

	public ePlayPositionIdentifier positionIdentifier;

	public eStatOverlayCategory statOverlayCategory;         
	public List<StatEvent> statEventsAll;
	public List<StatEvent> statEvents;

	public PlayData playData;

	public StatOverlay(eStatOverlayCategory type, PlayData playDataToUse) {
		statEventsAll = new List<StatEvent> ();
		statEvents = new List<StatEvent> ();
		playData = playDataToUse;
		statOverlayCategory = type;
	}

	public void DetermineStatEventsDisplayTime() {
		if (statEvents.Count > 0) {
			statEvents [0].time = time + 0.35f;
		}

		if (statEvents.Count > 1) {
			for (int i = 1; i < statEvents.Count; i++) {
				statEvents [i].time = statEvents [i - 1].time + 1.5f;
			}
		}

		if (statOverlayCategory == eStatOverlayCategory.Hitting_Apex) {
			finalDisplayTime = 2.0f;
		}
	}

	public void DetermineStatOverlayCategory() {

		if (statOverlayCategory == eStatOverlayCategory.Fielding_General) {
			if (statEventsAll.Find (x => x.statEventType == eStatEventType.Arm_Strength) != null) {
				statOverlayCategory = eStatOverlayCategory.Fielding_Throw;
			}

			if (statEventsAll.Find (x => x.statEventType == eStatEventType.Pop_Time) != null) {
				statOverlayCategory = eStatOverlayCategory.Fielding_Throw_Tag_Up;
			}
		} else if (statOverlayCategory == eStatOverlayCategory.Hitting_General) {
			if (statEventsAll.Find (x => x.statEventType == eStatEventType.Home_Run_Trot) != null) {
				statOverlayCategory = eStatOverlayCategory.Hitting_HomeRun;
			}
		} else if (statOverlayCategory == eStatOverlayCategory.Baserunning_General) {
			if (statEventsAll.Find (x => x.statEventType == eStatEventType.Base_Steal_Time_First_To_Second) != null ||
				statEventsAll.Find (x => x.statEventType == eStatEventType.Base_Steal_Time_Second_To_Third) != null || 
				statEventsAll.Find (x => x.statEventType == eStatEventType.Base_Steal_Time_Third_To_Home) != null) {

				statOverlayCategory = eStatOverlayCategory.Baserunning_Steal;
			}
		}
	}

	public void PopulateStatEvents() {

		if (statOverlayCategory == eStatOverlayCategory.Pitching) {
			AddStatEventType (eStatEventType.Regular_Speed);
			AddStatEventType (eStatEventType.Extension);
			AddStatEventType (eStatEventType.Perceived_Pitch_Velocity);
			AddStatEventType (eStatEventType.Spin_Rate);
			AddStatEventType (eStatEventType.Pitcher_Release);
		} else if (statOverlayCategory == eStatOverlayCategory.Hitting_General) {
			AddStatEventType (eStatEventType.Exit_Velocity);
			AddStatEventType (eStatEventType.Launch_Angle);
		} else if (statOverlayCategory == eStatOverlayCategory.Hitting_HomeRun) {
			AddStatEventType (eStatEventType.Exit_Velocity);
			AddStatEventType (eStatEventType.Launch_Angle);
			AddStatEventType (eStatEventType.Hit_Distance);
			AddStatEventType (eStatEventType.Projected_Hit_Distance);
		} else if (statOverlayCategory == eStatOverlayCategory.Hitting_Apex) {
			AddStatEventType (eStatEventType.Hang_Time);
			AddStatEventType (eStatEventType.Projected_Hit_Distance);
		} else if (statOverlayCategory == eStatOverlayCategory.Fielding_General) {
			AddStatEventType (eStatEventType.Top_Speed);
			AddStatEventType (eStatEventType.Distance_Covered);
			AddStatEventType (eStatEventType.Route_Efficiency);
		} else if (statOverlayCategory == eStatOverlayCategory.Fielding_Throw) {
			AddStatEventType (eStatEventType.Arm_Strength);
			AddStatEventType (eStatEventType.Throw_Distance);
		} else if (statOverlayCategory == eStatOverlayCategory.Fielding_Throw_Tag_Up) {
			AddStatEventType (eStatEventType.Arm_Strength);
			AddStatEventType (eStatEventType.Pop_Time);
		} else if (statOverlayCategory == eStatOverlayCategory.Baserunning_General) {
			AddStatEventType (eStatEventType.Extra_Bases_First_To_Home);
			AddStatEventType (eStatEventType.Extra_Bases_Second_To_Home);
			AddStatEventType (eStatEventType.Extra_Bases_Home_To_Second);
			AddStatEventType (eStatEventType.Extra_Bases_Home_To_Third);
			AddStatEventType (eStatEventType.Time_To_Next_Base_First_To_Second);
			AddStatEventType (eStatEventType.Time_To_Next_Base_Second_To_Third);
			AddStatEventType (eStatEventType.Time_To_Next_Base_Third_To_Home);
			AddStatEventType (eStatEventType.Secondary_Lead_Distance);
			AddStatEventType (eStatEventType.Top_Speed);
		} else if (statOverlayCategory == eStatOverlayCategory.Baserunning_Steal) {
			AddStatEventType (eStatEventType.Lead_Distance);
			AddStatEventType (eStatEventType.Base_Steal_Time_First_To_Second);
			AddStatEventType (eStatEventType.Base_Steal_Time_Second_To_Third);
			AddStatEventType (eStatEventType.Base_Steal_Time_Third_To_Home);
		}
	}

	private void AddStatEventType(eStatEventType statEventType) {
		StatEvent statEvent = statEventsAll.Find (x => x.statEventType == statEventType);
		if (statEvent != null && statEvents.Count < 4) {
			statEvents.Add (statEvent);
		}
	}

	private StatEvent GetStatEventTypeIfPresent(eStatEventType statEventType) {
		return statEventsAll.Find (x => x.statEventType == statEventType);
	}
}

public class StatEvent {
    public float time;
    public bool processed = false;

    public int targetId;
    public int targetMlbId;

    public int statEventTypeInt;
    public eStatEventType statEventType;

    public string statEventCategoryString;
    public eStatEventCategory statEventCategory;

    public string nameString;
    public string unit;

    public string description = "";
    public string valueString;

    public string hitType;
    public string hitTravelDistance;

    public StatEvent()
    {

    }

    public StatEvent(StatEvent statEvent)
    {
        time = statEvent.time;
        processed = statEvent.processed;

        targetId = statEvent.targetId;
        targetMlbId = statEvent.targetMlbId;

        statEventTypeInt = statEvent.statEventTypeInt;
        statEventType = statEvent.statEventType;

        statEventCategoryString = statEvent.statEventCategoryString;
        statEventCategory = statEvent.statEventCategory;

        nameString = statEvent.nameString;
        unit = statEvent.unit;

        description = statEvent.description;
        valueString = statEvent.valueString;

        hitType = statEvent.hitType;
        hitTravelDistance = statEvent.hitTravelDistance;
    }

	public void DetermineStatEventCategory() {
		statEventCategory = eStatEventCategory.Fielders_C_P_IF_OF;
		if (statEventCategoryString.Equals("Pitching")) {
			statEventCategory = eStatEventCategory.Pitching;
		} else if (statEventCategoryString.Equals("Batting")) {
			statEventCategory = eStatEventCategory.Batting;
		} else if (statEventCategoryString.Equals("Baserunning")) {
			statEventCategory = eStatEventCategory.Baserunning;
		} else if (statEventCategoryString.Equals("Fielders (IF / OF)")) {
			statEventCategory = eStatEventCategory.Fielders_IF_OF;
		} else if (statEventCategoryString.Equals("Fielders (OF)")) {
			statEventCategory = eStatEventCategory.Fielders_OF;
		} else if (statEventCategoryString.Equals("Fielders (C / P / IF / OF)")) {
			statEventCategory = eStatEventCategory.Fielders_C_P_IF_OF;
		} else if (statEventCategoryString.Equals("Catcher Fielding")) {
			statEventCategory = eStatEventCategory.Catcher_Fielding;
		} else {
			Debug.LogError ("eStatEventCategory not found for: " + statEventCategoryString + ", " + nameString + ", " + description);
		}
	}

	public void UpdateUnitStrings() {
		unit = unit.ToUpper ();

		if (unit.Equals ("SECONDS")) {
			unit = "SEC";
		} else if (unit.Equals ("FEET")) {
			unit = "FT";
		} else if (unit.Equals ("METERS PER SQUARE SECOND")) {
			unit = "SEC";
		} else if (unit.Equals ("STRING")) {
			unit = "";
		} else if (unit.Equals ("DEGREES")) {
			unit = "DEG";
		} else if (unit.Equals ("PERCENTAGE")) {
			unit = "%";
		}
	}

	public string GetValueDisplayString() {
		string toReturn = valueString;
		string stringFormat = "F2";
		float valueFloat = 0;

		if (statEventType == eStatEventType.Spin_Rate) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Extension) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Regular_Speed) {	
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Exit_Velocity) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Launch_Vector) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Launch_Angle) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Pitcher_Release) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.First_Step) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Top_Speed) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Lead_Distance) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Secondary_Lead_Distance) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Acceleration) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Speed_Dig) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Extra_Bases_Home_To_Second) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Extra_Bases_Home_To_Third) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Extra_Bases_First_To_Third) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Extra_Bases_First_To_Home) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Extra_Bases_Second_To_Home) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Arm_Strength) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Positioning) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.First_Step_Stealing) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.First_Step_Efficiency) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Route_Efficiency) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Distance_Covered) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Exchange) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Pick_Off_Steal_Exchange) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Pop_Time) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Hang_Time) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Perceived_Pitch_Velocity) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Home_Run_Trot) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Hit_Distance) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Projected_Hit_Distance) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Top_Acceleration) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Hit_Type) {
			stringFormat = "G";
		} else if (statEventType == eStatEventType.Throwing_Accuracy) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_First_To_Second) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_Second_To_Third) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_Third_To_Home) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Throw_Distance) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Max_Height) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Base_Steal_Time_First_To_Second) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Base_Steal_Time_Second_To_Third) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Base_Steal_Time_Third_To_Home) {
			stringFormat = "F2";
		} else if (statEventType == eStatEventType.Pitching_Position) {
			stringFormat = "G";
		} else if (statEventType == eStatEventType.Generated_Velocity) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Estimated_Swing_Speed) {
			stringFormat = "F1";
		} else if (statEventType == eStatEventType.Throw_Fielder_Angle) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Backspin_Rate) {
			stringFormat = "F0";
		} else if (statEventType == eStatEventType.Sidespin_Rate) {
			stringFormat = "F0";
		}

		float.TryParse (valueString, out valueFloat);
		toReturn = valueFloat.ToString (stringFormat);

		return toReturn;
	}

	public string GetDisplayName() {
		string toReturn = "";

		if (statEventType == eStatEventType.None) {
			toReturn = "[NONE]";
		} else if (statEventType == eStatEventType.Hit_Travel_Time) {
			toReturn = "Hit Travel Time";
		} else if (statEventType == eStatEventType.Hit_Travel_Distance) {
			toReturn = "Hit Travel Distance";
		} else if (statEventType == eStatEventType.Spin_Rate) {
			toReturn = "Spin Rate";
		} else if (statEventType == eStatEventType.Extension) {
			toReturn = "Extension";
		} else if (statEventType == eStatEventType.Regular_Speed) {	
			toReturn = "Velocity";
		} else if (statEventType == eStatEventType.Exit_Velocity) {
			toReturn = "Exit Velocity";
		} else if (statEventType == eStatEventType.Launch_Vector) {
			toReturn = "Launch Vector";
		} else if (statEventType == eStatEventType.Launch_Angle) {
			toReturn = "Launch Angle";
		} else if (statEventType == eStatEventType.Pitcher_Release) {
			toReturn = "Pitcher Release";
		} else if (statEventType == eStatEventType.First_Step) {
			toReturn = "First Step";
		} else if (statEventType == eStatEventType.Top_Speed) {
			toReturn = "Max Speed";
		} else if (statEventType == eStatEventType.Lead_Distance) {
			toReturn = "Lead Distance";
		} else if (statEventType == eStatEventType.Secondary_Lead_Distance) {
			toReturn = "Secondary Lead Distance";
		} else if (statEventType == eStatEventType.Acceleration) {
			toReturn = "Acceleration";
		} else if (statEventType == eStatEventType.Speed_Dig) {
			toReturn = "Home to 1st";
		} else if (statEventType == eStatEventType.Extra_Bases_Home_To_Second) {
			toReturn = "Home to 2nd";
		} else if (statEventType == eStatEventType.Extra_Bases_Home_To_Third) {
			toReturn = "Home to 3rd";
		} else if (statEventType == eStatEventType.Extra_Bases_First_To_Third) {
			toReturn = "1st to 3rd";
		} else if (statEventType == eStatEventType.Extra_Bases_First_To_Home) {
			toReturn = "1st to Home";
		} else if (statEventType == eStatEventType.Extra_Bases_Second_To_Home) {
			toReturn = "2nd to Home";
		} else if (statEventType == eStatEventType.Arm_Strength) {
			toReturn = "Arm Strength";
		} else if (statEventType == eStatEventType.Positioning) {
			toReturn = "Positioning";
		} else if (statEventType == eStatEventType.First_Step_Stealing) {
			toReturn = "First Step";
		} else if (statEventType == eStatEventType.First_Step_Efficiency) {
			toReturn = "First Step Efficiency";
		} else if (statEventType == eStatEventType.Route_Efficiency) {
			toReturn = "Route Efficiency";
		} else if (statEventType == eStatEventType.Distance_Covered) {
			toReturn = "Distance Covered";
		} else if (statEventType == eStatEventType.Exchange) {
			toReturn = "Exchange";
		} else if (statEventType == eStatEventType.Pick_Off_Steal_Exchange) {
			toReturn = "Pick-Off Steal Exchange";
		} else if (statEventType == eStatEventType.Pop_Time) {
			toReturn = "Pop Time";
		} else if (statEventType == eStatEventType.Hang_Time) {
			toReturn = "Hang Time";
		} else if (statEventType == eStatEventType.Perceived_Pitch_Velocity) {
			toReturn = "Perceived Velocity";
		} else if (statEventType == eStatEventType.Home_Run_Trot) {
			toReturn = "Home Run Trot";
		} else if (statEventType == eStatEventType.Hit_Distance) {
			toReturn = "Hit Distance";
		} else if (statEventType == eStatEventType.Projected_Hit_Distance) {
			toReturn = "Projected Hit Distance";
			if (ScheduleDataManager.instance.currentPlayData != null && ScheduleDataManager.instance.currentPlayData.IsHomeRun ()) {
				toReturn = "Projected HR Distance";
			}
		} else if (statEventType == eStatEventType.Top_Acceleration) {
			toReturn = "Top Acceleration";
		} else if (statEventType == eStatEventType.Hit_Type) {
			toReturn = "Hit Type";
		} else if (statEventType == eStatEventType.Throwing_Accuracy) {
			toReturn = "Throwing Accuracy";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_First_To_Second) {
			toReturn = "1st to 2nd";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_Second_To_Third) {
			toReturn = "2nd to 3rd";
		} else if (statEventType == eStatEventType.Time_To_Next_Base_Third_To_Home) {
			toReturn = "3rd to Home";
		} else if (statEventType == eStatEventType.Throw_Distance) {
			toReturn = "Throw Distance";
		} else if (statEventType == eStatEventType.Max_Height) {
			toReturn = "Max Height";
		} else if (statEventType == eStatEventType.Base_Steal_Time_First_To_Second) {
			toReturn = "1st to 2nd";
		} else if (statEventType == eStatEventType.Base_Steal_Time_Second_To_Third) {
			toReturn = "2nd to 3rd";
		} else if (statEventType == eStatEventType.Base_Steal_Time_Third_To_Home) {
			toReturn = "3rd to Home";
		} else if (statEventType == eStatEventType.Pitching_Position) {
			toReturn = "Pitching Position";
		} else if (statEventType == eStatEventType.Generated_Velocity) {
			toReturn = "Generated Velocity";
		} else if (statEventType == eStatEventType.Estimated_Swing_Speed) {
			toReturn = "Estimated Swing Speed";
		} else if (statEventType == eStatEventType.Throw_Fielder_Angle) {
			toReturn = "Throw Angle";
		} else if (statEventType == eStatEventType.Backspin_Rate) {
			toReturn = "Backspin Rate";
		} else if (statEventType == eStatEventType.Sidespin_Rate) {
			toReturn = "Sidespin Rate";
		}

		return toReturn;
	}
}

public enum eSegmentType {
	Unknown,
	BaseballPitch,
	BaseballPickoff,
	BaseballHit,
	BaseballCatcherThrow,
	BaseballBounce,
	BaseballDeflection,
	BaseballThrow
}

public class SegmentEventData {
	public float time;
	public float speed;
	public Vector3 position;
	public Vector3 velocity;

	public SegmentEventData(Dictionary<string,object> segmentEventData) {
		time = float.Parse(segmentEventData["Time"].ToString()) + 3.0f;
		speed = float.Parse(segmentEventData["Speed"].ToString());

		Dictionary<string,object> startDataItemVelocity = segmentEventData["Velocity"] as Dictionary<string,object>;
		velocity = new Vector3 (float.Parse(startDataItemVelocity ["X"].ToString()), float.Parse(startDataItemVelocity ["Y"].ToString()), float.Parse(startDataItemVelocity ["Z"].ToString()));

		Dictionary<string,object> startDataItemPosition = segmentEventData["Position"] as Dictionary<string,object>;
		position = new Vector3 (float.Parse(startDataItemPosition ["X"].ToString()), float.Parse(startDataItemPosition ["Y"].ToString()), float.Parse(startDataItemPosition ["Z"].ToString()));
	}
}

public class SegmentEvent {
	public eSegmentType segmentType;
	public List<string> reducedConfidence;

	public float time;					//Same as startData.time

	public SegmentEventData startData;
	public SegmentEventData endData;

	public List<float> trajectoryPolynomialX;
	public List<float> trajectoryPolynomialY;
	public List<float> trajectoryPolynomialZ;

	public bool processed;

	public float Duration() {
		return startData.time - endData.time;
	}
}

public class PositionalData {
    public string timeStampString;
    public Double timeStampSeconds;

    public List<PlayPosition> positions;
    //So the json data is organized by time stamps, and then within each time stamp there's positonal data for players, umps, etc.
    //It's not ideal because there's no garauntee that each time stamp will include an element for each player, ump, what have you.
    //This is ok for the POC but this should be reorganized, likely into a list of player data by positionIdentifier, containing time coded positonal data.
    //That should make stepping through more straightforward and also make it easier to interpolate across missing data.
    //And yes, it's confusing that the word position is used both here and there!

    public float time;      //Note, this is timeStampSeconds minus beginOfPlayTimeStampSeconds to provide an offset that correlates to BallPosition.time

	public PlayPosition GetPlayPositionForPositionIdentifier(ePlayPositionIdentifier playPositionIdentifier) {
		PlayPosition playPositionFound = null;
		for (int i = 0; i < positions.Count; i++) {
			if (positions [i].playPositionIdentifier == playPositionIdentifier) {
				playPositionFound = positions [i];
				break;
			}
		}

		return playPositionFound;
	}
}

public class PlayPosition {
    public Vector3 position;
    public ePlayPositionType playPositionType;
    public ePlayPositionIdentifier playPositionIdentifier;            
}

public enum eBallMeasurementType {
    None,
    Measured,
    Estimated
}

public class BallPositionalData {
    public List<BallPosition> ballPositions;
}

public class BallPosition {
    public float timeCodeOffset;
    public eBallMeasurementType ballMeasurementType;
    public Vector3 position;
  
    public Double timeCode;
    public float time;
    public Vector3 velocity;

}
	
public enum eDataSource {
	Unknown,
	Trackman,
	ChyronHego,
	Polynomial
}

public class DataPoint {
	public Vector3 position;
	public float time;

	public bool outOfView = false;
	public float realiabilityFactor = 1.0f;	//Score from 0 to 1.0f with zero being completely unrealible

	public eDataSource dataSource;

	public int TimeMS
	{
		get { return Mathf.RoundToInt(time * 1000f); }
	}

	public DataPoint (Vector3 pos, float timeValue) {
		position = pos;
		time = timeValue;
	}
}
