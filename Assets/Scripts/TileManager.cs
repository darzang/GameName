using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour {
    private GameObject _environment;
    private GameManager _gameManager;
    public MazeCell[,] mazeCells;
    private int _mazeRow;
    private int _mazeColumn;
    private void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetMazeCells(MazeCell[,] mazeCells, int mazeRow, int mazeColumn) {
        this.mazeCells = mazeCells;
        _mazeColumn = mazeColumn;
        _mazeRow = mazeRow;
    }

    public int GetMapSize() {
        return mazeCells.Length;
    }
    
    private void InstantiateTilesScores() {
        foreach (MazeCell cell in mazeCells) {
            cell.score = 0;
            cell.action = null;
        }
    }

    private List<MazeCell> GetNeighborWalkableTiles(MazeCell mazecell) {
        // Returns the neighbor tiles that the player can directly reach
        List<MazeCell> neighborTiles = new List<MazeCell>();

        if (!mazecell.northWall && !mazeCells[(int)mazecell.transform.position.x-1, (int) mazecell.transform.position.z].southWall) {
            neighborTiles.Add(mazeCells[(int)mazecell.transform.position.x-1, (int) mazecell.transform.position.z]);
        }
        if (!mazecell.southWall && !mazeCells[(int)mazecell.transform.position.x+1, (int) mazecell.transform.position.z].northWall) {
            neighborTiles.Add(mazeCells[(int)mazecell.transform.position.x+1, (int) mazecell.transform.position.z]);
        }
        if (!mazecell.westWall && !mazeCells[(int)mazecell.transform.position.x, (int) mazecell.transform.position.z-1].eastWall) {
            neighborTiles.Add(mazeCells[(int)mazecell.transform.position.x, (int) mazecell.transform.position.z-1]);
        }
        if (!mazecell.eastWall && !mazeCells[(int)mazecell.transform.position.x, (int) mazecell.transform.position.z+1].westWall) {
            neighborTiles.Add(mazeCells[(int)mazecell.transform.position.x, (int) mazecell.transform.position.z+1]);
        }
        
        return neighborTiles;
    }
    
    public List<GameObject> GetNeighborTiles(GameObject mazeCell) {
        List<GameObject> neighborTiles = new List<GameObject>();
        if (mazeCell.transform.position.x - 1 > 0) {
            MazeCell northCell = mazeCells[(int) mazeCell.transform.position.x - 1, (int) mazeCell.transform.position.z];
            if (northCell) neighborTiles.Add(northCell.gameObject);        
        }
        if (mazeCell.transform.position.x + 1 < _mazeRow) {
            MazeCell southCell = mazeCells[(int) mazeCell.transform.position.x + 1, (int) mazeCell.transform.position.z];
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
        InstantiateTilesScores();
        bool updated;
        do {
            updated = false;
            foreach (MazeCell mazeCell in  mazeCells) {
                // Check Neighbor tiles
                List<MazeCell> neighborTiles = GetNeighborWalkableTiles(mazeCell);
                foreach (MazeCell neighborCell in neighborTiles) {
                    if (neighborCell.isExit) {
                        if (mazeCell.score == 1) continue;
                        mazeCell.score = 1;
                        SetAction(mazeCell, neighborCell);
                        // gameManager.InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
                        updated = true;
                    } else if (
                        neighborCell.score < mazeCell.score
                        && mazeCell.score != neighborCell.score + 1
                        ) {
                        mazeCell.score = neighborCell.score + 1;
                        SetAction(mazeCell, neighborCell);
                        // gameManager.InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
                        updated = true;
                    }
                }
            }
        } while (updated) ;
    }
    private void SetAction(MazeCell cell, MazeCell neighborCell) {
        if (neighborCell.transform.position.z > cell.transform.position.z) {
            cell.action = "FORWARD";
        } else if (neighborCell.transform.position.z < cell.transform.position.z) {
            cell.action = "BACKWARD";
        } else if (neighborCell.transform.position.x > cell.transform.position.x) {
            cell.action = "RIGHT";
        } else if (neighborCell.transform.position.x < cell.transform.position.x) {
            cell.action = "LEFT";
        }
    }

    public List<GameObject> GetAllTiles() {
        return GameObject.FindGameObjectsWithTag("MazeCell").ToList();
    }

    public MazeCell GetTileUnderPlayer () {
        Ray ray = new Ray (_gameManager.player.transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit, 10);
        if (hit.collider) {
            return hit.collider.transform.parent.GetComponent<MazeCell>();
        }
        return null;
    }

    public void AddToRevealedTiles (GameObject tile, List<GameObject> revealedTiles) {
        if (!HasBeenRevealed(tile, revealedTiles)) {
            _gameManager.revealedTilesInRun.Add(tile);
        }
    }
    public bool HasBeenRevealed (GameObject tile, List<GameObject> revealedTiles) {
        return revealedTiles.Any(revealedTile => revealedTile == tile);
    }

    /*
     * Returns the position between the tile and the player (Unity distance)
     */
    public float[] GetRelativePosition (GameObject player, GameObject tile) {
        Vector3 playerPosition = player.transform.position;
        Vector3 tilePosition = tile.transform.position;
        float x = playerPosition.x - tilePosition.x;
        float z = playerPosition.z - tilePosition.z;
        return new [] { x, z };
    }
}
