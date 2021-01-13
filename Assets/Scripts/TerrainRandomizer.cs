using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainRandomizer : MonoBehaviour
{
    [System.Serializable]
    public class DetailLevel
    {
        public float scale;
        public float roughness;
    }

    public float maxOffset = 100.0f;
    [SerializeField]
    public List<DetailLevel> detailLevels;

    private Terrain terrain;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeHeightMap();
        }
    }

    public void RandomizeHeightMap()
    {
        var resolution = terrain.terrainData.heightmapResolution;
        var offset = Random.Range(0.0f, maxOffset);
        var heightMap = new float[resolution, resolution];

        foreach (var detailLevel in detailLevels)
        {
            Debug.LogFormat("{0}, {1}", detailLevel.scale, detailLevel.roughness);
            for (var x=0;x<resolution;x++)
            {
                for(var y=0;y<resolution;y++)
                {
                    var sampleX = offset + detailLevel.roughness * x / resolution;
                    var sampleY = offset + detailLevel.roughness * y / resolution;

                    heightMap[x, y] += Mathf.PerlinNoise(sampleX, sampleY) * detailLevel.scale;
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }
}
