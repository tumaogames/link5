using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Chip : MonoBehaviour
{
    public int Id;

    void Awake()
    {   
        transform.localScale = Vector2.zero;    
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnEffect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnEffect()
    {
        transform.DOScale(Vector2.one, .1f).OnComplete(()=>
        {
            transform.DOPunchScale(new Vector2(.25f, .25f), .7f, 6, 25);
        });
    }

    public bool IsAdjacent(Chip chip)
    {
        float distance = Vector2.Distance(transform.position, chip.transform.position);
        if (distance > 1.4f && distance < 1.5f)
        {
            distance = Mathf.Ceil(distance);
        }
        //Debug.Log("From: " + transform.name + " -- To: " + chip.transform.name + " " + distance + "--" + Vector2.Distance(transform.position, chip.transform.position));
        if (Mathf.Abs(Mathf.RoundToInt(distance)) == 1)
        {
            return true;
        }
        return false;
    }
}
