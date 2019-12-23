using System.Collections.Generic;
[System.Serializable]
public class GameData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> spawnTiles;
    public GameData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> spawnTilesIn ) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         spawnTiles = spawnTilesIn;
    }
}
