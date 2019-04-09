using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScorecardManager))]
public class ScorecardManagerEditor : Editor
{
    bool toggleFreeGraphic = true;
    bool toggleFeatureGraphic = true;


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ScorecardManager myScript = (ScorecardManager)target;
        int iNumber = EditorGUILayout.IntSlider("Number of Active Slots",myScript.activeSlots, 0, 6);
        if (iNumber != myScript.activeSlots)
            myScript.ActivateSlots(iNumber);

        toggleFreeGraphic = EditorGUILayout.Toggle("Toggle Free Game Graphic", toggleFreeGraphic);
        if (toggleFreeGraphic != myScript.freeToggled)
            myScript.TestSuiteToggleFreeGraphic(toggleFreeGraphic);

        toggleFeatureGraphic = EditorGUILayout.Toggle("Toggle Featured Game Graphic", toggleFeatureGraphic);
        if (toggleFeatureGraphic != myScript.featureToggle)
            myScript.TestSuiteToggleFeatureGraphic(toggleFeatureGraphic);
        //bool toggleFeatured = EditorGUILayout.Toggle("Toggle Free Game Graphic", myScript.featuredOnOff);


        if (GUILayout.Button("Get All Scorecards"))
            myScript.SetScorecardsVar();

        if (GUILayout.Button("Initialize ScoreCards"))
            myScript.InitializeScorecards();

        if (GUILayout.Button("Run Test Suite Main UI"))
        {
            myScript.TestSuiteMainUI();
        }

        if (GUILayout.Button("Run Test Suite In progress"))
        {
            myScript.TestSuiteInProgress();
        }

        if (GUILayout.Button("Reset Test Suite In progress"))
        {
            myScript.ResetInProgress();
        }

        if (GUILayout.Button("Realign Grid"))
        {
            myScript.RealignGrid();
        }
    }
}