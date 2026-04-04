using UnityEngine;
using System.Collections.Generic;

public class BulletWaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnWave(int count)
    {
        var indices = new List<int>(spawnPoints.Length);
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            indices.Add(i);
        }
        
        for (int i = 0; i < indices.Count; i++)
        {
            int j = Random.Range(0, indices.Count);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        int spawnCount = Mathf.Min(count, spawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform point = spawnPoints[indices[i]];
            GameObject instance = Instantiate(bulletPrefab, point.position, point.rotation);
            instance.SetActive(true);
        }
    }
}
