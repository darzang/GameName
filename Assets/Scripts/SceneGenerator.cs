using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SceneGenerator : MonoBehaviour {
    public Transform gameManagerPrefab;
    public Transform uiManagerPrefab;
    public Transform fragmentManager;
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

    private MazeCellManager _mazeCellManager;
    private void Start() {
        _maze = GameObject.Find("Maze");
        _mazeCells = GenerateMazeIfExists();
        _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
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
        Transform gameManagerObject = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);
        gameManagerObject.gameObject.name = "GameManager";
    }

    // Maze Generation related
    private GameObject InstantiateMazeCell(int row, int column) {
        Transform mazeCellObject = Instantiate(mazeCellPrefab, new Vector3(row, 0, column), Quaternion.identity,
            _maze.transform);
        mazeCellObject.gameObject.name = $"MazeCell_{row}_{column}";
        MazeCell mazeCell = mazeCellObject.GetComponent<MazeCell>();
        mazeCell.score = mazeRow * mazeColumn;
        _mazeCellManager.GetWall(mazeCell, MazeCell.Walls.Ceiling).SetActive(!hideCeiling);
        mazeCell.x = row;
        mazeCell.z = column;
        mazeCell.fragmentNumber = 0;
        mazeCell.name = $"MazeCell_{row}_{column}";
        return mazeCellObject.gameObject;
    }

    private void InstantiateMaze() {
        int index = 0;
        for (int r = 0; r < mazeRow; r++) {
            for (int c = 0; c < mazeColumn; c++) {
                _mazeCells[r, c] = InstantiateMazeCell(r, c).gameObject.GetComponent<MazeCell>();
                if (index % 3 == 0) {
                    _mazeCellManager.InstantiateLight(_mazeCells[r, c]);
                }
                index++;
            }
        }

        int x = Random.Range(0, _mazeCells.GetLength(0));
        int z = Random.Range(0, _mazeCells.GetLength(1));
        _mazeCells[x, z].isExit = true;
        _mazeCellManager.GetWall(_mazeCells[x, z], MazeCell.Walls.Floor).GetComponent<Renderer>().material = exitMaterial;
    }

    private IEnumerator HuntAndKillCoroutine() {
        _mazeCells[_currentRow, _currentColumn].visited = true;
        _mazeCellManager.SetFloorColor(_mazeCells[_currentRow, _currentColumn], Color.blue);
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
        // Coloring here was used for visual demo of what was going on
        // _mazeCellManager.SetFloorColor(_mazeCells[_currentRow, _currentColumn], Color.blue);
        while (!_courseComplete) {
            Kill();
            Hunt();
        }
        DestroyDoubleWalls();
    }

    private void Kill() {
        if (RouteStillAvailable(_currentRow, _currentColumn)) {
            int direction = Random.Range(1, 5);
            // _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
            if (direction == 1 && CellIsAvailable(_currentRow - 1, _currentColumn)) {
                // Going north, so destroy current north wall + north cell's south wall
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn], MazeCell.Walls.North);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow -1, _currentColumn], MazeCell.Walls.South);
                _currentRow--;
            }
            else if (direction == 2 && CellIsAvailable(_currentRow + 1, _currentColumn)) {
                // Going south
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn],MazeCell.Walls.South);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow + 1, _currentColumn],MazeCell.Walls.North);
                _currentRow++;
            }
            else if (direction == 3 && CellIsAvailable(_currentRow, _currentColumn + 1)) {
                // Going east
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn],MazeCell.Walls.East);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn + 1],MazeCell.Walls.West);
                _currentColumn++;
            }
            else if (direction == 4 && CellIsAvailable(_currentRow, _currentColumn - 1)) {
                // Going west
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn],MazeCell.Walls.West);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn - 1],MazeCell.Walls.East);
                _currentColumn--;
            }

            _mazeCells[_currentRow, _currentColumn].visited = true;
        }
    }

    private void Hunt() {
        _courseComplete = true;
        for (int r = 0; r < mazeRow; r++) {
            for (int c = 0; c < mazeColumn; c++) {
                if (!_mazeCells[r, c].visited && CellHasAnAdjacentVisitedCell(r, c)) {
                    _courseComplete = false;
                    _currentRow = r;
                    _currentColumn = c;
                    DestroyAdjacentWall(_currentRow, _currentColumn);
                    _mazeCells[_currentRow, _currentColumn].visited = true;
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
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column],MazeCell.Walls.North);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row - 1, column],MazeCell.Walls.South);
                wallDestroyed = true;
            }
            else if (direction == 2 && row < (mazeRow - 2) && _mazeCells[row + 1, column].visited) {
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column],MazeCell.Walls.South);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row + 1, column],MazeCell.Walls.North);
                wallDestroyed = true;
            }
            else if (direction == 3 && column > 0 && _mazeCells[row, column - 1].visited) {
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column],MazeCell.Walls.West);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column - 1],MazeCell.Walls.East);
                wallDestroyed = true;
            }
            else if (direction == 4 && column < mazeColumn - 2 && _mazeCells[row, column + 1].visited) {
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column],MazeCell.Walls.East);
                _mazeCellManager.DestroyWallIfExists(_mazeCells[row, column + 1],MazeCell.Walls.West);
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
            _currentRow = (int) mazeCell.x;
            _currentColumn = (int) mazeCell.z;
            // Check tile north
            if (CellExists(_currentRow - 1, _currentColumn)
                && _mazeCells[_currentRow - 1, _currentColumn].hasSouthWall
                && mazeCell.hasNorthWall
            ) {
                _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.North);
            }

            // Check tile west
            if (CellExists(_currentRow, _currentColumn - 1)
                && _mazeCells[_currentRow, _currentColumn - 1].hasEastWall
                && mazeCell.hasWestWall
            ) {
                _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.West);
            }
        }
    }


    
    private void CreateLevelData() {
        List<MazeCell> mazeCells = new List<MazeCell>();
        foreach (MazeCell mazeCell in _mazeCells) {
            mazeCells.Add(mazeCell);
        }
        FileManager.SaveLevelDataFile(
            new LevelData(
                0, 
                false, 
                mazeCells, 
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
        foreach (MazeCell mazeCellData in levelData.mazeCells) {
            GameObject cellObject = InstantiateMazeCell( mazeCellData.x, mazeCellData.z);
            MazeCell mazeCell = cellObject.GetComponent<MazeCell>();
            
            if (!mazeCellData.hasEastWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.East);
            if (!mazeCellData.hasWestWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.West);
            if (!mazeCellData.hasNorthWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.North);
            if (!mazeCellData.hasSouthWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.South);
            if(mazeCellData.hasLight) _mazeCellManager.InstantiateLight(mazeCell);
            if (mazeCellData.isExit) {
                cellObject.transform.Find("Floor").GetComponent<Renderer>().material = exitMaterial;
                mazeCell.isExit = true;
            }
            mazeCell.action = mazeCellData.action;
            if (mazeCellData.hasArrow) {
                _mazeCellManager.InstantiateArrow(mazeCell);
            }

            mazeCell.permanentlyRevealed = mazeCellData.permanentlyRevealed;

            mazeCells[mazeCellData.x, mazeCellData.z] = mazeCell;
        }

        return mazeCells;
    }
    
    
}