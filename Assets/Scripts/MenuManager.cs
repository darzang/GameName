using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    public GameObject ceiling;
    void Start() {
        Cursor.visible = true;
        anim = player.GetComponent<Animation>();
        ceiling.SetActive(true);
    }

    void Update() {

        Ray forwardRay = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            default:
                Debug.LogError($"Case not covered {button.name}");
                break;
        }
    }
}
