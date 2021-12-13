using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public List<Tile> Tiles;

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
        Tiles = new List<Tile>(GetComponentsInChildren<Tile>());
    }

    // Update is called once per frame
    void Update()
    {
                
    }

    public Tile FindTileByPosition(Vector3 position)
    {
        int tileCount = Tiles.Count;
        for (int i = 0; i < tileCount; i++)
        {
            if (Tiles[i].transform.position == position)
            {
                return Tiles[i];
            }
        }

        return null;
    }

    private List<Tile> GetValidTiles()
    {
        List<Tile> validTiles = new List<Tile>();
        int tileCount = Tiles.Count;
        for (int i = 0; i < tileCount; i++)
        {
            if (!Tiles[i].isOccupied)
            {
                validTiles.Add(Tiles[i]);
            }
        }

        return validTiles;
    }

    public Vector3 FindRandomTilePosition()
    {
        List<Tile> validTiles = GetValidTiles();

        return validTiles[Random.Range(0, validTiles.Count)].transform.position;
    }
}
