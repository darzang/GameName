using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileManager : MonoBehaviour {
    private GameObject environment;
    public GameManager gameManager;


    GameObject[] GetMapArray () {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] ExitTile = GameObject.FindGameObjectsWithTag ("Exit");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).Concat (ExitTile).ToArray ();
        return mapElements;
    }

    public int GetMapSize() {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] ExitTile = GameObject.FindGameObjectsWithTag ("Exit");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).Concat (ExitTile).ToArray ();
        return mapElements.Length;
    }

    private GameObject[, ] GetMap2D () {
        GameObject[] mapElements = GetMapArray ();
        int mapSize = (int) Math.Sqrt (mapElements.Length + 1);
        GameObject[, ] map = new GameObject[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int z = 0; z < mapSize; z++) {
                map[x, z] = Array.Find (mapElements, element => 
                    (int) element.transform.position.x == x && (int) element.transform.position.z == z);
            }
        }
        return map;
    }

    public List<GameObject> GetNeighborsTiles (int x, int z) {
        /*
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

    public List<GameObject> GetTilesByType(string type) {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag (type));
    }

    public GameObject GetTileUnderPlayer () {
        Ray ray = new Ray (gameManager.player.transform.position, Vector3.down);
        return Physics.Raycast (ray, out RaycastHit hit, 10) ? hit.collider.gameObject : null;
    }

    public void AddToRevealedTiles (GameObject tile, List<GameObject> revealedTiles) {
        if(!HasBeenRevealed(tile, revealedTiles)) gameManager.revealedTiles.Add(tile);
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

    public List<string> GetTilesNames(List<GameObject> tileList) {
        List<string> tilesNames = new List<string>();
        foreach (GameObject tile in tileList) {
            tilesNames.Add(tile.name);
        }
        return tilesNames;
    }
}
