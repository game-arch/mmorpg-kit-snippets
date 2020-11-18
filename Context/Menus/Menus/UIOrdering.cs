using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOrdering : MonoBehaviour
{
    public bool shouldAffectOrdering = true;
    public bool canBeClosedByEscape = true;
    public List<UIOrdering> closePeersOnEscape = new List<UIOrdering>();
    [Tooltip("It will toggle `ui` when key with `keyCode` pressed or button with `buttonName` pressed.")]
    public KeyCode keyCode;
    [Tooltip("It will toggle `ui` when key with `keyCode` pressed or button with `buttonName` pressed.")]
    public string buttonName;

    [Tooltip("indicator for performing behaviors when certain elements are shown or hidden")]
    public string uiType;


    protected UISceneGameplay mainUi;


    protected UISceneGameplay MainUi
    {
        get
        {
            if (mainUi == null)
                mainUi = GetComponentInParent<UISceneGameplay>();
            return mainUi;
        }
    }

    public void Show()
    {
        Debug.Log("Show!");
        UIBase uiBase = GetComponent<UIBase>();
        if (uiBase != null)
            uiBase.Show();
        else
            gameObject.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("Hide!");
        UIBase uiBase = GetComponent<UIBase>();
        if (uiBase != null)
            uiBase.Hide();
        else
            gameObject.SetActive(false);
        if (MainUi.orderedUIs.Contains(this))
            MainUi.orderedUIs.Remove(this);
    }

    public void Toggle()
    {
        if (IsVisible())
            Hide();
        else
            Show();
    }

    public bool IsVisible()
    {
        UIBase uiBase = GetComponent<UIBase>();
        if (uiBase != null)
            return uiBase.IsVisible();
        return gameObject.activeInHierarchy;
    }

    public void UpdateInput()
    {

        if (Input.GetKeyDown(keyCode) || InputManager.GetButtonDown(buttonName))
        {
            Debug.Log(gameObject.name + "Toggle!");
            Toggle();
        }
        if (shouldAffectOrdering)
        {
            if (MainUi.orderedUIs.Contains(this) && !IsVisible())
            {
                MainUi.orderedUIs.Remove(this);
                foreach (UIOrdering peer in closePeersOnEscape)
                {
                    peer.Hide();
                }
            }
            if (!MainUi.orderedUIs.Contains(this) && IsVisible())
            {
                MainUi.orderedUIs.Add(this);
            }
        }
    }
}
