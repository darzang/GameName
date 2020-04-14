using System;

[Serializable]
public class MazeCell {
    public enum Walls { West, East, North, South, Ceiling, Floor}
    public int score;
    public string action = "NULL";
    public bool visited;
    public bool isExit;
    public bool hasNorthWall = true;
    public bool hasSouthWall = true;
    public bool hasEastWall = true;
    public bool hasWestWall = true;
    public bool permanentlyRevealed;
    public bool revealedForCurrentRun;
    public bool hasLight;
    public bool hasArrow;
    public bool hasFragment;
    public bool hasBattery;
    public int fragmentNumber;
    public int x;
    public int z;
    public string name;
}
