using UnityEngine;
using System.Collections.Generic;

public class FireScript : MonoBehaviour
{
    public float damagePerSecond = 5f;

    private Dictionary<Collider, float> targetsInFire = new Dictionary<Collider, float>();

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Tree")) && gameObject.activeInHierarchy)
        {
            if (!targetsInFire.ContainsKey(other))
            {
                targetsInFire.Add(other, 0f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetsInFire.ContainsKey(other))
        {
            targetsInFire.Remove(other);
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        float damage = damagePerSecond * Time.deltaTime;

        List<Collider> toRemove = new List<Collider>();

        foreach (var pair in targetsInFire)
        {
            Collider target = pair.Key;

            if (target == null)
            {
                toRemove.Add(target);
                continue;
            }

            if (target.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
            }
        }

        foreach (var target in toRemove)
        {
            targetsInFire.Remove(target);
        }
    }

    private void OnDisable()
    {
        targetsInFire.Clear();
    }
}
