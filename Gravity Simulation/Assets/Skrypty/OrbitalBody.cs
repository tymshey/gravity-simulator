using UnityEngine;

[System.Serializable]
public class OrbitalBody
{
    [Tooltip("Mass in arbitrary units (1 = 1e24 kg)")]
    public float mass = 1f;

    public Vector3 velocity;

    [HideInInspector] public float realMass; // kg
    [HideInInspector] public Vector3 acceleration;
    [HideInInspector] public Transform transform;

    const float MASS_SCALE = 1e24f;

    public void Initialize()
    {
        transform = transform ?? null;
        realMass = mass * MASS_SCALE;
    }
}
