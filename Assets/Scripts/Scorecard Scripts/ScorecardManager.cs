using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScorecardManager : MonoBehaviour
{
    private ScorecardItem[] scorecards;
    public CustomScroll scollCustom;
    public int activeSlots = 0;

    public GameObject noGamesAvailableDisplay;

    public IEnumerator RealignGrid()
    {
        //TestSuiteMainUI();
        UnityEngine.UI.HorizontalLayoutGroup[] rowAlignment = GetComponentsInChildren<UnityEngine.UI.HorizontalLayoutGroup>();
        for (int i = 0; i < rowAlignment.Length; i++)
        {
            rowAlignment[i].enabled = true;
        }
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < rowAlignment.Length; i++)
        {
            rowAlignment[i].enabled = false;
        }
    }

    public void InitializeScorecards()
    {
        //Initialize Test Suite on all children to gather components
        for (int i = 0; i < activeSlots; i++)
        {
            scorecards[i].Initialize();
        }
    }

    public void TestSuiteMainUI()
    {
        for (int i = 0; i < activeSlots; i++)
        {
            eTeamId home = (eTeamId)Random.Range(0, (int)eTeam.Length - 1);
            eTeamId away = (eTeamId)Random.Range(0, (int)eTeam.Length - 1);
            SetSlotDetails(i,
                new TeamData(home, Random.Range(0, 99), Random.Range(0, 99)),
                new TeamData(away, Random.Range(0, 99), Random.Range(0, 99)),
                new int?[2] { Random.Range(0, 99), Random.Range(0, 99) }
                );
        }
    }

    public void TestSuiteInProgress()
    {
        for (int i = 0; i < activeSlots; i++)
        {
            scorecards[i].gameObject.SetActive(false);
            SetSlotDataInProgress(
                i,
                Random.Range(0, 3),
                Random.Range(0, 2),
                Random.Range(0, 2),
                new bool[3] {
                    (Random.Range(0,1) == 0)? true:false,
                    (Random.Range(0, 1) == 0) ? true : false ,
                    (Random.Range(0, 1) == 0) ? true : false
                }
            );
            scorecards[i].gameObject.SetActive(true);
        }
    }

    public void ResetInProgress()
    {
        for (int i = 0; i < scorecards.Length; i++)
        {
            scorecards[i].gameObject.SetActive(false);
            SetSlotDataInProgress(i, 0, 0, 0, new bool[3] { false, false, false });
            scorecards[i].gameObject.SetActive(true);
        }
    }

    public bool featureToggle;
    public void TestSuiteToggleFeatureGraphic(bool onOff)
    {
        featureToggle = onOff;
        for (int i = 0; i < scorecards.Length; i++)
        {
            scorecards[i].ToggleFeaturedGraphic(onOff);
        }
    }

    public bool freeToggled;
    public void TestSuiteToggleFreeGraphic(bool onOff)
    {
        freeToggled = onOff;
        for (int i = 0; i < scorecards.Length; i++)
        {
            scorecards[i].ToggleFreeGraphic(onOff);
        }
    }

    public void ResetAllScorecards()
    {
        //TODO call reset on each scorecard
    }

    public void SetScorecardsVar()
    {
        scorecards = GetAllScorecards();
    }

    ScorecardItem[] GetAllScorecards()
    {
        return GetScorecardsFromRow(transform);
    }

    ScorecardItem[] GetScorecardsFromRow(Transform transformToCheck)
    {
        List<ScorecardItem> returnValue = new List<ScorecardItem>();
        if (transformToCheck.gameObject.GetComponent<ScorecardItem>() == null)
            for (int i = 0; i < transformToCheck.childCount; i++)
            {
                returnValue.AddRange(GetScorecardsFromRow(transformToCheck.GetChild(i)));
            }
        else
            returnValue.Add(transformToCheck.GetComponent<ScorecardItem>());
        return returnValue.ToArray();
    }
    
    public void ActivateSlots(int slotsToActivate)
    {
        if (scollCustom == null)
        {
            //slotsToActivate = (slotsToActivate < 1) ? 1 : slotsToActivate;
            for (int i = 0; i < slotsToActivate; i++)
            {
                scorecards[i].gameObject.SetActive(true);
            }
            for (int i = slotsToActivate; i < scorecards.Length; i++)
            {
                scorecards[i].gameObject.SetActive(false);
            }
            if (slotsToActivate == 0)
            {
                if (noGamesAvailableDisplay != null)
                    noGamesAvailableDisplay.SetActive(true);
            }
            else
            {
                if(noGamesAvailableDisplay != null)
                noGamesAvailableDisplay.SetActive(false);
            }
        } 
        else
        {
            scollCustom.SetActiveSlots(slotsToActivate);
        }
        
        SetScorecardsVar();
        if (slotsToActivate > scorecards.Length)
            slotsToActivate = scorecards.Length;
        //Debug.Log("Activating Slots " + slotsToActivate.ToString());
        activeSlots = slotsToActivate;
    }

    public void DeactivateAllSlots()
    {
        ActivateSlots(0);
    }

    public void SetScorecardData(int iScoreCard, ScheduleGameData gameData)
    {
        if (iScoreCard < scorecards.Length)
        {
            //TODO Switch data called based on game state
            //SetSlotDetails(iScoreCard, gameData.homeTeam, gameData.awayTeam); Previous Calls
            SetSlotDetails(iScoreCard, gameData.homeTeam, gameData.awayTeam, new int?[2] { gameData.homeTeam.wins, gameData.homeTeam.lost});
        }
        else
        {
            throw new System.Exception("iScorecard used not valid");
        }
    }

    public void SetSlotDetails(int slot, TeamData homeTeam, TeamData awayTeam, int?[] score)
    {
        //Randomize Other Data until calls are made
        scorecards[slot].SetScorecardDetails(homeTeam, awayTeam, score,Random.Range(0, 3), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 9), (Random.Range(0, 1) == 0) ? true : false,
            new bool[3] { (Random.Range(0, 1) == 0) ? true : false, (Random.Range(0, 1) == 0) ? true : false, (Random.Range(0, 1) == 0) ? true : false }
            );
        scorecards[slot].DisplayDataDetails();
    }

    public void SetSlotDetails(int slot, TeamData homeTeam, TeamData awayTeam, int?[] score, int balls, int strikes, int outs, int inning, bool upDown, bool[] basesLoaded)
    {
        scorecards[slot].SetScorecardDetails(homeTeam, awayTeam, score, balls, strikes, outs, inning, upDown, basesLoaded);
        scorecards[slot].DisplayDataDetails();
    }

    public void SetSlotData(int slot, TeamData homeTeam, TeamData awayTeam, int balls, int strikes, int outs, bool[] basesLoaded)
    {
        SetSlotMainUI(slot, homeTeam, awayTeam);
        SetSlotDataInProgress(slot,balls,strikes,outs,basesLoaded);
    }

    private void SetSlotMainUI(int slot, TeamData homeTeam, TeamData awayTeam)
    {
        scorecards[slot].SetHomeAwayTeam(homeTeam,awayTeam);
    }

    private void SetSlotDataPreGame()
    {
        //Test out slot data
    }

    private void SetSlotDataPostGame()
    {
        //Test out slot data
    }

    /// <summary>
    /// This is for an in-progress game
    /// </summary>
    /// <param name="slot">Which scorecard to modify</param>
    /// <param name="basesLoaded">format a bool[3] toggling which base is active</param>
    private void SetSlotDataInProgress(int slot, int balls, int strikes, int outs,bool[] basesLoaded)
    {
        Debug.Log("Setting In progress data for slot " + slot.ToString());
        Debug.Log(string.Join(" | ", new string[3] { balls.ToString(), strikes.ToString(), outs.ToString() }));
        scorecards[slot].SetInProgressData(balls, strikes, outs, basesLoaded);
    }

}
