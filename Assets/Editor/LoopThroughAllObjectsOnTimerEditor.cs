using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LoopThroughAllObjectsOnTimer))]
public class LoopThroughAllObjectsOnTimerEditor : Editor
{
    Transform endingPos;
    Transform playerPos;

    bool selected = false;
    bool initialized = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LoopThroughAllObjectsOnTimer myScript = (LoopThroughAllObjectsOnTimer)target;
        
        if (GUILayout.Button("Cycle Objects"))
        {
            myScript.CycleObjects();
        }
        if (GUILayout.Button("Next Object"))
        {
            myScript.NextObject();
        }
        if (GUILayout.Button("Previous Object"))
        {
            myScript.PreviousObject();
        }

        if (myScript.startCycle)
            if (GUILayout.Button("Stop Cycling"))
            {
                myScript.Stop();
            }
        if (!myScript.startCycle)
            if (GUILayout.Button("Start Cycling"))
            {
                myScript.StartCycling();
            }
        if(myScript.forwardBack)
            if (GUILayout.Button("Cycle Back"))
            {
                myScript.ToggleForwardBack(false);
            }
        if(!myScript.forwardBack)
            if (GUILayout.Button("Cycle Forward"))
            {
                myScript.ToggleForwardBack(true);
            }
    }
}