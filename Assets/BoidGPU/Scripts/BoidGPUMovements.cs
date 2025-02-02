using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class BoidGPUMovements : MonoBehaviour
{
    [SerializeField] private ListBoidGPUVariable boids;
    [SerializeField] private QuadBounds quadBounds;
    [SerializeField] private NativeQuadTree<float2> quadTree;
    private NativeArray<Matrix4x4> matrices;  // Đổi tên biến từ matrix -> matrices
    private NativeArray<float2> velocities;

    private float radius = 2f;
    private float forwardSpeed = 5f;
    private float visionAngle = 270f;
    private float turnSpeed = 10f;

    private void Start()
    {
        int boidCount = boids.boidTransform.Count;  // Kiểm tra danh sách boidVariable có hợp lệ không
        if (boidCount == 0)
        {
            Debug.LogError("Không có Boid nào trong danh sách!");
            return;
        }

        quadTree = new NativeQuadTree<float2>(quadBounds, Allocator.Persistent, maxDepth: 8, maxLeafElements: 256);

        // Khởi tạo NativeArray
        matrices = new NativeArray<Matrix4x4>(boidCount, Allocator.Persistent);
        velocities = new NativeArray<float2>(boidCount, Allocator.Persistent);

        for (int i = 0; i < boidCount; i++)
        {
            matrices[i] = boids.boidTransform[i].matrix;  // Lưu ma trận transform của boid
            velocities[i] = (Vector2)boids.boidTransform[i].direction;  // Lưu hướng di chuyển của boid
        }
    }

    private void Update()
    {
        UpdateQuadTree();
        ApplyRule();
    }
    private void UpdateQuadTree()
    {
        int boidCount = boids.boidTransform.Count;
        if (boidCount == 0) return;

        var forward = new NativeArray<QuadElement<float2>>(boidCount, Allocator.TempJob);

        var updateQuadElementJob = new UpdateQuadElementJob
        {
            matrices = matrices,  // Truyền mảng ma trận transform của boid
            forward = forward
        };

        var updateQuadElementJobHandle = updateQuadElementJob.Schedule(boidCount, 64); // Chạy song song
        updateQuadElementJobHandle.Complete();

        //Debug.Log("boidCount: " + boidCount);
        //Debug.Log("forward: " + forward);
        quadTree.ClearAndBulkInsert(forward);
        forward.Dispose();
    }

    private void ApplyRule()
    {
        // Tạo job BoidMovementsJob và truyền tham số vào job
        var boidMovementsJob = new BoidMovementsJob
        {
            quadTree = quadTree,
            velocities = velocities,
            matrices = matrices,
            quadBounds = quadBounds,
            turnSpeed = turnSpeed,
            forwardSpeed = forwardSpeed,
            radius = radius,
            visionAngle = visionAngle,
            deltaTime = Time.deltaTime,
        };

        // Sử dụng NativeArray<Matrix4x4> như một tham số cho job
        JobHandle boidMovementsJobHandle = boidMovementsJob.Schedule(matrices.Length, 64);  // Chỉ số 64 là số lượng công việc tối đa cho mỗi lần xử lý (có thể thay đổi tùy nhu cầu)
        boidMovementsJobHandle.Complete();  // Đảm bảo job được hoàn thành

        // Vẽ tất cả các boid bằng Graphics.DrawMeshInstanced
        if (boids.boidMaterial[0].boidMesh != null && boids.boidMaterial[0].boidMaterial != null)
        {
            Graphics.DrawMeshInstanced(boids.boidMaterial[0].boidMesh, 0, boids.boidMaterial[0].boidMaterial, matrices.ToArray(), matrices.Length, boids.boidMaterial[0].propertyBlock);
        }
    }


    private void OnDrawGizmosSelected() => quadTree.DrawGizmos(quadBounds);
    private void OnDestroy()
    {
        matrices.Dispose();
        quadTree.Dispose();
        velocities.Dispose();
    }

    [BurstCompile]
    private struct UpdateQuadElementJob : IJobParallelFor
    {
        public NativeArray<Matrix4x4> matrices; // Mảng ma trận của boid
        public NativeArray<QuadElement<float2>> forward;

        public void Execute(int index)
        {
            Vector4 column3 = matrices[index].GetColumn(3); // Lấy cột 3 (vị trí)
            float2 position = new float2(column3.x, column3.y); // Chuyển thành float2 từ Vector4

            float3 dir = matrices[index].MultiplyVector(Vector3.right); // Lấy hướng phía trước

            forward[index] = new QuadElement<float2>
            {
                position = position,
                element = dir.xy
            };
        }
    }


    [BurstCompile]
    private struct BoidMovementsJob : IJobParallelFor
    {
        public NativeQuadTree<float2> quadTree;
        public NativeArray<float2> velocities;
        public NativeArray<Matrix4x4> matrices;
        public QuadBounds quadBounds;
        public float turnSpeed;
        public float forwardSpeed;
        public float radius;
        public float visionAngle;
        public float deltaTime;

        public void Execute(int index)
        {
            // Lấy vận tốc hiện tại và chuyển đổi sang Vector2
            Vector2 velocity = velocities[index];

            // Nội suy vận tốc
            velocity = Vector2.Lerp(velocity, CalculateVelocity(index), turnSpeed / 2 * deltaTime);

            // Cập nhật vị trí mới từ ma trận
            Vector3 currentPosition = matrices[index].GetColumn(3); // Lấy vị trí từ cột 3 của ma trận (translation)
            currentPosition += (Vector3)(velocity * deltaTime);

            // Áp dụng giới hạn biên
            currentPosition = Boundary(currentPosition, currentPosition);

            // Cập nhật lại vị trí trong mảng matrices (ma trận mới)
            matrices[index] = Matrix4x4.TRS(currentPosition, matrices[index].rotation, Vector3.one);

            // Tính toán góc xoay 2D nếu vận tốc khác không
            if (velocity != Vector2.zero)
            {
                // Tính góc theo radian và chuyển sang độ
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

                // Cập nhật ma trận với góc xoay mới
                if (math.abs(angle) > 120f && math.abs(angle) < 300f)
                {
                    matrices[index] = Matrix4x4.TRS(currentPosition, Quaternion.Euler(180f, 0f, -angle), Vector3.one);
                }
                else
                {
                    matrices[index] = Matrix4x4.TRS(currentPosition, Quaternion.Euler(0f, 0f, angle), Vector3.one);
                }
            }

            // Lưu lại vận tốc mới
            velocities[index] = velocity;
        }

        private Vector2 CalculateVelocity(int index)
        {
            // Sử dụng GetColumn(3) để lấy cột 3, sau đó chuyển đổi thành float3
            float3 currentPosition = new float3(matrices[index].m03, matrices[index].m13, matrices[index].m23);     // Lấy vị trí từ ma trận
            Vector2 currentForward = matrices[index].MultiplyVector(Vector3.right); // Lấy vector hướng từ ma trận

            var separation = Vector2.zero;
            var alignment = Vector2.zero;
            var cohesion = Vector2.zero;

            var boidsInRange = BoidsInRange(currentPosition);
            var boidCount = boidsInRange.Length;
            for (var i = 0; i < boidCount; i++)
            {
                // Nếu cần tính toán tầm nhìn, bạn có thể bỏ comment InVisionCone để lọc các boids trong tầm nhìn
                separation -= Separation(currentPosition.xy, boidsInRange[i].position.xy);
                alignment += (Vector2)boidsInRange[i].element.xy;
                cohesion += (Vector2)boidsInRange[i].position.xy;
            }

            separation = separation.normalized;
            alignment = Alignment(alignment, currentForward, boidCount);
            cohesion = Cohesion(cohesion, currentPosition.xy, boidCount);
            Vector3 velocity = (currentForward
                + separation
                + 0.2f * alignment
                + cohesion
            ).normalized * forwardSpeed;
            //Debug.Log("velocity: " + velocity);

            // Áp dụng giới hạn biên
            currentPosition = Boundary(currentPosition, currentPosition);

            boidsInRange.Dispose();
            return velocity;
        }

        private NativeList<QuadElement<float2>> BoidsInRange(float3 position)
        {
            var results = new NativeList<QuadElement<float2>>(Allocator.Temp);
            QuadBounds queryBounds = new QuadBounds(position.xy, new float2(radius, radius));
            quadTree.RangeQuery(queryBounds, results);
            return results;
        }

        private bool InVisionCone(Vector2 position, Vector2 forward, Vector2 boidPosition)
        {
            Vector2 directionToPosition = boidPosition - position;
            float dotProduct = Vector2.Dot(forward.normalized, directionToPosition);
            float cosHalfVisionAngle = Mathf.Cos(visionAngle * 0.5f * Mathf.Deg2Rad);
            return dotProduct >= cosHalfVisionAngle;
        }

        private Vector2 Separation(Vector2 currentPosition, Vector2 boidPosition)
        {
            float ratio = Mathf.Clamp01((boidPosition - currentPosition).magnitude / radius);
            return (1 - ratio) * (boidPosition - currentPosition);
        }

        private Vector2 Alignment(Vector2 direction, Vector2 forward, int boidCount)
        {
            if (boidCount != 0) direction /= boidCount;
            else direction = forward;

            return direction.normalized;
        }

        private Vector2 Cohesion(Vector2 center, Vector2 position, int boidCount)
        {
            if (boidCount != 0) center /= boidCount;
            else center = position;

            return (center - position).normalized;
        }

        private readonly Vector3 Boundary(Vector3 position, float3 currentPosition)
        {
            float2 limitBounds = quadBounds.extents * 0.95f;
            if (currentPosition.x > quadBounds.center.x + limitBounds.x ||
                currentPosition.x < quadBounds.center.x - limitBounds.x)
            {
                currentPosition.x = currentPosition.x > 0 ?
                    quadBounds.center.x - limitBounds.x : quadBounds.center.x + limitBounds.x;
                position = currentPosition;
            }
            if (currentPosition.y > quadBounds.center.y + limitBounds.y ||
                currentPosition.y < quadBounds.center.y - limitBounds.y)
            {
                currentPosition.y = currentPosition.y > 0 ?
                    quadBounds.center.y - limitBounds.y : quadBounds.center.y + limitBounds.y;
                position = currentPosition;
            }

            return position;
        }
    }

}
