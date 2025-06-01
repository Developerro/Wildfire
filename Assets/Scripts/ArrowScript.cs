using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public int damage = 20;
    public GameObject arrowPrefab;

    private Rigidbody rb;
    private bool stuck = false;
    private Vector3 lastPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!stuck)
        {
            CheckRaycastHit();
        }
    }

    private void Update()
    {
        if (!stuck)
        {
            RotateArrow();
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

    private void CheckRaycastHit()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        float distance = movement.magnitude;

        if (distance > 0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(lastPosition, movement.normalized, out hit, distance))
            {
                if (!hit.collider.isTrigger)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        return;
                    }

                    transform.position = hit.point;
                    StickArrow(hit.collider, hit.point, transform.rotation);
                }
            }
        }

        lastPosition = currentPosition;
    }

    private void StickArrow(Collider hitCollider, Vector3 hitPosition, Quaternion hitRotation)
    {
        if (stuck) return;
        stuck = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (arrowPrefab != null)
        {
            GameObject stuckArrow = Instantiate(arrowPrefab, hitPosition, hitRotation);
            stuckArrow.transform.SetParent(hitCollider.transform, true);

            Destroy(stuckArrow.GetComponent<ArrowScript>());
            Rigidbody cloneRb = stuckArrow.GetComponent<Rigidbody>();
            if (cloneRb != null)
                Destroy(cloneRb);
        }

        gameObject.SetActive(false);

        if (hitCollider.CompareTag("Enemy"))
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
