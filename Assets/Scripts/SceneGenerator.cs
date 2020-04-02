using System.Collections;
using UnityEngine;
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
    private bool hideCeiling = false;

    private void Start() {
        _maze = GameObject.Find("Maze");
        _mazeCells = new MazeCell[mazeRow, mazeColumn];
        InstantiateMaze();
        HuntAndKill();
        InstantiateManagers();
    }

    private void InstantiateManagers() {
        Transform fragmentManagerObject = Instantiate(fragmentManager, Vector3.zero, Quaternion.identity);
        fragmentManagerObject.gameObject.name = "FragmentManager";
        Transform uiManagerObject = Instantiate(uiManagerPrefab, Vector3.zero, Quaternion.identity);
        uiManagerObject.gameObject.name = "UIManager";
        Transform tileManagerObject = Instantiate(tileManagerPrefab, Vector3.zero, Quaternion.identity);
        tileManagerObject.gameObject.name = "TileManager";
        tileManagerObject.GetComponent<TileManager>().SetMazeCells(_mazeCells, mazeRow, mazeColumn);
        Transform gameManagerObject = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);
        gameManagerObject.gameObject.name = "GameManager";
    }
    
    // Maze Generation related
    private GameObject InstantiateMazeCell(int row, int column) {
        Transform mazeCell = Instantiate(mazeCellPrefab, new Vector3(row, 0, column), Quaternion.identity,
            _maze.transform);
        mazeCell.gameObject.name = $"MazeCell_{row}_{column}";
        mazeCell.GetComponent<MazeCell>().ceiling.SetActive(!hideCeiling);
        return mazeCell.gameObject;
    }

    private void InstantiateMaze() {
        int index = 0;
        for (int r = 0; r < mazeRow; r++) {
            for (int c = 0; c < mazeColumn; c++) {
                _mazeCells[r, c] = InstantiateMazeCell(r, c).gameObject.GetComponent<MazeCell>();
                if (index % 3 == 0) {
                    Transform light = Instantiate(lightPrefab, _mazeCells[r, c].transform);
                    light.localPosition = new Vector3(0,0.9f,0);
                }
                index++;
            }
        }

        int x = Random.Range(0, _mazeCells.GetLength(0));
        int z = Random.Range(0, _mazeCells.GetLength(1));
        _mazeCells[x, z].isExit = true;
        _mazeCells[x, z].floor.GetComponent<Renderer>().material = exitMaterial;
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
                _mazeCells[_currentRow, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn].northWall);
                _mazeCells[_currentRow - 1, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow - 1, _currentColumn].southWall);
                _currentRow--;
            }
            else if (direction == 2 && CellIsAvailable(_currentRow + 1, _currentColumn)) {
                // Going south
                _mazeCells[_currentRow, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn].southWall);
                _mazeCells[_currentRow + 1, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow + 1, _currentColumn].northWall);
                _currentRow++;
            }
            else if (direction == 3 && CellIsAvailable(_currentRow, _currentColumn + 1)) {
                // Going east
                _mazeCells[_currentRow, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn].eastWall);
                _mazeCells[_currentRow, _currentColumn + 1]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn + 1].westWall);
                _currentColumn++;
            }
            else if (direction == 4 && CellIsAvailable(_currentRow, _currentColumn - 1)) {
                // Going west
                _mazeCells[_currentRow, _currentColumn]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn].westWall);
                _mazeCells[_currentRow, _currentColumn - 1]
                    .DestroyWallIfExists(_mazeCells[_currentRow, _currentColumn - 1].eastWall);
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
                _mazeCells[row, column].DestroyWallIfExists(_mazeCells[row, column].northWall);
                _mazeCells[row - 1, column].DestroyWallIfExists(_mazeCells[row - 1, column].southWall);
                wallDestroyed = true;
            }
            else if (direction == 2 && row < (mazeRow - 2) && _mazeCells[row + 1, column].visited) {
                _mazeCells[row, column].DestroyWallIfExists(_mazeCells[row, column].southWall);
                _mazeCells[row + 1, column].DestroyWallIfExists(_mazeCells[row + 1, column].northWall);
                wallDestroyed = true;
            }
            else if (direction == 3 && column > 0 && _mazeCells[row, column - 1].visited) {
                _mazeCells[row, column].DestroyWallIfExists(_mazeCells[row, column].westWall);
                _mazeCells[row, column - 1].DestroyWallIfExists(_mazeCells[row, column - 1].eastWall);
                wallDestroyed = true;
            }
            else if (direction == 4 && column < mazeColumn - 2 && _mazeCells[row, column + 1].visited) {
                _mazeCells[row, column].DestroyWallIfExists(_mazeCells[row, column].eastWall);
                _mazeCells[row, column + 1].DestroyWallIfExists(_mazeCells[row, column + 1].westWall);
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
                && _mazeCells[_currentRow - 1, _currentColumn].southWall
                && mazeCell.northWall
                ) {
                mazeCell.DestroyWallIfExists(mazeCell.northWall);
            }
            // Check tile west
            if (CellExists(_currentRow, _currentColumn-1)
                && _mazeCells[_currentRow, _currentColumn-1].eastWall
                && mazeCell.westWall
            ) {
                mazeCell.DestroyWallIfExists(mazeCell.westWall);
            }
        }
    }

}