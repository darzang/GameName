using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SceneGenerator : MonoBehaviour {
    public Transform gameManagerPrefab;
    public Transform uiManagerPrefab;
    public Transform fragmentManager;
    public Material exitMaterial;
    public Transform mazeCellPrefab;
    private GameObject _maze;
    private int _currentRow = 0;
    private int _currentColumn = 0;
    private bool _courseComplete = false;
    private bool hideCeiling = true;
    private GameObject _arrows;
    public Transform arrowPrefab;

    private MazeCellManager _mazeCellManager;
    private void Start() {
        _maze = GameObject.Find("Maze");
        _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
        _mazeCellManager.mazeCells = GenerateMazeIfExists();
        if (_mazeCellManager.mazeCells == null) {
            _mazeCellManager.mazeCells = new List<MazeCell>();
            Debug.Log($"Generating new maze for {SceneManager.GetActiveScene().name}");
            InstantiateMaze();
            HuntAndKill();
            CreateLevelData();
        }
        Debug.Log($"mazeCells in mazeCellManagers Before managers: {_mazeCellManager.mazeCells.Count}");
        InstantiateManagers();
        Debug.Log($"mazeCells in mazeCellManagers After managers: {_mazeCellManager.mazeCells.Count}");
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
    private MazeCell InstantiateMazeCell(int row, int column) {
        Transform mazeCellObject = Instantiate(mazeCellPrefab, new Vector3(row, 0, column), Quaternion.identity,
            _maze.transform);
        mazeCellObject.gameObject.name = $"MazeCell_{row}_{column}";
        MazeCell mazeCell = new MazeCell {
            score = _mazeCellManager.mazeRow * _mazeCellManager.mazeColumn,
            x = row,
            z = column,
            fragmentNumber = 0,
            name = $"MazeCell_{row}_{column}"
        };
        _mazeCellManager.GetWall(mazeCell, MazeCell.Walls.Ceiling).SetActive(!hideCeiling);
        return mazeCell;
    }

    private void InstantiateMaze() {
        int index = 0;
        int exitX = Random.Range(0, _mazeCellManager.GetMapSize()-1);
        int exitZ = Random.Range(0, _mazeCellManager.GetMapSize()-1);
        for (int r = 0; r < _mazeCellManager.mazeRow; r++) {
            for (int c = 0; c < _mazeCellManager.mazeColumn; c++) {
                MazeCell newCell = InstantiateMazeCell(r, c);

                if (index % 3 == 0) {
                    _mazeCellManager.InstantiateLight(newCell);
                }

                if (r == exitX && c == exitZ) {
                    newCell.isExit = true;
                    _mazeCellManager.GetWall(newCell, MazeCell.Walls.Floor)
                            .GetComponent<Renderer>().material = exitMaterial;
                }
                _mazeCellManager.mazeCells.Add(newCell);
                index++;
            }
        }
    }
    
    private void HuntAndKill() {
        _mazeCellManager.GetCellIfExists(_currentRow, _currentColumn).visited = true;
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
            MazeCell currentCell = _mazeCellManager.GetCellIfExists(_currentRow, _currentColumn);
            // _mazeCells[_currentRow, _currentColumn].SetFloorColor(Color.blue);
            if (direction == 1 && CellIsAvailable(_currentRow - 1, _currentColumn)) {
                // Going north, so destroy current north wall + north cell's south wall
                _mazeCellManager.DestroyWallIfExists(currentCell, MazeCell.Walls.North);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(_currentRow -1, _currentColumn), MazeCell.Walls.South); 
                _currentRow--;
            }
            else if (direction == 2 && CellIsAvailable(_currentRow + 1, _currentColumn)) {
                // Going south
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.South);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(_currentRow +1, _currentColumn),MazeCell.Walls.North);
                _currentRow++;
            }
            else if (direction == 3 && CellIsAvailable(_currentRow, _currentColumn + 1)) {
                // Going east
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.East);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(_currentRow, _currentColumn +1),MazeCell.Walls.West);
                _currentColumn++;
            }
            else if (direction == 4 && CellIsAvailable(_currentRow, _currentColumn - 1)) {
                // Going west
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.West);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(_currentRow, _currentColumn-1),MazeCell.Walls.East);
                _currentColumn--;
            }
            currentCell.visited = true;
        }
    }

    private void Hunt() {
        _courseComplete = true;
        for (int r = 0; r < _mazeCellManager.mazeRow; r++) {
            for (int c = 0; c < _mazeCellManager.mazeColumn; c++) {
                if (!_mazeCellManager.GetCellIfExists(r, c).visited && CellHasAnAdjacentVisitedCell(r, c)) {
                    _courseComplete = false;
                    _currentRow = r;
                    _currentColumn = c;
                    DestroyRandomWall(_currentRow, _currentColumn);
                    _mazeCellManager.GetCellIfExists(r, c).visited = true;
                    return;
                }
            }
        }
    }

    private bool CellIsAvailable(int row, int column) {
        return row >= 0 && row < _mazeCellManager.mazeRow && column >= 0 && column < _mazeCellManager.mazeColumn && !_mazeCellManager.GetCellIfExists(row, column).visited;
    }


    private bool RouteStillAvailable(int row, int column) {
        return (row > 0 && !_mazeCellManager.GetCellIfExists(row-1, column).visited)
               || (row < _mazeCellManager.mazeRow - 1 && !_mazeCellManager.GetCellIfExists(row + 1, column).visited)
               || (column > 0 && !_mazeCellManager.GetCellIfExists(row, column - 1).visited)
               || (column < _mazeCellManager.mazeColumn - 1 && !_mazeCellManager.GetCellIfExists(row, column + 1).visited);
    }

    private bool CellHasAnAdjacentVisitedCell(int row, int column) {
        return (row > 0 && _mazeCellManager.GetCellIfExists(row - 1, column).visited)
               || (row < (_mazeCellManager.mazeRow - 2) && _mazeCellManager.GetCellIfExists(row + 1, column).visited)
               || (column > 0 && _mazeCellManager.GetCellIfExists(row, column - 1).visited)
               || (column < (_mazeCellManager.mazeColumn - 2) && _mazeCellManager.GetCellIfExists(row, column + 1).visited);
    }

    private void DestroyRandomWall(int row, int column) {
        bool wallDestroyed = false;

        while (!wallDestroyed) {
            int direction = Random.Range(1, 5);
            MazeCell currentCell = _mazeCellManager.GetCellIfExists(row, column);
            if (direction == 1 && row > 0 && _mazeCellManager.GetCellIfExists(row - 1, column).visited) {
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.North);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(row - 1, column),MazeCell.Walls.South);
                wallDestroyed = true;
            }
            else if (direction == 2 && row < (_mazeCellManager.mazeRow - 2) && _mazeCellManager.GetCellIfExists(row + 1, column).visited) {
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.South);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(row + 1, column),MazeCell.Walls.North);
                wallDestroyed = true;
            }
            else if (direction == 3 && column > 0 && _mazeCellManager.GetCellIfExists(row, column - 1).visited) {
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.West);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(row, column - 1),MazeCell.Walls.East);
                wallDestroyed = true;
            }
            else if (direction == 4 && column < _mazeCellManager.mazeColumn - 2 && _mazeCellManager.GetCellIfExists(row, column + 1).visited) {
                _mazeCellManager.DestroyWallIfExists(currentCell,MazeCell.Walls.East);
                _mazeCellManager.DestroyWallIfExists(_mazeCellManager.GetCellIfExists(row, column + 1),MazeCell.Walls.West);
                wallDestroyed = true;
            }
        }
    }

    private bool CellExists(float row, float column) {
        return row >= 0 && row < _mazeCellManager.mazeRow && column >= 0 && column < _mazeCellManager.mazeColumn;
    }

    private void DestroyDoubleWalls() {
        // Destroy double walls where two adjacent cells share a common wall
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            _currentRow =  mazeCell.x;
            _currentColumn =  mazeCell.z;
            // Check tile north
            if (CellExists(_currentRow - 1, _currentColumn)
                && _mazeCellManager.GetCellIfExists(_currentRow - 1, _currentColumn).hasSouthWall
                && mazeCell.hasNorthWall
            ) {
                _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.North);
            }

            // Check tile west
            if (CellExists(_currentRow, _currentColumn - 1)
                && _mazeCellManager.GetCellIfExists(_currentRow, _currentColumn - 1).hasEastWall
                && mazeCell.hasWestWall
            ) {
                _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.West);
            }
        }
    }


    
    private void CreateLevelData() {
        List<MazeCell> mazeCells = new List<MazeCell>();
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            mazeCells.Add(mazeCell);
        }
        FileManager.SaveLevelDataFile(
            new LevelData(
                0, 
                false, 
                mazeCells, 
                _mazeCellManager.mazeRow, 
                _mazeCellManager.mazeColumn
                ),
            SceneManager.GetActiveScene().name);
    }

    private List<MazeCell> GenerateMazeIfExists() {
        LevelData levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);

        if (levelData == null) return null;
        Debug.Log($"Generating existing maze for {SceneManager.GetActiveScene().name}");
        List<MazeCell> mazeCells = new List<MazeCell>();
        foreach (MazeCell mazeCellData in levelData.mazeCells) {
            MazeCell mazeCell = InstantiateMazeCell( mazeCellData.x, mazeCellData.z);
            
            if (!mazeCellData.hasEastWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.East);
            if (!mazeCellData.hasWestWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.West);
            if (!mazeCellData.hasNorthWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.North);
            if (!mazeCellData.hasSouthWall) _mazeCellManager.DestroyWallIfExists(mazeCell, MazeCell.Walls.South);
            if(mazeCellData.hasLight) _mazeCellManager.InstantiateLight(mazeCell);
            if (mazeCellData.isExit) {
                _mazeCellManager.GetWall(mazeCell, MazeCell.Walls.Floor).GetComponent<Renderer>().material = exitMaterial;
                mazeCell.isExit = true;
            }
            mazeCell.action = mazeCellData.action;
            if (mazeCellData.hasArrow) {
                _mazeCellManager.InstantiateArrow(mazeCell);
            }

            mazeCell.x = mazeCellData.x;
            mazeCell.z = mazeCellData.z;
            mazeCell.name = mazeCellData.name;

            mazeCell.permanentlyRevealed = mazeCellData.permanentlyRevealed;
            _mazeCellManager.mazeCells.Add(mazeCell);
        }

        return _mazeCellManager.mazeCells;
    }
    
    
}