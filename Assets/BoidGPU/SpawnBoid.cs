using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private Mesh boidMesh;       // Mesh hình chữ nhật (Quad)
    [SerializeField] private Material boidMaterial; // Chất liệu có hỗ trợ GPU Instancing
    [SerializeField] private int boidCount = 1000;  // Số lượng boid

    private Matrix4x4[] matrices;                // Ma trận chuyển đổi
    private MaterialPropertyBlock propertyBlock; // Block tài nguyên bổ sung

    void Start()
    {
        // Kiểm tra boidMesh và boidMaterial hợp lệ
        if (boidMesh == null || boidMaterial == null)
        {
            Debug.LogError("BoidMesh hoặc BoidMaterial không được gán!");
            return;
        }

        // Kiểm tra xem Material có hỗ trợ GPU Instancing không
        if (!boidMaterial.enableInstancing)
        {
            Debug.LogError("Material không hỗ trợ GPU Instancing. Hãy bật 'Enable GPU Instancing' trong Inspector.");
            return;
        }

        // Khởi tạo
        matrices = new Matrix4x4[boidCount];
        propertyBlock = new MaterialPropertyBlock();

        // Tạo ma trận cho từng boid
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-100f, 100f), Random.Range(-60f, 60f), 0); // Vị trí 2D
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f)); // Xoay 2D
            Vector3 scale = new Vector3(1f, 1f, 1f); // Kích thước của boid
            matrices[i] = Matrix4x4.TRS(position, rotation, scale); // Tạo ma trận chuyển đổi
        }

        // Cài đặt màu sắc cho tất cả boids
        propertyBlock.SetColor("_Color", Color.cyan);

        //// Đặt camera nhìn toàn cảnh boid
        //Camera.main.transform.position = new Vector3(0, 0, -10); // Camera nhìn từ xa
        //Camera.main.orthographic = true; // Sử dụng camera kiểu chiếu trực giao
        //Camera.main.orthographicSize = 60; // Thiết lập phạm vi nhìn
    }

    void Update()
    {
        // Kiểm tra xem mesh và material có hợp lệ không
        if (boidMesh == null || boidMaterial == null) return;

        // Vẽ tất cả boids sử dụng GPU Instancing
        Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, matrices, boidCount, propertyBlock);
    }
}
