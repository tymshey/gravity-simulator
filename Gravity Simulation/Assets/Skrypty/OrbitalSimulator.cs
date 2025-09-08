using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrbitalSimulator : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float G = 6.67430e-11f; // gravitational constant
    public float timeStep = 3600f; // 1 hour per step
    public float timeScale = 1f; // 1x normal speed
    public float unitScale = 1e6f; // 1 unity unit = 1000 km

    [Header("Control")]
    public bool paused = false;
    public TrajectoryManager trajectoryManager;

    [Header("UI")]
    public TMP_Text pausedText;

    private List<MassObject> bodies = new List<MassObject>();
    private HashSet<MassObject> trackedBodies = new HashSet<MassObject>();

    public List<MassObject> GetBodies()
    {
        return bodies;
    }

    public void SetTimeScale(float factor)
    {
        timeStep = 3600f * factor;
        Debug.Log($"Time warp set to {factor}x");
    }

    void FixedUpdate()
    {
        if (paused) return;

        UpdateBodyList();

        if (bodies.Count < 2) return;

        LeapfrogStep();

        foreach (var body in bodies)
        {
            if (body != null)
                trajectoryManager.RecordPosition(body, body.transform.position);
        }
    }

    void UpdateBodyList()
    {
        MassObject[] allObjects = GameObject.FindObjectsOfType<MassObject>();

        foreach (var obj in allObjects)
        {
            if (!trackedBodies.Contains(obj))
            {
                trackedBodies.Add(obj);
                bodies.Add(obj);
            }
        }
    }

    public void RemoveMassObject(MassObject obj)
    {
        if (obj == null) return;

        if (bodies.Contains(obj))
            bodies.Remove(obj);

        if (trackedBodies.Contains(obj))
            trackedBodies.Remove(obj);
    }

    public void ClearAllBodies()
    {
        for (int i = bodies.Count - 1; i >= 0; i--)
        {
            if (bodies[i] != null)
                Destroy(bodies[i].gameObject);
        }
        bodies.Clear();

        Debug.Log("[OrbitalSimulator] Cleared all objects.");
    }

    void LeapfrogStep()
    {
        if (bodies.Count < 2) return;

        int n = bodies.Count;
        Vector3[] velocities = new Vector3[n];
        Vector3[] accelerations = new Vector3[n];
        double[] realMasses = new double[n];

        for (int i = 0; i < bodies.Count; i++)
        {
            var b = bodies[i];
            if (b == null) continue; // ABSOLUTE CINEMA

            velocities[i] = b.velocity;
            realMasses[i] = b.mass * 1e24;
        }

        for (int i = 0; i < bodies.Count; i++)
        {
            var bi = bodies[i];
            if (bi == null) continue;
            Vector3 pos_i = bi.transform.position * unitScale;

            for (int j = i + 1; j < bodies.Count; j++)
            {
                var bj = bodies[j];
                if (bj == null) continue;
                Vector3 pos_j = bj.transform.position * unitScale;

                Vector3 dir = pos_j - pos_i;
                float distSqr = dir.sqrMagnitude + 1e-12f;
                double force = G * realMasses[i] * realMasses[j] / distSqr;

                Vector3 acc_i = dir.normalized * (float)(force / realMasses[i]);
                Vector3 acc_j = -dir.normalized * (float)(force / realMasses[j]);

                accelerations[i] += acc_i;
                accelerations[j] += acc_j;
            }
        }

        float halfDt = timeStep * 0.5f * timeScale;

        for (int i = 0; i < bodies.Count; i++)
        {
            var b = bodies[i];
            if (b == null) continue;

            velocities[i] += accelerations[i] * halfDt;

            Vector3 pos = b.transform.position * unitScale;
            pos += velocities[i] * timeStep;
            b.transform.position = pos / unitScale;

            accelerations[i] = Vector3.zero;
        }

        for (int i = 0; i < bodies.Count; i++)
        {
            var bi = bodies[i];
            if (bi == null) continue;
            Vector3 pos_i = bi.transform.position * unitScale;

            for (int j = i + 1; j < bodies.Count; j++)
            {
                var bj = bodies[j];
                if (bj == null) continue;
                Vector3 pos_j = bj.transform.position * unitScale;

                Vector3 dir = pos_j - pos_i;
                float distSqr = dir.sqrMagnitude + 1e-12f;
                double force = G * realMasses[i] * realMasses[j] / distSqr;

                Vector3 acc_i = dir.normalized * (float)(force / realMasses[i]);
                Vector3 acc_j = -dir.normalized * (float)(force / realMasses[j]);

                accelerations[i] += acc_i;
                accelerations[j] += acc_j;
            }
        }

        for (int i = 0; i < bodies.Count; i++)
        {
            var b = bodies[i];
            if (b == null) continue;

            velocities[i] += accelerations[i] * halfDt;
            b.velocity = velocities[i];
        }
    }

    public void TogglePause()
    {
        paused = !paused;
        Debug.Log(paused ? "Simulation Paused" : "Simulation Playing");

        if (pausedText != null)
            pausedText.enabled = paused;
    }

    void Start()
    {
        if (pausedText != null)
            pausedText.enabled = false;
    }
}
