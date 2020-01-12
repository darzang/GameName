using System.Collections.Generic;
[System.Serializable]
public class GameData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<string> totalDiscoveredTiles;
    public bool exitRevealed;
    public bool levelOver;
    public GameData(int tryNumber,  List<Fragment> mapFragmentsIn, List<string> totalDiscoveredTilesIn, bool exitRevealedIn, bool levelOverIn = false) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         totalDiscoveredTiles = totalDiscoveredTilesIn;
         exitRevealed = exitRevealedIn;
         levelOver = levelOverIn;
    }
}
