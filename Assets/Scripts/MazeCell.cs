using UnityEngine;

public class MazeCell : MonoBehaviour {
    public int score;
    public string action;
    public bool visited = false;
    public bool isExit = false;
    public GameObject northWall, southWall, eastWall, westWall, floor, ceiling;

    public void DestroyWallIfExists(GameObject wall) {
        if (wall) {
            Destroy(wall);
        }
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
