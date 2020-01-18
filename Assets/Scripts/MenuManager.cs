using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    public GameObject ceiling;
    private Camera mainCamera;
    public GameObject helpText1;
    public GameObject helpText2;
    public GameObject helpNextButton;
    public GameObject helpPreviousButton;
    public PlayerData playerData;
    private int levelMax;
    private void Start() {
        playerData= FileManager.LoadPlayerDataFile();
        if (playerData == null) {
            playerData = new PlayerData(1,800,1,1,1,1,0,false, 10);
            FileManager.SavePlayerDataFile(playerData);
        }
        Debug.Log(JsonUtility.ToJson(playerData, true));
        Cursor.visible = false;
        anim = player.GetComponent<Animation>();
        ceiling.SetActive(true);
        mainCamera = Camera.main;
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
            case "HelpButton":
                anim.Play("MainToHelp");
                break;
            case "ControlsButton":
                anim.Play("MainToControls");
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
            case "ControlsBackButton":
                anim.Play("ControlsToMain");
                break;
            case "HelpNextButton":
                helpText1.SetActive(false);
                helpText2.SetActive(true);
                helpNextButton.SetActive(false);
                helpPreviousButton.SetActive(true);
                break;
            case "HelpPreviousButton":
                helpText1.SetActive(true);
                helpText2.SetActive(false);
                helpNextButton.SetActive(true);
                helpPreviousButton.SetActive(false);
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
            default:
                Debug.LogError($"Case not covered {button.name}");
                break;
        }
    }
}
