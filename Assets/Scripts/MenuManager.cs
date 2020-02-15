using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    public GameObject ceiling;
    private Camera mainCamera;
    public PlayerData playerData;
    public GameObject batteryMaxText;
    public GameObject batteryUseText;
    public GameObject lightText;
    public GameObject cashTexts;
    public AudioClip openingAudio;
    private Light playerLamp;
    private GameObject eyeLids;
    private AudioSource playerAudio;

    private bool initOver;

    async void Start() {
        StartCoroutine(nameof(OpenEyes));


        ceiling.SetActive(true);
        playerLamp = player.GetComponentInChildren<Light>();
        playerAudio = player.GetComponent<AudioSource>();
        playerData = FileManager.LoadPlayerDataFile();
        if (playerData == null) {
            playerData = new PlayerData();
            FileManager.SavePlayerDataFile(playerData);
            playerLamp.range = 2f;
            playerLamp.intensity = 3;
            playerLamp.spotAngle = 30;
        }
        else {
            playerLamp.range *= playerData.lightMultiplier;
            playerLamp.intensity *= playerData.lightMultiplier;
            playerLamp.spotAngle *= playerData.lightMultiplier;
        }

        playerLamp.enabled = false;
        SetSkillsText();
        Debug.Log(JsonUtility.ToJson(playerData, true));
        Cursor.visible = false;
        mainCamera = Camera.main;
        initOver = true;
    }

    private IEnumerator OpenEyes() {
        Debug.Log($"{Time.fixedTime} Calling Open ");
        anim = player.GetComponent<Animation>();
        eyeLids = player.transform.Find("EyeLids").gameObject;
        anim.Play("EyeLidOpen");
        yield return new WaitForSeconds(1);
        eyeLids.SetActive(false);
        playerLamp.enabled = true;
        Debug.Log($"{Time.fixedTime} Open Over ");
    }
    private IEnumerator CloseEyes(int levelNumber) {
        Debug.Log($"{Time.fixedTime} Calling Close ");
        eyeLids.SetActive(true);
        playerLamp.enabled = false;
        anim.Play("EyeLidClose");
        playerAudio.PlayOneShot(openingAudio);
        yield return new WaitForSeconds(3f);
        Debug.Log($"{Time.fixedTime} Close Over Close ");
        SceneManager.LoadScene($"Level{levelNumber}");
    }
    
    private void SetSkillsText() {
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
            case "BatteryMaxUpgradeButton":
                HandleUpgrade("BatteryMax", playerData.batteryMaxLevel);
                break;
            case "BatteryUseUpgradeButton":
                HandleUpgrade("BatteryUse", playerData.batteryUseLevel);
                break;
            case "LightUpgradeButton":
                HandleUpgrade("Light", playerData.lightLevel);
                break;
            case "ButtonForward":
                // TODO: change playerPrefs
            default:
                if (button.name.Contains("Level")) {
                    Debug.Log($"sub: {button.name.Substring(5, 1)}");
                    Int32.TryParse(button.name.Substring(5, 1), out int levelNumber);
                    if (levelNumber == 1) {
                        Int32.TryParse(button.name.Substring(5, 2), out int level10);
                        if (level10 == 10) {
                            levelNumber = 10;
                        }
                    }

                    Debug.Log($"Clicked on button level {levelNumber}");

                    if (playerData.levelCompleted > levelNumber - 2) {
                        Debug.Log($"Loading level {levelNumber}");
                        eyeLids.SetActive(true);
                        anim.Play("EyeLidClose");

                        StartCoroutine(CloseEyes(levelNumber));
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
                    playerLamp.range = 2f * playerData.lightMultiplier;
                    playerLamp.intensity = 3 * playerData.lightMultiplier;
                    playerLamp.spotAngle = 30 * playerData.lightMultiplier;
                    break;
            }

            playerData.cash -= cost;
            SetSkillsText();
            FileManager.SavePlayerDataFile(playerData);
        }
    }
}