using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeCellManager : MonoBehaviour {
    private GameObject _environment;
    private GameManager _gameManager;
    public MazeCell[,] mazeCells;
    private int _mazeRow;
    private int _mazeColumn;
    public Transform arrowPrefab;
    private GameObject _arrows;

    // Origin is top left corner corner, Z++ = east, X++ = South
    private void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _arrows = GameObject.Find("Arrows").gameObject;
    }

    public void SetMazeCells(MazeCell[,] mazeCells, int mazeRow, int mazeColumn) {
        this.mazeCells = mazeCells;
        _mazeRow = mazeRow;
        _mazeColumn = mazeColumn;
    }

    public int GetMapSize() {
        return mazeCells.Length;
    }

    private bool CellIsAvailable(int row, int column) {
        return row >= 0 && row < _mazeRow && column >= 0 && column < _mazeColumn;
    }

    private MazeCell GetCellIfExists(int row, int column) {
        if (CellIsAvailable(row, column)) return mazeCells[row, column];
        return null;
    }

    public void ShowCeiling(bool show) {
        foreach (MazeCell mazeCell in mazeCells) {
            mazeCell.ceiling.SetActive(show);
        }
    }

    private List<MazeCell> GetNeighborWalkableTiles(MazeCell mazeCell) {
        // Returns the neighbor tiles that the player can directly reach
        List<MazeCell> neighborTiles = new List<MazeCell>();
        int x = (int) mazeCell.transform.position.x;
        int z = (int) mazeCell.transform.position.z;

        MazeCell northCell = GetCellIfExists(x - 1, z);
        if (northCell && !northCell.southWall && !mazeCell.northWall) {
            neighborTiles.Add(northCell);
        }

        MazeCell southCell = GetCellIfExists(x + 1, z);
        if (southCell && !southCell.northWall && !mazeCell.southWall) {
            neighborTiles.Add(southCell);
        }

        MazeCell eastCell = GetCellIfExists(x, z + 1);
        if (eastCell && !eastCell.westWall && !mazeCell.eastWall) {
            neighborTiles.Add(eastCell);
        }

        MazeCell westCell = GetCellIfExists(x, z - 1);
        if (westCell && !westCell.eastWall && !mazeCell.westWall) {
            neighborTiles.Add(westCell);
        }

        return neighborTiles;
    }

    public List<GameObject> GetNeighborTiles(GameObject mazeCell) {
        List<GameObject> neighborTiles = new List<GameObject>();
        if (mazeCell.transform.position.x - 1 > 0) {
            MazeCell northCell =
                mazeCells[(int) mazeCell.transform.position.x - 1, (int) mazeCell.transform.position.z];
            if (northCell) neighborTiles.Add(northCell.gameObject);
        }

        if (mazeCell.transform.position.x + 1 < _mazeRow) {
            MazeCell southCell =
                mazeCells[(int) mazeCell.transform.position.x + 1, (int) mazeCell.transform.position.z];
            if (southCell) neighborTiles.Add(southCell.gameObject);
        }

        if (mazeCell.transform.position.z - 1 > 0) {
            MazeCell westCell = mazeCells[(int) mazeCell.transform.position.x, (int) mazeCell.transform.position.z - 1];
            if (westCell) neighborTiles.Add(westCell.gameObject);
        }

        if (mazeCell.transform.position.z + 1 < _mazeColumn) {
            MazeCell eastCell = mazeCells[(int) mazeCell.transform.position.x, (int) mazeCell.transform.position.z + 1];
            if (eastCell) neighborTiles.Add(eastCell.gameObject);
        }

        return neighborTiles;
    }

    public void DoPathPlanning() {
        Debug.Log("Doing path planning");
        bool updated;
        do {
            updated = false;
            foreach (MazeCell mazeCell in mazeCells) {
                // Check Neighbor tiles
                List<MazeCell> neighborTiles = GetNeighborWalkableTiles(mazeCell);
                foreach (MazeCell neighborCell in neighborTiles) {
                    if (neighborCell.isExit) {
                        neighborCell.score = 0;
                        if (mazeCell.score == 1) continue;
                        updated = true;
                        mazeCell.score = 1;
                        SetAction(mazeCell, neighborCell);
                        InstantiateArrow(mazeCell);
                    }
                    else if (
                        neighborCell.score < mazeCell.score
                        && mazeCell.score != neighborCell.score + 1
                    ) {
                        mazeCell.score = neighborCell.score + 1;
                        SetAction(mazeCell, neighborCell);
                        updated = true;
                        InstantiateArrow(mazeCell);
                    }
                }
            }
        } while (updated);
        
    }

    private void SetAction(MazeCell cell, MazeCell neighborCell) {
        if (neighborCell.transform.position.z > cell.transform.position.z) {
            cell.action = "EAST";
        }
        else if (neighborCell.transform.position.z < cell.transform.position.z) {
            cell.action = "WEST";
        }
        else if (neighborCell.transform.position.x > cell.transform.position.x) {
            cell.action = "SOUTH";
        }
        else if (neighborCell.transform.position.x < cell.transform.position.x) {
            cell.action = "NORTH";
        }
        else {
            Debug.Log("Action not found");
        }
    }

    public List<GameObject> GetAllTiles() {
        return GameObject.FindGameObjectsWithTag("MazeCell").ToList();
    }

    public MazeCell GetTileUnderPlayer() {
        Ray ray = new Ray(_gameManager.player.transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit, 10);
        if (hit.collider) {
            return hit.collider.transform.parent.GetComponent<MazeCell>();
        }
        Debug.LogWarning("No tile under player");

        return null;
    }

    public void AddToRevealedTiles(MazeCell cell, List<MazeCell> revealedCells) {
        if (!HasBeenRevealed(cell, revealedCells)) {
            _gameManager.revealedCellsInRun.Add(cell);
        }
    }

    public bool HasBeenRevealed(MazeCell tile, List<MazeCell> revealedTiles) {
        return revealedTiles.Any(revealedTile => revealedTile == tile);
    }

    /*
     * Returns the position between the tile and the player (Unity distance)
     */
    public float[] GetRelativePosition(GameObject player, MazeCell tile) {
        Vector3 playerPosition = player.transform.position;
        Vector3 tilePosition = tile.transform.position;
        float x = playerPosition.x - tilePosition.x;
        float z = playerPosition.z - tilePosition.z;
        return new[] {x, z};
    }
    
    public void InstantiateArrow(MazeCell mazeCell) {
        if (GameObject.Find($"Arrow_{mazeCell.gameObject.name}")) {
            Destroy(GameObject.Find($"Arrow_{mazeCell.gameObject.name}"));
        }

        float angle = 0;
        switch (mazeCell.action) {
            case "SOUTH":
                angle = 270;
                break;            
            case "EAST":
                angle = 180;
                break;
            case "NORTH":
                angle = 90;
                break;
            case "WEST":
                angle = 0;
                break;
        }

        Transform arrow = Instantiate(arrowPrefab, new Vector3(
            mazeCell.transform.position.x,
            mazeCell.transform.position.y + 0.05f,
            mazeCell.transform.position.z
        ), Quaternion.identity);
        arrow.name = $"Arrow_{mazeCell.gameObject.name}";
        arrow.transform.eulerAngles = new Vector3(0, angle, 0);
        arrow.SetParent(_arrows.transform);
        mazeCell.arrow = arrow.gameObject;
    }
}