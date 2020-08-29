using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    [SerializeField] private string horizontalInputName;
    [SerializeField] private string verticalInputName;
    private CharacterController _charController;
    public float fuelCount;
    private Light _playerLamp;
    public bool lockPlayer;
    public GameManager gameManager;
    private bool _doubleTap;
    private float _doubleTapTime;
    public bool isDead;

    private void Awake() {
        _playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();
        if (SceneManager.GetActiveScene().name != "MenuScene") {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            fuelCount = gameManager.playerData.batteryMax;
        } else {
            _playerLamp.enabled = true;
        }
        _charController = GetComponent<CharacterController>();
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "MenuScene") return;
        if (Input.GetKeyUp("k")) fuelCount -= 100;
        if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d")
            || Input.GetKey("up") || Input.GetKey("left") || Input.GetKey("right") || Input.GetKey("down")) {
            if (fuelCount > 0 && !lockPlayer && !gameManager.gameIsPaused) {
                PlayerMovement();
            }
        }
        if (fuelCount <= 0 && !isDead) {
            isDead = true;
        }
        if (Input.GetKeyUp("l") && _doubleTap) {
            if (Time.time - _doubleTapTime < 0.3f) {
                _doubleTapTime = 0f;
                fuelCount += 300;
                if (fuelCount > gameManager.playerData.batteryMax) fuelCount = gameManager.playerData.batteryMax;
            }
            _doubleTap = false;
        }

        if (Input.GetKeyUp("l") && !_doubleTap) {
            _doubleTap = true;
            _doubleTapTime = Time.time;
        }

        if (_playerLamp.enabled && !gameManager.gameIsPaused) {
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
        fuelCount -= gameManager.playerData.fuelConsumption * totalMovement.magnitude * (Time.deltaTime + 1);
        _charController.SimpleMove(totalMovement);
    }
}