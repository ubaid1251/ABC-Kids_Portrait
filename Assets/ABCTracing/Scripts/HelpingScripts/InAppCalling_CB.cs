//using Firebase.Analytics;//lock
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class InAppCalling_CB : MonoBehaviour
{
    public Text LocalizedPriceText;
    public TMP_Text LocalizedPriceTextT;
    public int IAPIndex = 0;
    public UnityEvent OnPurchaseFail;
    // Use this for initialization
    void OnEnable()
    {
        //print(IAPIndex);
        //if (LocalizedPriceText)//lock
        //    LocalizedPriceText.text = unityInAppPurchase_CB.keys[IAPIndex].priceInDollars;//lock
        //if (LocalizedPriceTextT)//lock
        //    LocalizedPriceTextT.text = LocalizedPriceTextT.text = "In Just " + unityInAppPurchase_CB.keys[IAPIndex].priceInDollars;//lock

        StartCoroutine(SetLocalPrice());//lock
    }

    // Update is called once per frame
    public void BuyInApp(/*int value*/)
    {
        //soundref.instance.playTap();
        try
        {
            //IAPIndex = value;
            unityInAppPurchase_CB.Failed += OnPurchaseFailed;//lock
            unityInAppPurchase_CB.instance.GetComponent<unityInAppPurchase_CB>().BuyProductID(unityInAppPurchase_CB.keys[IAPIndex].Id, IAPIndex);//lock
        }
        catch (Exception ex)
        {
            unityInAppPurchase_CB.Failed -= OnPurchaseFailed;//lock
            //GeneralScript._instance.SendExceptionEmail(ex.Message.ToString());
        }

    }
    public void BuyInAppCustom(int value)
    {
        try
        {
            //IAPIndex = value;
            unityInAppPurchase_CB.Failed += OnPurchaseFailed;//lock
            unityInAppPurchase_CB.instance.GetComponent<unityInAppPurchase_CB>().BuyProductID(unityInAppPurchase_CB.keys[value].Id, value);//lock
        }
        catch (Exception ex)
        {
            unityInAppPurchase_CB.Failed -= OnPurchaseFailed;//lock
            //GeneralScript._instance.SendExceptionEmail(ex.Message.ToString());
        }

    }
    public void OnPurchaseFailed()
    {
        string reason = PlayerPrefs.GetString("ParentalReason");
        //FirebaseAnalytics.LogEvent("ParentsPopupClosed", new Parameter("reason", reason), new Parameter("result", "Failed"));//lock
        //Debug.Log("in  purchase failed ");
        unityInAppPurchase_CB.Failed -= OnPurchaseFailed;//lock
        OnPurchaseFail.Invoke();

    }
    //lock
    IEnumerator SetLocalPrice()
    {
        yield return new WaitUntil(() => unityInAppPurchase_CB.instance.IsInitilized);
        try
        {
            Product p = unityInAppPurchase_CB.instance.GetComponent<unityInAppPurchase_CB>().GetProductDetail(unityInAppPurchase_CB.keys[IAPIndex].Id);
            if (p.availableToPurchase)
            {
                if (p != null)
                {
                    string priceString = p.metadata.localizedPriceString;

                    string amount = "";

                    for (int index = 0; index < priceString.Length; index++)
                    {
                        if (Char.IsDigit(priceString[index]) || priceString[index] == '.' || priceString[index] == ',')
                            amount += priceString[index];
                    }

                    float price = float.Parse(amount);
                    string currency = priceString.Remove(priceString.Length - amount.Length, amount.Length);

                    Debug.Log(priceString.Length + " " + amount.Length + " " + price + " " + currency);
                    price = Mathf.RoundToInt(price);

                    //Debug.Log("Real price from store");
                    PlayerPrefs.SetString("MyPrice" + IAPIndex, p.metadata.localizedPriceString.ToString());
                    if (LocalizedPriceText)
                        LocalizedPriceText.text = p.metadata.localizedPriceString.ToString();
                    if (LocalizedPriceTextT)
                        LocalizedPriceTextT.text = LocalizedPriceTextT.text = "In Just " + currency + "" + price.ToString();
                }
            }
            else
            {
                if (PlayerPrefs.HasKey("MyPrice" + IAPIndex))
                {
                    if (LocalizedPriceText)
                        LocalizedPriceText.text = PlayerPrefs.GetString("MyPrice" + IAPIndex, "14.99$");
                    if (LocalizedPriceTextT)
                        LocalizedPriceTextT.text = PlayerPrefs.GetString("MyPrice" + IAPIndex, "14.99$");
                }
                else
                {
                    if (LocalizedPriceText)
                        LocalizedPriceText.text = unityInAppPurchase_CB.keys[IAPIndex].priceInDollars;
                    if (LocalizedPriceTextT)
                        LocalizedPriceTextT.text = unityInAppPurchase_CB.keys[IAPIndex].priceInDollars;
                }

                //Debug.Log("Not available to purchase ");
            }
        }
        catch (Exception ex)
        {
            //LocalizedPriceText.text = unityInAppPurchase_CB.keys[IAPIndex].priceInDollars;
            Debug.Log("Exception in setting price text  " + ex.Message);
        }

    }
}
