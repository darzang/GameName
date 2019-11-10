using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{
    public string tilesDiscovered;
    public int tryCount;
    public List<List<string>> mapFragments;
    public GameData(List<GameObject> discoveredTiles, int tryNumber,  List<List<string>> mapFragmentsIn ) {
         foreach (GameObject tile in discoveredTiles) tilesDiscovered += tile.name + "|";
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
    }
}
