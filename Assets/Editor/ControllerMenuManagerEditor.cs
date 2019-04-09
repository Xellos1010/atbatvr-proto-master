using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ControllerMenuManager))]
public class ControllerMenuManagerEditor : Editor
{
    Transform endingPos;
    Transform playerPos;

    bool selected = false;
    bool initialized = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ControllerMenuManager myScript = (ControllerMenuManager)target;
        
        if (GUILayout.Button("Toggle Menu"))
        {
            //myScript.ToggleMainMenu();
        }

        if (GUILayout.Button("Set Out Pos"))
        {
            //myScript.SetOutPos();
        }

        if (GUILayout.Button("Set In Pos"))
        {
           // myScript.SetInPos();
        }
    }
}