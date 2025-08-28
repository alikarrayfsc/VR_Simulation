using UnityEngine;

[ExecuteAlways]
public class RobotCenterOfMass : MonoBehaviour
{
    public Rigidbody rb;
    public float steelDensity = 7850f; // kg/m³

    [Header("Collected Colliders (auto-filled)")]
    public Collider[] colliders;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        CollectColliders();
    }

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        CollectColliders();
        Recalculate();
    }

    [ContextMenu("Collect Colliders")]
    void CollectColliders()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    [ContextMenu("Recalculate Center of Mass")]
    public void Recalculate()
    {
        if (!rb || colliders == null || colliders.Length == 0) return;

        double totalMass = 0;
        Vector3 weighted = Vector3.zero;

        foreach (var c in colliders)
        {
            if (!c || c.isTrigger) continue;

            double vol = 0;
            Vector3 center = c.bounds.center;

            if (c is BoxCollider box)
            {
                Vector3 size = Vector3.Scale(box.size, Abs(box.transform.lossyScale));
                vol = size.x * size.y * size.z;
                center = box.transform.TransformPoint(box.center);
            }
            else if (c is WheelCollider wheel)
            {
                float r = wheel.radius * Mathf.Max(wheel.transform.lossyScale.x, wheel.transform.lossyScale.z);
                float w = wheel.suspensionDistance * Mathf.Abs(wheel.transform.lossyScale.y);
                vol = Mathf.PI * r * r * w; // cylinder approx
                center = wheel.transform.position;
            }
            else if (c is MeshCollider mesh)
            {
                Vector3 s = mesh.bounds.size;
                vol = Mathf.Abs(s.x * s.y * s.z); // rough
                center = mesh.bounds.center;
            }

            double m = vol * steelDensity;
            totalMass += m;
            weighted += (float)m * center;
        }

        if (totalMass <= 0) return;

        Vector3 worldCoM = weighted / (float)totalMass;
        rb.centerOfMass = rb.transform.InverseTransformPoint(worldCoM);

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    void OnDrawGizmosSelected()
    {
        if (!rb) return;
        Vector3 world = rb.transform.TransformPoint(rb.centerOfMass);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(world, 0.05f);
        Debug.Log($"World CoM: {world}");
        Debug.Log($"Local CoM: {rb.centerOfMass}");
    }
       
}