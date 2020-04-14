using System;
using System.Collections.Generic;

[Serializable]
public class LevelData: object {
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<MazeCell> mazeCells;
    public LevelData(
        int tryNumber,
        List<MazeCell> mazeCellsIn
        ) {
         tryCount = tryNumber;
         mazeCells = mazeCellsIn;
    }
}
