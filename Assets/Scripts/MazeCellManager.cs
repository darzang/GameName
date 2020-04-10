using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeCellManager : MonoBehaviour {
    public MazeCell[,] mazeCells;
    private int _mazeRow;
    private int _mazeColumn;
    public Transform arrowPrefab;
    private GameObject _arrows;
    public Transform lightPrefab;

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
            GetWall(mazeCell, MazeCell.Walls.Ceiling).SetActive(show);
        }
    }

    private List<MazeCell> GetNeighborWalkableTiles(MazeCell mazeCell) {
        // Returns the neighbor tiles that the player can directly reach
        List<MazeCell> neighborTiles = new List<MazeCell>();
        int x = mazeCell.x;
        int z = mazeCell.z;

        MazeCell northCell = GetCellIfExists(x - 1, z);
        if (northCell != null && !northCell.hasSouthWall && !mazeCell.hasNorthWall) {
            neighborTiles.Add(northCell);
        }

        MazeCell southCell = GetCellIfExists(x + 1, z);
        if (southCell != null && !southCell.hasNorthWall && !mazeCell.hasSouthWall) {
            neighborTiles.Add(southCell);
        }

        MazeCell eastCell = GetCellIfExists(x, z + 1);
        if (eastCell != null && !eastCell.hasWestWall && !mazeCell.hasEastWall) {
            neighborTiles.Add(eastCell);
        }

        MazeCell westCell = GetCellIfExists(x, z - 1);
        if (westCell != null && !westCell.hasEastWall && !mazeCell.hasWestWall) {
            neighborTiles.Add(westCell);
        }

        return neighborTiles;
    }

    public List<MazeCell> GetNeighborTiles(MazeCell mazeCell) {
        List<MazeCell> neighborTiles = new List<MazeCell>();
        if (mazeCell.x - 1 > 0) {
            MazeCell northCell = mazeCells[ mazeCell.x - 1,  mazeCell.z];
            if (northCell != null) neighborTiles.Add(northCell);
        }

        if (mazeCell.x + 1 < _mazeRow) {
            MazeCell southCell = mazeCells[ mazeCell.x + 1,  mazeCell.z];
            if (southCell != null) neighborTiles.Add(southCell);
        }

        if (mazeCell.z - 1 > 0) {
            MazeCell westCell = mazeCells[mazeCell.x,  mazeCell.z - 1];
            if (westCell != null) neighborTiles.Add(westCell);
        }

        if (mazeCell.z + 1 < _mazeColumn) {
            MazeCell eastCell = mazeCells[ mazeCell.x, mazeCell.z + 1];
            if (eastCell != null) neighborTiles.Add(eastCell);
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
        if (neighborCell.z > cell.z) {
            cell.action = "EAST";
        }
        else if (neighborCell.z < cell.z) {
            cell.action = "WEST";
        }
        else if (neighborCell.x > cell.x) {
            cell.action = "SOUTH";
        }
        else if (neighborCell.x < cell.x) {
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
        float x = playerPosition.x - tile.x;
        float z = playerPosition.z - tile.z;
        return new[] {x, z};
    }
    
    public void InstantiateArrow(MazeCell mazeCell) {
        if (!_arrows) {
            _arrows = GameObject.Find("Arrows").gameObject;
        }
        if (GameObject.Find($"Arrow_{mazeCell.name}")) {
            //TODO: Just flip it
            Destroy(GameObject.Find($"Arrow_{mazeCell.name}"));
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
            mazeCell.x,
            0.05f,
            mazeCell.z
        ), Quaternion.identity);
        arrow.name = $"Arrow_{mazeCell.name}";
        arrow.transform.eulerAngles = new Vector3(0, angle, 0);
        arrow.SetParent(_arrows.transform);
        mazeCell.hasArrow = true;
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
            if (mazeCell.name == name) return mazeCell;
        }
        return null;
    }
    
    public bool AllCellsDiscovered() {
        if (GetMazeAsList().Find(cell => !cell.permanentlyRevealed) != null) return false;
        return true;
    }

    public void DestroyWallIfExists(MazeCell mazeCell, MazeCell.Walls wall) {
        GameObject wallToDestroy = null;
        switch (wall) {
            case MazeCell.Walls.East:
                mazeCell.hasEastWall = false;
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("EastWall").gameObject;
                break;
            case MazeCell.Walls.North:
                mazeCell.hasNorthWall = false;
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("NorthWall").gameObject;
                break;
            case MazeCell.Walls.West:
                mazeCell.hasWestWall = false;
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("WestWall").gameObject;
                break;
            case MazeCell.Walls.South:
                mazeCell.hasSouthWall = false;
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("SouthWall").gameObject;
                break;
            case MazeCell.Walls.Ceiling:
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("Ceiling").gameObject;
                break;
            default:
                Debug.LogError($"Trying to destroy a wall that doesn't exist ! {wall}");
                break;
        }
        Destroy(wallToDestroy);
    }
    
    public GameObject GetWall(MazeCell mazeCell, MazeCell.Walls wall) {
        GameObject wallToReturn = null;
        switch (wall) {
            case MazeCell.Walls.East:
                if (!mazeCell.hasEastWall) {
                    Debug.LogError($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                mazeCell.hasEastWall = false;
                wallToReturn = transform.Find("EastWall").gameObject;
                break;
            case MazeCell.Walls.North:
                if (!mazeCell.hasNorthWall) {
                    Debug.LogError($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                mazeCell.hasNorthWall = false;
                wallToReturn = transform.Find("NorthWall").gameObject;
                break;
            case MazeCell.Walls.West:
                if (!mazeCell.hasWestWall) {
                    Debug.LogError($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                mazeCell.hasWestWall = false;
                wallToReturn = transform.Find("WestWall").gameObject;
                break;
            case MazeCell.Walls.South:
                if (!mazeCell.hasSouthWall) {
                    Debug.LogError($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                mazeCell.hasSouthWall = false;
                wallToReturn = transform.Find("SouthWall").gameObject;
                break;
            case MazeCell.Walls.Ceiling:
                wallToReturn = transform.Find("Ceiling").gameObject;
                break;
            case MazeCell.Walls.Floor:
                wallToReturn = transform.Find("Floor").gameObject;
                break;
            default:
                Debug.LogError($"Trying to get a wall that doesn't exist ! {wall}");
                break;
        }

        return wallToReturn;
    }
    
    public void SetCollidersTrigger(MazeCell mazeCell, bool trigger) {
        // This is used for cells in fragments so they can be picked up
        foreach(MazeCell.Walls wallType in Enum.GetValues(typeof(MazeCell.Walls)))
        {
            GetWall(mazeCell, wallType).GetComponent<BoxCollider>().isTrigger = trigger;
        }
    }
    
    public void SetFloorColor(MazeCell mazeCell, Color color) {
        GetWall(mazeCell, MazeCell.Walls.Floor).GetComponent<Renderer>().material.color = color;
    }
    
    public void InstantiateLight(MazeCell mazeCell) {
        Transform cellObject = GameObject.Find(mazeCell.name).transform;
        Transform light = Instantiate(lightPrefab, cellObject);
        light.localPosition = new Vector3(0, 0.9f, 0);
        mazeCell.hasLight = true;
    }
}