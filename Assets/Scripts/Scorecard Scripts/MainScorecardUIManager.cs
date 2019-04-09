using UnityEngine;
using System.Collections;

public class MainScorecardUIManager : MonoBehaviour {

    [SerializeField]
    private TeamPanelInfo homeTeamPanel;
    [SerializeField]
    private TeamPanelInfo awayTeamPanel;
    [SerializeField]
    private Transform freeGameOfTheDay;
    [SerializeField]
    private Transform featuredGame;
    // Use this for initialization
    void Awake()
    {
        //Initialize();
	}

    public void Initialize()
    {
        InitializeMainGraphicHolders();
    }

    void InitializeMainGraphicHolders()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (homeTeamPanel == null)
                if (transform.GetChild(i).gameObject.name.ToUpper().Contains("HOME"))
                    homeTeamPanel = transform.GetChild(i).GetComponent<TeamPanelInfo>();
            if (awayTeamPanel == null)
                if (transform.GetChild(i).gameObject.name.ToUpper().Contains("AWAY"))
                    awayTeamPanel = transform.GetChild(i).GetComponent<TeamPanelInfo>();
            if(freeGameOfTheDay == null)
                if (transform.GetChild(i).gameObject.name.ToUpper().Contains("FREE"))
                    freeGameOfTheDay = transform.GetChild(i);
            if(featuredGame == null)
                if (transform.GetChild(i).gameObject.name.ToUpper().Contains("FEATURED"))
                    featuredGame = transform.GetChild(i);
        }
        homeTeamPanel.Initialize();
        awayTeamPanel.Initialize();
    }

    public void SetHomeTeam(TeamData team)
    {
        //Debug.Log(team);
        homeTeamPanel.SetTeam(team.team.ToString(), System.String.Join(" - ", new string[2] {team.wins.ToString(),team.lost.ToString()}));
    }

    public void SetAwayTeam(TeamData team)
    {
        awayTeamPanel.SetTeam(team.team.ToString(), System.String.Join(" - ", new string[2] { team.wins.ToString(), team.lost.ToString() }));
    }

    
    public void ToggleFreeGame(bool active)
    {
        freeGameOfTheDay.gameObject.SetActive(active);
    }

    
    public void ToggleFeatureGameActive(bool active)
    {
        featuredGame.gameObject.SetActive(active);
    }
}
