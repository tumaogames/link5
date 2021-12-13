using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerManager : MonoBehaviour
{
    public static ScannerManager Instance;
    public List<BoardScanner> boardScanners;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Scan()
    {
        StopAllCoroutines();
        StartCoroutine(ScanRoutine());
    }

    private IEnumerator ScanRoutine()
    {
        yield return new WaitForSeconds(.02f);

        int scannersCount = boardScanners.Count;
        for (int i = 0; i < scannersCount; i++)
        {
            boardScanners[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(.02f);
            boardScanners[i].gameObject.SetActive(false);
        }
    }

    public void Clear()
    {
        foreach (BoardScanner scanner in boardScanners)
        {
            scanner.ClearLists();
        }
    }
}
