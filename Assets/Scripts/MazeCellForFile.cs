using System;

[Serializable]
public class MazeCellForFile {
    public int x;
    public int z;
    public int score = int.MaxValue;
    public string action = "NULL";
    public bool visited = false;
    public bool isExit = false;
    public bool permanentlyRevealed = false;
    public bool hasNorthWall = false;
    public bool hasSouthWall = false;
    public bool hasEastWall = false;
    public bool hasWestWall = false;
    public bool hasArrow = false;
    public bool hasLight = false;
}
