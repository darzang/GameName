using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    public GameObject ceiling;
    void Start()
    {
        Cursor.visible = true;
        anim = player.GetComponent<Animation>();
        ceiling.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

        Ray forwardRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(forwardRay, out hit, 10))
        {
            if (hit.collider.gameObject.tag == "MenuButton")
            {
                text = hit.collider.gameObject.GetComponentInChildren<TextMeshPro>();
                text.color = Color.blue;
            }
            else if (text)
            {
                text.color = Color.yellow;
            }
        }
    }

    public void HandleClick(GameObject button)
    {
        switch (button.name)
        {
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
        }
    }
}
