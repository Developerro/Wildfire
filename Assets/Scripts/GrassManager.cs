using UnityEngine;

public class GrassManager : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;

    private int[,] originalDetails;
    public int detailLayer = 0;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        if (terrain != null)
        {
            terrainData = terrain.terrainData;
            SaveOriginalGrass();
        }
    }

    private void SaveOriginalGrass()
    {
        int[,] details = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailLayer);
        originalDetails = new int[details.GetLength(0), details.GetLength(1)];

        for (int y = 0; y < details.GetLength(0); y++)
        {
            for (int x = 0; x < details.GetLength(1); x++)
            {
                originalDetails[y, x] = details[y, x];
            }
        }
    }

    private void ResetGrass()
    {
        if (terrainData != null && originalDetails != null)
        {
            terrainData.SetDetailLayer(0, 0, detailLayer, originalDetails);
        }
    }

    private void OnDisable()
    {
        ResetGrass();
    }

    private void OnApplicationQuit()
    {
        ResetGrass();
    }

    [ContextMenu("Reset Grass Manually")]
    public void ResetGrassManually()
    {
        ResetGrass();
    }
}
