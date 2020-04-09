using System;
using UnityEngine;

public class MazeCell : MonoBehaviour {
    public enum Walls { West, East, North, South, Ceiling, Floor};
    public int score;
    public string action = "NULL";
    public bool visited = false;
    public bool isExit = false;
    public bool hasNorthWall = true;
    public bool hasSouthWall = true;
    public bool hasEastWall = true;
    public bool hasWestWall = true;
    public bool permanentlyRevealed = false;
    public bool revealedForCurrentRun = false;
    public bool hasLight = false;
    public bool hasArrow = false;
    public int fragmentNumber = 0;
    public int x;
    public int z;


    public void DestroyWallIfExists(Walls wall) {
        GameObject wallToDestroy = null;
        switch (wall) {
            case Walls.East:
                hasEastWall = false;
                wallToDestroy = transform.Find("EastWall").gameObject;
                break;
            case Walls.North:
                hasNorthWall = false;
                wallToDestroy = transform.Find("NorthWall").gameObject;
                break;
            case Walls.West:
                hasWestWall = false;
                wallToDestroy = transform.Find("WestWall").gameObject;
                break;
            case Walls.South:
                hasSouthWall = false;
                wallToDestroy = transform.Find("SouthWall").gameObject;
                break;
            case Walls.Ceiling:
                hasSouthWall = false;
                wallToDestroy = transform.Find("Ceiling").gameObject;
                break;
            default:
                Debug.LogError($"Trying to destroy a wall that doesn't exist ! {wall}");
                break;
        }
        Destroy(wallToDestroy);
    }

    public void SetFloorColor(Color color) {
        GetWall(Walls.Floor).GetComponent<Renderer>().material.color = color;
    }

    public GameObject GetWall(Walls wall) {
        GameObject wallToReturn = null;
        switch (wall) {
            case Walls.East:
                if (!hasEastWall) {
                    Debug.Log($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                hasEastWall = false;
                wallToReturn = transform.Find("EastWall").gameObject;
                break;
            case Walls.North:
                if (!hasNorthWall) {
                    Debug.Log($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                hasNorthWall = false;
                wallToReturn = transform.Find("NorthWall").gameObject;
                break;
            case Walls.West:
                if (!hasWestWall) {
                    Debug.Log($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                hasWestWall = false;
                wallToReturn = transform.Find("WestWall").gameObject;
                break;
            case Walls.South:
                if (!hasSouthWall) {
                    Debug.Log($"Trying to remove already deleted wall: {wall} of {transform.name}");
                    return null;
                }
                hasSouthWall = false;
                wallToReturn = transform.Find("SouthWall").gameObject;
                break;
            case Walls.Ceiling:
                wallToReturn = transform.Find("Ceiling").gameObject;
                break;
            case Walls.Floor:
                hasSouthWall = false;
                wallToReturn = transform.Find("Floor").gameObject;
                break;
            default:
                Debug.LogError($"Trying to get a wall that doesn't exist ! {wall}");
                break;
        }

        return wallToReturn;
    }

    public void SetCollidersTrigger(bool trigger) {
        // This is used for cells in fragments so they can be picked up
        foreach(Walls wallType in Enum.GetValues(typeof(Walls)))
        {
            GetWall(wallType).GetComponent<BoxCollider>().isTrigger = trigger;
        }
    }
}
