using UnityEngine;

public struct BoidGPUTransform
{
    public Matrix4x4 matrix;  // Ma trận chuyển đổi cho boid
    public Vector3 direction; // Hướng di chuyển của boid
}

public struct BoidGPUMaterial
{
    public Material boidMaterial;       // Material hỗ trợ GPU Instancing
    public Mesh boidMesh;               // Mesh cho boid (tam giác)
    public MaterialPropertyBlock propertyBlock;          // Block tài nguyên bổ sung
}

public class SpawnBoid : MonoBehaviour
{
    [SerializeField] private ListBoidGPUVariable boids;   // Danh sách boid
    [SerializeField] private int boidCount = 1000;        // Số lượng boid
    [SerializeField] private Material boidMaterial;      // Material cho boid
    private Mesh boidMesh;              // Mesh cho boid (cần khởi tạo)
    private MaterialPropertyBlock propertyBlock;          // Block tài nguyên bổ sung

    void Start()
    {
        if (boidMaterial == null)
        {
            Debug.LogError("BoidMaterial không được gán!");
            return;
        }

        if (!boidMaterial.enableInstancing)
        {
            Debug.LogError("Material không hỗ trợ GPU Instancing. Hãy bật 'Enable GPU Instancing' trong Inspector.");
            return;
        }

        // Kiểm tra xem boidMesh đã được gán chưa, nếu chưa thì tạo một Mesh mặc định
        if (boidMesh == null)
        {
            boidMesh = CreateTriangleMesh(); // Tạo một mesh tam giác nếu boidMesh chưa được gán
            if (boidMesh == null)
            {
                Debug.LogError("boidMesh không hợp lệ.");
                return;
            }
        }

        // Nếu danh sách boid đã có phần tử, chúng ta sẽ clear để tạo lại
        if (boids.boidTransform.Count > 0)
            boids.boidTransform.Clear();

        // Khởi tạo MaterialPropertyBlock
        propertyBlock = new MaterialPropertyBlock();

        // Tạo các boid mới
        for (int i = 0; i < boidCount; i++)
        {
            // Tạo vị trí ngẫu nhiên cho boid
            Vector3 position = new Vector3(Random.Range(-80f, 80f), Random.Range(-40f, 40f), 0);
            Quaternion rotation = Quaternion.Euler(180f, 0, Random.Range(0f, 360f));
            Vector3 direction;

            // **Lấy hướng phía trước dựa trên rotation**
            if (Mathf.Abs(rotation.z) > 120 && Mathf.Abs(rotation.z) < 300f)
                direction = rotation * Vector3.left; // Lấy trục Y làm hướng phía trước
            else
                direction = rotation * Vector3.right; // Lấy trục Y làm hướng phía trước

            // Tạo ma trận cho boid
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

            // Lưu trữ dữ liệu boid
            BoidGPUTransform boidData_1 = new BoidGPUTransform
            {
                matrix = matrix,
                direction = direction,
            };

            boids.boidTransform.Add(boidData_1);
        }

        // Lưu trữ dữ liệu boid
        BoidGPUMaterial boidData_2 = new BoidGPUMaterial
        {
            boidMaterial = boidMaterial,
            boidMesh = boidMesh,
            propertyBlock = propertyBlock
        };

        boids.boidMaterial.Add(boidData_2);

        // Thiết lập camera (nếu cần)
        //SetupCamera();
    }

    //void Update()
    //{
    //    if (boidMesh == null || boidMaterial == null) return;

    //    for (int i = 0; i < boidCount; i++)
    //    {
    //        BoidGPUVariable boidData = boids.boidVariable[i];

    //        Vector3 newPosition = boidData.matrix.MultiplyPoint(Vector3.zero) + boidData.direction * Time.deltaTime;

    //        // **Giữ nguyên rotation ban đầu**
    //        Quaternion rotation = boidData.matrix.rotation;

    //        boidData.matrix = Matrix4x4.TRS(newPosition, rotation, Vector3.one);
    //        boids.boidVariable[i] = boidData;
    //    }

    //    Matrix4x4[] matrices = new Matrix4x4[boidCount];
    //    for (int i = 0; i < boidCount; i++)
    //    {
    //        matrices[i] = boids.boidVariable[i].matrix;
    //    }

    //    Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, matrices, boidCount, propertyBlock);
    //}

    Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(-1.25f, -1f, 0f);
        vertices[1] = new Vector3(1.25f, -1f, 0f);
        vertices[2] = new Vector3(0f, 0.5f, 0f);

        Vector2[] uv = new Vector2[3];
        uv[0] = new Vector2(-0.75f, 0f);
        uv[1] = new Vector2(1.25f, 0f);
        uv[2] = new Vector2(1f, 1.5f);

        int[] triangles = new int[3];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateBounds();

        return mesh;
    }

    void SetupCamera()
    {
        if (Camera.main == null)
        {
            Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 0, 10);
            cam.orthographic = true;
            //cam.orthographicSize = 130;
        }
        else
        {
            Camera.main.transform.position = new Vector3(0, 0, 10);
            Camera.main.orthographic = true;
            //Camera.main.orthographicSize = 130;
        }
        Camera.main.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}
