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
    }

    void Update () { }

    GameObject[] GetMapElements () {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).ToArray ();
        // foreach (GameObject element in mapElements) {
        //     Debug.Log (element.transform.position);
        // }
        return mapElements;
    }

    public GameObject[, ] Get3DMap () {
        GameObject[] mapElements = GetMapElements();
        int mapSize = (int) Math.Sqrt (mapElements.Length);
        GameObject[, ] map = new GameObject[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int z = 0; z < mapSize; z++) {
                map[x, z] = Array.Find (mapElements, element => ((int) element.transform.position.x == x && (int) element.transform.position.z == z));
            }
        }
        return map;
    }

}