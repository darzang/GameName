using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileManager : MonoBehaviour {
    public Transform obstacleTile;
    public Transform floorTile;
    public Transform wallTile;

    private GameObject player;
    private GameObject environment;
    void Awake () {
        GameObject[] mapElements = GetMapElements ();
        GameObject[, ] map = Get3DMap (mapElements);

    }

    void Update () { }

    GameObject[] GetMapElements () {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] MapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).ToArray ();
        // foreach (GameObject element in MapElements) {
        //     Debug.Log (element.transform.position);
        // }
        return MapElements;
    }

    GameObject[, ] Get3DMap (GameObject[] MapElements) {
        int mapSize = (int) Math.Sqrt (MapElements.Length);
        GameObject[, ] map = new GameObject[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int z = 0; z < mapSize; z++) {
                map[x, z] = Array.Find (MapElements, element => ((int) element.transform.position.x == x && (int) element.transform.position.z == z));
                // Debug.Log ("X: " + x + " Z: " + z + ": " + map[x, z].tag);
            }
        }
        return map;
    }

}