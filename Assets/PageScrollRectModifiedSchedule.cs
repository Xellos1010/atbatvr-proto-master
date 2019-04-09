using UnityEngine;
using System.Collections;

public class PageScrollRectModifiedSchedule : PagedScrollRect {

    public int calendarPage;

    public void BringUpCalendar()
    {
        if(ActivePageIndex != 1)
            SnapToPage(1, false);   
    }

    public void BringUpSchedules()
    {
        if (ActivePageIndex != 0)
            SnapToPage(0, false);
    }

}
