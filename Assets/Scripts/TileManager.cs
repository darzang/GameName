using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileManager : MonoBehaviour {
    public Transform obstacleTile;
    public Transform floorTile;
    public Transform wallTile;

    public GameObject player;
    private GameObject environment;

    public GameObject[, ] tileMap;
    public List<GameObject> revealedTiles;
    private GameObject currentPlayerTile;

    void Awake () {
        // StartCoroutine (WaitForXSeconds (1));
        Debug.Log ("Starting Awake TileM");
        currentPlayerTile = GetTileUnderPlayer ();
        tileMap = Get3DMap ();
        Debug.Log ("startingPlayerTile " + currentPlayerTile.transform.position.x + " | " + currentPlayerTile.transform.position.z);
        Debug.Log ("Ending Awake TileM");

    }

    void Update () {
        currentPlayerTile = GetTileUnderPlayer ();
    }

    IEnumerator WaitForXSeconds (int x) {
        yield return new WaitForSeconds (x);

    }
    GameObject[] GetMapElements () {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).ToArray ();
        return mapElements;
    }

    public GameObject[, ] Get3DMap () {
        GameObject[] mapElements = GetMapElements ();
        int mapSize = (int) Math.Sqrt (mapElements.Length);
        GameObject[, ] map = new GameObject[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int z = 0; z < mapSize; z++) {
                map[x, z] = Array.Find (mapElements, element => ((int) element.transform.position.x == x && (int) element.transform.position.z == z));
                // Debug.Log("Adding x " + map[x, z].transform.position.x + " | "  + map[x, z].transform.position.z) ;
            }
        }
        return map;
    }

    public List<GameObject> GetNeighborsTiles (int x, int z) {
        /**
        length
            z-1 x+1 | X+1  | zx+1       
            Z-1  | tile | Z+1
        0   zx-1 | X-1  | x-1 z+1
            0           length
         */
        Debug.Log ("Getting Tile Above for " + x + " | " + z);
        Debug.Log ("Getting Tile Above for " + x + " | " + z);
        // Debug.Log(tileMap.ToString());
        // Debug.Log(tileMap.GetLength(1));
        GameObject tileAbove = Get3DMap () [x + 1, z];
        GameObject tileAboveLeft = Get3DMap () [x + 1, z - 1];
        GameObject tileAboveRight = Get3DMap () [x + 1, z + 1];
        GameObject tileBelowLeft = Get3DMap () [x - 1, z - 1];
        GameObject tileBelowRight = Get3DMap () [x - 1, z + 1];
        GameObject tileBelow = Get3DMap () [x - 1, z];
        GameObject tileLeft = Get3DMap () [x, z - 1];
        GameObject tileRight = Get3DMap () [x, z + 1];
        return new List<GameObject> {
            tileAbove,
            tileBelow,
            tileLeft,
            tileRight,
            tileAboveLeft,
            tileAboveRight,
            tileBelowLeft,
            tileBelowRight
        };
    }

    public GameObject GetTileUnderPlayer () {

        RaycastHit hit;
        Ray ray = new Ray (player.transform.position, Vector3.down);
        if (Physics.Raycast (ray, out hit, 10)) {
            // Debug.Log ("Under Player: " + hit.collider.gameObject.transform.position.x + " | " + hit.collider.gameObject.transform.position.z);
            return hit.collider.gameObject;
        } else {
            return null;
        }

    }

    public void AddToRevealedTiles (GameObject tile) {
        revealedTiles.Add (tile);
    }
    public bool HasBeenRevealed (GameObject tile) {
        foreach (GameObject revealedTile in revealedTiles) {
            if (revealedTile == tile) return true;
        }
        return false;
    }

    public float[] GetRelativePosition (GameObject player, GameObject tile) {
        float x = player.transform.position.x - tile.transform.position.x;
        float z = player.transform.position.z - tile.transform.position.z;
        return new float[] { x, z };
    }

}