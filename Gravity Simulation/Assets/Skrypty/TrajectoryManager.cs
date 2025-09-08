using System.Collections.Generic;
using UnityEngine;

public class TrajectoryManager : MonoBehaviour
{
    private Dictionary<MassObject, List<Vector3>> paths = new Dictionary<MassObject, List<Vector3>>();
    private Material lineMaterial;
    private bool active = true;

    void Awake()
    {
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
    }

    public void AddMassObject(MassObject obj)
    {
        if (!paths.ContainsKey(obj))
            paths[obj] = new List<Vector3>();
    }

    public void RemoveMassObject(MassObject obj)
    {
        if (paths.ContainsKey(obj))
            paths.Remove(obj);
    }

    public void RecordPosition(MassObject obj, Vector3 position)
    {
        if (!active || obj == null) return;

        if (!paths.ContainsKey(obj))
            paths[obj] = new List<Vector3>();

        paths[obj].Add(position);
    }

    public void SetTrajectoriesActive(bool state)
    {
        active = state;
    }

    public void ClearAllTrajectories()
    {
        paths.Clear();
        Debug.Log("All trajectories cleared.");
    }

    void OnRenderObject()
    {
        if (!active) return;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);

        foreach (var kvp in paths)
        {
            var points = kvp.Value;
            for (int i = 1; i < points.Count; i++)
            {
                GL.Vertex(points[i - 1]);
                GL.Vertex(points[i]);
            }
        }

        GL.End();
        GL.PopMatrix();
    }
}