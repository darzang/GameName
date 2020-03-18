using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour {
    private GameObject _environment;
    private GameManager _gameManager;
    public List<GameObject> floorTiles;

    private void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public int GetMapSize() {
        GameObject[] wallTiles = GameObject.FindGameObjectsWithTag ("Wall");
        GameObject[] obstacleTiles = GameObject.FindGameObjectsWithTag ("Obstacle");
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag ("Floor");
        GameObject[] exitTile = GameObject.FindGameObjectsWithTag ("Exit");
        GameObject[] mapElements = wallTiles.Concat (obstacleTiles).Concat (floorTiles).Concat (exitTile).ToArray ();
        return mapElements.Length;
    }
    
    private void InstantiateFloorTiles() {
        floorTiles = GameObject.FindGameObjectsWithTag("Floor").ToList();
        foreach (GameObject tile in floorTiles) {
            tile.GetComponent<Tile>().score = floorTiles.Count;
            tile.GetComponent<Tile>().action = null;
        }
    }

    private List<GameObject> GetNeighborWalkableTiles(GameObject tile) {
        List<GameObject> neighborTiles = new List<GameObject>();
        List<Vector3> directions = new List<Vector3> { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
        foreach (Vector3 direction in directions) {
            if (!Physics.Raycast(tile.transform.position, direction, out RaycastHit hit, 1)) continue;
            if (hit.collider.gameObject.CompareTag("Floor") || hit.collider.gameObject.CompareTag("Exit"))
                neighborTiles.Add(hit.collider.gameObject);
        }
        return neighborTiles;
    }
    
    public List<GameObject> GetNeighborTiles(GameObject tile) {
        List<GameObject> neighborTiles = new List<GameObject>();
        List<Vector3> directions = new List<Vector3> { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
        foreach (Vector3 direction in directions) {
            if (!Physics.Raycast(tile.transform.position, direction, out RaycastHit hit, 1)) continue;
            GameObject neighbour = hit.collider.gameObject;
            if(neighbour.CompareTag("Floor") || neighbour.CompareTag("Exit") || neighbour.CompareTag("Obstacle") ||  neighbour.CompareTag("Wall"))
                neighborTiles.Add(hit.collider.gameObject);
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
                List<GameObject> neighborTiles = GetNeighborWalkableTiles(tile);
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

    public List<GameObject> GetAllTiles() {
        List<GameObject> walls = GameObject.FindGameObjectsWithTag("Wall").ToList();
        List<GameObject> obstacles = GameObject.FindGameObjectsWithTag("Obstacle").ToList();
        List<GameObject> floors = GameObject.FindGameObjectsWithTag("Floor").ToList();
        List<GameObject> exit = GameObject.FindGameObjectsWithTag("Exit").ToList();
        return walls.Concat(obstacles.Concat(floors.Concat(exit))).ToList();
    }

    public GameObject GetTileUnderPlayer () {
        Ray ray = new Ray (_gameManager.player.transform.position, Vector3.down);
        return Physics.Raycast (ray, out RaycastHit hit, 10) ? hit.collider.gameObject : null;
    }

    public void AddToRevealedTiles (GameObject tile, List<GameObject> revealedTiles) {
        if (!HasBeenRevealed(tile, revealedTiles)) {
            _gameManager.revealedTilesInRun.Add(tile);
        }
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
}
