﻿using TMPro;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public Material defaultMaterial;
    public Material glowingMaterial;
    public MenuManager menuManager;
    public Material levelNotAvailableMaterial;
    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }

    private void OnMouseDown()
    {
        menuManager.HandleClick(gameObject);
    }

    private void OnMouseExit()
    {
        if (transform.GetComponent<TextMeshPro>().material == levelNotAvailableMaterial) return;
        transform.GetComponent<MeshRenderer>().material = defaultMaterial;
        if (!gameObject.name.Contains("ButtonLevel"))
        {
            transform.GetComponent<TextMeshPro>().fontSize -= 5;
        }
    }

    private void OnMouseEnter()
    {
        if (transform.GetComponent<TextMeshPro>().material == levelNotAvailableMaterial) return;
        defaultMaterial = transform.GetComponent<MeshRenderer>().material;
        transform.GetComponent<MeshRenderer>().material = glowingMaterial;
        if (!gameObject.name.Contains("ButtonLevel"))
        {
            transform.GetComponent<TextMeshPro>().fontSize += 5;
        }
    }
}
