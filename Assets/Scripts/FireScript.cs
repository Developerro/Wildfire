using UnityEngine;

public class FireScript : MonoBehaviour
{
    public Terrain terrain;
    public float burnRadius = 2f;
    public int detailLayer = 0;
    public GameObject firePrefab;
    public float timeBetweenSpreads = 2f;
    public float spreadDelayBeforeStart = 1f;
    public float spreadRadius = 5f;
    public int totalSpreads = 5;
    public float burnTime = 6f;
    public float burnTimeRandomRange = 2f; 
    public ParticleSystem fireParticles;   

    public static int activeFires = 0;      
    public static int maxFires = 64;        

    private TerrainData terrainData;
    private bool burned = false;
    private bool canSpread = false;
    private int spreadsDone = 0;
    private int[,] originalDetails;
    private bool isFading = false;          

    private void Start()
    {
        activeFires++;

        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }
        terrainData = terrain.terrainData;

        SaveOriginalGrass();

        Invoke(nameof(BurnGrass), 0.2f);
        Invoke(nameof(EnableSpread), spreadDelayBeforeStart);

        // Pequena aleatoriedade no tempo de vida
        float randomBurnTime = burnTime + Random.Range(-burnTimeRandomRange, burnTimeRandomRange);
        Invoke(nameof(FadeAndDestroy), randomBurnTime);
    }

    private void OnDestroy()
    {
        activeFires--;
    }

    private void SaveOriginalGrass()
    {
        originalDetails = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailLayer);
    }

    private void EnableSpread()
    {
        canSpread = true;
    }

    private void BurnGrass()
    {
        if (terrain == null || terrainData == null) return;
        if (burned) return;

        Vector3 terrainPos = transform.position - terrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            terrainPos.x / terrainData.size.x,
            0,
            terrainPos.z / terrainData.size.z
        );

        int detailMapX = Mathf.RoundToInt(normalizedPos.x * terrainData.detailWidth);
        int detailMapZ = Mathf.RoundToInt(normalizedPos.z * terrainData.detailHeight);

        int radiusInDetails = Mathf.RoundToInt(burnRadius * terrainData.detailWidth / terrainData.size.x);

        int startX = Mathf.Clamp(detailMapX - radiusInDetails / 2, 0, terrainData.detailWidth - 1);
        int startZ = Mathf.Clamp(detailMapZ - radiusInDetails / 2, 0, terrainData.detailHeight - 1);
        int width = Mathf.Clamp(radiusInDetails, 1, terrainData.detailWidth - startX);
        int height = Mathf.Clamp(radiusInDetails, 1, terrainData.detailHeight - startZ);

        int[,] details = (int[,])terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailLayer);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int posX = startX + x;
                int posZ = startZ + y;
                if (posX < terrainData.detailWidth && posZ < terrainData.detailHeight)
                {
                    details[posZ, posX] = 0;
                }
            }
        }

        terrainData.SetDetailLayer(0, 0, detailLayer, details);

        burned = true;

        InvokeRepeating(nameof(SpreadFire), 0f, timeBetweenSpreads);
    }

    private void SpreadFire()
    {
        if (!canSpread) return;
        if (firePrefab == null) return;
        if (spreadsDone >= totalSpreads)
        {
            CancelInvoke(nameof(SpreadFire));
            return;
        }
        if (activeFires >= maxFires) return; 

        int firesToSpawn = 2;

        for (int i = 0; i < firesToSpawn; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);

            spawnPos.y = terrain.SampleHeight(spawnPos) + terrain.transform.position.y;

            GameObject newFire = Instantiate(firePrefab, spawnPos, Quaternion.identity);

            FireScript newFireScript = newFire.GetComponent<FireScript>();
            if (newFireScript != null)
            {
                newFireScript.totalSpreads = Mathf.Max(0, this.totalSpreads - 1);
                newFireScript.spreadRadius = this.spreadRadius;
                newFireScript.timeBetweenSpreads = this.timeBetweenSpreads;
                newFireScript.burnTime = this.burnTime;
                newFireScript.burnTimeRandomRange = this.burnTimeRandomRange;
            }
        }

        spreadsDone++;
    }

    private void FadeAndDestroy()
    {
        if (isFading) return;
        isFading = true;

        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = 0; 
            fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy(gameObject, 2f); 
    }

    private void OnApplicationQuit()
    {
        if (terrainData != null && originalDetails != null)
        {
            terrainData.SetDetailLayer(0, 0, detailLayer, originalDetails);
        }
    }
}
