using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button btn;

    void Start()
    {
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log("Button clicked!");
    }
}
