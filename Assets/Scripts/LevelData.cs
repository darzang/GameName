using System;
using System.Collections.Generic;

[Serializable]
public class LevelData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<MazeCell> totalDiscoveredCells;
    public bool allFragmentsPickedUp;
    public LevelData(int tryNumber,  List<Fragment> mapFragmentsIn, List<MazeCell> totalDiscoveredCellsIn, bool allFragmentsPickedUpIn) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
         totalDiscoveredCells = totalDiscoveredCellsIn;
         allFragmentsPickedUp = allFragmentsPickedUpIn;
    }
}
