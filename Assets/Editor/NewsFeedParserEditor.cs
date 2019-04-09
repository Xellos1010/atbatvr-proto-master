using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NewsFeedParser))]
public class NewsFeedParserEditor : Editor
{
    Transform endingPos;
    Transform playerPos;

    bool selected = false;
    bool initialized = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NewsFeedParser myScript = (NewsFeedParser)target;
        
        if (GUILayout.Button("Set Sprites To Atlas"))
        {
            myScript.SetSpritesToAtlas();
        }

        if (GUILayout.Button("Set Article Icons"))
        {
            myScript.PopulateArticleContent();
        }

        if (GUILayout.Button("Set In Pos"))
        {
           // myScript.SetInPos();
        }
    }
}