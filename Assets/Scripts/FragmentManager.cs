using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;
using Random = UnityEngine.Random;

public class FragmentManager : MonoBehaviour {
    public Transform wallPrefab;
    public Transform obstaclePrefab;
    public Transform floorPrefab;
    public Transform exitPrefab;
    public Material exitMaterial;
    public Material WallMaterial;
    public Material FloorMaterial;
    public Material ObstacleMaterial;
    public Transform fragmentPrefab;

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
            List<string> tilesNameInFragments = new List<string>();
            List<GameObject> tilesInFragments = new List<GameObject>();

            // First Tile needs to be floorTile to get a spawnTile
            List<GameObject> floorTilesLeft = availableFloorTiles
                .Where(floorTile => !tilesLeadingToIncompleteFragment.Find(tile => tile == floorTile)).ToList();
            if (floorTilesLeft.Count == 0) {
                break;
            }

            GameObject firstTile = floorTilesLeft[Random.Range(0, floorTilesLeft.Count - 1)];
            availableTiles.Remove(firstTile);
            availableFloorTiles.Remove(firstTile);
            string spawnTile = firstTile.name;
            tilesNameInFragments.Add(firstTile.name);
            tilesInFragments.Add(firstTile);

            for (int j = 1; j < minFragmentSize; j++) {
                // Get all neighbor tiles of current tiles in fragments
                List<GameObject> availableNeighborTiles = new List<GameObject>();
                foreach (GameObject tile in tilesInFragments) {
                    List<GameObject> neighborTiles = tileManager.GetNeighborTiles(tile);
                    foreach (GameObject neighborTile in neighborTiles) {
                        if (!tilesInFragments.Find(t => t == neighborTile)
                            && !availableNeighborTiles.Find(t => t == neighborTile)
                            && availableTiles.Find(t => t == neighborTile)) {
                            availableNeighborTiles.Add(neighborTile);
                        }
                    }
                }

                if (availableNeighborTiles.Count == 0) {
                    foreach (GameObject tile in tilesInFragments) {
                        availableTiles.Add(tile);
                        if (tile.CompareTag("Floor")) availableFloorTiles.Add(tile);
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
                        currentDistance += Vector3.Distance(fragmentTile.transform.position,
                            neighborTile.transform.position);
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
                fragments.Add(new Fragment(tilesNameInFragments, spawnTile, i + 1));
            }
            else {
                i -= 1; // Retry to do another fragment starting from another tile
            }
        }

        // Add the lonely tiles
        foreach (GameObject tileLeft in availableTiles) {
            Fragment closestFragment = GetFragmentForTile(fragments, tileLeft);
            Fragment updatedFragment = fragments.Find(frg => frg == closestFragment);
            if (updatedFragment != null) {
                updatedFragment.tiles.Add(tileLeft.name);
            }
        }

        int totalTilesInFragments = 0;
        foreach (Fragment fragment in fragments) {
            totalTilesInFragments += fragment.tiles.Count;
        }

        Debug.Log(
            $"{fragments.Count} fragments, {totalTilesInFragments} tiles covered");
        return fragments;
    }

    public Fragment GetFragmentForTile(List<Fragment> fragments, GameObject tile) {
        // Get distance of each available neighborTiles
        float distanceMax = 100f;
        Fragment closestFragment = null;
        foreach (Fragment fragment in fragments) {
            float currentDistance = 0;
            foreach (string tileName in fragment.tiles) {
                GameObject fragmentTile = GameObject.Find(tileName);
                currentDistance += Vector3.Distance(fragmentTile.transform.position, tile.transform.position);
            }

            if (currentDistance < distanceMax) {
                distanceMax = currentDistance;
                closestFragment = fragment;
            }
        }
        return closestFragment;
    }

    public void InstantiateFragment(Fragment fragmentIn) {
        GameObject spawnTile = GameObject.Find(fragmentIn.spawnTile);
        Vector3 position = spawnTile.transform.position;
        // Quaternion quaternion = Quaternion.identity;
        // quaternion.z = 60;
        Transform fragment = Instantiate(fragmentPrefab, new Vector3(
            position.x,
            position.y + 0.35f,
            position.z
        ), Quaternion.identity);
        fragment.name = $"Fragment_{fragmentIn.number}";
        fragment.SetParent(GameObject.Find("Fragments").transform);
        
        foreach (string tileName in fragmentIn.tiles) {
            GameObject realTile = GameObject.Find(tileName);
            Transform tilePrefab;
            Material tileMaterial;
            switch (realTile.tag) {
                case "Wall":
                    tilePrefab = wallPrefab;
                    tileMaterial = WallMaterial;
                    break;
                case "Obstacle":
                    tilePrefab = obstaclePrefab;
                    tileMaterial = ObstacleMaterial;
                    break;
                case "Exit":
                    tilePrefab = exitPrefab;
                    tileMaterial = exitMaterial;
                    break;
                case "Floor":
                    tilePrefab = floorPrefab;
                    tileMaterial = FloorMaterial;
                    break;
                default:
                    Debug.Log($"Tag not found for tile {tileName} with tag {realTile.tag}");
                    tileMaterial = exitMaterial;
                    tilePrefab = exitPrefab;
                    break;
            }


            Transform fragmentTile = Instantiate(tilePrefab, new Vector3(
                realTile.transform.position.x,
                realTile.transform.position.y,
                realTile.transform.position.z
            ), Quaternion.identity);
            if (tilePrefab == exitPrefab) {
                fragmentTile.gameObject.GetComponentInChildren<Light>().enabled = false;
            }

            fragmentTile.gameObject.GetComponent<Renderer>().material = tileMaterial;
            fragmentTile.gameObject.GetComponent<BoxCollider>().enabled = false;
            fragment.gameObject.isStatic = false;
            fragmentTile.transform.localScale = new Vector3(1f,1f,1f);
            if (tilePrefab == floorPrefab || tilePrefab == exitPrefab) {
                Vector3 localScale = fragmentTile.transform.localScale;
                localScale.y = 0.01f;
                fragmentTile.transform.localScale = localScale;
            }
            // fragmentTile.transform.localScale = Vector3.one;
            fragmentTile.SetParent(fragment);
        }
        fragment.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
        // fragment.transform.rotation = Quaternion.Euler(0, 0, 60f);
    }
}