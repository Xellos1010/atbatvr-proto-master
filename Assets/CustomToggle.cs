using UnityEngine;
using System.Collections;

public class CustomToggle : MonoBehaviour {

	public void ToggleActive()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}
