using System;
using System.Collections.Generic;

[Serializable]
public class LevelData: object {
    public int mazeRow;
    public int mazeColumns;
    public int tryCount;
    public List<Fragment> mapFragments;
    // public List<MazeCell> permanentlyDiscoveredCells;
    public List<MazeCellForFile> mazeCellsForFile;
    public bool allFragmentsPickedUp;
    public bool pathPlanningDone;
    public LevelData(
        int tryNumber,
        // List<Fragment> mapFragmentsIn,
        // List<MazeCell> permanentlyDiscoveredCellsIn,
        bool allFragmentsPickedUpIn,
        List<MazeCellForFile> mazeCellsForFileIn,
        int rows,
        int columns
        ) {
         tryCount = tryNumber;
         // mapFragments = mapFragmentsIn;
         // permanentlyDiscoveredCells = permanentlyDiscoveredCellsIn;
         allFragmentsPickedUp = allFragmentsPickedUpIn;
         pathPlanningDone = false;
         mazeCellsForFile = mazeCellsForFileIn;
         mazeRow = rows;
         mazeColumns = columns;
    }
}
