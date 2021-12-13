using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FacebookUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image profileImage;
    public Button inviteButton;

    // Start is called before the first frame update
    void Start()
    {
        FacebookManager.Instance.facebookUI = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
