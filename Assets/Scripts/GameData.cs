using System.Collections.Generic;
[System.Serializable]
public class GameData {
    public int tryCount;
    public List<List<string>> mapFragments;
    public List<string> spawnTiles;
    public GameData(int tryNumber,  List<List<string>> mapFragmentsIn, List<string> spawnTilesIn ) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         spawnTiles = spawnTilesIn;
    }
}
