using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    private bool stuck = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;

        StickArrow();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (stuck) return;

        StickArrow();
    }

    private void StickArrow()
    {
        stuck = true;
        rb.isKinematic = true; 
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!stuck)
        {
            RaycastHit hit;
            Vector3 nextPosition = transform.position + rb.linearVelocity * Time.fixedDeltaTime;

            if (Physics.Linecast(transform.position, nextPosition, out hit))
            {
                transform.position = hit.point;
                StickArrow();
            }
        }
    }
}
