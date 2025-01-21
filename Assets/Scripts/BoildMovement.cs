using System;
using UnityEngine;

public class BoildMovement : MonoBehaviour
{
    private float forwardSpeed = 5f;
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
}
