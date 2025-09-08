using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectListUI : MonoBehaviour
{
    [Header("References")]
    public OrbitalSimulator simulator;
    public RectTransform contentPanel;
    public GameObject listItemPrefab;
    public MassSelector massSelector;

    [Header("Layout Settings")]
    public int padding = 10;
    public int spacing = 5;

    [Header("Refresh")]
    public float checkInterval = 0.25f;
    private float _nextCheckTime;

    private readonly List<GameObject> currentItems = new List<GameObject>();
    private readonly List<MassObject> _lastBodiesSnapshot = new List<MassObject>();
    private readonly List<MassObject> _tmpBodies = new List<MassObject>();

    void Awake()
    {
        if (massSelector == null)
            massSelector = FindObjectOfType<MassSelector>();
    }

    void Start()
    {
        var layout = contentPanel.GetComponent<VerticalLayoutGroup>() ?? contentPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        var fitter = contentPanel.GetComponent<ContentSizeFitter>() ?? contentPanel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ForceRefreshList();
    }

    void Update()
    {
        if (Time.unscaledTime >= _nextCheckTime)
        {
            _nextCheckTime = Time.unscaledTime + checkInterval;
            if (BodiesChanged())
                ForceRefreshList();
        }
    }

    bool BodiesChanged()
    {
        _tmpBodies.Clear();
        if (simulator == null) return false;

        var bodies = simulator.GetBodies();
        if (bodies == null) return false;

        foreach (var b in bodies)
            if (b != null) _tmpBodies.Add(b);

        if (_tmpBodies.Count != _lastBodiesSnapshot.Count) return true;

        for (int i = 0; i < _tmpBodies.Count; i++)
            if (!_lastBodiesSnapshot.Contains(_tmpBodies[i]))
                return true;

        return false;
    }

    void ForceRefreshList()
    {
        foreach (var item in currentItems)
            if (item != null) Destroy(item);
        currentItems.Clear();
        _lastBodiesSnapshot.Clear();

        if (simulator == null)
        {
            Debug.LogWarning("[ObjectListUI] Simulator not assigned.");
            return;
        }

        var bodies = simulator.GetBodies();
        if (bodies == null) return;

        foreach (var body in bodies)
        {
            if (body == null) continue;
            _lastBodiesSnapshot.Add(body);

            GameObject newItem = Instantiate(listItemPrefab, contentPanel);
            newItem.transform.localScale = Vector3.one;

            var btn = newItem.GetComponent<Button>() ?? newItem.AddComponent<Button>();
            var img = newItem.GetComponent<Image>() ?? newItem.AddComponent<Image>();
            img.raycastTarget = true;
            if (btn.targetGraphic == null) btn.targetGraphic = img;
            if (img.color.a < 0.01f) img.color = new Color(1, 1, 1, 0.05f);

            TMP_Text label = newItem.GetComponentInChildren<TMP_Text>() ?? newItem.GetComponent<TMP_Text>();
            if (label != null) label.text = $"{body.name} (mass: {body.mass:F2})";

            MassObject captured = body;
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[ObjectListUI] Clicked {captured?.name}");
                if (massSelector == null)
                {
                    Debug.LogError("[ObjectListUI] MassSelector not assigned/found; cannot select.");
                    return;
                }
                if (captured == null)
                {
                    Debug.LogWarning("[ObjectListUI] Referenced MassObject was destroyed.");
                    return;
                }
                massSelector.SelectObject(captured);
            });

            currentItems.Add(newItem);
        }
    }

    public void MarkDirtyAndRefreshNow()
    {
        _nextCheckTime = 0f;
        ForceRefreshList();
    }
}
