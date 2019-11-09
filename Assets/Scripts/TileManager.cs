using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileManager : MonoBehaviour {
    public Transform obstacleTile;
    public Transform floorTile;
    public Transform wallTile;

    public GameObject player;
    private GameObject environment;

    public List<GameObject> revealedTiles;

    GameObject[] GetMapArray () {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).ToArray ();
        return mapElements;
    }

    public GameObject[, ] GetMap2D () {
        GameObject[] mapElements = GetMapArray ();
        int mapSize = (int) Math.Sqrt (mapElements.Length);
        GameObject[, ] map = new GameObject[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int z = 0; z < mapSize; z++) {
                map[x, z] = Array.Find (mapElements, element => (
                    (int) element.transform.position.x == x && (int) element.transform.position.z == z));
            }
        }
        return map;
    }

    public List<GameObject> GetNeighborsTiles (int x, int z) {
        /**
        length
            ... | X+1  | ...
            Z-1 | tile | Z+1
        0   ... | X-1  | ...
            0           length
        */
        return new List<GameObject> {
            GetMap2D () [x + 1, z],
            GetMap2D () [x + 1, z - 1],
            GetMap2D () [x + 1, z + 1],
            GetMap2D () [x - 1, z - 1],
            GetMap2D () [x - 1, z + 1],
            GetMap2D () [x - 1, z],
            GetMap2D () [x, z - 1],
            GetMap2D () [x, z + 1]
        };
    }

    public GameObject GetTileUnderPlayer () {
        RaycastHit hit;
        Ray ray = new Ray (player.transform.position, Vector3.down);
        if (Physics.Raycast (ray, out hit, 10)) {
            return hit.collider.gameObject;
        }
        return null;
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
