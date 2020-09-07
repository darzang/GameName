using TMPro;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    private void OnMouseExit() {
        transform.GetComponent<TextMeshProUGUI>().fontSize -= 5;
    }

    private void OnMouseEnter() {
        transform.GetComponent<TextMeshProUGUI>().fontSize += 5;
    }
}
