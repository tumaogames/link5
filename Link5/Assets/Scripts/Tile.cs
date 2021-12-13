using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Normal,
        Blocked,
        Wild
    }

    public TileType type;
    public int value;
    public bool isOccupied;

    // Start is called before the first frame update
    void Start()
    {
        switch (type)
        {
            case TileType.Normal:
                break;
            case TileType.Blocked:
                value = -1;
                break;
            case TileType.Wild:
                value = 0;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
