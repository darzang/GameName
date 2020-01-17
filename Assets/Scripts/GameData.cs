using System.Collections.Generic;
[System.Serializable]
public class GameData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> totalDiscoveredTiles;
    public bool levelOver;
    public GameData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> totalDiscoveredTilesIn,  bool levelOverIn = false) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         totalDiscoveredTiles = totalDiscoveredTilesIn;
         levelOver = levelOverIn;
    }
}
