using System.Collections.Generic;
[System.Serializable]
public class GameData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> discoveredTiles;
    public GameData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> discoveredTilesIn ) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         discoveredTiles = discoveredTilesIn;
    }
}
