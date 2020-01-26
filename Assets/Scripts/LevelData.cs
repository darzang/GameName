using System.Collections.Generic;
[System.Serializable]
public class LevelData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> totalDiscoveredTiles;
    public bool allFragmentsPickedUp;
    public LevelData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> totalDiscoveredTilesIn, bool allFragmentsPickedUpIn) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         totalDiscoveredTiles = totalDiscoveredTilesIn;
         allFragmentsPickedUp = allFragmentsPickedUpIn;
    }
}
