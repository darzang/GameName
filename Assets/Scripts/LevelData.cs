using System;
using System.Collections.Generic;

[Serializable]
public class LevelData: object {
    public int mazeRow;
    public int mazeColumns;
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<MazeCell> mazeCells;
    public bool allFragmentsPickedUp;
    public bool pathPlanningDone;
    public bool fragmentsGenerated;
    public LevelData(
        int tryNumber,
        bool allFragmentsPickedUpIn,
        List<MazeCell> mazeCellsIn,
        int rows,
        int columns
        ) {
         tryCount = tryNumber;
         allFragmentsPickedUp = allFragmentsPickedUpIn;
         pathPlanningDone = false;
         mazeCells = mazeCellsIn;
         mazeRow = rows;
         mazeColumns = columns;
    }
}
