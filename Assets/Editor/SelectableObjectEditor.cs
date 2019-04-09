using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SelectableObject))]
public class SelectableObjectEditor : Editor
{
    Transform endingPos;
    Transform playerPos;

    bool selected = false;
    bool initialized = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SelectableObject myScript = (SelectableObject)target;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ending Pos");
            endingPos = EditorGUILayout.ObjectField(endingPos, typeof(Transform), true) as Transform;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Pos");
            playerPos = EditorGUILayout.ObjectField(playerPos, typeof(Transform),true) as Transform;
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Test Set Selected"))
        {
            if (!initialized)
            {
                initialized = true;
                myScript.InitializeObject(endingPos.position, playerPos.position);
            }
            myScript.SetSelected();
        }
        if (GUILayout.Button("Test Unset Selected"))
        {
            myScript.UnSelectObject();
        }
    }
}