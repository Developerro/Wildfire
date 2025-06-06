using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healing : MonoBehaviour
{
    private List<Tree> treesInArea = new List<Tree>();
    public float healPerSecond = 25f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            Tree tree = other.GetComponent<Tree>();
            if (tree != null && !treesInArea.Contains(tree))
            {
                treesInArea.Add(tree);
            }
        }

        if (other.CompareTag("Fire"))
        {
            StartCoroutine(ExtinguishFire(other.gameObject));
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            Tree tree = other.GetComponent<Tree>();
            if (tree != null && treesInArea.Contains(tree))
            {
                tree.isHealing = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            Tree tree = other.GetComponent<Tree>();
            if (tree != null && treesInArea.Contains(tree))
            {
                treesInArea.Remove(tree);
                tree.isHealing = false;
            }
        }
    }

    void OnDisable()
    {
        foreach (Tree tree in treesInArea)
        {
            if (tree != null)
            {
                tree.isHealing = false;
            }
        }

        treesInArea.Clear();
    }

    void Update()
    {
        foreach (Tree tree in treesInArea)
        {
            if (tree != null && tree.isHealing)
            {
                tree.Heal(healPerSecond * Time.deltaTime);
            }
        }
    }

    IEnumerator ExtinguishFire(GameObject fire)
    {
        ParticleSystem[] particles = fire.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
        }

        FireArea fireArea = fire.transform.parent?.GetComponent<FireArea>();
        if (fireArea != null)
        {
            fireArea.NotifyFireDestroyedExternally(fire);
        }

        yield return new WaitForSeconds(2f);
        Destroy(fire);
    }
}
