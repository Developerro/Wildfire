using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireArea : MonoBehaviour
{
    public GameObject firePrefab;
    public GameObject bigFirePrefab;
    public float spreadInterval = 1f;
    public float spreadRadius = 3f;
    public float fireLifetime = 6f;

    public float fusionRadius = 2.5f;
    public int fusionThreshold = 6;

    public Terrain terrain;
    public float burnRadius = 2f;
    public int detailLayer = 0;

    private List<GameObject> activeFires = new List<GameObject>();
    private Collider area;
    private TerrainData terrainData;

    void Start()
    {
        area = GetComponent<Collider>();

        if (terrain == null)
            terrain = Terrain.activeTerrain;

        TerrainData originalData = terrain.terrainData;
        terrainData = Instantiate(originalData);
        terrain.terrainData = terrainData;

        Vector3 center = area.bounds.center;
        center.y = terrain.SampleHeight(center) + terrain.transform.position.y;

        StartCoroutine(SpawnDelayedFire(center));

        StartCoroutine(SpreadFire());
    }

    IEnumerator SpreadFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(spreadInterval);

            foreach (GameObject fire in activeFires)
            {
                if (fire == null) continue;

                Vector3 spreadPos = fire.transform.position + Random.insideUnitSphere * spreadRadius;
                spreadPos.y = terrain.SampleHeight(spreadPos) + terrain.transform.position.y;

                if (IsInsideArea(spreadPos) && !FireAlreadyExistsNear(spreadPos))
                {
                    StartCoroutine(SpawnDelayedFire(spreadPos));
                }
            }

            activeFires.RemoveAll(f => f == null);

            TryCreateBigFire();

            if (activeFires.Count == 0)
            {
                Destroy(gameObject);
                yield break;
            }
        }
    }

    IEnumerator SpawnDelayedFire(Vector3 position)
    {
        GameObject newFire = Instantiate(firePrefab, position, Quaternion.identity, transform);
        newFire.SetActive(false);

        yield return new WaitForSeconds(3f);

        newFire.SetActive(true);
        PlayAllParticles(newFire);
        activeFires.Add(newFire);
        BurnGrass(position);
        StartCoroutine(AutoDestroyWithFade(newFire, fireLifetime));
    }

    void BurnGrass(Vector3 worldPos)
    {
        if (terrain == null || terrainData == null) return;

        Vector3 terrainPos = worldPos - terrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            terrainPos.x / terrainData.size.x,
            0,
            terrainPos.z / terrainData.size.z
        );

        int detailX = Mathf.RoundToInt(normalizedPos.x * terrainData.detailWidth);
        int detailZ = Mathf.RoundToInt(normalizedPos.z * terrainData.detailHeight);

        int radius = Mathf.RoundToInt(burnRadius * terrainData.detailWidth / terrainData.size.x);

        int startX = Mathf.Clamp(detailX - radius / 2, 0, terrainData.detailWidth - 1);
        int startZ = Mathf.Clamp(detailZ - radius / 2, 0, terrainData.detailHeight - 1);
        int width = Mathf.Clamp(radius, 1, terrainData.detailWidth - startX);
        int height = Mathf.Clamp(radius, 1, terrainData.detailHeight - startZ);

        int[,] details = terrainData.GetDetailLayer(startX, startZ, width, height, detailLayer);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                details[y, x] = 0;
            }
        }

        terrainData.SetDetailLayer(startX, startZ, detailLayer, details);
    }

    bool IsInsideArea(Vector3 position)
    {
        return area.bounds.Contains(position);
    }

    bool FireAlreadyExistsNear(Vector3 position)
    {
        foreach (GameObject fire in activeFires)
        {
            if (fire != null && Vector3.Distance(fire.transform.position, position) < 1f)
                return true;
        }
        return false;
    }

    IEnumerator AutoDestroyWithFade(GameObject fire, float delay)
    {
        yield return new WaitForSeconds(delay - 1f);

        ParticleSystem[] particleSystems = fire.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        float fadeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float fadeFactor = 1f - (elapsedTime / fadeTime);

            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                Color startColor = main.startColor.color;
                main.startColor = new Color(startColor.r, startColor.g, startColor.b, fadeFactor);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        NotifyFireDestroyedExternally(fire);

        Destroy(fire);
    }

    void TryCreateBigFire()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject fire in activeFires)
        {
            if (fire == null || toRemove.Contains(fire)) continue;

            List<GameObject> nearbyFires = new List<GameObject>();

            foreach (GameObject other in activeFires)
            {
                if (other == null || fire == other || toRemove.Contains(other)) continue;

                if (Vector3.Distance(fire.transform.position, other.transform.position) <= fusionRadius)
                {
                    nearbyFires.Add(other);
                }
            }

            nearbyFires.Add(fire);

            if (nearbyFires.Count >= fusionThreshold)
            {
                Vector3 center = Vector3.zero;
                foreach (GameObject f in nearbyFires)
                    center += f.transform.position;
                center /= nearbyFires.Count;

                GameObject big = Instantiate(bigFirePrefab, center, Quaternion.identity, transform);
                PlayAllParticles(big);
                BurnGrass(center);
                StartCoroutine(AutoDestroyWithFade(big, fireLifetime * 2f));

                toRemove.AddRange(nearbyFires);
            }
        }

        foreach (GameObject f in toRemove)
        {
            if (f != null)
                Destroy(f);
            activeFires.Remove(f);
        }
    }

    void PlayAllParticles(GameObject go)
    {
        ParticleSystem[] psSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in psSystems)
            ps.Play();
    }

    public void NotifyFireDestroyedExternally(GameObject fire)
    {
        if (fire != null)
        {
            activeFires.Remove(fire);
            if (activeFires.Count == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
