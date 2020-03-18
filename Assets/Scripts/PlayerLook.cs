using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLook : MonoBehaviour {
    [SerializeField] private string mouseXInputName, mouseYInputName;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform playerBody;

    private float _xAxisClamp;
    public GameManager gameManager;

    private void Awake () {
        LockCursor ();
        _xAxisClamp = 0.0f;
        if (SceneManager.GetActiveScene().name != "MenuScene") {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    private void LockCursor () {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update () {
        if (SceneManager.GetActiveScene().name != "MenuScene") {
            if(!gameManager.gameIsPaused) CameraRotation ();
        }
        else {
            CameraRotation();
        }
    }

    private void CameraRotation () {
        float mouseX = Input.GetAxis (mouseXInputName) * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis (mouseYInputName) * mouseSensitivity * Time.deltaTime;

        _xAxisClamp += mouseY;

        if (_xAxisClamp > 90.0f) {
            _xAxisClamp = 90.0f;
            mouseY = 0.0f;
            ClampXAxisRotationToValue (270.0f);
        } else if (_xAxisClamp < -90.0f) {
            _xAxisClamp = -90.0f;
            mouseY = 0.0f;
            ClampXAxisRotationToValue (90.0f);
        }

        transform.Rotate (Vector3.left * mouseY);
        playerBody.Rotate (Vector3.up * mouseX);
    }

    private void ClampXAxisRotationToValue (float value) {
        Transform transform1 = transform;
        Vector3 eulerRotation = transform1.eulerAngles;
        eulerRotation.x = value;
        transform1.eulerAngles = eulerRotation;
    }
}
