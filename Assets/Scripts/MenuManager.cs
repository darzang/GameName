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
    public GameObject helpText1;
    public GameObject helpText2;
    public GameObject controlsText;
    public GameObject helpNextButton;
    public GameObject helpPreviousButton;
    public GameObject level1Check;
    public GameObject level2Check;
    public GameObject level3Check;
    private int levelMax;
    private void Start() {
        levelMax = GetLevelMax();
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

    public int GetLevelMax() {
        int totalScenes = SceneManager.sceneCountInBuildSettings - 1;
        int levelMax = 1;
        for (int i = 0; i < totalScenes; i++) {
            GameData gameData = GameDataManager.LoadFile($"Level{i + 1}");
            if (gameData == null) continue;
            if (gameData.levelOver) levelMax += 1;
        }
        if(levelMax >= 2) level1Check.SetActive(true);
        if(levelMax >= 3) level2Check.SetActive(true);
        if(levelMax >= 4) level3Check.SetActive(true);
        return levelMax;
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
            case "HelpButton":
                anim.Play("MainToHelp");
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
            case "HelpBackButton":
                anim.Play("HelpToMain");
                break;
            case "HelpNextButton":
                if (helpText2.activeSelf) {
                    helpText2.SetActive(false);
                    controlsText.SetActive(true);
                    helpNextButton.SetActive(false);
                    helpPreviousButton.SetActive(true);
                }
                else {
                    helpText1.SetActive(false);
                    helpText2.SetActive(true);
                    helpNextButton.SetActive(true);
                    helpPreviousButton.SetActive(true);
                }

                break;
            case "HelpPreviousButton":
                if (helpText2.activeSelf) {
                    helpText1.SetActive(true);
                    helpText2.SetActive(false);
                    helpNextButton.SetActive(true);
                    helpPreviousButton.SetActive(false);
                }
                else {
                    controlsText.SetActive(false);
                    helpText2.SetActive(true);
                    helpNextButton.SetActive(false);
                    helpPreviousButton.SetActive(true);
                }
                break;
            case "Level1Button":
                SceneManager.LoadScene("Level1");
                break;
            case "Level2Button" :
                if(levelMax > 1) SceneManager.LoadScene("Level2");
                break;
            case "Level3Button" :
                if(levelMax > 2) SceneManager.LoadScene("Level3");
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
