using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneGenerator : MonoBehaviour {
    public Transform gameManagerPrefab;
    public Transform tileManagerPrefab;
    public Transform uiManagerPrefab;
    public Transform fragmentManager;
    public Transform wallPrefab;
    public Transform obstaclePrefab;
    public Transform exitPrefab;
    public Transform floorPrefab;
    public Transform ceilingPrefab;
    public Transform lightPrefab;
    private GameObject walls;
    private GameObject obstacles;
    private GameObject floor;
    private GameObject ceiling;
    private GameObject lights;

    void Start() {
        Maps maps = GetComponent<Maps>();
        char[,] testMap = maps.Level1;
        walls = GameObject.Find("Environment").transform.Find("Walls").gameObject;
        obstacles = GameObject.Find("Environment").transform.Find("Obstacles").gameObject;
        floor = GameObject.Find("Environment").transform.Find("Floor").gameObject;
        ceiling = GameObject.Find("Environment").transform.Find("Ceiling").gameObject;
        lights = GameObject.Find("Environment").transform.Find("Lights").gameObject;
        CreateEnvironment(testMap);
        InstantiateManagers();
    }

    void InstantiateManagers() {
        Transform fragmentManagerObject = Instantiate(fragmentManager, Vector3.zero, Quaternion.identity);
        fragmentManagerObject.gameObject.name = "FragmentManager";
        Transform uiManagerObject = Instantiate(uiManagerPrefab, Vector3.zero, Quaternion.identity);
        uiManagerObject.gameObject.name = "UIManager";
        Transform tileManagerObject = Instantiate(tileManagerPrefab, Vector3.zero, Quaternion.identity);
        tileManagerObject.gameObject.name = "TileManager";
        Transform gameManagerObject = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);
        gameManagerObject.gameObject.name = "GameManager";    
    }

    // Update is called once per frame
    void CreateEnvironment(char[,] map) {
        Transform environmentPrefab;
        int totalCount = 0;
        for (int i = 0; i < map.GetLength(0); i++) {
            for (int j = 0; j < map.GetLength(1); j++) {
                float yAdjustment = 0;
                GameObject parent;
                switch (map[i, j]) {
                    case 'W':
                        environmentPrefab = wallPrefab;
                        parent = walls;
                        break;
                    case 'O':
                        environmentPrefab = obstaclePrefab;
                        parent = obstacles;
                        break;
                    case 'F':
                        environmentPrefab = floorPrefab;
                        parent = floor;
                        yAdjustment = -0.45f;
                        break;
                    case 'E':
                        environmentPrefab = exitPrefab;
                        parent = floor;
                        yAdjustment = -0.45f;
                        break;
                    default:
                        Debug.LogError("No prefab found ?!");
                        environmentPrefab = floorPrefab;
                        parent = floor;
                        break;
                }

                // Instantiate Element
                Vector3 position = new Vector3(i, 0 + yAdjustment, j);
                Transform environmentObject = Instantiate(environmentPrefab, position, Quaternion.identity);
                environmentObject.SetParent(parent.transform);
                environmentObject.gameObject.name = $"{(map[i, j] == 'E' ? "Exit" : map[i, j].ToString())}_{i}_{j}";
                // Instantiate Ceiling above
                Vector3 ceilingPosition = new Vector3(i, 0.5f, j);
                Transform ceilingObject = Instantiate(ceilingPrefab, ceilingPosition, Quaternion.identity);
                ceilingObject.SetParent(ceiling.transform);
                ceilingObject.gameObject.name = $"C_{i}_{j}";
                // Instantiate Light
                if (totalCount % 3 == 1) {
                    Vector3 lightPosition = new Vector3(i, 0.45f, j);
                    Transform lightObject = Instantiate(lightPrefab, lightPosition, Quaternion.identity);
                    lightObject.SetParent(lights.transform);
                    lightObject.gameObject.name = $"L_{i}_{j}";
                }

                totalCount++;
            }
        }
    }
}