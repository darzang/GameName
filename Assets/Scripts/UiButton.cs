using TMPro;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnMouseExit() {
        Debug.Log($"MouseExit {gameObject.name}");
        transform.GetComponent<TextMeshProUGUI>().fontSize -= 5;
    }

    private void OnMouseEnter() {
        Debug.Log($"MouseEnter {gameObject.name}");
        transform.GetComponent<TextMeshProUGUI>().fontSize += 5;
    }
}
