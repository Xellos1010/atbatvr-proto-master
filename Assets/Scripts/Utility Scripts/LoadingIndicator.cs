using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingIndicator : MonoBehaviour {

	public Image loadingImage;

	void Update () {
		loadingImage.transform.rotation = Quaternion.Euler (new Vector3 (loadingImage.transform.localEulerAngles.x, loadingImage.transform.localEulerAngles.y, loadingImage.transform.localEulerAngles.z - 2.0f));
	}
}
