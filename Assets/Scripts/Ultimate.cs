using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ultimate : MonoBehaviour
{
    public float speed = 100f;
    public float expandSpeed = 100f;
    public float maxScale = 25f;
    public float healAmount = 100f;

    private bool hasCollided = false;
    private HashSet<Player> playersInRange = new HashSet<Player>();

    void Start()
    {
        GetComponent<Rigidbody>().angularVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)
        {
            hasCollided = true;
            GetComponent<Collider>().isTrigger = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            StartCoroutine(ExpandAndHeal());
        }
    }

    IEnumerator ExpandAndHeal()
    {
        float currentScale = transform.localScale.x;

        while (currentScale < maxScale)
        {
            currentScale += Time.deltaTime * expandSpeed;
            transform.localScale = Vector3.one * currentScale;

            Collider[] colliders = Physics.OverlapSphere(transform.position, currentScale);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    Player player = col.GetComponent<Player>();
                    if (player != null && !playersInRange.Contains(player))
                    {
                        playersInRange.Add(player);
                        player.EnableFlash();
                    }
                }
            }

            HashSet<Player> toRemove = new HashSet<Player>();
            foreach (Player player in playersInRange)
            {
                if (player == null) continue;
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance > currentScale)
                {
                    player.DisableFlash();
                    toRemove.Add(player);
                }
            }
            foreach (Player p in toRemove)
                playersInRange.Remove(p);

            yield return null;
        }

        Collider[] finalColliders = Physics.OverlapSphere(transform.position, maxScale);
        foreach (Collider col in finalColliders)
        {
            if (col.CompareTag("Tree"))
            {
                Tree tree = col.GetComponent<Tree>();
                if (tree != null)
                {
                    tree.Heal(healAmount);
                    tree.ReviveTree(); 
                }
            }
            else if (col.CompareTag("Fire"))
            {
                Destroy(col.gameObject);
            }
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(ShrinkAndDestroy());
    }

    IEnumerator ShrinkAndDestroy()
    {
        float scale = transform.localScale.x;
        while (scale > 0.1f)
        {
            scale -= Time.deltaTime * expandSpeed;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        foreach (Player player in playersInRange)
        {
            if (player != null)
                player.DisableFlash();
        }
        playersInRange.Clear();
    }
}
