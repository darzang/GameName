using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SceneGenerator : MonoBehaviour {
    public Transform gameManagerPrefab;
    public Transform tileManagerPrefab;
    public Transform uiManagerPrefab;
    public Transform fragmentManager;
    public Transform lightPrefab;
    public Material exitMaterial;
    public int mazeRow = 10;
    public int mazeColumn = 10;
    public Transform mazeCellPrefab;
    private GameObject _maze;
    private MazeCell[,] _mazeCells;
    private int _currentRow = 0;
    private int _currentColumn = 0;
    private bool _courseComplete = false;
    private bool hideCeiling = true;
    private GameObject _arrows;
    public Transform arrowPrefab;

    private void Start() {
        _maze = GameObject.Find("Maze");
        _mazeCells = GenerateMazeIfExists();
        if (_mazeCells == null) {
            _mazeCells = new MazeCell[mazeRow,mazeColumn];
            Debug.Log($"Generating new maze for {SceneManager.GetActiveScene().name}");
            InstantiateMaze();
            HuntAndKill();
            CreateLevelData();
        }
        InstantiateManagers();
    }

    private void InstantiateManagers() {
        Transform fragmentManagerObject = Instantiate(fragmentManager, Vector3.zero, Quaternion.identity);
        fragmentManagerObject.gameObject.name = "FragmentManager";
        Transform uiManagerObject = Instantiate(uiManagerPrefab, Vector3.zero, Quaternion.identity);
        uiManagerObject.gameObject.name = "UIManager";
        Transform tileManagerObject = Instantiate(tileManagerPrefab, Vector3.zero, Quaternion.identity);
        tileManagerObject.gameObject.name = "TileManager";
        tileManagerObject.GetComponent<MazeCellManager>().SetMazeCells(_mazeCells, mazeRow, mazeColumn);
        Transform gameManagerObject = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);
        gameManagerObject.gameObject.name = "GameManager";
    }

    // Maze Generation related
    private GameObject InstantiateMazeCell(int row, int column) {
        Transform mazeCell = Instantiate(mazeCellPrefab, new Vector3(row, 0, column), Quaternion.identity,
            _maze.transform);
        mazeCell.gameObject.name = $"MazeCell_{row}_{column}";
        mazeCell.GetComponent<MazeCell>().score = mazeRow * mazeColumn;
        mazeCell.GetComponent<MazeCell>().GetWall(MazeCell.Walls.Ceiling).SetActive(!hideCeiling);
        return mazeCell.gameObject;
    }

    private void InstantiateMaze() {
        int index = 0;
        for (int r = 0; r < mazeRow; r++) {
            for (int c = 0; c < mazeColumn; c++) {
                _mazeCells[r, c] = InstantiateMazeCell(r, c).gameObject.GetComponent<MazeCell>();
                if (index % 3 == 0) {
                    InstantiateLight(_mazeCells[r, c]);
                    _mazeCells[r, c].hasLight = true;
                }
                index++;
            }
        }

        int x = Random.Range(0, _mazeCells.GetLength(0));
        int z = Random.Range(0, _mazeCells.GetLength(1));
        _mazeCells[x, z].isExit = true;
        _mazeCells[x, z].GetWall(MazeCell.Walls.Floor).GetComponent<Renderer>().material = exitMaterial;
    }

    private IEnumerator HuntAndKillCoroutine() {
        _mazeCells[_currentRow, _currentColumn].visited = true;
        _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
        while (!_courseComplete) {
            while (RouteStillAvailable(_currentRow, _currentColumn)) {
                Kill();
                yield return new WaitForSeconds(0.001f);
            }
            Hunt();
            yield return new WaitForSeconds(0.001f);
        }
    }

    private void HuntAndKill() {
        _mazeCells[_currentRow, _currentColumn].visited = true;
        _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
        while (!_courseComplete) {
            Kill();
            Hunt();
        }
        DestroyDoubleWalls();
    }

    private void Kill() {
        if (RouteStillAvailable(_currentRow, _currentColumn)) {
            int direction = Random.Range(1, 5);
            _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
            if (direction == 1 && CellIsAvailable(_currentRow - 1, _currentColumn)) {
                // Going north, so destroy current north wall + north cell's south wall
                _mazeCells[_currentRow, _currentColumn].DestroyWallIfExists(MazeCell.Walls.North);
                _mazeCells[_currentRow - 1, _currentColumn].DestroyWallIfExists(MazeCell.Walls.South);
                _currentRow--;
            }
            else if (direction == 2 && CellIsAvailable(_currentRow + 1, _currentColumn)) {
                // Going south
                _mazeCells[_currentRow, _currentColumn].DestroyWallIfExists(MazeCell.Walls.South);
                _mazeCells[_currentRow + 1, _currentColumn].DestroyWallIfExists(MazeCell.Walls.North);
                _currentRow++;
            }
            else if (direction == 3 && CellIsAvailable(_currentRow, _currentColumn + 1)) {
                // Going east
                _mazeCells[_currentRow, _currentColumn].DestroyWallIfExists(MazeCell.Walls.East);
                _mazeCells[_currentRow, _currentColumn + 1].DestroyWallIfExists(MazeCell.Walls.West);
                _currentColumn++;
            }
            else if (direction == 4 && CellIsAvailable(_currentRow, _currentColumn - 1)) {
                // Going west
                _mazeCells[_currentRow, _currentColumn].DestroyWallIfExists(MazeCell.Walls.West);
                _mazeCells[_currentRow, _currentColumn - 1].DestroyWallIfExists(MazeCell.Walls.East);
                _currentColumn--;
            }

            _mazeCells[_currentRow, _currentColumn].visited = true;
            _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.yellow);
        }
    }

    private void Hunt() {
        _courseComplete = true;
        _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
        for (int r = 0; r < mazeRow; r++) {
            for (int c = 0; c < mazeColumn; c++) {
                if (!_mazeCells[r, c].visited && CellHasAnAdjacentVisitedCell(r, c)) {
                    _courseComplete = false;
                    _currentRow = r;
                    _currentColumn = c;
                    DestroyAdjacentWall(_currentRow, _currentColumn);
                    _mazeCells[_currentRow, _currentColumn].visited = true;
                    _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.yellow);
                    return;
                }
            }
        }
    }

    private bool CellIsAvailable(int row, int column) {
        return row >= 0 && row < mazeRow && column >= 0 && column < mazeColumn && !_mazeCells[row, column].visited;
    }


    private bool RouteStillAvailable(int row, int column) {
        return (row > 0 && !_mazeCells[row - 1, column].visited)
               || (row < mazeRow - 1 && !_mazeCells[row + 1, column].visited)
               || (column > 0 && !_mazeCells[row, column - 1].visited)
               || (column < mazeColumn - 1 && !_mazeCells[row, column + 1].visited);
    }

    private bool CellHasAnAdjacentVisitedCell(int row, int column) {
        return (row > 0 && _mazeCells[row - 1, column].visited)
               || (row < (mazeRow - 2) && _mazeCells[row + 1, column].visited)
               || (column > 0 && _mazeCells[row, column - 1].visited)
               || (column < (mazeColumn - 2) && _mazeCells[row, column + 1].visited);
    }

    private void DestroyAdjacentWall(int row, int column) {
        bool wallDestroyed = false;

        while (!wallDestroyed) {
            int direction = Random.Range(1, 5);

            if (direction == 1 && row > 0 && _mazeCells[row - 1, column].visited) {
                _mazeCells[row, column].DestroyWallIfExists(MazeCell.Walls.North);
                _mazeCells[row - 1, column].DestroyWallIfExists(MazeCell.Walls.South);
                wallDestroyed = true;
            }
            else if (direction == 2 && row < (mazeRow - 2) && _mazeCells[row + 1, column].visited) {
                _mazeCells[row, column].DestroyWallIfExists(MazeCell.Walls.South);
                _mazeCells[row + 1, column].DestroyWallIfExists(MazeCell.Walls.North);
                wallDestroyed = true;
            }
            else if (direction == 3 && column > 0 && _mazeCells[row, column - 1].visited) {
                _mazeCells[row, column].DestroyWallIfExists(MazeCell.Walls.West);
                _mazeCells[row, column - 1].DestroyWallIfExists(MazeCell.Walls.East);
                wallDestroyed = true;
            }
            else if (direction == 4 && column < mazeColumn - 2 && _mazeCells[row, column + 1].visited) {
                _mazeCells[row, column].DestroyWallIfExists(MazeCell.Walls.East);
                _mazeCells[row, column + 1].DestroyWallIfExists(MazeCell.Walls.West);
                wallDestroyed = true;
            }
        }
    }

    private bool CellExists(float row, float column) {
        return row >= 0 && row < mazeRow && column >= 0 && column < mazeColumn;
    }

    private void DestroyDoubleWalls() {
        // Destroy double walls where two adjacent cells share a common wall
        foreach (MazeCell mazeCell in _mazeCells) {
            _currentColumn = (int) mazeCell.transform.position.z;
            _currentRow = (int) mazeCell.transform.position.x;
            // Check tile north
            if (CellExists(_currentRow - 1, _currentColumn)
                && _mazeCells[_currentRow - 1, _currentColumn].hasSouthWall
                && mazeCell.hasNorthWall
            ) {
                mazeCell.DestroyWallIfExists(MazeCell.Walls.North);
            }

            // Check tile west
            if (CellExists(_currentRow, _currentColumn - 1)
                && _mazeCells[_currentRow, _currentColumn - 1].hasEastWall
                && mazeCell.hasWestWall
            ) {
                mazeCell.DestroyWallIfExists(MazeCell.Walls.West);
            }
        }
    }

    private void InstantiateLight(MazeCell mazeCell) {
        Transform light = Instantiate(lightPrefab, mazeCell.transform);
        light.localPosition = new Vector3(0, 0.9f, 0);
    }


    private List<MazeCellForFile> FormatMazeCells(MazeCell[,] mazeCells) {
        List<MazeCellForFile> mazeCellsFormatted = new List<MazeCellForFile>();
        foreach (MazeCell mazeCell in mazeCells) {
            MazeCellForFile mazeCellFormatted = new MazeCellForFile {
                isExit = mazeCell.isExit,
                hasLight = mazeCell.hasLight,
                hasEastWall = mazeCell.hasEastWall,
                hasWestWall = mazeCell.hasWestWall,
                hasNorthWall = mazeCell.hasNorthWall,
                hasSouthWall = mazeCell.hasSouthWall,
                x = (int) mazeCell.transform.position.x,
                z = (int) mazeCell.transform.position.z
            };
            mazeCellsFormatted.Add(mazeCellFormatted);
        }

        return mazeCellsFormatted;
    }
    private void CreateLevelData() {
        FileManager.SaveLevelDataFile(
            new LevelData(
                0, 
                false, 
                FormatMazeCells(_mazeCells), 
                _mazeCells.GetLength(0), 
                _mazeCells.GetLength(1)
                ),
            SceneManager.GetActiveScene().name);
    }

    private MazeCell[,] GenerateMazeIfExists() {
        LevelData levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);

        if (levelData == null) return null;
        Debug.Log($"Generating existing maze for {SceneManager.GetActiveScene().name}");
        MazeCell[,] mazeCells = new MazeCell[levelData.mazeRow, levelData.mazeColumns];
        foreach (MazeCellForFile mazeCellForFile in levelData.mazeCellsForFile) {
            GameObject cellObject = InstantiateMazeCell((int) mazeCellForFile.x, (int) mazeCellForFile.z);
            MazeCell mazeCell = cellObject.GetComponent<MazeCell>();
            if (!mazeCellForFile.hasEastWall) mazeCell.DestroyWallIfExists(MazeCell.Walls.East);
            if (!mazeCellForFile.hasWestWall) mazeCell.DestroyWallIfExists(MazeCell.Walls.West);
            if (!mazeCellForFile.hasNorthWall) mazeCell.DestroyWallIfExists(MazeCell.Walls.North);
            if (!mazeCellForFile.hasSouthWall) mazeCell.DestroyWallIfExists(MazeCell.Walls.South);
            if(mazeCellForFile.hasLight) InstantiateLight(mazeCell);
            if (mazeCellForFile.isExit) {
                cellObject.transform.Find("Floor").GetComponent<Renderer>().material = exitMaterial;
                mazeCell.isExit = true;
            }
            mazeCell.action = mazeCellForFile.action;
            if (mazeCellForFile.hasArrow) {
                mazeCell.hasArrow = true;
                InstantiateArrow(mazeCell);
            }

            mazeCell.permanentlyRevealed = mazeCellForFile.permanentlyRevealed;

            mazeCells[mazeCellForFile.x, mazeCellForFile.z] = mazeCell;
        }

        return mazeCells;
    }
    
    private void InstantiateArrow(MazeCell mazeCell) {
        if (!_arrows) {
            _arrows = GameObject.Find("Arrows").gameObject;
        }
        if (GameObject.Find($"Arrow_{mazeCell.gameObject.name}")) {
            //TODO: Just flip it
            Destroy(GameObject.Find($"Arrow_{mazeCell.gameObject.name}"));
        }

        float angle = 0;
        switch (mazeCell.action) {
            case "SOUTH":
                angle = 270;
                break;            
            case "EAST":
                angle = 180;
                break;
            case "NORTH":
                angle = 90;
                break;
            case "WEST":
                angle = 0;
                break;
        }

        Transform arrow = Instantiate(arrowPrefab, new Vector3(
            mazeCell.transform.position.x,
            mazeCell.transform.position.y + 0.05f,
            mazeCell.transform.position.z
        ), Quaternion.identity);
        arrow.name = $"Arrow_{mazeCell.gameObject.name}";
        arrow.transform.eulerAngles = new Vector3(0, angle, 0);
        arrow.SetParent(_arrows.transform);
        mazeCell.hasArrow = true;
    }
}