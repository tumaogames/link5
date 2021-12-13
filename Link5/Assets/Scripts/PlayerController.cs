using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    private Tile selectedTile;
    private int previousTileValue;
    public GameObject scanner;
    private Camera mainCamera;
    public bool isPlayerTurn;
    private int chipId;
    public bool StopPlayerInputs = true;

    // Start is called before the first frame update
    void Start()
    {
        chipId = 0;
        mainCamera = Camera.main;
        isPlayerTurn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isGamePaused || GameManager.Instance.isGameOver)
        {
            return;
        }
        if(StopPlayerInputs)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit2D = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, LayerMask.NameToLayer("IgnoreRaycast"));

            if (hit2D.collider != null)
            {
                selectedTile = hit2D.collider.transform.GetComponent<Tile>();
                
                if (ValidMove() && isPlayerTurn)
                {
                    GameManager Game = GameManager.Instance;
                    if (Game.gameMode == GameMode.Tutorial)
                    {
                        TutorialMode(Game);
                    }
                    else
                    {
                        PlaceChipOnTile();
                    }
                }
                else
                {
                    GameManager.Instance.UIManager.SetTurnStatusText("Wrong Turn!");
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.WrongMoveSFX);
                }
            }
        }
    }

    private void TutorialMode(GameManager Game)
    {
        void CanContinue()
        {
            Game.TutorialConditionMeet = true;
            PlaceChipOnTile();
        }
        //Normal Play
        if (Game.TutorialCondition == TutorialCondition.NoCondition)
        {
            if (selectedTile.value >= previousTileValue || selectedTile.type == Tile.TileType.Wild)
            {
                CanContinue();
            }
            else
            {
                GameManager.Instance.ShowTutorialPanel(101);
                GameManager.Instance.PlayerWrongMove();
            }
        }// Continue tutorial when clicked on Lower value Tile
        if (Game.TutorialCondition == TutorialCondition.LowerTitleClicked)
        {
            if (selectedTile.value < previousTileValue)
            {
                GameManager.Instance.ShowTutorialPanel(101);
            }
            else
            {
                CanContinue();
            }
        }// Continue tutorial when clicked on Higher value Tile
        if (Game.TutorialCondition == TutorialCondition.HigherTileClicked)
        {
            if (selectedTile.value > previousTileValue)
            {
                CanContinue();
            }
        }
        // Continue tutorial when clicked on Higher Or Equal value Tile
        if (Game.TutorialCondition == TutorialCondition.HigherOrEqualClicked)
        {
            if (selectedTile.value >= previousTileValue)
            {
                CanContinue();
            }
        }// Continue tutorial when click on Yellow Tile 
        if (Game.TutorialCondition == TutorialCondition.YellowTileClicked)
        {
            if (selectedTile.type == Tile.TileType.Wild)
            {
                CanContinue();
            }
            else
            {
                GameManager.Instance.ShowTutorialPanel(104);
            }
        }
    }

    private void PlaceChipOnTile()
    {
        SpawnChip(GameManager.Instance.localSelection, selectedTile.transform.position);
        selectedTile.isOccupied = true;
        previousTileValue = selectedTile.value;

        MoveData moveData = new MoveData();
        moveData.ChipPosition = selectedTile.transform.position;

        GameManager.Instance.MakeTurn(1, moveData);

        GameManager.Instance.tutorialEndedTurn = true;
    }

    public void SpawnChip(GameObject chipPrefab, Vector3 position, bool isLocal = true)
    {
        ScannerManager.Instance.Clear();
        SoundManager.Instance.PlaySFX(SoundManager.Instance.ChipSFX);
        GameObject chip = Instantiate(chipPrefab, position, Quaternion.identity) as GameObject;

        if (isLocal)
        {
            chipId = Random.Range(0, 101);
            chip.GetComponent<Chip>().Id = chipId;
            chip.transform.name = transform.name + chipId.ToString();
            scanner.transform.position = position;
            GameManager.Instance.localChips.Add(chip.GetComponent<Chip>());
        }
        else
        {
            GameManager.Instance.remoteChips.Add(chip.GetComponent<Chip>());
        }
    }

    private bool ValidMove()
    {
        bool isValidMove = false;
        if (!StopPlayerInputs)
        {
            switch (GameManager.Instance.gameMode)
            {
                case GameMode.Tutorial:
                    switch (selectedTile.type)
                    {
                        case Tile.TileType.Normal:
                            isValidMove = !selectedTile.isOccupied;
                            break;
                        case Tile.TileType.Blocked:
                            break;
                        case Tile.TileType.Wild:
                            isValidMove = !selectedTile.isOccupied;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    switch (selectedTile.type)
                    {
                        case Tile.TileType.Normal:
                            isValidMove = !selectedTile.isOccupied && (selectedTile.value >= previousTileValue);
                            break;
                        case Tile.TileType.Blocked:
                            break;
                        case Tile.TileType.Wild:
                            isValidMove = !selectedTile.isOccupied;
                            break;
                        default:
                            break;
                    }
                    break;
            }
 
        }

        return isValidMove;
    }
}
