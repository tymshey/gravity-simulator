using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectSpawnerUI : MonoBehaviour
{
    [Header("UI References")]
    public Button addObjectButton;
    public Button removeObjectButton;
    public Slider massSlider;
    public Slider densitySlider;
    public TMP_Text selectedMassText;
    public TMP_Text selectedDensityText;
    public TMP_Text selectedPositionText;
    public TMP_Text selectedVelocityText;
    public TMP_Text selectedSOIText;
    public Toggle trajectoryToggle;
    public Toggle soiVisualizationToggle;
    public Button clearTrajectoriesButton;

    [Header("Spawner")]
    public MassManager massManager;
    public GridDrawer gridDrawer;
    [SerializeField] private Transform universeParent;

    [Header("Spawn Settings")]
    public float spawnHeight = 2f;

    [Header("Simulation")]
    public OrbitalSimulator orbitalSimulator;
    public TrajectoryManager trajectoryManager;

    private MassObject selectedObject;

    void Start()
    {
        addObjectButton.onClick.AddListener(SpawnObject);

        if (removeObjectButton != null)
            removeObjectButton.onClick.AddListener(RemoveSelectedObject);

        if (clearTrajectoriesButton != null)
            clearTrajectoriesButton.onClick.AddListener(ClearTrajectories);

        massSlider.onValueChanged.AddListener(OnMassSliderChanged);
        densitySlider.onValueChanged.AddListener(OnDensitySliderChanged);

        massSlider.interactable = false;
        densitySlider.interactable = false;

        selectedMassText.text = "No object selected";
        selectedDensityText.text = "No object selected";
        selectedPositionText.text = "No object selected";
        if (selectedVelocityText != null)
            selectedVelocityText.text = "No object selected";
        if (selectedSOIText != null)
            selectedSOIText.text = "No object selected";

        if (soiVisualizationToggle != null)
        {
            soiVisualizationToggle.onValueChanged.AddListener(OnSOIVisualizationToggleChanged);
            soiVisualizationToggle.isOn = false; // default: no SOI
        }
    }

    void SpawnObject()
    {
        Vector3 spawnPos = GetCrosshairSpawnPosition();

        var obj = massManager.SpawnObject(spawnPos);
        if (obj != null)
        {
            if (universeParent != null)
                obj.transform.SetParent(universeParent);

            Debug.Log($"Spawned object: {obj.name} at position {obj.transform.position}");

            if (gridDrawer != null)
                gridDrawer.AddMassObject(obj);

            trajectoryManager.AddMassObject(obj);

            SelectObject(obj);
        }
        else
        {
            Debug.LogWarning("Failed to spawn object!");
        }
    }

    private void RemoveSelectedObject()
    {
        if (selectedObject != null)
        {
            Debug.Log($"Removing object: {selectedObject.name}");

            trajectoryManager.RemoveMassObject(selectedObject);

            if (orbitalSimulator != null)
                orbitalSimulator.RemoveMassObject(selectedObject);

            Destroy(selectedObject.gameObject);

            SelectObject(null);
        }
        else
        {
            Debug.Log("No object selected to remove");
        }
    }

    Vector3 GetCrosshairSpawnPosition()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Plane gridPlane = new Plane(Vector3.up, Vector3.zero);

        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            return hitPoint + Vector3.up * spawnHeight;
        }
        else
        {
            return cam.transform.position + cam.transform.forward * 10f;
        }
    }

    public void SelectObject(MassObject obj)
    {
        selectedObject = obj;

        if (selectedObject != null)
        {
            massSlider.interactable = true;
            densitySlider.interactable = true;

            massSlider.value = obj.mass;
            densitySlider.value = obj.density;
        }
        else
        {
            massSlider.interactable = false;
            densitySlider.interactable = false;
        }

        UpdateObjectText();
    }

    void OnMassSliderChanged(float value)
    {
        if (selectedObject != null)
        {
            selectedObject.mass = value;
            selectedObject.SetDensity(selectedObject.density);
            UpdateObjectText();
        }
    }

    void OnDensitySliderChanged(float value)
    {
        if (selectedObject != null)
        {
            selectedObject.SetDensity(value); // auto rescale
            UpdateObjectText();
        }
    }

    void UpdateObjectText()
    {
        if (selectedObject != null)
        {
            selectedMassText.text = $"Mass: {selectedObject.mass:F2}";
            selectedDensityText.text = $"Density: {selectedObject.density:F2}";
            selectedPositionText.text = $"Position: X:{selectedObject.transform.position.x:F2} " +
                                        $"Y:{selectedObject.transform.position.y:F2} " +
                                        $"Z:{selectedObject.transform.position.z:F2}";

            if (selectedVelocityText != null)
            {
                float speed = selectedObject.velocity.magnitude;
                selectedVelocityText.text = $"Velocity: {speed:F2}";
            }

            if (selectedSOIText != null && selectedObject != null)
            {
                string soiDisplay;

                if (float.IsPositiveInfinity(selectedObject.sphereOfInfluence))
                    soiDisplay = "Infinity";
                else if (float.IsNegativeInfinity(selectedObject.sphereOfInfluence))
                    soiDisplay = "-Infinity";
                else if (float.IsNaN(selectedObject.sphereOfInfluence))
                    soiDisplay = "NaN";
                else
                    soiDisplay = selectedObject.sphereOfInfluence.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

                selectedSOIText.text = "SOI Radius: " + soiDisplay;
            }
        }
        else
        {
            selectedMassText.text = "No object selected";
            selectedDensityText.text = "No object selected";
            selectedPositionText.text = "No object selected";
            if (selectedVelocityText != null)
                selectedVelocityText.text = "No object selected";
            if (selectedSOIText != null)
                selectedSOIText.text = "No object selected";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnObject();
        }
        
        if (selectedObject != null)
        {
            selectedPositionText.text = $"Position: X:{selectedObject.transform.position.x:F2} " +
                                        $"Y:{selectedObject.transform.position.y:F2} " +
                                        $"Z:{selectedObject.transform.position.z:F2}";

            if (selectedVelocityText != null)
            {
                float speed = selectedObject.velocity.magnitude;
                selectedVelocityText.text = $"Velocity: {speed:F2}";
            }

            if (selectedSOIText != null)
            {
                selectedSOIText.text = $"SOI Radius: {selectedObject.sphereOfInfluence:F2}";
            }
        }
    }

    public void OnTrajectoryToggleChanged(bool active)
    {
        if (trajectoryManager != null)
        {
            trajectoryManager.SetTrajectoriesActive(active);
            Debug.Log("Trajectory toggle: " + active);
        }
        else
        {
            Debug.LogWarning("TrajectoryManager not assigned!");
        }
    }

    private void OnSOIVisualizationToggleChanged(bool active)
    {
        foreach (var obj in massManager.objects)
        {
            obj.showSOI = active;
        }
    }

    public void ClearTrajectories()
    {
        if (trajectoryManager != null)
        {
            trajectoryManager.ClearAllTrajectories();
        }
        else
        {
            Debug.LogWarning("TrajectoryManager not assigned!");
        }
    }
}
