using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FragmentManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Fragment> GenerateRandomFragments(List<GameObject> tiles) {
        int nbFragments = (int) Math.Round(tiles.Count / 15.0);
        int tilesPerFragment = (int) Math.Round((double) (tiles.Count / nbFragments));
        List<string> usedTiles = new List<string>();
        List<Fragment> fragments = new List<Fragment>();
        for (int i = 0; i < nbFragments; i++) {
            List<string> tilesInFragment = new List<string>();
            for (int j = 0; j < tilesPerFragment; j++) {
                GameObject tile;
                do {
                    tile = tiles[Random.Range(0, tiles.Count - 1)];
                    if (!usedTiles.Contains(tile.name)) {
                        usedTiles.Add(tile.name);
                        tilesInFragment.Add(tile.name);
                    }
                } while (usedTiles.Contains(tile.name));
            }

            string spawnTile = tilesInFragment[Random.Range(0, tilesInFragment.Count - 1)];
            fragments.Add(new Fragment(tilesInFragment, spawnTile, i));
        }

        return fragments;
    }
}
