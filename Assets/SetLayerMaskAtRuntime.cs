using UnityEngine;
using System.Collections;

public class SetLayerMaskAtRuntime : MonoBehaviour {

    public string[] layerMasks;
	// Use this for initialization
	void Start ()
    {
        ChangeLayerMask();
	}

    public void ChangeLayerMask(string[] layerMasks)
    {
        this.layerMasks = layerMasks;
        // change mask
        //GetComponent<Camera>().cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer(layerMask));
        ChangeLayerMask();
    }

    public void ChangeLayerMask()
    {

        // change mask
        for (int i = 0; i < layerMasks.Length; i++)
        {
            GetComponent<Camera>().cullingMask ^= (1 << LayerMask.NameToLayer(layerMasks[i]));
    }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
