using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using libCoinPaymentsNET;

public class CointPaymentsProduct : MonoBehaviour
{
    public float  price;
    public Currency currency;
    public string productName;
    public float value;
    public CointPaymentsManager m_manager;

    public void OnClickButton()
    {
        m_manager.SetCurrentProduct(price, currency, productName);
        m_manager.OnClickProductButton();
    }
}
