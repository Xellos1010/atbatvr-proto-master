using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class GameItem : MonoBehaviour {

    public string gameId;
    private ScheduleGameData scheduleGameData;

    public eTeamId awayTeamId;
    public eTeamId homeTeamId;

    public Text awayTeamNameLabel;
    public Text homeTeamNameLabel;
    public Text awayTeamScoreLabel;
    public Text homeTeamScoreLabel;

    public Image awayTeamLogo;
    public Image homeTeamLogo;

    public Image background;

    public bool isSelected;

    public void SetupGameItem(ScheduleGameData scheduleGameDataToUse) {
        scheduleGameData = scheduleGameDataToUse;

        gameId = scheduleGameData.gameId;

        awayTeamId = scheduleGameData.awayTeam.teamId;
        homeTeamId = scheduleGameData.homeTeam.teamId;

        eTeamAbbrv awayTeamAbbrv = (eTeamAbbrv)(int)awayTeamId;
        eTeamAbbrv homeTeamAbbrv = (eTeamAbbrv)(int)homeTeamId;

        awayTeamNameLabel.text = awayTeamAbbrv.ToString();
        homeTeamNameLabel.text = homeTeamAbbrv.ToString();
       
        eTeam awayTeam = (eTeam)Enum.Parse(typeof(eTeam), awayTeamId.ToString());  //Ugly enum juggling...
        eTeam homeTeam = (eTeam)Enum.Parse(typeof(eTeam), homeTeamId.ToString());

        awayTeamNameLabel.text = ((eTeamAbbrv)awayTeam).ToString();
        homeTeamNameLabel.text = ((eTeamAbbrv)homeTeam).ToString();

        //TODO Get Team Record and Set
        //awayTeamScoreLabel.text = scheduleGameData.awayTeamScore.ToString();
        //homeTeamScoreLabel.text = scheduleGameData.homeTeamScore.ToString();

		Sprite awayLogo = TeamDataObject.GetTeamLogoSprite (awayTeamId);
		Sprite homeLogo = TeamDataObject.GetTeamLogoSprite (homeTeamId);

		if (awayLogo != null) {
			awayTeamLogo.sprite = TeamDataObject.GetTeamLogoSprite (awayTeamId);
		}
		if (homeLogo != null) {
			homeTeamLogo.sprite = TeamDataObject.GetTeamLogoSprite (homeTeamId);
		}
    }

    public void GameItemUnselected() {
        isSelected = false;
        background.gameObject.SetActive(false);
    }

    public void GameItemSelected() {
        //MenuManager.GameSelected(this);
        isSelected = true;
        background.gameObject.SetActive(true);
    }

	public bool IsGameCompleted() {
		return scheduleGameData.gameCompleted;
	}
}
