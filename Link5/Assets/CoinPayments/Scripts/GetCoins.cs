using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Link5;

public class GetCoins : MonoBehaviour
{
    public CointPaymentsProduct m_coinProduct;
    public float CurrentCoinsAmount = 1000; // Started coins amount. In your project it can be set up from CoinsManager or from PlayerPrefs and so on
    public float PreviousCoinsAmount;
    public Text CoinsDeltaText;         // Pop-up text with wasted or rewarded coins amount
    public Text CurrentCoinsText;

    private void Awake()
    {
        
        CurrentCoinsAmount = (float)GameData.GetInstance().gamePoints;
        PreviousCoinsAmount = CurrentCoinsAmount;
        CurrentCoinsText.text = CurrentCoinsAmount.ToString();
    }
    public void OnPaidReceived()
    {
        LeanTween.scale(this.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.6f).setEase(LeanTweenType.easeOutElastic);
    }

    public void OnClickGetCoinsButton()
    {
        LeanTween.scale(this.GetComponent<RectTransform>(),Vector3.zero, 0.6f).setEase(LeanTweenType.easeOutElastic);
        CurrentCoinsAmount += m_coinProduct.value;
        GameData.GetInstance().IncreasePoints(m_coinProduct.value);
        CoinsDeltaText.text = "+" + m_coinProduct.value;
        CoinsDeltaText.gameObject.SetActive(true);
        StartCoroutine(UpdateCoinsAmount());
        StartCoroutine(HideCoinsDelta());
    }

    private IEnumerator HideCoinsDelta()
    {
        yield return new WaitForSeconds(1f);
        CoinsDeltaText.gameObject.SetActive(false);
    }

    private IEnumerator UpdateCoinsAmount()
    {
        // Animation for increasing and decreasing of coins amount
        const float seconds = 0.5f;
        float elapsedTime = 0;

        while (elapsedTime < seconds)
        {
            CurrentCoinsText.text = Mathf.Floor(Mathf.Lerp(PreviousCoinsAmount, CurrentCoinsAmount, (elapsedTime / seconds))).ToString();
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
       
        PreviousCoinsAmount = CurrentCoinsAmount;
        CurrentCoinsText.text = CurrentCoinsAmount.ToString();
    }
}
