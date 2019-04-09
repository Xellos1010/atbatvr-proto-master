using UnityEngine;
using System.Collections;

public class ScorecardDetailsManager : MonoBehaviour
{
    public ScorecardItem detailedItemView;

	public void SetFocusScorecard(ScorecardItem item)
    {
        detailedItemView.ToggleFreeGraphic(item.freeGameOfTheDay);
        detailedItemView.ToggleFeaturedGraphic(item.featured);
        detailedItemView.SetScorecardDetails(item.details);
        detailedItemView.DisplayDataDetails();
    }
}
