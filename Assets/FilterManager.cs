using UnityEngine;
using System.Collections;

public class FilterManager : MonoBehaviour {
    
    public TabGroup TabManager;

    /// <summary>
    /// This is used to handle Button OnClicks from LogoHolder
    /// </summary>
    /// <param name="teamName"></param>
	public void FilterContentByTeam(UnityEngine.UI.Image teamSpriteHolder)
    {
        FilterContentByTeam(teamSpriteHolder.sprite.name);
    }


    public void FilterContentByTeam(string teamName)
    {
        //Turn On news Tab
        //Set News Tab to Filter Content By team
        TabManager.OpenNews();
        //Make sure nothing is running
        //TabManager.ReturnNewsObject().GetComponent<VRArticleManager>().StopAllCoroutines();
        NewsFeedParser.instance.StopAllCoroutines();
        //Setup Data manager to pull new team data
        //TODO abstract this to make specified target window - for encapsulation
        StartCoroutine(WaitForNewsFeedObjectToUpdateUnity(teamName));

    }

    IEnumerator WaitForNewsFeedObjectToUpdateUnity(string teamName)
    {
        yield return null;
        Debug.Log("Setting filter to " + teamName);
        NewsFeedParser.instance.LoadNewsFeedByFilter(teamName);
    }
}
