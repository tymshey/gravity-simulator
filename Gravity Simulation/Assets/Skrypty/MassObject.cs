using UnityEngine;
using System.Collections.Generic;

public class MassObject : MonoBehaviour
{
    [Header("Physical properties")]
    public float mass = 1f;
    public float density = 1f;

    [Header("Simulation")]
    public Vector3 velocity = Vector3.zero; // m/s

    [Header("Sphere of Influence (auto-calculated)")]
    [HideInInspector] public float centralMass;
    [HideInInspector] public float distanceToCentralBody;
    [HideInInspector] public float sphereOfInfluence;

    [Header("SOI Visualization")]
    public Material soiMaterial;
    public GameObject soiSphere;
    private Material runtimeMat;

    [HideInInspector] public bool showSOI = false;

    private Vector3 originalScale;

    private static List<MassObject> allBodies = new List<MassObject>();

    void Awake()
    {
        allBodies.Add(this);

        originalScale = transform.localScale;
        UpdateScale();

        showSOI = false;

        CreateSOISphere();
        UpdateSOI();
        UpdateSOISphere();
    }

    void OnDestroy()
    {
        allBodies.Remove(this);
    }

    void Update()
    {
        FindCentralBody();
        UpdateSOI();
        UpdateSOISphere();
    }

    public void SetDensity(float newDensity)
    {
        density = Mathf.Max(0.01f, newDensity);
        UpdateScale();
    }

    private void UpdateScale()
    {
        float targetVolume = mass / density;
        float currentVolume = originalScale.x * originalScale.y * originalScale.z;
        float scaleFactor = Mathf.Pow(targetVolume / currentVolume, 1f / 3f);
        transform.localScale = originalScale * scaleFactor;
    }

    private void FindCentralBody()
    {
        MassObject bestCandidate = null;
        float bestDist = float.MaxValue;

        foreach (var body in allBodies)
        {
            if (body == this) continue;
            if (body.mass <= mass) continue;

            float dist = Vector3.Distance(transform.position, body.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestCandidate = body;
            }
        }

        if (bestCandidate != null)
        {
            centralMass = bestCandidate.mass;
            distanceToCentralBody = bestDist;
        }
        else
        {
            centralMass = 0f;
            distanceToCentralBody = 0f;
        }
    }

    private void UpdateSOI()
    {
        if (centralMass > 0f && distanceToCentralBody > 0f)
        {
            sphereOfInfluence = distanceToCentralBody * Mathf.Pow(mass / (3f * centralMass), 1f / 3f);
        }
        else
        {
            sphereOfInfluence = float.PositiveInfinity;
        }
    }

    private void CreateSOISphere()
    {
        soiSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        soiSphere.name = gameObject.name + "_SOI";

        soiSphere.transform.position = transform.position;
        Destroy(soiSphere.GetComponent<Collider>());

        Renderer r = soiSphere.GetComponent<Renderer>();
        if (soiMaterial != null)
            r.material = Instantiate(soiMaterial);
        else
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 1f, 1f, 0.2f);
            mat.SetFloat("_Mode", 3);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            r.material = mat;
        }

        soiSphere.SetActive(false);
    }

    private void UpdateSOISphere()
    {
        if (soiSphere != null)
        {
            soiSphere.transform.position = transform.position;

            bool shouldShow = showSOI && !float.IsPositiveInfinity(sphereOfInfluence);

            soiSphere.SetActive(shouldShow);

            if (shouldShow)
            {
                float diameter = sphereOfInfluence * 2f;
                soiSphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            }
        }
    }
}
