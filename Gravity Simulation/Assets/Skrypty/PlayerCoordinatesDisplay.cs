using UnityEngine;

public class PlayerCoordinatesDisplay : MonoBehaviour
{
    public Transform playerTransform;

    void OnGUI()
    {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(225, 40, 300, 25), $"X: {pos.x:F2}", style);
        GUI.Label(new Rect(225, 65, 300, 25), $"Y: {pos.y:F2}", style);
        GUI.Label(new Rect(225, 90, 300, 25), $"Z: {pos.z:F2}", style);
    }
}