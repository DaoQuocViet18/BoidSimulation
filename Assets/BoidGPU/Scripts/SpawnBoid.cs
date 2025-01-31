using UnityEngine;

public struct BoidGPUVariable
{
    public Matrix4x4 matrix;  // Ma trận chuyển đổi cho boid
    public Vector3 direction; // Hướng di chuyển của boid
}

public class SpawnBoid : MonoBehaviour
{
    [SerializeField] private ListBoidGPUVariable boids;   // Danh sách boid
    [SerializeField] private Material boidMaterial;       // Material hỗ trợ GPU Instancing
    [SerializeField] private int boidCount = 1000;         // Số lượng boid

    [SerializeField] private Mesh boidMesh;               // Mesh cho boid (tam giác)
    private MaterialPropertyBlock propertyBlock;          // Block tài nguyên bổ sung

    void Start()
    {
        // Kiểm tra boidMaterial hợp lệ
        if (boidMaterial == null)
        {
            Debug.LogError("BoidMaterial không được gán!");
            return;
        }

        // Kiểm tra xem Material có hỗ trợ GPU Instancing không
        if (!boidMaterial.enableInstancing)
        {
            Debug.LogError("Material không hỗ trợ GPU Instancing. Hãy bật 'Enable GPU Instancing' trong Inspector.");
            return;
        }

        // Tạo mesh tam giác đơn
        boidMesh = CreateTriangleMesh();

        // Kiểm tra boidMesh hợp lệ
        if (boidMesh == null)
        {
            Debug.LogError("boidMesh không hợp lệ.");
            return;
        }

        // Xóa danh sách boid cũ (nếu có)
        if (boids.boidTransform.Count > 0)
            boids.boidTransform.Clear();

        // Khởi tạo ma trận cho từng boid
        propertyBlock = new MaterialPropertyBlock();

        // Tạo ma trận cho từng boid và lưu trữ vào boidTransform
        for (int i = 0; i < boidCount; i++)
        {
            // Tạo vị trí ngẫu nhiên cho boid
            Vector3 position = new Vector3(Random.Range(-200f, 200f), Random.Range(-130f, 130f), 0);
            // Tạo hướng di chuyển ngẫu nhiên cho boid
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;

            // Tạo ma trận cho boid
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            Vector3 scale = new Vector3(1f, 1f, 1f);
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

            // Tạo đối tượng BoidGPUVariable và lưu vào danh sách boidTransform
            BoidGPUVariable boidData = new BoidGPUVariable
            {
                matrix = matrix,   // Lưu ma trận chuyển đổi
                direction = direction // Lưu hướng di chuyển
            };

            // Thêm boidData vào danh sách boids
            boids.boidTransform.Add(boidData);
        }

        // Cài đặt màu sắc cho tất cả boids
        //propertyBlock.SetColor("_Color", Color.cyan);

        // Đặt camera nhìn toàn cảnh boid
        SetupCamera();
    }

    void Update()
    {
        // Kiểm tra xem mesh và material có hợp lệ không
        if (boidMesh == null || boidMaterial == null) return;

        // Cập nhật vị trí cho từng boid
        for (int i = 0; i < boidCount; i++)
        {
            // Lấy dữ liệu boid từ danh sách (lưu vào biến tạm)
            BoidGPUVariable boidData = boids.boidTransform[i];

            // Cập nhật vị trí (di chuyển boid theo hướng)
            Vector3 newPosition = boidData.matrix.MultiplyPoint(Vector3.zero) + boidData.direction * Time.deltaTime;

            // Cập nhật ma trận chuyển đổi với vị trí mới
            boidData.matrix = Matrix4x4.TRS(newPosition, Quaternion.identity, Vector3.one);

            // Cập nhật lại boid trong danh sách
            boids.boidTransform[i] = boidData;
        }

        // Tạo mảng ma trận để vẽ các boids (dùng GPU Instancing)
        Matrix4x4[] matrices = new Matrix4x4[boidCount];
        for (int i = 0; i < boidCount; i++)
        {
            matrices[i] = boids.boidTransform[i].matrix;
        }

        // Vẽ tất cả boids sử dụng GPU Instancing
        Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, matrices, boidCount, propertyBlock);
    }

    // Hàm tạo mesh cho một tam giác duy nhất
    Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();

        // Xác định 3 đỉnh cho một tam giác trong không gian 2D
        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(-1.25f, -1f, 0f); // Điểm dưới trái
        vertices[1] = new Vector3(1.25f, -1f, 0f);  // Điểm dưới phải
        vertices[2] = new Vector3(0f, 0.5f, 0f);    // Điểm trên

        // Thêm thông tin UV cho các đỉnh
        Vector2[] uv = new Vector2[3];
        uv[0] = new Vector2(-1f, 0f);
        uv[1] = new Vector2(1f, 0f);
        uv[2] = new Vector2(1f, 2f);

        // Các chỉ số cho tam giác
        int[] triangles = new int[3];
        triangles[0] = 0; // Tam giác 1 (dùng các chỉ số đỉnh)
        triangles[1] = 1;
        triangles[2] = 2;

        // Cập nhật các chỉ số và các đỉnh cho mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        // Tính toán lại các ranh giới của mesh (bounding box)
        mesh.RecalculateBounds();

        return mesh;
    }

    // Hàm để setup camera nhìn boid
    void SetupCamera()
    {
        if (Camera.main == null)
        {
            Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 0, 10);
            cam.orthographic = true;
            cam.orthographicSize = 130;
        }
        else
        {
            Camera.main.transform.position = new Vector3(0, 0, 10);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 130;
        }
        Camera.main.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}
