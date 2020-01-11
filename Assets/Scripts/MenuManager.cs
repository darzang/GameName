using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    public GameObject ceiling;
    private Camera mainCamera;
    public GameObject easyWrapper;
    public GameObject mediumWrapper;
    public GameObject hardWrapper;
    private void Start() {
        Cursor.visible = true;
        anim = player.GetComponent<Animation>();
        ceiling.SetActive(true);
        mainCamera = Camera.main;
        if (PlayerPrefs.GetString("Difficulty") == "") {
            mediumWrapper.SetActive(true);
        }
        else {
            switch (PlayerPrefs.GetString("Difficulty")) {
                case "Easy":
                    easyWrapper.SetActive(true);
                    break;
                case "Medium":
                    mediumWrapper.SetActive(true);
                    break;
                case "Hard":
                    hardWrapper.SetActive(true);
                    break;
            }
        }
    }

    private void Update() {

        Ray forwardRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, 10)) {
            if (hit.collider.gameObject.CompareTag("MenuButton")) {
                text = hit.collider.gameObject.GetComponentInChildren<TextMeshPro>();
                text.color = Color.blue;
            } else if (text) {
                text.color = Color.yellow;
            }
        }
    }

    public void HandleClick(GameObject button) {
        Debug.Log($"Clicked on {button.name}");
        switch (button.name) {
            case "OptionsButton":
                anim.Play("MainToOptions");
                break;
            case "CreditsButton":
                anim.Play("MainToCredits");
                break;
            case "PlayButton":
                anim.Play("MainToPlay");
                break;
            case "QuitButton":
                Application.Quit(); // Doesn't work with Unity editor
                break;
            case "OptionsBackButton":
                anim.Play("OptionsToMain");
                break;
            case "CreditsBackButton":
                anim.Play("CreditsToMain");
                break;
            case "PlayBackButton":
                anim.Play("PlayToMain");
                break;
            case "Level1Button":
                SceneManager.LoadScene("Level1");
                break;
            case "Level2Button" :
                SceneManager.LoadScene("Level2");
                break;
            case "Level3Button" :
                SceneManager.LoadScene("Level3");
                break;
            case "EasyButton" :
                easyWrapper.SetActive(true);
                mediumWrapper.SetActive(false);
                hardWrapper.SetActive(false);
                PlayerPrefs.SetString("Difficulty", "Easy");
                Debug.Log($"Difficulty set to {PlayerPrefs.GetString("Difficulty")}");
                break;
            case "MediumButton" :
                easyWrapper.SetActive(false);
                mediumWrapper.SetActive(true);
                hardWrapper.SetActive(false);
                PlayerPrefs.SetString("Difficulty", "Medium");
                Debug.Log($"Difficulty set to {PlayerPrefs.GetString("Difficulty")}");
                break;
            case "HardButton" :
                easyWrapper.SetActive(false);
                mediumWrapper.SetActive(false);
                hardWrapper.SetActive(true);
                PlayerPrefs.SetString("Difficulty", "Hard");
                Debug.Log($"Difficulty set to {PlayerPrefs.GetString("Difficulty")}");
                break;
            default:
                Debug.LogError($"Case not covered {button.name}");
                break;
        }
    }
}
