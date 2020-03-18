using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuManager : MonoBehaviour {
    private TextMeshPro _text;
    public GameObject player;
    private Animation _anim;
    public GameObject ceiling;
    public PlayerData playerData;
    public GameObject batteryMaxText;
    public GameObject batteryUseText;
    public GameObject lightText;
    public GameObject cashTexts;
    public AudioClip openingAudio;
    private Light _playerLamp;
    private GameObject _eyeLids;
    private AudioSource _playerSoundsAudioSource;
    public Material levelNotAvailableMaterial;

    private void Start() {
        StartCoroutine(nameof(OpenEyes));
        InstantiatePlayerPrefs();

        ceiling.SetActive(true);
        _playerLamp = player.GetComponentInChildren<Light>();
        _playerSoundsAudioSource = player.transform.Find("SoundsAudioSource").GetComponent<AudioSource>();
        playerData = FileManager.LoadPlayerDataFile();
        if (playerData == null) {
            playerData = new PlayerData();
            FileManager.SavePlayerDataFile(playerData);
        }
        else {
            _playerLamp.range *= playerData.lightMultiplier;
            _playerLamp.intensity *= playerData.lightMultiplier;
            _playerLamp.spotAngle *= playerData.lightMultiplier;
        }

        _playerLamp.enabled = false;
        SetSkillsText();
        Debug.Log(JsonUtility.ToJson(playerData, true));
        Cursor.visible = false;
        HandleAvailableLevels();
    }

    private void HandleAvailableLevels() {
        int maxLevelAvailable = playerData.levelCompleted + 1;
        GameObject playMenu = GameObject.Find("PlayMenu").gameObject;
        for (int i = 0; i < playMenu.transform.childCount; i++) {
            // Filter PlayTitle and PlayBackButton
            GameObject child = playMenu.transform.GetChild(i).gameObject;
            if (!child.name.Contains("Level")) continue;
            int.TryParse(child.name.Substring(child.name.Length - 1, 1), out int levelNumber);
            if (levelNumber == 0) {
                levelNumber = 10;
            }

            if (levelNumber > maxLevelAvailable) {
                child.GetComponent<MeshRenderer>().material = levelNotAvailableMaterial;
            }
        }
    }

    private void InstantiatePlayerPrefs() {
        if (!PlayerPrefs.HasKey("EnableMusic")) {
            PlayerPrefs.SetInt("EnableMusic", 1);
            PlayerPrefs.SetInt("EnableSounds", 1);
        }
        else {
            if (PlayerPrefs.GetInt("EnableSounds") == 1) {
                GameObject.Find("OptionsSoundsButton").GetComponent<TextMeshPro>().text = "V";
            }
            else {
                GameObject.Find("OptionsSoundsButton").GetComponent<TextMeshPro>().text = "X";
                player.transform.Find("SoundsAudioSource").GetComponent<AudioSource>().enabled = false;
                SetFlickeringLightEnable(false);
            }

            if (PlayerPrefs.GetInt("EnableMusic") == 1) {
                GameObject.Find("OptionsMusicButton").GetComponent<TextMeshPro>().text = "V";
            }
            else {
                GameObject.Find("OptionsMusicButton").GetComponent<TextMeshPro>().text = "X";
                player.transform.Find("BackgroundAudioSource").GetComponent<AudioSource>().enabled = false;
            }
        }
    }

    private IEnumerator OpenEyes() {
        _anim = player.GetComponent<Animation>();
        _eyeLids = GameObject.Find("EyeLids").gameObject;
        _anim.Play("EyeLidOpen");
        yield return new WaitForSeconds(1);
        _eyeLids.SetActive(false);
        _playerLamp.enabled = true;
    }

    private IEnumerator CloseEyes(int levelNumber) {
        _eyeLids.SetActive(true);
        _playerLamp.enabled = false;
        _anim.Play("EyeLidClose");
        _playerSoundsAudioSource.PlayOneShot(openingAudio);
        yield return new WaitForSeconds(3f);
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
                _anim.Play("MainToOptions");
                break;
            case "CreditsButton":
                _anim.Play("MainToCredits");
                break;
            case "SkillsButton":
                _anim.Play("MainToSkills");
                break;
            case "PlayButton":
                _anim.Play("MainToPlay");
                break;
            case "HelpButton":
                _anim.Play("MainToHelp");
                break;
            case "ControlsButton":
                _anim.Play("MainToControls");
                break;
            case "QuitButton":
                Application.Quit(); // Doesn't work with Unity editor
                break;
            case "OptionsBackButton":
                _anim.Play("OptionsToMain");
                break;
            case "SkillsBackButton":
                _anim.Play("SkillsToMain");
                break;
            case "CreditsBackButton":
                _anim.Play("CreditsToMain");
                break;
            case "PlayBackButton":
                _anim.Play("PlayToMain");
                break;
            case "HelpBackButton":
                _anim.Play("HelpToMain");
                break;
            case "ControlsBackButton":
                _anim.Play("ControlsToMain");
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
            case "OptionsMusicButton":
                if (PlayerPrefs.GetInt("EnableMusic") == 1) {
                    PlayerPrefs.SetInt("EnableMusic", 0);
                    button.GetComponent<TextMeshPro>().text = "X";
                    player.transform.Find("BackgroundAudioSource").GetComponent<AudioSource>().enabled = false;
                }
                else {
                    PlayerPrefs.SetInt("EnableMusic", 1);
                    button.GetComponent<TextMeshPro>().text = "V";
                    player.transform.Find("BackgroundAudioSource").GetComponent<AudioSource>().enabled = true;
                }

                break;
            case "OptionsSoundsButton":
                if (PlayerPrefs.GetInt("EnableSounds") == 1) {
                    PlayerPrefs.SetInt("EnableSounds", 0);
                    button.GetComponent<TextMeshPro>().text = "X";
                    _playerSoundsAudioSource.enabled = false;
                    SetFlickeringLightEnable(false);
                }
                else {
                    PlayerPrefs.SetInt("EnableSounds", 1);
                    button.GetComponent<TextMeshPro>().text = "V";
                    _playerSoundsAudioSource.enabled = true;
                    SetFlickeringLightEnable(true);
                }
                break;
            default:
                if (button.name.Contains("Level")) {
                    int.TryParse(button.name.Substring(button.name.Length - 1, 1), out int levelNumber);
                    if (levelNumber == 0) {
                        levelNumber = 10;
                    }
                    if (playerData.levelCompleted > levelNumber - 2) {
                        Debug.Log($"Loading level {levelNumber}");
                        _eyeLids.SetActive(true);
                        _anim.Play("EyeLidClose");
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

        if (playerData.cash < cost) return;
        Debug.Log($"Upgrade {skill} possible");
        switch (skill) {
            case "BatteryMax":
                if (playerData.batteryMaxLevel < 3) {
                    // TODO: Make this clean
                    playerData.batteryMaxLevel += 1;
                    playerData.batteryMax += 100;
                }
                break;
            case "BatteryUse":
                if (playerData.batteryUseLevel < 3) {
                    playerData.batteryUseLevel += 1;
                    playerData.fuelConsumption -= 0.1f;
                }
                break;
            case "Light":
                if (playerData.lightLevel < 3) {
                    playerData.lightLevel += 1;
                    playerData.lightMultiplier += 0.1f;
                    _playerLamp.range = playerData.baseLightRange * playerData.lightMultiplier;
                    _playerLamp.intensity = playerData.baseLightIntensity * playerData.lightMultiplier;
                    _playerLamp.spotAngle = playerData.baseLightAngle * playerData.lightMultiplier;
                }
                break;
        }

        playerData.cash -= cost;
        SetSkillsText();
        FileManager.SavePlayerDataFile(playerData);
    }

    private void SetFlickeringLightEnable(bool state) {
        GameObject lights = GameObject.Find("Lights").gameObject;
        foreach (AudioSource lightAudio in lights.transform.GetComponentsInChildren<AudioSource>()) {
            lightAudio.enabled = state;
        }
    }
}