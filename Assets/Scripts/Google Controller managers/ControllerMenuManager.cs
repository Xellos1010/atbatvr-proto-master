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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;
using System.Collections;

public class ControllerMenuManager : MonoBehaviour
{
    public CanvasGroup mainAlphaCanvas;
    private RectTransform mainAlphaCanvasRect;
    public CanvasGroup[] subCanvases;
    public Canvas[] menus;

    public int iActiveCanvas;
    public int iCanvasToActivate;
    public float timeMenuTransitions = 1;

    [SerializeField]
    private bool toggled = false;
    [SerializeField]
    public bool transitioning = false;

    public Vector3 outpos;
    public Vector3 inpos;


#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    void Awake()
    {
        mainAlphaCanvasRect = mainAlphaCanvas.gameObject.GetComponent<RectTransform>();
        InitializeMenus();
    }

    void Start()
    {
        //mainAlphaCanvasRect = mainAlphaCanvas.gameObject.GetComponent<RectTransform>();
    }

    void InitializeMenus()
    {
        mainAlphaCanvas.alpha = 0;
        InitializeActiveCanvas();
        iActiveCanvas = 3;
        mainAlphaCanvasRect.anchoredPosition = outpos;
        toggled = false;
        transitioning = false;
        //InitializeSubMenus();
        //TODO: Initialize the inside menus so menu 0 is active
    }

    void InitializeActiveCanvas()
    {
        //Find the Middle Child and that should be the active Canvas
        //iActiveCanvas = (int)Mathf.roun ((float)mainAlphaCanvas.transform.childCount) / 2.0f;
    }

    public void ToggleMainMenu()
    {
        if (!transitioning)
        {
            if (toggled)
                HideMainMenu();
            else
                ShowMainmenu();
            transitioning = true;
            StartCoroutine(ReactivateAfter(timeMenuTransitions));
        }
    }

    public void SetInPos()
    {
        inpos = mainAlphaCanvas.transform.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetOutPos()
    {
        outpos = mainAlphaCanvas.transform.GetComponent<RectTransform>().anchoredPosition;
    }

    void ShowMainmenu()
    {
        TransitionInMenu();
    }

    void HideMainMenu()
    {
        TransitionOutMenu();
    }

    void TransitionInMenu()
    {
        LeanTween.value (mainAlphaCanvas.gameObject, mainAlphaCanvasRect.anchoredPosition.y, inpos.y, 1);
        LeanTween.alphaCanvas(mainAlphaCanvas, 1, 1);//  value(mainAlphaCanvas.gameObject, mainAlphaCanvasRect.anchoredPosition.y, inpos.y, 1);

        //iTween.ValueTo(mainAlphaCanvas.gameObject, MoveYTo(inpos.y));
        //iTween.ValueTo(mainAlphaCanvas.gameObject, AlphaTo(1));
        toggled = true;
    }

    void TransitionOutMenu()
    {
        LeanTween.value(mainAlphaCanvas.gameObject, mainAlphaCanvasRect.anchoredPosition.y, outpos.y, 1);
        LeanTween.alphaCanvas(mainAlphaCanvas, 0, 1);//  value(mainAlphaCanvas.gameObject, mainAlphaCanvasRect.anchoredPosition.y, inpos.y, 1);

        //iTween.ValueTo(mainAlphaCanvas.gameObject, MoveYTo(outpos.y));
        //iTween.ValueTo(gameObject, AlphaTo(0));
        toggled = false;
    }

    Hashtable MoveYTo(float yValue)
    {
        Hashtable movementTransition = new Hashtable();
        movementTransition.Add("from", mainAlphaCanvasRect.anchoredPosition.y);
        movementTransition.Add("to", yValue);
        movementTransition.Add("onupdatetarget", gameObject);
        movementTransition.Add("onupdate", "UpdateY");
        return movementTransition;
    }

    Hashtable AlphaTo(float value)
    {
        Hashtable alphaTransition = new Hashtable();
        alphaTransition.Add("from",mainAlphaCanvas.alpha);
        alphaTransition.Add("to", value);
        alphaTransition.Add("onupdatetarget", gameObject);
        alphaTransition.Add("onupdate", "UpdateAlpha");
        return alphaTransition;
    }

    public void UpdateAlpha(float newValue)
    {
        mainAlphaCanvas.alpha = newValue;
    }

    public void UpdateY(float newValue)
    {
        mainAlphaCanvasRect.anchoredPosition = new Vector2(mainAlphaCanvasRect.anchoredPosition.x,newValue);
    }

    IEnumerator ReactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        transitioning = false;
    }

    public void InitializeMenuActive(float touchPosX)
    {
        if (touchPosX > .5)
            if (iActiveCanvas + 1 > subCanvases.Length)
                iCanvasToActivate = 0;
            else
                iCanvasToActivate += 1;
        else if (touchPosX < .5)
        {
            if (iActiveCanvas - 1 < 0)
                iCanvasToActivate = subCanvases.Length-1;
            else
                iCanvasToActivate -= 1;
        }
        else
        {
            Debug.Log("You touched the exact middle...Here's some confetti");
            //Fire Confetti
        }
        //If touch.x < .5 then go from left to right else initialize right to left
        //if touch.x == .5 then Shoot Confeti (Easter Egg for being so precise)
    }

    public void UpdateMenuPosition(Vector2 touchPos)
    {
        //If the initalized side is left and touchpos.x => .5 then bring the active menu forward
        //If the initalized side is right and touchpos.x <= .5 then bring the active menu forward
    }



    void BringMenuForward(Transform menuToBringForward)
    {
        menuToBringForward.SetAsLastSibling();
    }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}
