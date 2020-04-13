﻿using System;
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
    private MazeCellManager _mazeCellManager;

    public List<Fragment> GenerateRandomFragments() {
        if (!_mazeCellManager) _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
        // How many fragments do we want ?
        //TODO: Bigger fragments in the beginning, than more so harder to find all of them ?
        int nbFragments = (int) Math.Round(_mazeCellManager.GetMapSize() / 15.0);

        // How many cells per fragment minimum ? 
        int minFragmentSize = (int) Math.Round((double) (_mazeCellManager.GetMapSize() / nbFragments));

        List<Fragment> fragments = new List<Fragment>();
        List<MazeCell> availableCells = _mazeCellManager.mazeCells.ToList();

        // Keep track of cells that lead to a dead end (will be dispatched at the end)
        List<MazeCell> cellsLeadingToIncompleteFragment = new List<MazeCell>();

        // Create each fragment
        for (int i = 0; i < nbFragments; i++) {
            List<MazeCell> cellsInFragment = new List<MazeCell>();

            // Get cells that doesn't lead to a dead end
            List<MazeCell> currentCellsLeft = new List<MazeCell>();
            foreach (MazeCell availableCell in availableCells) {
                if (cellsLeadingToIncompleteFragment.Find(cell => cell == availableCell) != null) continue;
                currentCellsLeft.Add(availableCell);
            }

            if (currentCellsLeft.Count == 0) {
                // If there is no cells left we quit the loop and will dispatch the rest
                break;
            }

            // Select the first tile randomly among all tiles left
            MazeCell firstTile = currentCellsLeft[Random.Range(0, currentCellsLeft.Count - 1)];
            availableCells.Remove(firstTile);
            cellsInFragment.Add(firstTile);

            for (int j = 1; j < minFragmentSize; j++) {
                // Get all neighbor tiles of current tiles in fragments
                List<MazeCell> availableNeighborTiles = new List<MazeCell>();
                foreach (MazeCell tile in cellsInFragment) {
                    List<MazeCell> neighborTiles = _mazeCellManager.GetNeighborTiles(tile);
                    foreach (MazeCell neighborTile in neighborTiles) {
                        if (neighborTile.fragmentNumber == 0
                            && availableNeighborTiles.Find(t => t == neighborTile) == null
                            && availableCells.Find(t => t == neighborTile) != null) {
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
                        currentDistance += Vector3.Distance(
                            new Vector3(fragmentTile.x, 0, fragmentTile.z),
                            new Vector3(neighborTile.x, 0, neighborTile.z));
                    }

                    if (!(currentDistance < distanceMax)) continue;
                    distanceMax = currentDistance;
                    closestTile = neighborTile;
                }

                cellsInFragment.Add(closestTile);
                availableCells.Remove(closestTile);
            }

            if (cellsInFragment.Count >= minFragmentSize) {
                fragments.Add(new Fragment(cellsInFragment, i + 1));
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
                updatedFragment.cellsInFragment.Add(tileLeft);
            }
        }

        int totalTilesInFragments = 0;
        foreach (Fragment fragment in fragments) {
            totalTilesInFragments += fragment.cellsInFragment.Count;
            foreach (MazeCell cell in fragment.cellsInFragment) {
                //TODO: This might be buggy
                cell.fragmentNumber = fragment.number;
            }
        }

        Debug.Log($"{fragments.Count} fragments, {totalTilesInFragments} tiles covered");
        return fragments;
    }

    public Fragment GetFragmentForTile(List<Fragment> fragments, MazeCell cell) {
        return fragments.Find(fragment => fragment.number == cell.fragmentNumber);
    }

    public void InstantiateFragment(Fragment fragmentIn) {
        if (!_mazeCellManager) _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
        // Get a random spawn tile from the cells contained in the fragment
        List<MazeCell> availableFloorTiles = new List<MazeCell>();
        foreach (MazeCell mazeCell in fragmentIn.cellsInFragment) {
            if (mazeCell.hasFragment || mazeCell.hasBattery || mazeCell.isExit) {
                Debug.Log($"Can't spawn fragment {fragmentIn.number} on {mazeCell.name}");
                continue;
            }
            availableFloorTiles.Add(mazeCell);
        }
        MazeCell spawnCell= availableFloorTiles[Random.Range(0, availableFloorTiles.Count - 1)];
        _mazeCellManager.GetCellByName(spawnCell.name).fragmentNumber = fragmentIn.number;
        _mazeCellManager.GetCellByName(spawnCell.name).hasFragment = true;
        Transform fragment = Instantiate(fragmentPrefab, new Vector3(spawnCell.x, 0.35f, spawnCell.z), Quaternion.identity);
        fragment.name = $"Fragment_{fragmentIn.number}";
        fragment.SetParent(GameObject.Find("Fragments").transform);

        List<Vector3> fragmentCellsPositions = new List<Vector3>();
        List<Transform> fragmentTiles = new List<Transform>();
        foreach (MazeCell mazeCell in fragmentIn.cellsInFragment) {
            Transform fragmentCell = Instantiate(mazeCellPrefab, new Vector3(
                mazeCell.x,
                0,
                mazeCell.z
            ), Quaternion.identity);
            fragmentCellsPositions.Add(fragmentCell.transform.position);
            fragmentTiles.Add(fragmentCell);
            fragmentCell.SetParent(fragment);
            GameObject fragmentCellGameObject = fragmentCell.gameObject;
            fragmentCellGameObject.name = $"FragmentTile_{mazeCell.name}";
            fragmentCellGameObject.isStatic = false;
            fragmentCellGameObject.tag = "FragmentTile";

            fragment.gameObject.isStatic = false;
            fragmentCellGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            // Remove walls that are not in the "real" cell
            MazeCell realCell = _mazeCellManager.GetCellByName(mazeCell.name);
            realCell.fragmentNumber = fragmentIn.number;
            // TODO: This should Check neighbor cells as well to don't destry the wall if the neighbor has a wall
            if (!realCell.hasSouthWall) Destroy(fragmentCellGameObject.transform.Find("SouthWall").gameObject);
            if (!realCell.hasEastWall) Destroy(fragmentCellGameObject.transform.Find("EastWall").gameObject);
            if (!realCell.hasWestWall) Destroy(fragmentCellGameObject.transform.Find("WestWall").gameObject);
            if (!realCell.hasNorthWall) Destroy(fragmentCellGameObject.transform.Find("NorthWall").gameObject);

            Destroy(fragmentCellGameObject.transform.Find("Ceiling").gameObject);
            foreach (Transform wall in fragmentCellGameObject.transform) {
                wall.GetComponent<BoxCollider>().isTrigger = true;
            }
        }

        // Shift the tiles to be in center of parent gameObject
        Vector3 offset = GetCenterPointBetween(fragmentCellsPositions);
        foreach (Transform tile in fragmentTiles) {
            // tile.transform.position += offset;
        }

        fragment.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    }

    private Vector3 GetCenterPointBetween(List<Vector3> positions) {
        Vector3 center = new Vector3(0, 0, 0);
        foreach (Vector3 position in positions) {
            center += position;
        }

        return center /= positions.Count; //TODO: Why /= and not just / ?
    }
}