using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FragmentManager : MonoBehaviour {
    // Start is called before the first frame update

    public List<Fragment> GenerateRandomFragments(List<GameObject> tiles, List<GameObject> floorTiles,
        TileManager tileManager) {
        int nbFragments = (int) Math.Round(tiles.Count / 15.0);
        int minFragmentSize = (int) Math.Round((double) (tiles.Count / nbFragments));
        Debug.Log($"{tiles.Count} tiles in map");
        List<Fragment> fragments = new List<Fragment>();
        List<GameObject> availableFloorTiles = floorTiles;
        List<GameObject> availableTiles = tiles;
        List<GameObject> tilesLeadingToIncompleteFragment = new List<GameObject>();
        
        // Create each fragment
        for (int i = 0; i < nbFragments; i++) {
            Debug.Log($"Creating Fragment {i + 1}");
            List<string> tilesNameInFragments = new List<string>();
            List<GameObject> tilesInFragments = new List<GameObject>();
            
            // First Tile needs to be floorTile to get a spawnTile
            List<GameObject> floorTilesLeft = availableFloorTiles.Where(floorTile => !tilesLeadingToIncompleteFragment.Find(tile => tile == floorTile)).ToList();
            if (floorTilesLeft.Count == 0) {
                Debug.Log("No more fragment possible");
                break;
            }
            GameObject firstTile = floorTilesLeft[Random.Range(0, floorTilesLeft.Count - 1)];
            availableTiles.Remove(firstTile);
            availableFloorTiles.Remove(firstTile);
            string spawnTile = firstTile.name;
            tilesNameInFragments.Add(firstTile.name);
            tilesInFragments.Add(firstTile);

            for (int j = 1; j < minFragmentSize; j++) {
                Debug.Log($"Getting tile {j + 1} of Fragment {i + 1}");
                // Get all neighbor tiles of current tiles in fragments
                List<GameObject> availableNeighborTiles = new List<GameObject>();
                foreach (GameObject tile in tilesInFragments) {
                    List<GameObject> neighborTiles = tileManager.GetNeighborTiles(tile);
                    foreach (GameObject neighborTile in neighborTiles) {
                        if (!tilesInFragments.Find(t => t == neighborTile)
                            && !availableNeighborTiles.Find(t => t == neighborTile)
                            && availableTiles.Find(t=> t == neighborTile)) {
                            availableNeighborTiles.Add(neighborTile);
                        }
                    }
                }

                if (availableNeighborTiles.Count == 0) {
                    Debug.Log($"Fragment {i + 1} cannot be completed, no neighbor tiles for tile {j + 1}");
                    foreach (GameObject tile in tilesInFragments) {
                        availableTiles.Add(tile);
                        if(tile.CompareTag("Floor")) availableFloorTiles.Add(tile);
                    }
                    tilesLeadingToIncompleteFragment.Add(firstTile);
                    break;
                }
                // Get distance of each available neighborTiles
                float distanceMax = 100f;
                GameObject closestTile = null;
                foreach (GameObject neighborTile in availableNeighborTiles) {
                    float currentDistance = 0;
                    foreach (GameObject fragmentTile in tilesInFragments) {
                        currentDistance += Vector3.Distance(fragmentTile.transform.position, neighborTile.transform.position);
                    }

                    if (currentDistance < distanceMax) {
                        distanceMax = currentDistance;
                        closestTile = neighborTile;
                    }
                }
                tilesInFragments.Add(closestTile);
                tilesNameInFragments.Add(closestTile.name);
                availableTiles.Remove(closestTile);
                if (closestTile.CompareTag("Floor")) availableFloorTiles.Remove(closestTile);
            }

            if (tilesInFragments.Count >= minFragmentSize) {
                fragments.Add(new Fragment(tilesNameInFragments, spawnTile, i+1));
            }
            else {
                Debug.Log($"Not enough tile in fragment {i +1}, retrying");
                i -= 1; // Retry to do another fragment starting from another tile
            }
        }
        //TODO: Here add the lonely tiles
        int totalTilesInFragments = 0;
        foreach (Fragment fragment in fragments) {
            totalTilesInFragments += fragment.tiles.Count;
        }
        Debug.Log($"{fragments.Count} fragments, {totalTilesInFragments} tiles covered, {availableTiles.Count} still left");
        return fragments;
    }

    public List<Fragment> GenerateRandomFragmentsOld(List<GameObject> tiles, List<GameObject> floorTiles,
        TileManager tileManager) {
        int nbFragments = (int) Math.Round(tiles.Count / 15.0);
        int tilesPerFragment = (int) Math.Round((double) (tiles.Count / nbFragments));
        List<GameObject> availableTiles = tiles;
        List<Fragment> fragments = new List<Fragment>();
        List<GameObject> availableSpawnTiles = floorTiles;
        for (int i = 0; i < nbFragments; i++) {
            List<string> tilesInFragment = new List<string>();
            for (int j = 0; j < tilesPerFragment; j++) {
                GameObject tile = availableTiles[Random.Range(0, availableTiles.Count - 1)];
                tilesInFragment.Add(tile.name);
                availableTiles.Remove(tile);
            }

            string spawnTile = availableSpawnTiles[Random.Range(0, availableSpawnTiles.Count - 1)].name;
            fragments.Add(new Fragment(tilesInFragment, spawnTile, i));
        }

        return fragments;
    }
}