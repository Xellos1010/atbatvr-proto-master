using UnityEngine;
using System.Collections;

public class SelectableObject : MonoBehaviour
{
    [SerializeField]
    private Vector3 startingPos;
    [SerializeField]
    private Vector3 endingPos;
    [SerializeField]
    private Vector3 playerPos;

    public bool initialized;
    private bool selected = false;
    public float transitionTime;

    public bool draggable = false;

    public Material originalMat;

    virtual public void Awake()
    {
        initialized = false;
        selected = false;
        startingPos = transform.position;
        originalMat = gameObject.GetComponent<Renderer>().sharedMaterial;
    }

    public void SetOriginalMaterial()
    {
        gameObject.GetComponent<Renderer>().sharedMaterial = originalMat;
    }

    virtual public void InitializeObject(Vector3 endingPos, Vector3 playerPos)
    {
        if (!initialized)
        {
            SetTargetPosition(endingPos);
            SetPlayerPosition(playerPos);
        }
    }

    virtual public void ResetObject()
    {
        endingPos = Vector3.zero;
        playerPos = Vector3.zero;
        initialized = false;
    }

    virtual public void SetTargetPosition(Vector3 position)
    {
        endingPos = position;
    }

    virtual public void SetPlayerPosition(Vector3 position)
    {
        playerPos = position;
    }

    // Use this for initialization of Selecting the Object
    virtual public void SetSelected()
    {
        LeanTween.move(gameObject, endingPos, transitionTime);
        //iTween.MoveTo(gameObject,MoveToEndingPos());
        selected = true;
    }
    
    virtual public void UnSelectObject()
    {
        //Transition Object to Starting Position
        LeanTween.move(gameObject, startingPos, transitionTime);
        //iTween.MoveTo(gameObject, MoveToStartingPos());
        selected = false;
    }



    /*Hashtable MoveToStartingPos()
    {
        Hashtable returnValue = new Hashtable();

        returnValue.Add("position",startingPos);
        //returnValue.Add("looktarget",playerPos);
        returnValue.Add("time",transitionTime);
        returnValue.Add("easetype", iTween.EaseType.linear);

        return returnValue;
    }

    Hashtable MoveToEndingPos()
    {
        Hashtable returnValue = new Hashtable();

        returnValue.Add("position", endingPos);
        //returnValue.Add("looktarget", playerPos);
        returnValue.Add("time", transitionTime);
        returnValue.Add("easetype", iTween.EaseType.linear);

        return returnValue;
    }
    */
    

}
