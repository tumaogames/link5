using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using libCoinPaymentsNET;


public class UICoinPaymentController : MonoBehaviour
{


    public InputField amountField;
    public InputField currencyField;
    public InputField payAddressField;
    public InputField txIdField;
    public InputField emailField;

    public Text productNameText;
    public Text statusText;
    public Text amountText;
    public Text dialogTex;

    public RectTransform dialog;
    public RectTransform contentPay;
    public RectTransform contentInfo;
    
    bool createdTx  = false;




    public void OpenWindow(string amount, string currency, string productName = null)
    {
        amountField.text = amount;
        currencyField.text = currency;
        productNameText.text = productName;

        RectTransform panelWindows = this.GetComponent<RectTransform>();
        panelWindows.localScale = Vector3.zero;
        LeanTween.scale(panelWindows, new Vector3(1f, 1f, 1f),0.6f).setEase(LeanTweenType.easeOutElastic);

    }

    

    public void ShowTxInfo(TXInfo txInfo, string txID)
    {
        statusText.text = txInfo.status_text;
        amountText.text = txInfo.amountf.ToString() + " " +  txInfo.coin ;
        txIdField.text = txID;
        payAddressField.text = txInfo.payment_address;
        createdTx = true;
        contentInfo.gameObject.SetActive(false);
        contentPay.gameObject.SetActive(true);
       

    }

    public void HideWindow()
    {

        this.GetComponent<RectTransform>().localScale = Vector3.zero;
        contentInfo.gameObject.SetActive(true);
        contentPay.gameObject.SetActive(false);
        createdTx = false;

    }

    public bool IsCreatedTX()
    {
        return createdTx;
    }

    public void ShowStatusDialog(string message)
    {
        dialog.gameObject.SetActive(true);
        dialogTex.text = message;
    }

    public void HideDialog()
    {
        dialog.gameObject.SetActive(false);
    }

  
   

}
