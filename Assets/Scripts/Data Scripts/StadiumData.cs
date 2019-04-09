using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Adboard {
	public Material Material;
	public Texture2D DefaultTex;
}

public class StadiumData : MonoBehaviour {
	public string StadiumName;
	public Material PlayerJumbo;
	public List<Adboard> Adboards;

    void Start() {
        //Debug.Log("StadiumData, Start called");
		/*if (MenuManager.Instance != null) {
			MenuManager.BallparkWasLoaded ();
		}*/
    }
}
