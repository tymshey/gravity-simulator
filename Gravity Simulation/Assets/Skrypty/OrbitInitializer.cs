using System.Collections.Generic;
using UnityEngine;

public class OrbitInitializer : MonoBehaviour
{
    [Header("Central Body")]
    public MassObject centralBody;

    [Header("Orbit Settings")]
    public Vector3 orbitAxis = Vector3.up;

    [Header("Simulation Scale")]
    public float unitScale = 1e6f;   // 1 Unity unit = 1000000 m
    public double G = 6.67430e-11;   // gravitational constant
    public double massScale = 1e24;  // 1 mass unit = 1e24 kg

    private HashSet<MassObject> initializedSatellites = new HashSet<MassObject>();

    void Update()
    {
        if (centralBody == null) return;

        MassObject[] allSatellites = GameObject.FindObjectsOfType<MassObject>();
        foreach (var sat in allSatellites)
        {
            if (sat == centralBody) continue;

            if (!initializedSatellites.Contains(sat))
            {
                SetCircularOrbit(centralBody, sat, orbitAxis);
                initializedSatellites.Add(sat);
            }
        }
    }

    public void SetCircularOrbit(MassObject central, MassObject satellite, Vector3 axis)
    {
        Vector3 r = satellite.transform.position - central.transform.position;
        float distance = r.magnitude * unitScale;
        float centralMassKg = (float)(central.mass * massScale);

        float speed = Mathf.Sqrt((float)(G * centralMassKg / distance));

        Vector3 direction = Vector3.Cross(r.normalized, axis.normalized);

        satellite.velocity = direction * speed / unitScale;
    }
}
