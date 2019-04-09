using UnityEngine;
using System.Collections;

public class LoopThroughAllObjectsOnTimer : MonoBehaviour {

    public GameObject[] ListOfObjects;
    public float timer = 2;
    public bool forwardBack;
    public int currentObject = 0;
    public bool startCycle = true;
    public bool cycling = false;

    public bool startCycleOnLoad = false;

	// Use this for initialization
	void Start ()
    {
        if (startCycleOnLoad)
        {
            ActivateObject();
            StartCycling();
        }
    }

    public void StartCycling()
    {
        startCycle = true;
        Resume();
    }

    IEnumerator CycleThroughObjects()
    {
        yield return new WaitForSeconds(timer);
        CycleObjects();
    }

    public void Resume()
    {
        Debug.Log("Resuming");
        cycling = true;
        StartCoroutine(CycleThroughObjects());
    }

    public void Stop()
    {
        Pause();
        startCycle = false;
    }

    public void Pause()
    {
        cycling = false;
        StopAllCoroutines();
    }

    public void CycleObjects()
    {
        Pause();
        IncrementObject(forwardBack);
        ActivateObject();
        if (startCycle)
            Resume();
    }

    public void NextObject()
    {
        Stop();
        IncrementObject(true);
        ActivateObject();
    }

    public void PreviousObject()
    {
        Stop();
        IncrementObject(false);
        ActivateObject();
    }

    void IncrementObject(bool UpDown)
    {
        if (UpDown)
            if (currentObject + 1 > ListOfObjects.Length - 1)
            { currentObject = 0; }
            else
            { currentObject += 1; }
        else
            if (currentObject - 1 < 0)
            { currentObject = ListOfObjects.Length - 1; }
            else
            { currentObject -= 1; }
        Debug.Log("Incremented " + UpDown);
    }

    void ActivateObject()
    {
        Debug.Log("Activating Object " + currentObject);
        for (int i = 0; i < ListOfObjects.Length; i++)
            if (i == currentObject)
                ListOfObjects[i].SetActive(true);
            else
                ListOfObjects[i].SetActive(false);
    }

    void ActivateObject(int gameObject)
    {
        currentObject = gameObject;
        ActivateObject();
    }

    public void ToggleForwardBack(bool forwardBack)
    {
        this.forwardBack = forwardBack;
    }

    public void Update()
    {
        if (!Application.isMobilePlatform)
        {
            //Keyboard controls
            if (Input.GetKeyDown(KeyCode.Space))
                if (cycling)
                    Stop();
                else
                {
                    StartCycling();
                    CycleObjects();
                }
            if (Input.GetKeyDown(KeyCode.RightArrow))
                NextObject();
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                PreviousObject();
        }
        else
        {
            if(GvrController.AppButtonUp)
            {
                NextObject();
            }
        }
    }
}
