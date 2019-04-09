using UnityEngine;

public class TeamPanelInfo : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Image teamLogo;
    [SerializeField]
    private UnityEngine.UI.Text teamStats;
    // Use this for initialization
    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (teamLogo == null)
            teamLogo = GetComponentInChildren<UnityEngine.UI.Image>();
        if (teamStats == null)
            teamStats = GetComponentInChildren<UnityEngine.UI.Text>();
    }
	
	// Update is called once per frame
	public void SetTeam(string teamName, string teamScore)
    {
        teamLogo.sprite = TeamDataObject.GetTeamLogoSprite(teamName);
        try
        {
            teamStats.text = teamScore;
        }
        catch
        {
            Debug.Log("No Team Stats text available for " + gameObject.name);
        }
	}
}
