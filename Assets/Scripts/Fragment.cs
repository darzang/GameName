using System;
using System.Collections.Generic;

[Serializable]
public class Fragment {
    public List<string> cellsNamesInFragment;
    public string spawnCell; // TODO: Make it random in cellsInFragment cells
    public int number;
    public bool discovered;
    public bool arrowRevealed;

    public Fragment(List<string> cellsNamesInFragment, string spawnCell, int number) {
        this.cellsNamesInFragment = cellsNamesInFragment;
        this.spawnCell = spawnCell;
        this.number = number;
        discovered = false;
        arrowRevealed = false;
    }
}