using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuButton : MonoBehaviour
{
    public Material defaultMaterial;
    public Material glowingMaterial;
    public MenuManager menuManager;
    // Start is called before the first frame update
    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }

    private void OnMouseDown()
    {
       menuManager.HandleClick(gameObject);
    }

    private void OnMouseExit() {
        if (transform.GetComponent<TextMeshPro>().color == Color.red) return;
        transform.GetComponent<TextMeshPro>().fontSize -= 5;
        transform.GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    private void OnMouseEnter() {
        if (transform.GetComponent<TextMeshPro>().color == Color.red) return;
        defaultMaterial = transform.GetComponent<MeshRenderer>().material;
        transform.GetComponent<TextMeshPro>().fontSize += 5;
        transform.GetComponent<MeshRenderer>().material = glowingMaterial;
    }
}
