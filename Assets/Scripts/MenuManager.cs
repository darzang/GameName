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
    public GameObject batteryMaxText;
    public GameObject batteryUseText;
    public GameObject lightText;
    public GameObject cashTexts;

    private void Start() {
        playerData = FileManager.LoadPlayerDataFile();
        if (playerData == null) {
            playerData = new PlayerData();
            FileManager.SavePlayerDataFile(playerData);
        }
        SetSkillsText();
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
            }
            else if (text) {
                text.color = Color.yellow;
            }
        }
    }

    private void SetSkillsText() {
        // Set correct text for skills
        if (!batteryMaxText) {
            Debug.Log($"No batteryMaxText: {batteryMaxText}");
        }
        
        if (!batteryUseText) {
            Debug.Log($"No batteryUseText: {batteryUseText}");
        }
        
        if (!lightText) {
            Debug.Log($"No lightText: {lightText}");
        }

        switch (playerData.batteryMaxLevel) {
            case 0:
                batteryMaxText.GetComponent<TextMeshPro>().text = "[1][2][4]";
                break;
            case 1:
                batteryMaxText.GetComponent<TextMeshPro>().text = "[v][2][4]";
                break;
            case 2:
                batteryMaxText.GetComponent<TextMeshPro>().text = "[v][v][4]";
                break;
            case 3:
                batteryMaxText.GetComponent<TextMeshPro>().text = "[v][v][v]";
                GameObject.Find("BatteryMaxButton").GetComponent<TextMeshPro>().text = "MAXED";
                GameObject.Find("BatteryMaxButton").GetComponent<BoxCollider>().enabled = false;
                break;
        }

        switch (playerData.batteryUseLevel) {
            case 0:
                batteryUseText.GetComponent<TextMeshPro>().text = "[1][2][4]";
                break;
            case 1:
                batteryUseText.GetComponent<TextMeshPro>().text = "[v][2][4]";
                break;
            case 2:
                batteryUseText.GetComponent<TextMeshPro>().text = "[v][v][4]";
                break;
            case 3:
                batteryUseText.GetComponent<TextMeshPro>().text = "[v][v][v]";
                GameObject.Find("BatteryUseButton").GetComponent<TextMeshPro>().text = "MAXED";
                GameObject.Find("BatteryUseButton").GetComponent<BoxCollider>().enabled = false;
                break;
        }

        switch (playerData.lightLevel) {
            case 0:
                lightText.GetComponent<TextMeshPro>().text = "[1][2][4]";
                break;
            case 1:
                lightText.GetComponent<TextMeshPro>().text = "[v][2][4]";
                break;
            case 2:
                lightText.GetComponent<TextMeshPro>().text = "[v][v][4]";
                break;
            case 3:
                lightText.GetComponent<TextMeshPro>().text = "[v][v][v]";
                GameObject.Find("LightButton").GetComponent<TextMeshPro>().text = "MAXED";
                GameObject.Find("LightButton").GetComponent<BoxCollider>().enabled = false;
                break;
        }

        cashTexts.GetComponent<TextMeshPro>().text = $"COINS: {playerData.cash}";
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
            case "SkillsButton":
                anim.Play("MainToSkills");
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
            case "SkillsBackButton":
                anim.Play("SkillsToMain");
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
            case "BatteryMaxButton":
                HandleUpgrade("BatteryMax", playerData.batteryMaxLevel);
                break;
            case "BatteryUseButton":
                HandleUpgrade("BatteryUse", playerData.batteryUseLevel);
                break;
            case "LightButton":
                HandleUpgrade("Light", playerData.lightLevel);
                break;
            default:
                if (button.name.Contains("Level")) {
                    Debug.Log($"sub: {button.name.Substring(5,1)}");
                    Int32.TryParse(button.name.Substring(5,1), out int levelNumber);
                    if (levelNumber == 1) {
                        Int32.TryParse(button.name.Substring(5,2), out int level10);
                        if (level10 == 10) {
                            levelNumber = 10;
                        }
                    }
                    Debug.Log($"Clicked on button level {levelNumber}");

                    if (playerData.levelCompleted > levelNumber - 2) {
                        Debug.Log($"Loading level {levelNumber}");
                        SceneManager.LoadScene($"Level{levelNumber}");
                    }
                    else {
                        Debug.Log($"Can't load level {levelNumber}");
                    }
                }
                break;
        }
    }

    private void HandleUpgrade(string skill, int currentLevel) {
        int cost = 0;
        switch (currentLevel) {
            case 0:
                cost = 1;
                break;
            case 1:
                cost = 2;
                break;
            case 2:
                cost = 4;
                break;
        }

        if (playerData.cash >= cost) {
            Debug.Log($"Upgrade {skill} possible");
            switch (skill) {
                case "BatteryMax":
                    playerData.batteryMaxLevel += 1;
                    playerData.batteryMax += 100;
                    break;
                case "BatteryUse":
                    playerData.batteryUseLevel += 1;
                    playerData.fuelComsumption -= 0.1f;
                    break;
                case "Light":
                    playerData.lightLevel += 1;
                    playerData.lightMultiplier += 0.1f;
                    break;
            }

            playerData.cash -= cost;
            SetSkillsText();
            FileManager.SavePlayerDataFile(playerData);
        }
    }
}