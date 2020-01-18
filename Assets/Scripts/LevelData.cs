using System.Collections.Generic;
[System.Serializable]
public class LevelData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> totalDiscoveredTiles;
    public LevelData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> totalDiscoveredTilesIn) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         totalDiscoveredTiles = totalDiscoveredTilesIn;
    }
}
