using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float attackDistance = 5f;
    public GameObject flameObject;
    public float health = 100f;

    private NavMeshAgent agent;
    private Tree currentTarget;
    private bool lockedOnPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (flameObject != null)
            flameObject.SetActive(false);
    }

    void Update()
    {
        if (lockedOnPlayer)
        {
            AttackPlayer();
        }
        else
        {
            FindNearestTree();
            AttackTree();
        }
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
        {
            currentTarget = null;
        }
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

    void SetFlameActive(bool active)
    {
        if (flameObject == null) return;

        if (flameObject.activeSelf != active)
            flameObject.SetActive(active);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            Destroy(gameObject);
        }

        lockedOnPlayer = true;
        currentTarget = null;
    }
}
