using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class UIManager : MonoBehaviour
{
    [Header("GamePanel:")]
    public RectTransform ButtonPanel;
    public TextMeshProUGUI TurnStatusText;
    public TextMeshProUGUI WaitTimerText;

    [Header("GameOverPanel:")]
    public GameObject GameOverPanel;
    public GameObject StatPanel;
    public Image Background;
    public TextMeshProUGUI ResultText;
    public Color WinColor;
    public Color LoseColor;

    [Header("ConnectionPanel:")]
    public GameObject ConnectionPanelUI;
    public Image ConnectionPanelBackground;
    public TextMeshProUGUI FeedbackText;
    public Button CancelButton;
    
    private int waitTimer;

    // Start is called before the first frame update
    void Start()
    {
        foreach  (Button button in GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(()=> {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.ClickSFX);
            });
        }

        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Tutorial:
            case GameMode.SinglePlayer:
                ConnectionPanelUI.gameObject.SetActive(false);
                break;
            case GameMode.MultiPlayer:
                ConnectionPanelUI.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTimer()
    {
        waitTimer = 30;
        StopAllCoroutines();
        StartCoroutine(WaitTimerRoutine());
    }

    IEnumerator WaitTimerRoutine()
    {
        while (waitTimer >= 0)
        {
            yield return new WaitForSeconds(1f);

            --waitTimer;
            WaitTimerText.text = "(" + waitTimer.ToString() + ")";
            if (waitTimer <= 0)
            {
                OnCancelButtonClicked();
            }
        }
    }

    public void ShowGameOverUI(ResultType result)
    {
        GameOverPanel.SetActive(true);
        switch (result)
        {
            case ResultType.LocalWin:
                ResultText.color = WinColor;
                ResultText.text = "You Win!";
                break;
            case ResultType.LocalLoss:
                ResultText.color = LoseColor;
                ResultText.text = "You Lose!";
                break;
            default:
                break;
        }

        ResultText.GetComponent<RectTransform>().DOScale(Vector3.one, 1f).SetEase(Ease.InOutElastic).OnComplete(()=> 
        {
            ResultText.GetComponent<RectTransform>().DOAnchorPosY(200, 1f).SetEase(Ease.InOutBack).OnComplete(() =>
            {
                
            });

            Color color = Background.color;
            color.a = 1;
            Background.DOColor(color, 1f).OnComplete(() =>
            {
                StatPanel.SetActive(true);
            });
        });
    }

    public void SetTurnStatusText(string turnStatus)
    {
        TurnStatusText.text = turnStatus;
    }

    public void SetFeedbackText(string message)
    {
        FeedbackText.text = "";
        FeedbackText.text = message;
    }

    public void OnCancelButtonClicked()
    {
        GameManager.Instance.DisconnectPlayer();
    }

    internal void RefreshConnectionPanel(bool connectionState)
    {
        Color color = ConnectionPanelBackground.color;
        color.a = 0;
        if (connectionState)
        {
            if (!ConnectionPanelUI.activeInHierarchy)
            {
                ConnectionPanelUI.SetActive(connectionState);
                color.a = 1;
                ConnectionPanelBackground.DOColor(color, .1f);
                CancelButton.GetComponent<Image>().DOFade(1, .5f);
                FeedbackText.DOFade(1, .5f);
            }
        }
        else
        {
            if (ConnectionPanelUI.activeInHierarchy)
            {
                CancelButton.GetComponent<Image>().DOFade(0, .5f);
                FeedbackText.DOFade(0, .5f);
                ConnectionPanelBackground.DOColor(color, .5f).OnComplete(() =>
                {
                    ConnectionPanelUI.SetActive(connectionState);
                });
            }
        }
    }
}
