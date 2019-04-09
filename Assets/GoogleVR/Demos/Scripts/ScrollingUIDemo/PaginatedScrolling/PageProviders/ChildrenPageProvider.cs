// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// Provides pages to a PagedScrollRect.
///
/// Treats each child of the scroll rect as a page. The pages are ordered
/// by their sibling index in the scene hierarchy.
///
/// Instead of allocating/deallocating pages, they are added and removed simply by
/// setting them active/inactive.
///



public class ChildrenPageProvider : MonoBehaviour, IPageProvider {
    /// The pages, in order.
    /// The active page is moved to be the last sibling after the scroll rect
    /// is initialized, so we need to store the pages in
    /// a seprate list to maintain the correct order.
    private List<Transform> activePages = new List<Transform>();
    private List<Transform> cachedPages = new List<Transform>();
    /// The spacing between pages in local coordinates.
    [Tooltip("The spacing between pages.")]
    public float spacing = 2000.0f;

    public float GetSpacing() {
        return spacing;
    }

    public int GetNumberOfPages() {
        return activePages.Count;
    }

    void Awake()
    {
        /// Disable all the pages to make sure
        /// none of them are visible initially before
        /// scrolling.
        SetPages();
    }

    public void SetPages()
    {
        foreach (Transform page in transform)
        {
            page.gameObject.SetActive(false);
            activePages.Add(page);
        }
    }

    public void BringAllPageElementsToSurface()
    {
        foreach (Transform page in (activePages))
        {
            TiledPage pageTiledComponent = page.GetComponent<TiledPage>();
            RectTransform pageTiledRect = page.GetComponent<RectTransform>();
            for (int i = 0; i < pageTiledComponent.tiles.Length; i++)
            {
                pageTiledComponent.tiles[i].SetParent(transform, false);
                pageTiledComponent.tiles[i].gameObject.SetActive(false);

                //Add Components CanvasGroup and TileScrollEffect
                pageTiledComponent.tiles[i].gameObject.AddComponent<CanvasGroup>();
                pageTiledComponent.tiles[i].gameObject.AddComponent<TiledPage>();

                //Change Rects to fill area
                RectTransform _trans = pageTiledComponent.tiles[i].GetComponent<RectTransform>();
                //_trans.anchorMin = pageTiledRect.anchorMin;
                //_trans.anchorMax = pageTiledRect.anchorMax;
                //_trans.anchoredPosition = pageTiledRect.anchoredPosition;
                LeanTween.value(gameObject, _trans.anchorMin, pageTiledRect.anchorMin, 1);
                LeanTween.value(gameObject, _trans.anchorMax, pageTiledRect.anchorMax, 1);
                LeanTween.value(gameObject, _trans.anchoredPosition, pageTiledRect.anchoredPosition, 1);
                LeanTween.size(_trans, new Vector2(1800, 1800), 1);//new Vector2(1800,1800);

            }
        }
    }

    public void TweenHeightWidth()
    {

    }

    public void AddPagesToPool()
    {
        foreach (Transform page in (activePages))
        {
            page.SetParent(transform.parent);
            page.gameObject.SetActive(false);
        }
    }

    public int ReturnPageIndex(Transform page)
    {
        for (int i = 0; i < activePages.Count; i++)
        {
            if (activePages[i] == page)
                return i;
        }
        return -1;
    }



    public Transform ReturnPage (int index)
    {
        if (index > activePages.Count)
            return ReturnAPageFromPool();
        return activePages[index];
    }

    public RectTransform ProvidePage(int index) {
        Transform pageTransform = activePages[index];
        RectTransform page = pageTransform.GetComponent<RectTransform>();

        Vector2 middleAnchor = new Vector2(0.5f, 0.5f);
        page.anchorMax = middleAnchor;
        page.anchorMin = middleAnchor;

        pageTransform.gameObject.SetActive(true);

        return page;
    }

    public void RemovePage(int index)
    {
        CachePage(index);
        
    }

    public void SyncCacheActive()
    {
        for (int i = 0; i < cachedPages.Count; i++)
        {
            if (activePages.Contains(cachedPages[i]))
                activePages.Remove(cachedPages[i]);
        }
    }

    public void CachePage(int iPage)
    {
        cachedPages.Add(activePages[iPage]);
        activePages[iPage].SetParent(transform.parent);
        activePages[iPage].gameObject.SetActive(false);
        //activePages.Remove(activePages[iPage]);

        //Set parent to pool
    }

    public Transform ReturnAPageFromPool()
    {
        cachedPages[cachedPages.Count-1].SetParent(transform);
        cachedPages[cachedPages.Count - 1].gameObject.SetActive(true);
        Transform returnValue = cachedPages[cachedPages.Count - 1];
        activePages.Add(returnValue);
        cachedPages.RemoveAt(cachedPages.Count - 1);
        return returnValue;
    }

    public Transform ReturnAPageToPool()
    {
        activePages[activePages.Count-1].SetParent(transform.parent);
        activePages[activePages.Count - 1].gameObject.SetActive(false);
        Transform returnValue = activePages[activePages.Count - 1];
        cachedPages.Add(returnValue);
        activePages.RemoveAt(activePages.Count - 1);
        return returnValue;
    }

    public void RemovePage(int index, RectTransform page)
    {
        page.gameObject.SetActive(false);
    }

    public int AllPages()
    {
        return activePages.Count;
    }

    void ReturnPageFromCached()
    {

    }

    public void VerifySlotsActive(int numberOfSlots)
    {
        if(activePages.Count < numberOfSlots)
            for (int i = 0; i < numberOfSlots - activePages.Count; i++)
            {
                ReturnAPageFromPool();
            }
        else if(activePages.Count > numberOfSlots)
        {
            for (int i = activePages.Count; i > numberOfSlots; i--)
            {
                ReturnAPageToPool();
            }
        }
    }
}
