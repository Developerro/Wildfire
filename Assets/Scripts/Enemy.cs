using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private Transform player;
    public float attackDistance = 5f;
    public GameObject flameObject;
    public float health = 100f;
    public float enemySpeed = 4f;
    public Animator animator;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Tree currentTarget;
    private bool lockedOnPlayer = false;
    private bool isDead = false;

    private Renderer rend;
    private Color originalColor;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        agent.speed = enemySpeed;
        flameObject?.SetActive(false);
        rb.isKinematic = true;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (lockedOnPlayer && player != null)
            AttackPlayer();
        else
        {
            FindNearestTree();
            AttackTree();
        }

        Debug.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * 2f, Color.red);
    }

    void FindNearestTree()
    {
        Tree[] trees = GameObject.FindGameObjectsWithTag("Tree")
                                 .Select(go => go.GetComponent<Tree>())
                                 .Where(tree => tree != null && !tree.IsBurnt())
                                 .ToArray();

        if (trees.Length == 0)
        {
            currentTarget = null;
            return;
        }

        currentTarget = trees.OrderBy(tree => Vector3.Distance(transform.position, tree.transform.position)).FirstOrDefault();
    }

    void AttackTree()
    {
        if (currentTarget == null)
        {
            SetFlameActive(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.transform.position);
            SetFlameActive(false);
        }
        else
        {
            agent.isStopped = true;
            LookAt(currentTarget.transform.position);
            SetFlameActive(true);
        }

        if (currentTarget.IsBurnt())
            currentTarget = null;
    }

    void AttackPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetFlameActive(false);
        }
        else
        {
            agent.isStopped = true;
            LookAt(player.position);
            SetFlameActive(true);
        }
    }

    void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    protected virtual void SetFlameActive(bool active)
    {
        animator?.SetBool("isAttacking", active);
        if (flameObject != null && flameObject.activeSelf != active)
            flameObject.SetActive(active);
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        animator?.SetTrigger("Hit");

        StartCoroutine(HurtFlash());

        if (health <= 0f)
            Die();
        else
        {
            lockedOnPlayer = true;
            currentTarget = null;
        }
    }

    IEnumerator HurtFlash()
    {
        if (rend == null) yield break;

        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        SetFlameActive(false);
        animator?.SetBool("isAttacking", false);
        animator?.SetTrigger("Die");

        StartCoroutine(EnablePhysicsWithDelay());

        Destroy(gameObject, 3f);
    }

    IEnumerator EnablePhysicsWithDelay()
    {
        yield return new WaitForEndOfFrame();

        transform.position += Vector3.up * 0.01f;

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        Vector3 knockbackDir = (-transform.forward + Vector3.up * 0.1f).normalized;
        rb.AddForce(knockbackDir * 5f, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            float dist = hit.distance;
            if (dist < 0.05f)
                transform.position = hit.point + Vector3.up * 0.2f;
        }
    }
}
