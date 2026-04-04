using UnityEngine;

public class BulletWaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnWave(int count)
    {
        if (bulletPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }
        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject instance = Instantiate(bulletPrefab, point.position, point.rotation);
            instance.SetActive(true);
        }
    }
}
