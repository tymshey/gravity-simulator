using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float updateInterval = 0.25f; // co ile sekund akutalizacja
    private float accumulatedTime = 0f;
    private int frames = 0;
    private float fps = 0f;

    void Update()
    {
        accumulatedTime += Time.unscaledDeltaTime;
        frames++;

        if (accumulatedTime >= updateInterval)
        {
            fps = frames / accumulatedTime;

            // reset
            frames = 0;
            accumulatedTime = 0f;
        }
    }

    void OnGUI()
    {
        int width = Screen.width, height = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(225, 10, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string text = string.Format("FPS: {0:0.}", fps);
        GUI.Label(rect, text, style);
    }
}
