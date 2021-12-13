using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BoardScanner : MonoBehaviour
{
    public enum ScannerType
    {
        Horizontal,
        Vertical,
        DiagonalNE,
        DiagonalNW
    }

    public ScannerType scannerType;
    public List<Chip> chipsList;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearLists()
    {
        chipsList.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("LocalChip"))
        {
            Chip chip = collision.gameObject.GetComponent<Chip>();
            if (!chipsList.Contains(chip))
            {
                chipsList.Add(chip);
                if (chipsList.Count >= 5)
                {
                    Scan();
                }
            }            
        }
    }

    private void Scan()
    {
        switch (scannerType)
        {
            case ScannerType.Horizontal:
                chipsList = chipsList.OrderBy(x => x.transform.position.x).ToList();
                break;
            case ScannerType.Vertical:
                chipsList = chipsList.OrderBy(y => y.transform.position.y).ToList();
                break;
            case ScannerType.DiagonalNE:
                chipsList = chipsList.OrderBy(x => x.transform.position.y).ThenBy(y => y.transform.position.y).ToList();
                break;
            case ScannerType.DiagonalNW:
                chipsList = chipsList.OrderBy(x => x.transform.position.y).ThenBy(y => y.transform.position.y).ToList();
                break;
            default:
                break;
        }

        int adjacentChipsCount = 0;
        int chipsCount = chipsList.Count;
        for (int i = 0; i < chipsCount - 1; i++)
        {
            if (chipsList[i].IsAdjacent(chipsList[i + 1]))
            {
                ++adjacentChipsCount;
            }
            else
            {
                break;
            }
        }
        if (adjacentChipsCount >= 4)
        {
            GameManager.Instance.ShowResultAndGameOver(ResultType.LocalWin);
            return;
        }
    }
}
