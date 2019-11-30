using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    private TextMeshPro text;
    public GameObject player;
    private Animation anim;
    void Start()
    {
        Cursor.visible = true;
        anim = player.GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray forwardRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(forwardRay, out hit, 3))
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
                anim.Play("GoToOptions");
                break;
            case "BackButton" :
                anim.Play("OptionsToMain");
                break;
            default:
                break;

        }
    }
}
