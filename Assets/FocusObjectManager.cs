using UnityEngine;
using System.Collections;

public class FocusObjectManager : MonoBehaviour
{
    public ScorecardDetailsManager scorecardFocused;
	
    public void FocusOnScorecard(ScorecardItem scorecard)
    {
        scorecardFocused.SetFocusScorecard(scorecard);
    }

}
