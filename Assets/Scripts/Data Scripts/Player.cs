using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
   
	public ePlayPositionIdentifier positionIdentifier;	//Where they are on the field (e.g. Batter, Catcher, Runner on 3rd, etc), not where they normally play while on defense. 
	public ePlayPositionType positionType;

	public string playerId;
	public PlayerData playerData;

	public eTeamId teamId;
	public TeamDataObject teamData;

	private bool isHome = false;

	//Things to color
	public Material playerTrailHome;
	public Material playerTrailAway;

	//public PigeonCoopToolkit.Effects.Trails.Trail trail;
	public Light downLight;

	public Material volumeGlowHome;
	public Material volumeGlowAway;
	public Material playerEmissionHome;
	public Material playerEmissionAway;
	public Material playerEmissionAnimHome;
	public Material playerEmissionAnimAway;
	public Material dustFXHome;
	public Material dustFXAway;

	public MeshRenderer playerBase;
	public MeshRenderer playerVolumeGlow;
	public MeshRenderer playerBaseRotator;
	public MeshRenderer playerHomePlateEdge;
	public MeshRenderer playerHomePlate;
	public MeshRenderer playerVolumeParticle;


	/*public VLight vLight;
	public UISprite playerBacking;
	public UISprite playerGrid;
	public UITexture teamBacking;

	public UITexture playerImage;
	public UITexture teamImage;*/

	public GameObject playerContainer;
	public GameObject activeStatePlayerBase;
	public GameObject activeStatePlayer;

	/*public LookAtTarget lookAtTarget;

	public UILabel playerPosition;
	public UILabel playerJerseyNumber;
	public UILabel playerNameFirst;
	public UILabel playerNameLast;*/

	public bool useSecondaryColor = false;

	public Transform gazeTarget;

	//public UIPanel uiPanel;
	public float distanceFromCam;

    string m_PlayID;
    string m_LastPlayID;

    [HideInInspector]
    public string alternatePositionName;

    [HideInInspector]
    public string alternateNumber;

	public void UpdateLookAtTarget(Transform targetTransform) {
		//lookAtTarget.target = targetTransform;
	}

	public void ActivatePlayer(bool activate) {
		activeStatePlayer.SetActive (activate);
		activeStatePlayerBase.SetActive (activate);
	}

	public void ActivatePlayer() {
		activeStatePlayer.SetActive (true);
		activeStatePlayerBase.SetActive (true);
	}

	public void DeactivatePlayer() {
		activeStatePlayer.SetActive (false);
		activeStatePlayerBase.SetActive (false);
	}
		
	public void UpdateMaterials(bool isHomeTeam, eTeamId teamIdToUse) {
		/*teamData = DataManager.instance.GetTeamDataForTeamId (teamIdToUse);
		isHome = isHomeTeam;

		if (teamData == null) {
			if (isHome) {
				//Debug.Log ("teamData == null, using Padres instead");
				teamData = DataManager.instance.GetTeamDataForTeamId (eTeamId.Padres);
			} else {
				//Debug.Log ("teamData == null, using Brewers instead");
				teamData = DataManager.instance.GetTeamDataForTeamId (eTeamId.Brewers);
			}
		}*/
			
		Color teamColorToUse = useSecondaryColor ? teamData.teamColorSecondary : teamData.teamColorPrimary;

		if (teamData == null) {
			return;
		}

		downLight.color = Color.white;

		/*vLight.colorTint = teamColorToUse;
		playerBacking.color = teamColorToUse;
		playerGrid.color = teamColorToUse;
		teamBacking.color = teamColorToUse;*/

		return;

		if (isHomeTeam) {

			playerTrailHome.color = teamColorToUse;
			playerTrailHome.SetColor ("_EmissionColor", teamColorToUse);

			//trail.UpdateColor(teamColorToUse, playerTrailHome);

			volumeGlowHome.color = teamColorToUse;
			volumeGlowHome.SetColor ("_TintColor", teamColorToUse);

			playerEmissionHome.color = teamColorToUse;
			playerEmissionHome.SetColor ("_EmissionColor", teamColorToUse);
			playerEmissionHome.SetColor ("_DiffuseColor", teamColorToUse);

			playerEmissionAnimHome.color = teamColorToUse;
			playerEmissionAnimHome.SetColor ("_TintColor", teamColorToUse);

			dustFXHome.color = teamColorToUse;
			dustFXHome.SetColor ("_TintColor", teamColorToUse);

			//The home materials are assigned by default.... no need to make changes

		} else {

			playerTrailAway.color = teamColorToUse;
			playerTrailAway.SetColor ("_EmissionColor", teamColorToUse);

			//trail.UpdateColor(teamColorToUse, playerTrailAway);

			volumeGlowAway.color = teamColorToUse;
			volumeGlowAway.SetColor ("_TintColor", teamColorToUse);

			playerEmissionAway.color = teamColorToUse;
			playerEmissionAway.SetColor ("_EmissionColor", teamColorToUse);
			playerEmissionAway.SetColor ("_DiffuseColor", teamColorToUse);

			playerEmissionAnimAway.color = teamColorToUse;
			playerEmissionAnimAway.SetColor ("_TintColor", teamColorToUse);

			dustFXAway.color = teamColorToUse;
			dustFXAway.SetColor ("_TintColor", teamColorToUse);

			ReplaceMaterial (playerBase, playerEmissionAway, 1);

			playerVolumeGlow.material = volumeGlowAway;
			playerBaseRotator.material = playerEmissionAnimAway;
			playerHomePlateEdge.material = playerEmissionAnimAway;

			ReplaceMaterial (playerHomePlate, playerEmissionAway, 1);
			ReplaceMaterial (playerHomePlate, playerEmissionAnimAway, 2);

			playerVolumeParticle.material = dustFXAway;
		}

	}

	private void ReplaceMaterial(MeshRenderer meshRenderer, Material replacementMaterial, int index) {
		Material[] newMaterials = new Material[meshRenderer.materials.Length];
		for(int i=0; i<meshRenderer.materials.Length; i++){
			if (i == index) {
				newMaterials [i] = replacementMaterial;
			} else {
				newMaterials [i] = meshRenderer.materials [i];
			}
		}
		meshRenderer.materials = newMaterials;
	}

	public void SetupPlayer(string playerIdToSetup, eTeamId teamIdToUse, bool loadPlayerData) {
		playerId = playerIdToSetup;
		teamId = teamIdToUse;
		//teamData = DataManager.instance.GetTeamDataForTeamId (teamId);

		if (teamData == null) {
			if (isHome) {
				//Debug.Log ("teamData == null, using Padres instead");
				teamId = eTeamId.Padres;
			} else {
				//Debug.Log ("teamData == null, using Brewers instead");
				teamId = eTeamId.Brewers;
			}
			//teamData = DataManager.instance.GetTeamDataForTeamId (teamId);
		}
			
		teamId = teamIdToUse;

		UpdateTeamImage ();
		UpdatePlayerImage ();


		if (loadPlayerData) {
			StartCoroutine (LoadPlayerDataRoutine ());
		}
	}

	public void UpdatePlayerData(PlayerData playerDataToUse) {
         
		playerData = playerDataToUse;

        m_LastPlayID = m_PlayID;
        m_PlayID = ScheduleDataManager.instance.currentPlayData.m_playId;

        if(m_LastPlayID != m_PlayID)
        {
            alternatePositionName = "";
            alternateNumber = "";
        }

        // playerPosition.text = string.IsNullOrEmpty(alternatePositionName) ? playerData.GetPlayPositionIdentifierAbbrv() : alternatePositionName;
       /* playerPosition.text = GetPositionAbbrev();
        playerJerseyNumber.text = string.IsNullOrEmpty(alternateNumber) ? playerData.jerseyNumber.ToString() : alternateNumber;
		playerNameFirst.text = playerData.playerNameFirst;
		playerNameLast.text = playerData.playerNameLast;*/

		playerData.teamId = teamId;

        Debug.Log("Position: " + GetPositionAbbrev() + " playerJerseyNumber: " + playerData.alternateNumber);
		//Debug.Log ("player id: " + playerData.playerId + ", player: " + playerData.playerNameFirst + " " + playerData.playerNameLast + ", pos: " + playerData.positionIdentifier + ", teamId: " + playerData.teamId + ", isHome: " + isHome);
	}

	private void UpdatePlayerImage() {
		string playerImagePath = "Players/" + TeamDataObject.GetTeamAbbrvForTeam(teamData.teamInfo.team).ToString () + "/" + playerId;
		Texture2D playerTexture = Resources.Load(playerImagePath) as Texture2D;

		if (playerTexture == null) {
			playerImagePath = "Players/DefaultPlayer";
			playerTexture = Resources.Load(playerImagePath) as Texture2D;
		}

		//playerImage.mainTexture = playerTexture;
	}

	private void UpdateTeamImage() {
		string teamImagePath = "TeamLogos/" + teamData.teamInfo.team.ToString ();
		Texture2D teamTexture = Resources.Load(teamImagePath) as Texture2D;
		//teamImage.mainTexture = teamTexture;
	}

	private IEnumerator LoadPlayerDataRoutine() {
		yield return new WaitForEndOfFrame ();
		//DataManager.LoadPlayer (playerId, this, null);
	}

	public void UpdatePanelDepth(int panelDepth) {
		//uiPanel.depth = panelDepth;
	}

	public void ResetPlayerTrails() {
		//trail.ClearTrails ();
	}

    public string GetPositionAbbrev()
    {
        switch(positionIdentifier)
        {
            case ePlayPositionIdentifier.LeftFielder:
            {
                return "LF";
            }
            case ePlayPositionIdentifier.RightFielder:
            {
                return "RF";
            }
            case ePlayPositionIdentifier.CenterFielder:
            {
                return "CF";
            }
            case ePlayPositionIdentifier.Shortstop:
            {
                return "SS";
            }
            case ePlayPositionIdentifier.Catcher:
            {
                return "C";
            }
            case ePlayPositionIdentifier.Pitcher:
            {
                return "P";
            }
            
        }

        return (string.IsNullOrEmpty(alternatePositionName) ? playerData.GetPlayPositionIdentifierAbbrv() : alternatePositionName);
    }
}
