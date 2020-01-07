using System.Collections.Generic;

[System.Serializable]
public class Fragment {
    public List<string> tiles;
    public string spawnTile;
    public int number;
    public bool discovered;
    public bool arrowRevealed;
    public Fragment(List<string> tiles, string spawnTile, int number) {
        this.tiles = tiles;
        this.spawnTile = spawnTile;
        this.number = number;
        discovered = false;
        arrowRevealed = false;
    }
    
}
