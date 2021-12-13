using System.Collections.Generic;
using UnityEngine;
using libCoinPaymentsNET;
using UnityEngine.Events;

public enum Currency
{
    BTC, LTCT
}

public class CoinProduct{

    public float m_price;
    public Currency m_currency;
    public string m_productName;

    public  CoinProduct(float price, Currency currency,string productName = "Product Name")
    {
        this.m_price = price;
        this.m_currency = currency;
        this.m_productName = productName;

    }
}

public class CointPaymentsManager : MonoBehaviour
{

    //public string m_publicKey;
    //public string m_privateKey;
    CoinPayments m_cointPayments;
    public CoinProduct m_currentProduct;
    public CreateTXResponse m_currentCreatedTX;
    private string m_currentBuyerEmail;
    //UI 
    public UICoinPaymentController m_uiCoinPaymentController;
    public UnityEvent m_PaidEvent;
    

    private void Start()
    {
        m_cointPayments = new CoinPayments("5a4d67C857F05C2509267c400C2BE1f8Ef527c8CAE7C6fa3F4709406577d4e7b", "93382ce4654195242b2f6af4bbeb83f16e5e4f8eebb4a29b1c515101f21dca05");
        //SetCurrentProduct(0.0001f,Currency.LTCT);
        //OnClickProductButton();
    }

    public void OnClickProductButton()
    {
        m_uiCoinPaymentController.OpenWindow(m_currentProduct.m_price.ToString(),m_currentProduct.m_currency.ToString(), m_currentProduct.m_productName);
    }

    public void SetCurrentProduct(float amount, Currency currency, string productName = null)
    {

        m_currentProduct = new CoinProduct(amount, currency, productName);
    }

    

    public void OnClickAPIButton()
    {
       
        SortedList<string, string> parms = new SortedList<string, string>();
        parms["amount"] = m_currentProduct.m_price.ToString();
        parms["currency1"] = m_currentProduct.m_currency.ToString();
        parms["currency2"] = m_currentProduct.m_currency.ToString();
        if(m_uiCoinPaymentController.emailField.text.Length > 0)
        {
            parms["buyer_email"] = m_uiCoinPaymentController.emailField.text;
            
            m_currentCreatedTX = m_cointPayments.CallAPI("create_transaction",parms);
            Debug.LogError(m_currentCreatedTX.error);
            if(m_currentCreatedTX.result.address != null)
            {
                CheckStatus();
            }
            else
            {
                string message = m_currentCreatedTX.error != null ? m_currentCreatedTX.error : "Error ocurred, please try again later.";
                m_uiCoinPaymentController.ShowStatusDialog(message);
            
            }

        }
        else
        {
            m_uiCoinPaymentController.ShowStatusDialog("Please entrer a email to checkout transaction");
        }
       
       
    }

    private void CheckStatus(bool isCheckByButton = false)
    {
        SortedList<string, string> parms = new SortedList<string, string>();
        parms["txid"] = m_currentCreatedTX.result.txn_id;
        var resInfo = m_cointPayments.GetInfo("get_tx_info", parms);
        if (resInfo != null){
            Debug.Log(m_currentCreatedTX.result.address);
            Debug.Log(m_currentCreatedTX.result.amount);
            Debug.Log(m_currentCreatedTX.result.checkout_url);
            Debug.Log(m_currentCreatedTX.result.confirm_needed);
            Debug.Log(m_currentCreatedTX.result.dest_tag);
            Debug.Log(m_currentCreatedTX.result.txn_id);
            m_uiCoinPaymentController.ShowTxInfo(resInfo.result, m_currentCreatedTX.result.txn_id);
            if (isCheckByButton)
            {
                m_uiCoinPaymentController.ShowStatusDialog(resInfo.result.status_text);
                if (resInfo.result.status == 1 || resInfo.result.status == 100)
                {
                    Debug.Log("Succesfull pay!!!");
                    ResetValues();
                    if (m_PaidEvent != null)
                    {
                        m_PaidEvent.Invoke();
                    }
                }
            }
        }
    }

    public void OnClickCheckStatus()
    {
        CheckStatus(true);
    }

    public void OnClickOpenBrowser()
    {
        Application.OpenURL(m_currentCreatedTX.result.checkout_url);
    }


   public void OnClickCancelButton()
    {
        ResetValues();
    }


    public void ResetValues()
    {
        m_currentBuyerEmail = null;
        m_currentCreatedTX = null;
        m_currentProduct = null;
        m_uiCoinPaymentController.HideWindow();
    }

    public void TestPaidEvent()
    {
        m_uiCoinPaymentController.ShowStatusDialog("PAID!!!!!");
    }
}
