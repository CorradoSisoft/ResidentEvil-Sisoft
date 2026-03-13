using UnityEngine;

public class BloodPool : MonoBehaviour
{
    public float maxScale = 1.25f;
    public float growSpeed = 1.5f;
    private bool growing = false;
    private MeshRenderer mr;

    public static Vector3 bloodOffset = new Vector3(0f, 0.01f, 0f);

    public static GameObject Create(Vector3 position, Material mat)
    {
        GameObject obj = new GameObject("BloodPool");
        // ...
        obj.transform.position = position + Vector3.up * 0.01f;
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.mesh = CreateCircleMesh(32);

        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = mat;
        Debug.Log($"Materiale assegnato: {mat.name}, colore: {mat.color}");

        obj.transform.position = position + Vector3.up * 0.01f;
        obj.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
        obj.transform.localScale = Vector3.zero;

        BloodPool bp = obj.AddComponent<BloodPool>();
        bp.mr = renderer;
        bp.growing = true;
        return obj;
    }

    static Mesh CreateCircleMesh(int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            float radius = Random.Range(0.7f, 1f); // ← irregolarità
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = (i + 2 > segments) ? 1 : i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void Update()
    {
        if (!growing) return;

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            Vector3.one * maxScale,
            growSpeed * Time.deltaTime
        );

        if (transform.localScale.x >= maxScale)
            growing = false;
    }
}