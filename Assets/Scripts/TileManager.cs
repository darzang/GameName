using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour {
	public Transform wallTile;
	public Transform floorTile;
	public Transform borderTile;

	private GameObject player;
	private GameObject environment;
	void Start () {
        CreateMap();
	}

	void Update () { }


    void CreateMap(){
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log(WallTiles);
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag("Obstacle");
        Debug.Log(ObstacleTiles);
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag("Floor");
        Debug.Log(FloorTiles);
    }



}