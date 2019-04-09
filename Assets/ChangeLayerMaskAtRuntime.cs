using UnityEngine;
using System.Collections;

public class ChangeLayerMaskAtRuntime : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start ()
    {
        yield return null;
        //Camera[] cameras = GetComponentsInChildren<Camera>();
        SetLayerMaskAtRuntime layerMaskUnity = transform.GetChild(0).gameObject.AddComponent<SetLayerMaskAtRuntime>();//().cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Left"));
        layerMaskUnity.ChangeLayerMask(new string[3] {"UI","Left", "Default"});
        layerMaskUnity = transform.GetChild(1).gameObject.AddComponent<SetLayerMaskAtRuntime>();//().cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Left"));
        layerMaskUnity.ChangeLayerMask(new string[3] { "UI", "Right", "Default" });
        //transform.GetChild(1).GetComponent<Camera>().cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Right"));
        yield return new WaitForEndOfFrame();
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
