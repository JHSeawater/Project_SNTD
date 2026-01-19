using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;   // 소환할 적 프리팹
    public SnakeController snake;    // 길(Path) 정보를 가져올 뱀 컨트롤러

    [Header("Wave Settings")]
    public float spawnInterval = 1.5f; // 적 생성 간격
    public int enemyCount = 5;         // 이번 웨이브에 나올 적의 수

    public static WaveManager Instance { get; private set; }
    
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 뱀 게임이 끝나면 이 함수가 호출됩니다.
    public void StartWave()
    {
        Debug.Log("🌊 웨이브 시작! 적들이 몰려옵니다.");
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        // 뱀이 만든 길 데이터를 가져옵니다.
        List<Vector3> path = snake.finalPath;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(path);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy(List<Vector3> path)
    {
        // 1. 적 생성 (일단 화면 밖이나 0,0에서 생성하고 위치는 바로 이동시킴)
        GameObject enemy = Instantiate(enemyPrefab);
        
        // 2. 적에게 '이 길로 가라'고 명령서(Path) 전달
        if (enemy.TryGetComponent<EnemyMovement>(out EnemyMovement movement))
        {
            movement.SetPath(path);
        }
    }
}