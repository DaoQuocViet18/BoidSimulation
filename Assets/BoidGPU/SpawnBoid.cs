using UnityEngine;

public class SpawnBoid : MonoBehaviour
{
    [SerializeField] private Material boidMaterial; // Material hỗ trợ GPU Instancing
    [SerializeField] private int boidCount = 1000;  // Số lượng boid

    [SerializeField] private Mesh boidMesh;         // Mesh cho boid (tam giác)
    private Matrix4x4[] matrices;                   // Ma trận chuyển đổi cho boid
    private MaterialPropertyBlock propertyBlock;    // Block tài nguyên bổ sung

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

        // Khởi tạo ma trận cho từng boid
        matrices = new Matrix4x4[boidCount];
        propertyBlock = new MaterialPropertyBlock();

        // Tạo ma trận cho từng boid
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-200f, 200f), Random.Range(-130f, 130f), 0); // Vị trí 2D
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f)); // Xoay 2D
            Vector3 scale = new Vector3(1f, 1f, 1f); // Kích thước của boid
            matrices[i] = Matrix4x4.TRS(position, rotation, scale); // Tạo ma trận chuyển đổi
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

        // Vẽ tất cả boids sử dụng GPU Instancing
        Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, matrices, boidCount, propertyBlock);
    }

    // Hàm tạo mesh cho một tam giác duy nhất
    Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();

        // Xác định 3 đỉnh cho một tam giác trong không gian 2D (chỉ sử dụng x và y)
        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(-0.5f, -0.5f, 0f); // Điểm dưới trái
        vertices[1] = new Vector3(0.5f, -0.5f, 0f);  // Điểm dưới phải
        vertices[2] = new Vector3(0f, 0.5f, 0f);    // Điểm trên

        // Hệ số thu nhỏ UV

        // Thêm thông tin UV cho các đỉnh
        Vector2[] uv = new Vector2[3];
        uv[0] = new Vector2(-1f, 0f);
        uv[1] = new Vector2(1f, 0f);
        uv[2] = new Vector2(1f,2f); // UV cho tam giác


        // Các chỉ số cho tam giác
        int[] triangles = new int[3];
        triangles[0] = 0; // Tam giác 1 (dùng các chỉ số đỉnh)
        triangles[1] = 1;
        triangles[2] = 2;

        // Cập nhật các chỉ số và các đỉnh cho mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv; // Gán UV cho mesh

        // Tính toán lại các ranh giới của mesh (bounding box)
        mesh.RecalculateBounds();

        return mesh;
    }

    // Hàm để setup camera nhìn boid
    void SetupCamera()
    {
        // Kiểm tra xem camera chính đã tồn tại chưa
        if (Camera.main == null)
        {
            Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 0, 10); // Vị trí camera ban đầu
            cam.orthographic = true;
            cam.orthographicSize = 130;
        }
        else
        {
            Camera.main.transform.position = new Vector3(0, 0, 10); // Vị trí camera ban đầu
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 130;
        }

        // Quay camera 180 độ trên trục Y
        Camera.main.transform.rotation = Quaternion.Euler(0, 180, 0); // Quay camera 180 độ theo trục Y
    }

}
