using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BallparkManager : MonoBehaviour {

	public List<eTeamVenueId> ballparksAvailable;

	public eTeamId defaultBallparkTeam;

	private static BallparkManager instance;
	public static BallparkManager Instance
	{
		get { return instance; }
	}

	void Awake () {
		instance = this;
	}

	public static bool IsBallParkAvailableForTeam(eTeamId teamId) {
		eTeamVenueId venueId = GetVenueIdForTeam(teamId);
		return instance.ballparksAvailable.Contains (venueId);
	}

    public static void LoadDefaultBallpark() {
		LoadBallparkForTeam(instance.defaultBallparkTeam);
    }

	public static void LoadBallparkForTeam(eTeamId teamId) {
		eTeamVenueId venueId = GetVenueIdForTeam(teamId);
		string ballparkName = "Ballparks/" + venueId.ToString();
		LoadBallpark(ballparkName);
	}

    public static void LoadBallpark(string ballparkName) {
        //SceneManager.LoadScene(ballparkName, LoadSceneMode.Additive);
        Application.LoadLevelAdditive(ballparkName);
    }

	public static void UnloadBallpark(eTeamVenueId venueId) {
		string ballparkName = "Ballparks/" + venueId.ToString();
		UnloadBallpark(ballparkName);
	}

	public static void UnloadBallpark(string ballparkName) {
		SceneManager.UnloadScene (ballparkName);
	}

    void OnLevelWasLoaded(int level) {
        Debug.Log("OnLevelWasLoaded called, level: " + level);
    }

	public static eTeamVenueId GetDefaultVenueId() {
		return GetVenueIdForTeam (instance.defaultBallparkTeam);
	}

    public static eTeamVenueId GetVenueIdForTeam(eTeamId teamId) {

        eTeamVenueId venueId = eTeamVenueId.Unknown;

        switch (teamId) {
            case eTeamId.AngelsofAnaheim:
                venueId = eTeamVenueId.LosAngelesAngels;
                break;
            case eTeamId.Astros:
                venueId = eTeamVenueId.HoustonAstros;
                break;
            case eTeamId.Athletics:
                venueId = eTeamVenueId.OaklandAthletics;
                break;
            case eTeamId.BlueJays:
                venueId = eTeamVenueId.TorontoBlueJays;
                break;
            case eTeamId.Braves:
                venueId = eTeamVenueId.AtlantaBraves;
                break;
            case eTeamId.Brewers:
                venueId = eTeamVenueId.MilwaukeeBrewers;
                break;
            case eTeamId.Cardinals:
                venueId = eTeamVenueId.StLouisCardinals;
                break;
            case eTeamId.Cubs:
                venueId = eTeamVenueId.ChicagoCubs;
                break;
            case eTeamId.Diamondbacks:
                venueId = eTeamVenueId.ArizonaDiamondbacks;
                break;
            case eTeamId.Dodgers:
                venueId = eTeamVenueId.LosAngelesDodgers;
                break;
            case eTeamId.Giants:
                venueId = eTeamVenueId.SanFranciscoGiants;
                break;
            case eTeamId.Indians:
                venueId = eTeamVenueId.ClevelandIndians;
                break;
            case eTeamId.Mariners:
                venueId = eTeamVenueId.SeattleMariners;
                break;
            case eTeamId.Marlins:
                venueId = eTeamVenueId.MiamiMarlins;
                break;
            case eTeamId.Mets:
                venueId = eTeamVenueId.NewYorkMets;
                break;
            case eTeamId.Nationals:
                venueId = eTeamVenueId.WashingtonNationals;
                break;
            case eTeamId.Orioles:
                venueId = eTeamVenueId.BaltimoreOrioles;
                break;
            case eTeamId.Padres:
                venueId = eTeamVenueId.SanDiegoPadres;
                break;
            case eTeamId.Phillies:
                venueId = eTeamVenueId.PhiladelphiaPhillies;
                break;
            case eTeamId.Pirates:
                venueId = eTeamVenueId.PittsburghPirates;
                break;
            case eTeamId.Rangers:
                venueId = eTeamVenueId.TexasRangers;
                break;
            case eTeamId.Rays:
                venueId = eTeamVenueId.TampaBayRays;
                break;
            case eTeamId.Reds:
                venueId = eTeamVenueId.CincinnatiReds;
                break;
            case eTeamId.RedSox:
                venueId = eTeamVenueId.BostonRedSox;
                break;
            case eTeamId.Rockies:
                venueId = eTeamVenueId.ColoradoRockies;
                break;
            case eTeamId.Royals:
                venueId = eTeamVenueId.KansasCityRoyals;
                break;
            case eTeamId.Tigers:
                venueId = eTeamVenueId.DetroitTigers;
                break;
            case eTeamId.Twins:
                venueId = eTeamVenueId.MinnesotaTwins;
                break;
            case eTeamId.WhiteSox:
                venueId = eTeamVenueId.ChicagoWhiteSox;
                break;
            case eTeamId.Yankees:
                venueId = eTeamVenueId.NewYorkYankees;
                break;
            default:
                venueId = eTeamVenueId.SanDiegoPadres;
                break;
        }

        return venueId;
    }
}

public enum eTeamVenueId {
    Unknown = 0,
    BaltimoreOrioles = 2,
    BostonRedSox = 3,
    NewYorkYankees = 3313,
    TampaBayRays = 12,
    TorontoBlueJays = 14,
    ChicagoWhiteSox = 4,
    ClevelandIndians = 5,
    DetroitTigers = 2394,
    KansasCityRoyals = 7,
    MinnesotaTwins = 3312,
    HoustonAstros = 2392,
    LosAngelesAngels = 1,
    OaklandAthletics = 10,
    SeattleMariners = 680,
    TexasRangers = 13,
    AtlantaBraves = 16,
    MiamiMarlins = 4169,
    NewYorkMets = 3289,
    PhiladelphiaPhillies = 2681,
    WashingtonNationals = 3309,
    ChicagoCubs = 17,
    CincinnatiReds = 2602,
    MilwaukeeBrewers = 32,
    PittsburghPirates = 31,
    StLouisCardinals = 2889,
    ArizonaDiamondbacks = 15,
    ColoradoRockies = 19,
    LosAngelesDodgers = 22,
    SanDiegoPadres = 2680,
    SanFranciscoGiants = 2395
}
