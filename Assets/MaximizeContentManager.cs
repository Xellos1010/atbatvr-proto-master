using UnityEngine;
using System.Collections;

public class MaximizeContentManager : MonoBehaviour {

    static ChildrenPageProvider pageProvider;
    static PagedScrollRect pageHandler;

    static bool elementMaximized = false;

    // Use this for initialization
    void Awake()
    {
        pageProvider = GetComponent<ChildrenPageProvider>();
        pageHandler = GetComponent<PagedScrollRect>();
    }
	
    public static void MaximizeElement(MaximizeContent content)
    {
        if (!elementMaximized)
        {
            //Have element on page unparent - maximize and become new active page
            content.transform.SetParent(pageProvider.transform,true);
            RectTransform elementRect = content.gameObject.GetComponent<RectTransform>();
            pageProvider.BringAllPageElementsToSurface();
            pageProvider.AddPagesToPool();
            pageProvider.SetPages();
            pageHandler.SnapToPage(pageProvider.ReturnPageIndex(content.transform), true);
        }
        elementMaximized = true;
    }

    public static void MinimizeElement(MaximizeContent content)
    {
        //Find page element resides on and load that page of info
        //minimizeContent(content);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
