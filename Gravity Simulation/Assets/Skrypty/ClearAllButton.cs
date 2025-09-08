using UnityEngine;
using UnityEngine.UI;

public class ClearAllButton : MonoBehaviour
{
    [SerializeField] private OrbitalSimulator simulator;
    [SerializeField] private TrajectoryManager trajectoryManager;
    [SerializeField] private Button clearButton;

    void Awake()
    {
        if (clearButton == null)
            clearButton = GetComponent<Button>();
    }

    void Start()
    {
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearAllObjects);

        if (simulator == null)
            simulator = FindObjectOfType<OrbitalSimulator>();
        if (trajectoryManager == null)
            trajectoryManager = FindObjectOfType<TrajectoryManager>();
    }

    void ClearAllObjects()
    {
        Debug.Log("[ClearAllButton] Clearing everything…");

        if (trajectoryManager != null)
        {
            trajectoryManager.ClearAllTrajectories();
        }
        else
        {
            Debug.LogWarning("[ClearAllButton] TrajectoryManager not assigned!");
        }

        if (simulator != null)
        {
            simulator.ClearAllBodies();
        }
        else
        {
            Debug.LogWarning("[ClearAllButton] Simulator not assigned!");
        }
    }
}