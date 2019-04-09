using UnityEngine;

public class ScorecardItem : MonoBehaviour
{
    public MainScorecardUIManager mainGraphics;
    public ScoreUIPanelManager scorePanel;

    public ScorecardDetails details;

    public bool freeGameOfTheDay = false;
    public bool featured = false;

    public bool inProgress = true; // TODO hook into Feature graphic Display


    public UnityEngine.UI.Text scoreboard;

    void Awake()
    {
        //Initialize FreeGame and Highlights game Graphics
        //Initialize();
    }

    public void Initialize()
    {
        InitializeManagers();
        mainGraphics.Initialize();
        if(scorePanel != null)
            scorePanel.Initialize();
    }

    void InitializeManagers()
    {
        if (mainGraphics == null)
            mainGraphics = GetComponentInChildren<MainScorecardUIManager>();

        if (scorePanel == null)
        {
            try
            {
                scorePanel = GetComponentInChildren<ScoreUIPanelManager>();
            }
            catch
            { Debug.Log("No Score Panel for " + gameObject.name); }
        }
    }

    public void SetHomeAwayTeam(TeamData homeTeam, TeamData awayTeam)
    {
        details.awayTeam = awayTeam;
        details.homeTeam = homeTeam;
        SetHomeTeam(homeTeam);
        SetAwayTeam(awayTeam);
    }

    public void ToggleFeaturedGraphic(bool onOff)
    {
        featured = onOff;
        //TODO Update state of all graphics to show highlighted graphics
        mainGraphics.ToggleFeatureGameActive(onOff);
        if(scorePanel != null)
            scorePanel.SetHighlighted(onOff);

    }

    public void ToggleFreeGraphic(bool onOff)
    {
        freeGameOfTheDay = onOff;
        mainGraphics.ToggleFreeGame(onOff);
    }

    public void SetInProgressData(int balls, int strikes, int outs, bool[] basesLoaded)
    {
        details.BSO = new int[3] {balls,strikes,outs};
        details.basesLoaded = basesLoaded;
        SetBSO(balls,strikes,outs);
        SetBasesLoaded(basesLoaded);
    }

    public void SetScorecardDetails(TeamData homeTeam, TeamData awayTeam, int?[]score, int balls, int strikes, int outs, int inning, bool upDown, bool[] basesLoaded)
    {
        details.awayTeam = awayTeam;
        details.homeTeam = homeTeam;
        details.score = score;
        details.BSO = new int[3] { balls, strikes, outs };
        details.inning = inning;
        details.upDown = upDown;
        details.basesLoaded = basesLoaded;
    }

    public void SetScorecardDetails(ScorecardDetails details)
    {
        this.details = details;
        
    }

    public void DisplayDataDetails()
    {
        SetAwayTeam();
        SetHomeTeam();
        SetScore();
        SetBSO();
        SetInning();
        SetBasesLoaded();
    }


    void SetFeaturedGraphics()
    {

    }

    void SetFreeGraphics()
    {
        
    }

    void SetHomeTeam()
    {
        mainGraphics.SetHomeTeam(details.homeTeam);
    }

    void SetHomeTeam(TeamData team)
    {
        mainGraphics.SetHomeTeam(team);
    }

    void SetAwayTeam()
    {
        mainGraphics.SetAwayTeam(details.awayTeam);
    }

    void SetAwayTeam(TeamData team)
    {
        mainGraphics.SetAwayTeam(team);
    }
    
    void SetInning()
    {
        if (scorePanel != null)
            scorePanel.SetInning(details.inning,details.upDown);
    }

    void SetInning(int inning,bool upDown)
    {
        details.inning = inning;
        details.upDown = upDown;
        if (scorePanel != null)
            scorePanel.SetInning(inning, upDown);
    }

    void SetBasesLoaded()
    {
        if (scorePanel != null)
            scorePanel.SetBasesLoaded(details.basesLoaded);
    }

    void SetBasesLoaded(bool[] basesLoaded)
    {
        details.basesLoaded = basesLoaded;
        if (scorePanel != null)
            scorePanel.SetBasesLoaded(basesLoaded);
    }

    void SetBSO()
    {
        if (scorePanel != null)
            scorePanel.SetBSO(details.BSO[0], details.BSO[1], details.BSO[2]);
    }

    void SetBSO(int balls, int strikes, int outs)
    {
        details.BSO = new int[3] {balls,strikes,outs};
        if (scorePanel != null)
            scorePanel.SetBSO(balls, strikes, outs);
    }

    void SetScore()
    {
        scoreboard.text = System.String.Join(" - ", new string[2] { details.score[0].ToString(), details.score[1].ToString() });
    }

    void SetScore(int? hometeam, int? awayteam)
    {
        details.score = new int?[2] { hometeam,awayteam };
        scoreboard.text = System.String.Join(" - ", new string[2] {hometeam.ToString(),awayteam.ToString() });
    }
    
}

[System.Serializable]
public class ScorecardDetails
{
    public TeamData homeTeam;
    public TeamData awayTeam;

    public int?[] score;

    public int[] BSO;

    public bool[] basesLoaded;
    public int inning;
    public bool upDown;

    public ScorecardDetails()
    {
        
    }

    public ScorecardDetails(TeamData homeTeam, TeamData awayTeam)
    {
        this.homeTeam = homeTeam;
        this.awayTeam = awayTeam;
    }

    public ScorecardDetails(int balls, int strikes, int outs)
    {
        BSO = new int[3] { balls, strikes, outs };
    }

    public ScorecardDetails(int inning, bool upDown)
    {
        this.inning = inning;
        this.upDown = upDown;
    }
}