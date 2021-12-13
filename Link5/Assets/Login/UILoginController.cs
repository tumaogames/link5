using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoginController : MonoBehaviour
{
    public RectTransform m_mainPanel;
    public RectTransform loginPanel;
    public RectTransform signUpPanel;
    public RectTransform switchButtonSU;
    public RectTransform switchButtonLO;
    public RectTransform lockUIPanel;
    public RectTransform loginForm;
    public RectTransform singupForm;
    public RectTransform userPanel;
    public RectTransform startPanel;
    public RectTransform buyPanel;
    public RectTransform getPanel;
    public RectTransform pointsPanel;
    public RectTransform paidPanel;
    public RectTransform friendsPanel;
    public RectTransform startGamePanel;
    public RectTransform UserInfoSharePanel;
    public RectTransform uiLockPanel;
    public RectTransform warningPanel;
    public Text warningPanelText;
    [Tooltip("UI ToolTip for Room Name to Short.")]
    [SerializeField]
    private GameObject RoomNameToolTip;
    public string warningMsg;
    Vector3 originalPointsPosition;
    private RectTransform m_activePanel;
    public Text lockUIPanelText;
    bool isLogin;

    private void Awake()
    {
        m_activePanel = singupForm;
        originalPointsPosition = pointsPanel.localPosition;
        //RectTransform[] panels = gameObject.GetComponents<RectTransform>();
        //foreach(RectTransform rect in panels)
        //{
        //    Debug.Log("Panel Name: " + rect.gameObject.name);
        //}
        
    }
    // Start is called before the first frame update
    void Start()
    {
        m_mainPanel.localScale = Vector3.zero;
        LeanTween.scale(m_mainPanel, new Vector3(1f, 1f, 1f), 0.6f).setEase(LeanTweenType.easeOutBack);

        string origText = lockUIPanelText.text;
        lockUIPanelText.text = "";
        LeanTween.value(gameObject, 0, (float)origText.Length, 6f).setEase(LeanTweenType.easeOutQuad).setOnUpdate((float val) =>
        {
            lockUIPanelText.text = origText.Substring(0, Mathf.RoundToInt(val));
        }).setLoopClamp().setDelay(2.0f);
    }

    public void OnClickSwitchButton()
    {
        if (!isLogin)
        {
            ChangeForm(loginPanel, signUpPanel, switchButtonSU, switchButtonLO);
            isLogin = true;
        }
        else
        {
            ChangeForm(signUpPanel, loginPanel, switchButtonLO, switchButtonSU);
            isLogin = false;
        }

    }

    private void ChangeForm(RectTransform currentForm, RectTransform nextForm, RectTransform currentSwitch, RectTransform nextSwitch)
    {

        currentForm.localScale = Vector3.zero;
        currentSwitch.localScale = Vector3.zero;
        nextForm.localScale = new Vector3(1f, 1f, 1f);
        nextForm.RotateAround(nextForm.position, Vector3.up, -180f);
        LeanTween.rotateAround(nextForm, Vector3.up, 180f, 2f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(nextSwitch, new Vector3(1, 1, 1), 0.6f).setEase(LeanTweenType.easeOutElastic).setDelay(2f);
    }

    public void setActiveUnlockUI(bool active)
    {
        lockUIPanel.gameObject.SetActive(active);
        if (active)
        {
            Debug.Log("Lock screen");
        }
        else
        {
            Debug.Log("Unlock screen");
        }
    }

    public void ShowWarningPanel (bool active) 
    {
        if (active)
        {
            warningPanelText.text = warningMsg;
            LeanTween.scale(warningPanel, Vector3.one, 0.6f).setEase(LeanTweenType.easeOutElastic);
        }
        else
        {
            LeanTween.scale(warningPanel, Vector3.zero, 0.6f).setEase(LeanTweenType.easeOutElastic);
            warningPanelText.text = "";
        }
    }

    public void ChangeActivePanel(string panelName)
    {
        if (m_activePanel != null)
            LeanTween.scale(m_activePanel, Vector3.zero, 0.6f).setEase(LeanTweenType.easeOutElastic);

        switch (panelName)
        {
            case "loginPanel":
                m_activePanel = loginPanel;
                break;
            case "loginForm":
                m_activePanel = loginForm;
                break;
            case "singupForm":
                m_activePanel = singupForm;
                break;
            case "userPanel":
                m_activePanel = userPanel;
                break;
            case "startPanel":
                m_activePanel = startPanel;
                break;
            case "buyPanel":
                m_activePanel = buyPanel;
                break;
            case "getPanel":
                m_activePanel = getPanel;
                break;
            case "paidPanel":
                m_activePanel = paidPanel;
                break;
            case "startGamePanel":
                m_activePanel = startGamePanel;
                break;

        }
        LeanTween.scale(m_activePanel, Vector3.one, 0.6f).setEase(LeanTweenType.easeOutElastic);
    }
               
    public void ShowPointsPanel(bool show)
    {
        if (show)
            LeanTween.move(pointsPanel, Vector3.zero, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
        else
            LeanTween.move(pointsPanel, originalPointsPosition, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
    }

    public void ShowFriendsPanel(bool show)
    {
        if (show)
            LeanTween.scale(friendsPanel, Vector3.one, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
        else
            LeanTween.scale(friendsPanel, Vector3.zero, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
    }

    public void ShowUserInfo(bool show)
    {
        ShowHelperPanel(UserInfoSharePanel, show);
    }

    public void ShowAndHideRoomNameToolTip()
    {
        ShowAndHidePanel(RoomNameToolTip);
    }

    private void ShowAndHidePanel(GameObject Panel)
    {
        LeanTween.scale(Panel, Vector3.one, 0.5f).setEaseLinear();
        LeanTween.delayedCall(2.2f, () =>
        {
            LeanTween.scale(Panel, Vector3.zero, 0.4f).setEaseLinear();
            ChangeActivePanel("userPanel");
        });
    }

    private void ShowHelperPanel(GameObject Panel, bool show)
    {
        ShowHelperPanel(Panel.GetComponent<RectTransform>(), show);
    }

    private void ShowHelperPanel(RectTransform transform, bool show)
    {
        if (show)
            LeanTween.scale(transform, Vector3.one, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
        else
            LeanTween.scale(transform, Vector3.zero, 1f).setEase(LeanTweenType.clamp).setDelay(0.4f);
    }


    

}