using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RelativisticGridDeformer : MonoBehaviour
{
    [Header("Mass Settings")]
    public Transform massObject;
    public float schwarzschildRadius = 1f;
    public float curvatureScale = 1f; 
    public float effectRadius = 10f;

    private Mesh originalMesh;
    private Mesh deformedMesh;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        originalMesh = mf.mesh;

        deformedMesh = Instantiate(originalMesh);
        mf.mesh = deformedMesh;

        originalVertices = originalMesh.vertices;
        deformedVertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        if (massObject == null) return;

        Vector3 massPos = massObject.position;

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(originalVertices[i]);

            Vector2 dxz = new Vector2(worldPos.x - massPos.x, worldPos.z - massPos.z);
            float r = dxz.magnitude;

            float disp = 0f;
            if (r > schwarzschildRadius && r < effectRadius)
            {
                float zEmbed = 2.0f * Mathf.Sqrt(schwarzschildRadius * (r - schwarzschildRadius));
                disp = -curvatureScale * zEmbed;
            }

            Vector3 localPos = originalVertices[i];
            localPos.y += disp;

            deformedVertices[i] = localPos;
        }

        deformedMesh.vertices = deformedVertices;
        deformedMesh.RecalculateNormals();
    }
}
