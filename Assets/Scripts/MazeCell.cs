using UnityEngine;

public class MazeCell : MonoBehaviour {
    public int score;
    public string action = "NULL";
    public bool visited = false;
    public bool isExit = false;
    public GameObject northWall, southWall, eastWall, westWall, floor, ceiling, arrow;
    public bool permanentlyRevealed = false;
    public bool revealedForCurrentRun = false;
    public bool hasLight = false;
    public bool hasArrow = false;
    public int fragmentNumber = 0;


    public void DestroyWallIfExists(GameObject wall) {
        if (!wall) return;
        if (northWall && wall == northWall) {
            northWall = null;
        }

        if (southWall && wall == southWall) {
            southWall = null;
        }

        if (eastWall && wall == eastWall) {
            eastWall = null;
        }

        if (westWall && wall == westWall) {
            westWall = null;
        }
        if (ceiling && wall == ceiling) ceiling = null;
        Destroy(wall);
    }

    public void SetFloorColor(Color color) {
        floor.GetComponent<Renderer>().material.color = color;
    }

    public void SetCollidersTrigger(bool trigger) {
        northWall.GetComponent<BoxCollider>().isTrigger = trigger;
        southWall.GetComponent<BoxCollider>().isTrigger = trigger;
        eastWall.GetComponent<BoxCollider>().isTrigger = trigger;
        westWall.GetComponent<BoxCollider>().isTrigger = trigger;
        floor.GetComponent<BoxCollider>().isTrigger = trigger;
        ceiling.GetComponent<BoxCollider>().isTrigger = trigger;
    }
}
