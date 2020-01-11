using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileManager : MonoBehaviour {
    private GameObject environment;
    public GameManager gameManager;
    public List<GameObject> floorTiles;
    public int GetMapSize() {
        GameObject[] WallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] ObstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] FloorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] ExitTile = GameObject.FindGameObjectsWithTag ("Exit");
        GameObject[] mapElements = WallTiles.Concat (ObstacleTiles).Concat (FloorTiles).Concat (ExitTile).ToArray ();
        return mapElements.Length;
    }
    
    private void InstantiateFloorTiles() {
        floorTiles = GameObject.FindGameObjectsWithTag("Floor").ToList();
        foreach (GameObject tile in floorTiles) {
            tile.GetComponent<Tile>().score = floorTiles.Count;
            tile.GetComponent<Tile>().action = null;
        }
    }

    private List<GameObject> GetNeighborTiles(GameObject tile) {
        List<GameObject> neighborTiles = new List<GameObject>();
        RaycastHit hit;
        List<Vector3> directions = new List<Vector3> { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
        foreach (Vector3 direction in directions) {
            if (Physics.Raycast(tile.transform.position, direction,out hit, 1)) {
                if(hit.collider.gameObject.CompareTag("Floor") || hit.collider.gameObject.CompareTag("Exit"))
                    neighborTiles.Add(hit.collider.gameObject);
            }
        }
        return neighborTiles;
    }
    public void DoPathPlanning() {
        InstantiateFloorTiles();
        bool updated;
        do {
            updated = false;
            foreach (GameObject tile in floorTiles) {
                // Check Neighbor tiles
                List<GameObject> neighborTiles = GetNeighborTiles(tile);
                foreach (GameObject neighborTile in neighborTiles) {
                    if (neighborTile.CompareTag("Exit")) {
                        if (tile.GetComponent<Tile>().score == 1) continue;
                        tile.GetComponent<Tile>().score = 1;
                        SetAction(tile, neighborTile);
                        // gameManager.InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
                        updated = true;
                    } else if (
                        neighborTile.GetComponent<Tile>().score < tile.GetComponent<Tile>().score
                        && tile.GetComponent<Tile>().score != neighborTile.GetComponent<Tile>().score + 1
                        ) {
                        tile.GetComponent<Tile>().score = neighborTile.GetComponent<Tile>().score + 1;
                        SetAction(tile, neighborTile);
                        // gameManager.InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
                        updated = true;
                    }
                }
            }
        } while (updated) ;
    }
    private void SetAction(GameObject tile, GameObject neighborTile) {
        if (neighborTile.transform.position.z > tile.transform.position.z) {
            tile.GetComponent<Tile>().action = "FORWARD";
        } else if (neighborTile.transform.position.z < tile.transform.position.z) {
            tile.GetComponent<Tile>().action = "BACKWARD";
        } else if (neighborTile.transform.position.x > tile.transform.position.x) {
            tile.GetComponent<Tile>().action = "RIGHT";
        } else if (neighborTile.transform.position.x < tile.transform.position.x) {
            tile.GetComponent<Tile>().action = "LEFT";
        }
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
