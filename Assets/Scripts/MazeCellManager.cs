using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeCellManager : MonoBehaviour
{
    public List<MazeCell> mazeCells;
    public int mazeRow = 10;
    public int mazeColumn = 10;
    public Transform arrowPrefab;
    private GameObject _arrows;
    public Transform lightPrefab;
    public static Vector3 relativePosition;

    // Origin is top left corner corner, Z++ = east, X++ = South

    public void Start()
    {
        relativePosition = new Vector3();
    }
    public int GetMapSize()
    {
        return mazeCells.Count;
    }

    private bool CellIsAvailable(int row, int column)
    {
        return row >= 0 && row < mazeRow && column >= 0 && column < mazeColumn;
    }

    public MazeCell GetCellIfExists(int row, int column)
    {
        return CellIsAvailable(row, column) ? mazeCells.Find(cell => cell.x == row && cell.z == column) : null;
    }

    private List<MazeCell> GetReachableCells(MazeCell mazeCell)
    {
        // Returns the neighbor tiles that the player can directly reach
        List<MazeCell> reachableCells = new List<MazeCell>();
        int x = mazeCell.x;
        int z = mazeCell.z;

        if (!mazeCell.hasNorthWall)
        {
            MazeCell northCell = GetCellIfExists(x - 1, z);
            if (northCell != null && !northCell.hasSouthWall) reachableCells.Add(northCell);
        }

        if (!mazeCell.hasSouthWall)
        {
            MazeCell southCell = GetCellIfExists(x + 1, z);
            if (southCell != null && !southCell.hasNorthWall) reachableCells.Add(southCell);
        }

        if (!mazeCell.hasEastWall)
        {
            MazeCell eastCell = GetCellIfExists(x, z + 1);
            if (eastCell != null && !eastCell.hasWestWall) reachableCells.Add(eastCell);
        }

        if (!mazeCell.hasWestWall)
        {
            MazeCell westCell = GetCellIfExists(x, z - 1);
            if (westCell != null && !westCell.hasEastWall)
            {
                reachableCells.Add(westCell);
            }
        }

        return reachableCells;
    }

    public List<MazeCell> GetNeighborTiles(MazeCell mazeCell)
    {
        List<MazeCell> neighborTiles = new List<MazeCell>();
        if (mazeCell.x - 1 > 0)
        {
            MazeCell northCell = GetCellIfExists(mazeCell.x - 1, mazeCell.z);
            if (northCell != null) neighborTiles.Add(northCell);
        }

        if (mazeCell.x + 1 < mazeRow)
        {
            MazeCell southCell = GetCellIfExists(mazeCell.x + 1, mazeCell.z);
            if (southCell != null) neighborTiles.Add(southCell);
        }

        if (mazeCell.z - 1 > 0)
        {
            MazeCell westCell = GetCellIfExists(mazeCell.x, mazeCell.z - 1);
            if (westCell != null) neighborTiles.Add(westCell);
        }

        if (mazeCell.z + 1 < mazeColumn)
        {
            MazeCell eastCell = GetCellIfExists(mazeCell.x, mazeCell.z + 1);
            if (eastCell != null) neighborTiles.Add(eastCell);
        }

        return neighborTiles;
    }

    public void DoPathPlanning()
    {
        bool updated;
        do
        {
            updated = false;
            foreach (MazeCell mazeCell in mazeCells)
            {
                if (mazeCell.isExit) continue;
                // Check Neighbor tiles
                //TODO: Check if score doesn't exist, mean higher
                List<MazeCell> neighborTiles = GetReachableCells(mazeCell);
                foreach (MazeCell neighborCell in neighborTiles)
                {
                    if (neighborCell.isExit)
                    {
                        if (mazeCell.score == 1) continue;
                        updated = true;
                        mazeCell.score = 1;
                        SetAction(mazeCell, neighborCell);
                        // InstantiateArrow(mazeCell);
                    }
                    else if (
                        neighborCell.score < mazeCell.score
                        && mazeCell.score != neighborCell.score + 1
                    )
                    {
                        mazeCell.score = neighborCell.score + 1;
                        SetAction(mazeCell, neighborCell);
                        updated = true;
                        // InstantiateArrow(mazeCell);
                    }
                }
            }
        } while (updated);
    }

    private static void SetAction(MazeCell cell, MazeCell neighborCell)
    {
        if (neighborCell.z > cell.z)
        {
            cell.action = "EAST";
        }
        else if (neighborCell.z < cell.z)
        {
            cell.action = "WEST";
        }
        else if (neighborCell.x > cell.x)
        {
            cell.action = "SOUTH";
        }
        else if (neighborCell.x < cell.x)
        {
            cell.action = "NORTH";
        }
        else
        {
            Debug.LogError("Action not found");
        }
    }

    public MazeCell GetTileUnder(GameObject player)
    {
        Ray ray = new Ray(player.transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit, 10);
        return hit.collider ? GetCellByName(hit.collider.transform.parent.name) : null;
    }

    /*
     * Returns the position between the tile and the player (Unity distance)
     */
    public static Vector3 GetRelativePosition(GameObject player, MazeCell tile)
    {
        relativePosition.Set(
                    (player.transform.position.x - tile.x) * 10,
                    (player.transform.position.z - tile.z) * 10,
                    0
        );
        return relativePosition;
    }

    public void InstantiateArrow(MazeCell mazeCell)
    {
        if (!_arrows)
        {
            _arrows = GameObject.Find("Arrows").gameObject;
        }

        if (GameObject.Find($"Arrow_{mazeCell.name}"))
        {
            //TODO: Just flip it
            Destroy(GameObject.Find($"Arrow_{mazeCell.name}"));
        }

        float angle = 0;
        switch (mazeCell.action)
        {
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

    public int GetDiscoveredCellsCount()
    {
        return mazeCells.Count(mazeCell => mazeCell.permanentlyRevealed);
    }

    public MazeCell GetCellByName(string name)
    {
        MazeCell mazeCell = mazeCells.Find(cell => cell.name == name);
        if (mazeCell == null)
        {
            // Debug.LogError($"Cell not found: {name}");
        }

        return mazeCell;
    }

    public bool AllCellsDiscovered()
    {
        return mazeCells.Find(cell => !cell.permanentlyRevealed) == null;
    }

    public static void DestroyWallIfExists(MazeCell mazeCell, MazeCell.Walls wall)
    {
        GameObject wallToDestroy = null;
        switch (wall)
        {
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
            case MazeCell.Walls.Floor:
                wallToDestroy = GameObject.Find(mazeCell.name).transform.Find("Floor").gameObject;
                break;
            default:
                Debug.LogWarning($"Trying to destroy a wall that doesn't exist ! {wall}");
                break;
        }

        Destroy(wallToDestroy);
    }

    public static GameObject GetWall(MazeCell mazeCell, MazeCell.Walls wall)
    {
        GameObject cellObject = GameObject.Find(mazeCell.name);
        switch (wall)
        {
            case MazeCell.Walls.East:
                if (mazeCell.hasEastWall)
                {
                    mazeCell.hasEastWall = false;
                    return cellObject.transform.Find("EastWall").gameObject;
                }

                break;
            case MazeCell.Walls.North:
                if (mazeCell.hasNorthWall)
                {
                    mazeCell.hasNorthWall = false;
                    return cellObject.transform.Find("NorthWall").gameObject;
                }

                break;
            case MazeCell.Walls.West:
                if (mazeCell.hasWestWall)
                {
                    mazeCell.hasWestWall = false;
                    return cellObject.transform.Find("WestWall").gameObject;
                }

                break;
            case MazeCell.Walls.South:
                if (mazeCell.hasSouthWall)
                {
                    mazeCell.hasSouthWall = false;
                    return cellObject.transform.Find("SouthWall").gameObject;
                }

                break;
            case MazeCell.Walls.Ceiling:
                return cellObject.transform.Find("Ceiling").gameObject;
            case MazeCell.Walls.Floor:
                return cellObject.transform.Find("Floor").gameObject;
            default:
                Debug.LogError($"Trying to get a wall that doesn't exist ! {wall}");
                break;
        }

        return null;
    }

    public void InstantiateLight(MazeCell mazeCell)
    {
        Transform cellObject = GameObject.Find(mazeCell.name).transform;
        Transform light = Instantiate(lightPrefab, cellObject);
        light.localPosition = new Vector3(0, 0.9f, 0);
        mazeCell.hasLight = true;
    }
}