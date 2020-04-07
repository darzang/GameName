using System;
using System.Collections.Generic;

[Serializable]
public class Fragment {
    public List<string> cellsNamesInFragment;
    public int number;
    public bool discovered;

    public Fragment(List<string> cellsNamesInFragment, int number) {
        this.cellsNamesInFragment = cellsNamesInFragment;
        this.number = number;
        discovered = false;
    }
    
}