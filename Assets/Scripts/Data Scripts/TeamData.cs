using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TeamDataObject : MonoBehaviour {

	public Color teamColorPrimary;
	public Color teamColorSecondary;

    public TeamData teamInfo;

    public TeamDataObject(eTeamId teamToSet, int iWins, int iLose)
    {
        teamInfo = new TeamData(teamToSet, iWins, iLose);
    }

	public static eTeamAbbrv GetTeamAbbrvForTeam(eTeam team) {
		Array enumValues = Enum.GetValues (team.GetType ());
		int index = Array.IndexOf (enumValues, team);
		return (eTeamAbbrv)index;
	}

    public static Sprite GetTeamLogoSprite(eTeamId teamId) {
        return GetTeamLogoSprite (teamId.ToString());
    }

    public static Sprite GetTeamLogoSprite(eTeamId teamId, Vector2 imageDimensions) {
        return GetTeamLogoSprite (teamId.ToString(), imageDimensions);
    }

    public static Sprite GetTeamLogoSprite(string teamName) {
        return GetTeamLogoSprite (teamName, new Vector2 (256.0f, 256.0f));
    }

    public static Sprite GetTeamLogoSprite(string teamName, Vector2 imageDimensions) {
        //Logger.Log ("Utilities, GetTeamLogoSprite called, teamName: " + teamName + ", imageDimensions: " + imageDimensions);
        
        /*Sprite sprite = null;
        string teamTexture = "TeamLogos/" + teamName;

        Texture2D inputTexture = (Texture2D)(Texture2D)Resources.Load(teamTexture) as Texture2D;
		if (inputTexture != null) {
			inputTexture.filterMode = FilterMode.Trilinear;

			sprite = Sprite.Create (inputTexture,
				new Rect (0, 0, imageDimensions.x, imageDimensions.y),
				new Vector2 (0, 0),
				100.0f);
		}
        return sprite;*/
        return StaticSpriteAtlasManager.ReturnTeamLogo(teamName);
    }
}

[System.Serializable]
public class TeamData
{
    public eTeam team;
    public eTeamId teamId;
    public eTeamAbbrv teamAbbrv;
    public int? wins;
    public int? lost;

    public TeamData(eTeamId eteamID, int? iWins, int? iLost)
    {
        teamId = eteamID;
        team = (eTeam)Enum.Parse(typeof(eTeam),eteamID.ToString());
        wins = iWins;
        lost = iLost;
    }

    public static string ReturnTeamAbv(string teamName)
    {
//        Debug.Log("Switching " + teamName.Replace(" ", "").ToLower());
        switch(teamName.Replace(" ","").ToLower())
        {
            case "angelsofanaheim":
                return "ana";
            case "bluejays":
                return "tor";
            case "braves":
                return "atl";
            case "majorleaguebaseball":
                return "mlb";
            //TODO Implement Rest
            default:
                return null;
        }
    }
}

public enum eTeam {
    None = -1,
    Orioles = 0,
    RedSox,
    Yankees,
    Rays,
    BlueJays,
    WhiteSox,
    Indians,
    Tigers,
    Royals,
    Twins,
    Astros,
    AngelsofAnaheim,
    Athletics,
    Mariners,
    Rangers,
    Braves,
    Marlins,
    Mets,
    Phillies,
    Nationals,
    Cubs,
    Reds,
    Brewers,
    Pirates,
    Cardinals,
    Diamondbacks,
    Rockies,
    Dodgers,
    Padres,
    Giants,
    Length
}

public enum eTeamId {
    None = -1,
    Orioles = 110,
    RedSox = 111,
    Yankees = 147,
    Rays = 139,
    BlueJays = 141,
    WhiteSox = 145,
    Indians = 114,
    Tigers = 116,
    Royals = 118,
    Twins = 142,
    Astros = 117,
    AngelsofAnaheim = 108,
    Athletics = 133,
    Mariners = 136,
    Rangers = 140,
    Braves = 144,
    Marlins = 146,
    Mets = 121,
    Phillies = 143,
    Nationals = 120,
    Cubs = 112,
    Reds = 113,
    Brewers = 158,
    Pirates = 134,
    Cardinals = 138,
    Diamondbacks = 109,
    Rockies = 115,
    Dodgers = 119,
    Padres = 135,
    Giants = 137
}

public enum eTeamAbbrv {
    None,
    BAL,
    BOS,
    NYY,
    TB,
    TOR,
    CWS,
    CLE,
    DET,
    KC,
    MIN,
    HOU,
    LAA,
    OAK,
    SEA,
    TEX,
    ATL,
    MIA,
    NYM,
    PHI,
    WSH,
    CHC,
    CIN,
    MIL,
    PIT,
    STL,
    ARI,
    COL,
    LAD,
    SD,
    SF
}
