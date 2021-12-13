using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class StatPanel : MonoBehaviour
{
    public TextMeshProUGUI TurnCountText;
    public TextMeshProUGUI LocalNameText;
    public TextMeshProUGUI LocalMovesText;
    public TextMeshProUGUI RemoteNameText;
    public TextMeshProUGUI RemoteMovesText;

    private void OnEnable()
    {
        TurnCountText.text = GameManager.Instance.turnManager.Turn.ToString();
        LocalNameText.text = GameManager.Instance.LocalPlayerText.text;
        LocalMovesText.text = GameManager.Instance.localChips.Count.ToString();
        RemoteNameText.text = GameManager.Instance.RemotePlayerText.text;
        RemoteMovesText.text = GameManager.Instance.remoteChips.Count.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
