using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private ListBoidVariable boids;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private float boidCount;

    private void Start()
    {
        if (boids.boidMovements.Count > 0) boids.boidMovements.Clear(); 

        for (int i = 0; i < boidCount; i++)
        {
            float direction = Random.Range(0f, 360f);
            Vector3 position = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));

            GameObject boid = Instantiate(boidPrefab, position,
            Quaternion.Euler(Vector3.forward * direction) * boidPrefab.transform.localRotation);
            boid.transform.SetParent(transform);
            boids.boidMovements.Add(boid.GetComponent<BoildMovement>());
        }
           
    }
}
