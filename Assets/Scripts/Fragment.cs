using System;
using System.Collections.Generic;

[Serializable]
public class Fragment {
    public List<MazeCell> cellsInFragment;
    public int number;
    public bool discovered;

    public Fragment(List<MazeCell> cellsInFragment, int number) {
        this.cellsInFragment = cellsInFragment;
        this.number = number;
        discovered = false;
    }
    
}