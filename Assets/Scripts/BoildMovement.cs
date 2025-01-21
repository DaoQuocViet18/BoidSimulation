using System;
using System.Collections.Generic;
using UnityEngine;

public class BoildMovement : MonoBehaviour
{
    [SerializeField] private ListBoidVariable boids;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float forwardSpeed = 25f;
    [SerializeField] private float visionAngle = 270f;
    [SerializeField] private float turnSpeed = 16f;
    public Vector3 Velocity { get; private set; }

    private void FixedUpdate()
    {
        Velocity = Vector2.Lerp(Velocity, CaculateVelocity(), turnSpeed / 2 * Time.fixedDeltaTime);
        transform.position += Velocity * Time.fixedDeltaTime;
        LookRotaion();
    }

    private Vector3 CaculateVelocity()
    {
        var boidsInRange = BoidsInRange();
        Vector2 velocity = (Vector2)transform.forward 
                            + 1.7f * Separation(boidsInRange).normalized  // tăng Separation lên 70%
                            + 0.9f * Aligment(boidsInRange).normalized // giảm Aligment đi 90%
                            + Cohesion(boidsInRange).normalized
                            * forwardSpeed;
        return velocity;
    }

    private void LookRotaion()
    {
        transform.rotation = Quaternion.Slerp(transform.localRotation, 
            Quaternion.LookRotation(Velocity), turnSpeed * Time.fixedDeltaTime);
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

    private Vector2 Separation(List<BoildMovement> boildMovements)
    {
        Vector2 dirtection = Vector2.zero;
        foreach (var boid in boildMovements)
        {
            float radio = Mathf.Clamp01((boid.transform.position - transform.position).magnitude / radius);
            dirtection -= radio * (Vector2)(boid.transform.position - transform.position);
        }
        return dirtection.normalized;
    } 
        
    private Vector2 Aligment(List<BoildMovement> boildMovements)
    {
        Vector2 direction = Vector2.zero;
        foreach (var boid in boildMovements) direction += (Vector2)boid.Velocity;

        if (boildMovements.Count != 0) direction /= boildMovements.Count;
        else direction = Velocity;

        return direction.normalized;
    }

    private Vector2 Cohesion(List<BoildMovement> boildMovements)
    {
        Vector2 direction;
        Vector2 center = Vector2.zero;
        foreach (var boid in boildMovements) center += (Vector2)boid.transform.position;

        if (boildMovements.Count != 0) center /= boildMovements.Count;
        else center = Velocity;

        direction = center - (Vector2)transform.position;
        return direction.normalized;
    }
}
