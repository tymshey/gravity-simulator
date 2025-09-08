using UnityEngine;
using UnityEngine.UI;

public class GridToggle : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Toggle drawGridToggle;
    public GameObject gridObject;

    void Start()
    {
        if (drawGridToggle != null)
        {
            gridObject.SetActive(drawGridToggle.isOn);

            drawGridToggle.onValueChanged.AddListener(OnDrawGridChanged);
        }
    }

    void OnDrawGridChanged(bool isOn)
    {
        if (gridObject != null)
        {
            gridObject.SetActive(isOn);
        }
    }
}