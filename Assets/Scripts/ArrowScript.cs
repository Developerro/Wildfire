using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    private bool stuck = false;
    private Vector3 lastPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!stuck)
        {
            RotateArrow();
            CheckForCollision();
        }
    }

    private void RotateArrow()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(-rb.linearVelocity);
            transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0); 
        }
    }


    private void CheckForCollision()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        float distance = movement.magnitude;

        if (distance > 0f)
        {
            if (Physics.Linecast(lastPosition, currentPosition, out RaycastHit hit))
            {
                transform.position = hit.point;
                StickArrow(hit.collider);
            }
        }

        lastPosition = currentPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!stuck)
        {
            StickArrow(collision.collider);
        }
    }

    private void StickArrow(Collider hitCollider)
    {
        stuck = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.SetParent(hitCollider.transform);
    }
}
