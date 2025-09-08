using UnityEngine;
using System.Collections.Generic;

public class MassManager : MonoBehaviour
{
    public MassObject massObjectPrefab;
    public List<MassObject> objects = new List<MassObject>();

    void Awake()
    {
        MassObject[] existing = FindObjectsOfType<MassObject>();
        foreach (var obj in existing)
        {
            if (!objects.Contains(obj))
                objects.Add(obj);
        }
    }

    public MassObject SpawnObject(Vector3 position)
    {
        if (massObjectPrefab == null)
        {
            Debug.LogWarning("MassObject prefab not assigned!");
            return null;
        }

        MassObject obj = Instantiate(massObjectPrefab, position, Quaternion.identity);
        obj.name = "Object " + objects.Count;
        objects.Add(obj);
        return obj;
    }

    public void RemoveObject(MassObject obj)
    {
        if (objects.Contains(obj))
            objects.Remove(obj);
    }
}
