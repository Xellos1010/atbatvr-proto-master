using UnityEngine;
using System.Collections;

public class MaximizeContent : MonoBehaviour {
    
    public void ClickCalled()
    {
        MaximizeContentManager.MaximizeElement(this);
    }
    

}
