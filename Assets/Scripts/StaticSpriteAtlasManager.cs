using System.Collections.Generic;
using UnityEngine;
public static class StaticSpriteAtlasManager{

    public static Dictionary<string, Sprite> teamLogos;

    public static Sprite ReturnTeamLogo(string teamName)
    {
        if (teamLogos == null)
        {
            teamLogos = new Dictionary<string, UnityEngine.Sprite>();
            
            string teamTexture = "TeamLogos/" + teamName;

            //Texture2D inputTexture = (Texture2D)(Texture2D)Resources.Load(teamTexture) as Texture2D;
            Sprite[] initialLoad = Resources.LoadAll<Sprite>("TeamLogos/Team_Logos");
            for (int i = 0; i < initialLoad.Length; i++)
            {
                //Debug.Log(initialLoad[i].name);
                teamLogos[initialLoad[i].name] = initialLoad[i];
            }
            //Debug.Log("Team name called =" + teamName);
        }
        return teamLogos[teamName];
    }
}
