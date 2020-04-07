using System;
using System.Collections.Generic;

[Serializable]
public class LevelData: object {
    public int mazeRow;
    public int mazeColumns;
    public int tryCount;
    public List<Fragment> mapFragments;
    public List<MazeCellForFile> mazeCellsForFile;
    public bool allFragmentsPickedUp;
    public bool pathPlanningDone;
    public bool fragmentsGenerated;
    public LevelData(
        int tryNumber,
        bool allFragmentsPickedUpIn,
        List<MazeCellForFile> mazeCellsForFileIn,
        int rows,
        int columns
        ) {
         tryCount = tryNumber;
         allFragmentsPickedUp = allFragmentsPickedUpIn;
         pathPlanningDone = false;
         mazeCellsForFile = mazeCellsForFileIn;
         mazeRow = rows;
         mazeColumns = columns;
    }
}
