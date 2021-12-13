using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using NativeShareNamespace;

public class NativeShareManager : MonoBehaviour
{
    public static NativeShareManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NativeShare()
    {
        //NativeShare nativeShare = new NativeShare();
        //nativeShare.SetTitle("Link5!");
        //nativeShare.SetSubject("X challenged you to Link5!");
        //nativeShare.SetText("X challenged you to Link5! Url: ");
        //nativeShare.SetUrl(GetStoreURL());
        //nativeShare.Share();
    }

    private string GetStoreURL()
    {
        string url = "";
#if UNITY_ANDROID
        url = "";
#elif UNITY_IOS || UNITY_IPHONE
        link = "";
#endif
        return url;
    }
}
