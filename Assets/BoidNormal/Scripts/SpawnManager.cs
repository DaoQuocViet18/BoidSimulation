using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private ListBoidVariable boids;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private float boidCount;

    private void Awake()
    {
        if (boids.boidTransform.Count > 0) boids.boidTransform.Clear(); 

        for (int i = 0; i < boidCount; i++)
        {
            float direction = Random.Range(0f, 360f);
            Vector3 position = new Vector2(Random.Range(-60f, 60f), Random.Range(-30f, 30f));

            GameObject boid = Instantiate(boidPrefab, position,
            Quaternion.Euler(Vector3.forward * direction) * boidPrefab.transform.localRotation);
            //boid.transform.SetParent(transform);
            boids.boidTransform.Add(boid.transform);
        }
           
    }
}
