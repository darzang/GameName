using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    [SerializeField] private string horizontalInputName;
    [SerializeField] private string verticalInputName;
    private CharacterController charController;
    public float fuelCount;
    private Light playerLamp;
    public bool lockPlayer = false;
    public GameManager gameManager;

    private void Awake() {
        playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();
        if (SceneManager.GetActiveScene().name != "MenuScene") {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            fuelCount = gameManager.playerData.batteryMax;
        }
        else {
            playerLamp.enabled = true;
        }

        charController = GetComponent<CharacterController>();
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "MenuScene") return;
        if (Input.GetKeyUp("l")) fuelCount += 100;
        if (Input.GetKeyUp("k")) fuelCount -= 100;
        if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d")
            || Input.GetKey("up") || Input.GetKey("left") || Input.GetKey("right") || Input.GetKey("down")) {
            if (fuelCount > 0 && !lockPlayer && !gameManager.gameIsPaused) {
                PlayerMovement();
            }
        }

        if (playerLamp.enabled && !gameManager.gameIsPaused) {
            fuelCount -= gameManager.playerData.lightConsumption / 10 * (Time.deltaTime + 1);
        }
        // TODO: this should check fuel count and send to gameManager 


    }

    private void PlayerMovement() {
        float horizontalInput = Input.GetAxis(horizontalInputName) * gameManager.playerData.playerSpeed;
        float verticalInput = Input.GetAxis(verticalInputName) * gameManager.playerData.playerSpeed;
        Transform playerTransform = transform;
        Vector3 forwardMovement = playerTransform.forward * verticalInput;
        Vector3 rightMovement = playerTransform.right * horizontalInput;
        Vector3 totalMovement = forwardMovement + rightMovement;
        fuelCount -= gameManager.playerData.fuelComsumption * totalMovement.magnitude * (Time.deltaTime + 1);
        charController.SimpleMove(totalMovement);
    }
}