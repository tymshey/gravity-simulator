using UnityEngine;
using UnityEngine.UI;

public class TimeWarpUI : MonoBehaviour
{
    public OrbitalSimulator simulator;

    [Header("Time Warp Buttons")]
    public Button button1x;
    public Button button10x;
    public Button button100x;

    void Start()
    {
        button1x.onClick.AddListener(() => simulator.SetTimeScale(1f));
        button10x.onClick.AddListener(() => simulator.SetTimeScale(10f));
        button100x.onClick.AddListener(() => simulator.SetTimeScale(100f));
    }
}