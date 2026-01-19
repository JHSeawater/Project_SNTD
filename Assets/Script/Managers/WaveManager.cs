using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public GameObject EnemyPrefab;   // 소환할 적 프리팹
    public SnakeController TargetSnake;    // 길(Path) 정보를 가져올 뱀 컨트롤러

    [Header("Wave Settings")]
    public float SpawnInterval = 1.5f; // 적 생성 간격
    public int EnemyCount = 5;         // 이번 웨이브에 나올 적의 수
    
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
        if (TargetSnake == null)
        {
            Debug.LogError("WaveManager: TargetSnake가 할당되지 않았습니다!");
            yield break;
        }

        List<Vector3> path = TargetSnake.FinalPath;

        for (int i = 0; i < EnemyCount; i++)
        {
            SpawnEnemy(path);
            yield return new WaitForSeconds(SpawnInterval);
        }
    }

    void SpawnEnemy(List<Vector3> path)
    {
        if (EnemyPrefab == null) return;

        // 1. 적 생성 (일단 화면 밖이나 0,0에서 생성하고 위치는 바로 이동시킴)
        GameObject enemy = Instantiate(EnemyPrefab);
        
        // 2. 적에게 '이 길로 가라'고 명령서(Path) 전달
        if (enemy.TryGetComponent<EnemyMovement>(out EnemyMovement movement))
        {
            movement.SetPath(path);
        }
    }
}