using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class GridDrawer : MonoBehaviour
{
    [Header("Grid")]
    public int gridSize = 20;
    public float spacing = 1f;
    public Color lineColor = Color.white;

    [Header("Masses (dynamic)")]
    public List<MassObject> massObjects = new List<MassObject>();

    [Header("Physics / Units")]
    public float G = 1f;
    public float c = 10f;
    public float unitScale = 1f;

    [Header("Visual control")]
    public float curvatureScale = 1f;
    public float anchorRadius = 20f;
    public float maxDisp = 10f;
    public float effectRadius = 50f;

    [Header("Camera / Render")]
    public Camera cam;
    public float renderDistance = 50f;

    Material lineMaterial;
    Vector3[,] deformedGrid;

    void OnEnable()
    {
        if (lineMaterial == null)
        {
            Shader sh = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(sh);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        deformedGrid = new Vector3[gridSize + 1, gridSize + 1];
    }

    void OnDisable()
    {
        if (lineMaterial)
            DestroyImmediate(lineMaterial);
    }

    public void AddMassObject(MassObject obj)
    {
        if (obj != null && !massObjects.Contains(obj))
            massObjects.Add(obj);
    }

    public void RemoveMassObject(MassObject obj)
    {
        if (massObjects.Contains(obj))
            massObjects.Remove(obj);
    }

    void UpdateDeformedGrid() // I AM GOING TO KILL MYSELF (it is the flamm paraboloid)
    {
        if (deformedGrid == null || deformedGrid.GetLength(0) != gridSize + 1 || deformedGrid.GetLength(1) != gridSize + 1)
            deformedGrid = new Vector3[gridSize + 1, gridSize + 1];

        if (cam == null)
            cam = Camera.main;

        float camX = Mathf.Floor(cam.transform.position.x / spacing) * spacing;
        float camZ = Mathf.Floor(cam.transform.position.z / spacing) * spacing;

        float half = gridSize * 0.5f;
        Vector3 origin = new Vector3(camX - half * spacing, 0f, camZ - half * spacing);

        for (int i = 0; i <= gridSize; i++)
        {
            for (int j = 0; j <= gridSize; j++)
            {
                Vector3 p = new Vector3(origin.x + i * spacing, 0f, origin.z + j * spacing);
                float disp = 0f;

                foreach (var massObj in massObjects)
                {
                    if (massObj == null) continue;

                    Vector3 massLocal = transform.InverseTransformPoint(massObj.transform.position);
                    float rs = 2f * G * Mathf.Max(1e-9f, massObj.mass) / (c * c) * unitScale;
                    float effR = Mathf.Max(0.0001f, rs * 25f);
                    float anchorR = rs * 10f;

                    Vector2 dxz = new Vector2(p.x - massLocal.x, p.z - massLocal.z);
                    float r = dxz.magnitude;

                    if (r < effR)
                    {
                        float safeR = Mathf.Max(r, rs + 1e-4f);
                        float z = 2f * Mathf.Sqrt(rs * (safeR - rs)); // jebane gowno
                        float rRef = Mathf.Max(safeR, anchorR);
                        float zRef = 2f * Mathf.Sqrt(rs * Mathf.Max(rRef - rs, 0f)); // ABSOLUTE CINEMA
                        float d = curvatureScale * (z - zRef);

                        disp += Mathf.Clamp(d, -maxDisp, maxDisp);
                    }
                }

                p.y += disp;
                deformedGrid[i, j] = p;
            }
        }
    }

    void OnRenderObject()
    {
        if (lineMaterial == null) return;

        UpdateDeformedGrid();

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        for (int j = 0; j <= gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                GL.Vertex(deformedGrid[i, j]);
                GL.Vertex(deformedGrid[i + 1, j]);
            }
        }

        for (int i = 0; i <= gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GL.Vertex(deformedGrid[i, j]);
                GL.Vertex(deformedGrid[i, j + 1]);
            }
        }

        GL.End();
        GL.PopMatrix();
    }
}
