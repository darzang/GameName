using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FragmentManager : MonoBehaviour {
    public Transform mazeCellPrefab;
    public Material exitMaterial;
    public Material wallMaterial;
    public Material floorMaterial;
    public Material obstacleMaterial;
    public Transform fragmentPrefab;

    public List<Fragment> GenerateRandomFragments(List<MazeCell> mazeCells, MazeCellManager mazeCellManager) {
        
        // How many fragments do we want ?
        //TODO: Bigger fragments in the beginning, than more so harder to find all of them ?
        int nbFragments = (int) Math.Round(mazeCells.Count / 15.0);
        
        // How many cells per fragment minimum ? 
        int minFragmentSize = (int) Math.Round((double) (mazeCells.Count / nbFragments));
        
        List<Fragment> fragments = new List<Fragment>();
        List<MazeCell> availableCells = mazeCells.Where(cell => !cell.isExit).ToList();
        
        // Keep track of cells that lead to a dead end (will be dispatched at the end)
        List<MazeCell> cellsLeadingToIncompleteFragment = new List<MazeCell>();

        // Create each fragment
        for (int i = 0; i < nbFragments; i++) {
            List<string> cellsNamesInFragment = new List<string>();
            List<MazeCell> cellsInFragment = new List<MazeCell>();
            
            // Get cells that doesn't lead to a dead end
            List<MazeCell> currentCellsLeft = availableCells
                .Where(floorTile => !cellsLeadingToIncompleteFragment
                    .Find(tile => tile == floorTile)).ToList();

            if (currentCellsLeft.Count == 0) {
                // If there is no cells left we quit the loop and will dispatch the rest
                break;
            }

            // Select the first tile randomly among all tiles left
            MazeCell firstTile = currentCellsLeft[Random.Range(0, currentCellsLeft.Count - 1)];
            availableCells.Remove(firstTile);
            cellsNamesInFragment.Add(firstTile.name);
            cellsInFragment.Add(firstTile);

            for (int j = 1; j < minFragmentSize; j++) {
                // Get all neighbor tiles of current tiles in fragments
                List<MazeCell> availableNeighborTiles = new List<MazeCell>();
                foreach (MazeCell tile in cellsInFragment) {
                    List<MazeCell> neighborTiles = mazeCellManager.GetNeighborTiles(tile);
                    foreach (MazeCell neighborTile in neighborTiles) {
                        if (!cellsInFragment.Find(t => t == neighborTile)
                            && !availableNeighborTiles.Find(t => t == neighborTile)
                            && availableCells.Find(t => t == neighborTile)) {
                            availableNeighborTiles.Add(neighborTile);
                        }
                    }
                }

                if (availableNeighborTiles.Count == 0) {
                    foreach (MazeCell tile in cellsInFragment) {
                        availableCells.Add(tile);
                    }

                    cellsLeadingToIncompleteFragment.Add(firstTile);
                    break;
                }

                // Get distance of each available neighborTiles
                float distanceMax = 100f;
                MazeCell closestTile = null;
                foreach (MazeCell neighborTile in availableNeighborTiles) {
                    float currentDistance = 0;
                    foreach (MazeCell fragmentTile in cellsInFragment) {
                        currentDistance += Vector3.Distance(fragmentTile.transform.position,
                            neighborTile.transform.position);
                    }

                    if (!(currentDistance < distanceMax)) continue;
                    distanceMax = currentDistance;
                    closestTile = neighborTile;
                }

                cellsInFragment.Add(closestTile);
                cellsNamesInFragment.Add(closestTile.name);
                availableCells.Remove(closestTile);
            }

            if (cellsInFragment.Count >= minFragmentSize) {
                fragments.Add(new Fragment(cellsNamesInFragment, i + 1));
            }
            else {
                i -= 1; // Retry to do another fragment starting from another tile
            }
        }

        // Add the lonely tiles
        foreach (MazeCell tileLeft in availableCells) {
            Fragment closestFragment = GetFragmentForTile(fragments, tileLeft);
            Fragment updatedFragment = fragments.Find(frg => frg == closestFragment);
            if (updatedFragment != null) {
                updatedFragment.cellsNamesInFragment.Add(tileLeft.name);
            }
        }

        int totalTilesInFragments = 0;
        foreach (Fragment fragment in fragments) {
            totalTilesInFragments += fragment.cellsNamesInFragment.Count;
        }

        Debug.Log(
            $"{fragments.Count} fragments, {totalTilesInFragments} tiles covered");
        return fragments;
    }

    public Fragment GetFragmentForTile(List<Fragment> fragments, MazeCell cell) {
        // // Get distance of each available neighborTiles
        // float distanceMax = 100f;
        // Fragment closestFragment = null;
        // foreach (Fragment fragment in fragments) {
        //     float currentDistance = 0;
        //     foreach (string tileName in fragment.cellsNamesInFragment) {
        //         GameObject fragmentTile = GameObject.Find(tileName);
        //         currentDistance += Vector3.Distance(fragmentTile.transform.position, tile.transform.position);
        //     }
        //
        //     if (!(currentDistance < distanceMax)) continue;
        //     distanceMax = currentDistance;
        //     closestFragment = fragment;
        // }

        return fragments.Find(fragment => fragment.number == cell.fragmentNumber);

    }

    public void InstantiateFragment(Fragment fragmentIn) {
        // Get a random spawn tile from the cells contained in the fragment
        GameObject spawnTile = GameObject.Find(fragmentIn.cellsNamesInFragment[Random.Range(0,fragmentIn.cellsNamesInFragment.Count - 1)]);
        spawnTile.GetComponent<MazeCell>().fragmentNumber = fragmentIn.number;
        Vector3 position = spawnTile.transform.position;
        Transform fragment = Instantiate(fragmentPrefab, new Vector3(
            position.x,
            position.y + 0.35f,
            position.z
        ), Quaternion.identity);
        fragment.name = $"Fragment_{fragmentIn.number}";
        fragment.SetParent(GameObject.Find("Fragments").transform);

        List<Vector3> tilesPositions = new List<Vector3>();
        List<Transform> fragmentTiles = new List<Transform>();
        foreach (string tileName in fragmentIn.cellsNamesInFragment) {
            GameObject realTile = GameObject.Find(tileName);
            Vector3 position1 = realTile.transform.position;
            Transform fragmentTile = Instantiate(mazeCellPrefab, new Vector3(
                position1.x / 2,
                position1.y,
                position1.z / 2
            ), Quaternion.identity);
            tilesPositions.Add(fragmentTile.transform.position);
            fragmentTiles.Add(fragmentTile);
            fragmentTile.SetParent(fragment);
            GameObject tileObject = fragmentTile.gameObject;
            tileObject.name = $"FragmentTile_{realTile.name}";
            tileObject.isStatic = false;
            tileObject.tag = "FragmentTile";
            tileObject.GetComponent<MazeCell>().SetCollidersTrigger(true);
            fragment.gameObject.isStatic = false;
            fragmentTile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Remove walls that are not in the "real" cell
            MazeCell realCell = realTile.GetComponent<MazeCell>();
            MazeCell fragmentCell = fragmentTile.GetComponent<MazeCell>();
            
            // TODO: This should Check neighbor cells as well to don't destry the wall if the neighbor has a wall
            if(!realCell.southWall) fragmentCell.DestroyWallIfExists(fragmentCell.southWall);
            if(!realCell.eastWall) fragmentCell.DestroyWallIfExists(fragmentCell.eastWall);
            if(!realCell.westWall) fragmentCell.DestroyWallIfExists(fragmentCell.westWall);
            if(!realCell.northWall) fragmentCell.DestroyWallIfExists(fragmentCell.northWall);
            fragmentCell.DestroyWallIfExists(fragmentCell.ceiling);
        }
        // Shift the tiles to be in center of parent gameObject
        Vector3 offset = GetCenterPointBetween(tilesPositions);
        foreach (Transform tile in fragmentTiles) {
                tile.transform.position += offset;
        }
        fragment.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private Vector3 GetCenterPointBetween(List<Vector3> positions){
        Vector3 center = new Vector3(0,0,0);
        foreach (Vector3 position in positions) {
            center += position;
        }
        return center /= positions.Count; //TODO: Why /= and not just / ?
    }
}