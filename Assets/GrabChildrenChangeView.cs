using UnityEngine;
using System.Collections;

public class GrabChildrenChangeView : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Camera[] childObjects = GetComponentsInChildren<Camera>();
		for (int i = 0; i < childObjects.Length; i++) {
			childObjects[i].stereoTargetEye = (i==0)?StereoTargetEyeMask.Left:StereoTargetEyeMask.Right;
			if(i == 0)
			{
				childObjects[i].stereoTargetEye = StereoTargetEyeMask.Left;
				childObjects[i].cullingMask = 1 << LayerMask.NameToLayer("Left")
					;
			}
			else
			{
				childObjects[i].stereoTargetEye = StereoTargetEyeMask.Right;
				childObjects[i].cullingMask = 1 << LayerMask.NameToLayer("Right");
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
