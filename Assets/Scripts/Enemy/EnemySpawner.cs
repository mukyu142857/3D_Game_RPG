using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敌人预制体")]
    public GameObject enemyPrefab;

    [Header("刷怪时间")]
    public float spawnTime = 3f;
    private float spawnTimer;

    [Header("刷怪范围")]
    private float spawnRadius = 1f;

    [Header("怪物最大数量")]
    public int maxEnemyCount = 5;

    [Header("站立队列")]
    public int queueColumns = 3;
    public float queueSpacing = 1.5f;

    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Update()
    {
        // 清理已销毁的敌人引用
        aliveEnemies.RemoveAll(e => e == null);

        // 达到最大数量时不再刷怪
        if (aliveEnemies.Count >= maxEnemyCount)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 生成一个敌人
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: 未设置敌人预制体！");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        aliveEnemies.Add(newEnemy);
    }

    /// <summary>
    /// 在圆形范围内随机获取一个生成位置
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    /// <summary>
    /// 根据队列索引获取队列中的站位（按行列排列）
    /// </summary>
    public Vector3 GetQueuePosition(int index)
    {
        int row = index / queueColumns;
        int col = index % queueColumns;

        // 以生成器位置为中心，列居中排列
        float offsetX = (col - (queueColumns - 1) * 0.5f) * queueSpacing;
        float offsetZ = row * queueSpacing;

        return transform.position + new Vector3(offsetX, 0, offsetZ);
    }

    /// <summary>
    /// 在Scene视图中绘制刷怪范围（选中生成器时可见）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制半透明圆形表示刷怪范围
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // 绘制队列站位预览
        Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
        for (int i = 0; i < maxEnemyCount; i++)
        {
            Vector3 queuePos = GetQueuePosition(i);
            Gizmos.DrawWireCube(queuePos, new Vector3(0.5f, 0, 0.5f));
        }
    }
}
