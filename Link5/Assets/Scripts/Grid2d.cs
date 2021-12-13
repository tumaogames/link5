using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid2d : MonoBehaviour
{
    public GameObject[,] Grid;
    public int gridWidth;
    public int gridHeight;
    public float gridSizeX;
    public float gridSizeY;
    public GameObject tilePrefab;


    // Start is called before the first frame update
    void Start()
    {
        Grid = new GameObject[gridWidth,gridHeight];
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateGrid()
    {
        Vector2 offset = tilePrefab.GetComponent<SpriteRenderer>().bounds.size;
        float startX = transform.position.x;
        float startY = transform.position.y;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(i * gridSizeX, j * gridSizeY), Quaternion.identity, this.transform);
                Grid[i, j] = tile;
            }
        }
    }
}
