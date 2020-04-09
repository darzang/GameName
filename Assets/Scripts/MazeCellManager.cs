using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeCellManager : MonoBehaviour {
    public MazeCell[,] mazeCells;
    private int _mazeRow;
    private int _mazeColumn;
    public Transform arrowPrefab;
    private GameObject _arrows;

    // Origin is top left corner corner, Z++ = east, X++ = South

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

    public MazeCell GetCellIfExists(int row, int column) {
        if (CellIsAvailable(row, column)) return mazeCells[row, column];
        return null;
    }

    public void ShowCeiling(bool show) {
        foreach (MazeCell mazeCell in mazeCells) {
            mazeCell.GetWall(MazeCell.Walls.Ceiling).SetActive(show);
        }
    }

    private List<MazeCell> GetNeighborWalkableTiles(MazeCell mazeCell) {
        // Returns the neighbor tiles that the player can directly reach
        List<MazeCell> neighborTiles = new List<MazeCell>();
        int x = (int) mazeCell.transform.position.x;
        int z = (int) mazeCell.transform.position.z;

        MazeCell northCell = GetCellIfExists(x - 1, z);
        if (northCell && !northCell.hasSouthWall && !mazeCell.hasNorthWall) {
            neighborTiles.Add(northCell);
        }

        MazeCell southCell = GetCellIfExists(x + 1, z);
        if (southCell && !southCell.hasNorthWall && !mazeCell.hasSouthWall) {
            neighborTiles.Add(southCell);
        }

        MazeCell eastCell = GetCellIfExists(x, z + 1);
        if (eastCell && !eastCell.hasWestWall && !mazeCell.hasEastWall) {
            neighborTiles.Add(eastCell);
        }

        MazeCell westCell = GetCellIfExists(x, z - 1);
        if (westCell && !westCell.hasEastWall && !mazeCell.hasWestWall) {
            neighborTiles.Add(westCell);
        }

        return neighborTiles;
    }

    public List<MazeCell> GetNeighborTiles(MazeCell mazeCell) {
        List<MazeCell> neighborTiles = new List<MazeCell>();
        if (mazeCell.transform.position.x - 1 > 0) {
            MazeCell northCell =
                mazeCells[(int) mazeCell.transform.position.x - 1, (int) mazeCell.transform.position.z];
            if (northCell) neighborTiles.Add(northCell);
        }

        if (mazeCell.transform.position.x + 1 < _mazeRow) {
            MazeCell southCell =
                mazeCells[(int) mazeCell.transform.position.x + 1, (int) mazeCell.transform.position.z];
            if (southCell) neighborTiles.Add(southCell);
        }

        if (mazeCell.transform.position.z - 1 > 0) {
            MazeCell westCell = mazeCells[(int) mazeCell.transform.position.x, (int) mazeCell.transform.position.z - 1];
            if (westCell) neighborTiles.Add(westCell);
        }

        if (mazeCell.transform.position.z + 1 < _mazeColumn) {
            MazeCell eastCell = mazeCells[(int) mazeCell.transform.position.x, (int) mazeCell.transform.position.z + 1];
            if (eastCell) neighborTiles.Add(eastCell);
        }

        return neighborTiles;
    }

    public void DoPathPlanning() {
        Debug.Log("Doing path planning");
        bool updated;
        do {
            updated = false;
            foreach (MazeCell mazeCell in mazeCells) {
                if (mazeCell.isExit) continue;
                // Check Neighbor tiles
                //TODO: Check if score doesn't exist, mean higher
                List<MazeCell> neighborTiles = GetNeighborWalkableTiles(mazeCell);
                foreach (MazeCell neighborCell in neighborTiles) {
                    if (neighborCell.isExit) {
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

    public MazeCell GetTileUnder(GameObject player) {
        Ray ray = new Ray(player.transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit, 10);
        if (hit.collider) {
            return hit.collider.transform.parent.GetComponent<MazeCell>();
        }
        Debug.LogWarning("No tile under player");
        return null;
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
        if (!_arrows) {
            _arrows = GameObject.Find("Arrows").gameObject;
        }
        if (GameObject.Find($"Arrow_{mazeCell.gameObject.name}")) {
            //TODO: Just flip it
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
        mazeCell.hasArrow = true;
    }
    
    public List<MazeCellForFile> FormatMazeCells(MazeCell[,] mazeCells) {
        List<MazeCellForFile> mazeCellsFormatted = new List<MazeCellForFile>();
        foreach (MazeCell mazeCell in mazeCells) {
            MazeCellForFile mazeCellFormatted = new MazeCellForFile {
                isExit = mazeCell.isExit,
                hasLight = mazeCell.hasLight,
                permanentlyRevealed = mazeCell.permanentlyRevealed,
                hasEastWall = mazeCell.hasEastWall,
                hasWestWall = mazeCell.hasWestWall,
                hasNorthWall = mazeCell.hasNorthWall,
                hasSouthWall = mazeCell.hasSouthWall,
                x = (int) mazeCell.transform.position.x,
                z = (int) mazeCell.transform.position.z
            };
            mazeCellsFormatted.Add(mazeCellFormatted);
        }

        return mazeCellsFormatted;
    }
    public List<MazeCell> GetMazeAsList() {
        List<MazeCell> mazeCellsList = new List<MazeCell>();
        foreach (MazeCell mazeCell in mazeCells) {
            mazeCellsList.Add(mazeCell);
        }
        return mazeCellsList;
    }

    public int GetDiscoveredCellsCount() {
        return GetMazeAsList().Count(mazeCell => mazeCell.permanentlyRevealed);
    }

    public MazeCell GetCellFromFragmentNumber(int fragmentNumber) {
        foreach (MazeCell mazeCell in GetMazeAsList()) {
            if (mazeCell.fragmentNumber == fragmentNumber) return mazeCell;
        }
        return null;
    }

    public MazeCell GetCellFromName(string name) {
        foreach (MazeCell mazeCell in GetMazeAsList()) {
            if (mazeCell.gameObject.name == name) return mazeCell;
        }
        return null;
    }
    
    public bool AllCellsDiscovered() {
        if (GetMazeAsList().Find(cell => !cell.permanentlyRevealed)) return false;
        return true;
    }

    public void PrintMazeCells() {
        foreach (MazeCell mazeCell in GetMazeAsList()) {
            Debug.Log($"Cell {mazeCell.transform.position.x} {mazeCell.transform.position.z} permanently revealed ? : {mazeCell.permanentlyRevealed}");
        }
    }
}