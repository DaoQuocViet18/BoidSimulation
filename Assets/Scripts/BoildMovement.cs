using System;
using System.Collections.Generic;
using UnityEngine;

public class BoildMovement : MonoBehaviour
{
    [SerializeField] private ListBoidVariable boids;
    private float radius = 2f;
    private float forwardSpeed = 5f;
    private float visionAngle = 270f;
    public Vector3 Velocity { get; private set; }

    private void FixedUpdate()
    {
        Velocity = Vector2.Lerp(Velocity, transform.forward.normalized * forwardSpeed, 10f * Time.fixedDeltaTime);
        transform.position += Velocity * Time.fixedDeltaTime;
        LookRotaion();
    }

    private void LookRotaion()
    {
        transform.rotation = Quaternion.Slerp(transform.localRotation, 
            Quaternion.LookRotation(Velocity), Time.fixedDeltaTime);
    }

    private List<BoildMovement> BoidsInRange()
    {
        var listBoid = boids.boidMovements.FindAll(boids => boids != this
                && (boids.transform.position - transform.position).magnitude <= radius
                && InVisionCone(boids.transform.position));
        return listBoid;
    }

    private bool InVisionCone(Vector2 position)
    {
        Vector2 directionToPosition = position - (Vector2)transform.position;
        float dotProduct = Vector2.Dot(transform.forward, directionToPosition);
        float costHalfVisionAngle = Mathf.Cos(visionAngle * 0.5f * Mathf.Deg2Rad);
        return dotProduct >= costHalfVisionAngle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        var boidsInRange = BoidsInRange();
        foreach (var boid in boidsInRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, boid.transform.position);
        }
    }
}
