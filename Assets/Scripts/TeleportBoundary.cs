using UnityEngine;

public class TeleportBoundary : MonoBehaviour
{
    [SerializeField] private Boundary boundary;

    private void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.x) > boundary.XLimit)
        {
            if (transform.position.x > 0)
            {
                transform.position = new Vector3(-boundary.XLimit, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(boundary.XLimit, transform.position.y, transform.position.z);
            }
        }

        if (Mathf.Abs(transform.position.y) > boundary.YLimit) 
        {
            if (transform.position.y > 0)
            {
                transform.position = new Vector3(transform.position.x, -boundary.YLimit, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, boundary.YLimit, transform.position.z);
            }
        }
    }
}
