using System.Collections.Generic;
using UnityEngine;


public class FireScript : MonoBehaviour
{
    public float damagePerSecond = 5f;

    private Dictionary<Collider, float> targetsInFire = new Dictionary<Collider, float>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Player") || other.CompareTag("Tree"))
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
        float damage = damagePerSecond * Time.deltaTime;

        foreach (var pair in targetsInFire)
        {
            Collider target = pair.Key;

            if (target.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}