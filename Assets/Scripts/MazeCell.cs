using System;

[Serializable]
public class MazeCell {
    public enum Walls { West, East, North, South, Ceiling, Floor};
    public int score;
    public string action = "NULL";
    public bool visited = false;
    public bool isExit = false;
    public bool hasNorthWall = true;
    public bool hasSouthWall = true;
    public bool hasEastWall = true;
    public bool hasWestWall = true;
    public bool permanentlyRevealed = false;
    public bool revealedForCurrentRun = false;
    public bool hasLight = false;
    public bool hasArrow = false;
    public int fragmentNumber = 0;
    public int x;
    public int z;
    public string name;
}
