using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{
 public string tilesDiscovered;

 public GameData(List<GameObject> discoveredTiles)
 {
  foreach (GameObject tile in discoveredTiles)
  {
   tilesDiscovered += tile.name + "|";
  }
 }
}
