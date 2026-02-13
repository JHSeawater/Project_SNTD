using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // UI 제어용

[System.Serializable]
public class WaveData
{
    public string WaveName = "Wave 1";
    public GameObject EnemyPrefab; // 생성할 적 종류
    public int Count = 5;          // 적 수
    public float SpawnInterval = 1.0f; // 생성 간격
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public SnakeController TargetSnake;
    public TextMeshProUGUI WaveInfoText; // 웨이브 정보 표시 UI
    public GameObject StartWaveButton;   // [전투 시작] 버튼

    [Header("Waves")]
    public List<WaveData> Waves = new List<WaveData>(); // 웨이브 목록
    private int _currentWaveIndex = 0;
    private bool _isWaveRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (StartWaveButton != null) StartWaveButton.SetActive(false); // 처음엔 숨김
        UpdateWaveUI();
    }

    // 페이즈 2 진입 시 호출 (GameManager에서 호출)
    public void SetupDefensePhase()
    {
        Debug.Log("디펜스 준비 단계: 타워를 건설하고 시작 버튼을 누르세요.");
        if (StartWaveButton != null) StartWaveButton.SetActive(true);
        UpdateWaveUI();
    }

    // [전투 시작] 버튼 클릭 시 호출
    public void OnStartWaveButtonClicked()
    {
        if (_isWaveRunning) return;
        if (_currentWaveIndex >= Waves.Count)
        {
            Debug.Log("모든 웨이브 클리어!");
            return;
        }

        StartCoroutine(SpawnWaveRoutine(Waves[_currentWaveIndex]));
        
        // 버튼 숨기기 (웨이브 중에는 못 누르게)
        if (StartWaveButton != null) StartWaveButton.SetActive(false);
    }

    // 현재 살아있는 적의 수 (최적화용)
    [HideInInspector] public int EnemiesAlive = 0;

    IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        _isWaveRunning = true;
        Debug.Log($"🌊 {wave.WaveName} 시작!");

        List<Vector3> path = TargetSnake.FinalPath;

        for (int i = 0; i < wave.Count; i++)
        {
            SpawnEnemy(wave.EnemyPrefab, path);
            yield return new WaitForSeconds(wave.SpawnInterval);
        }

        // [최적화] FindObjectsWithTag 대신 변수 체크
        yield return new WaitUntil(() => EnemiesAlive == 0);

        WaveCompleted();
    }

    void WaveCompleted()
    {
        _isWaveRunning = false;
        _currentWaveIndex++;
        
        Debug.Log("웨이브 종료! 정비 시간입니다.");
        
        // 이자 지급 등 보상 로직
        GameManager.Instance.AddGold(100); 

        if (_currentWaveIndex >= Waves.Count)
        {
            Debug.Log("🎉 축하합니다! 모든 웨이브를 막아냈습니다!");
            // 게임 클리어 UI 호출
        }
        else
        {
            if (StartWaveButton != null) StartWaveButton.SetActive(true);
            UpdateWaveUI();
        }
    }

    void SpawnEnemy(GameObject prefab, List<Vector3> path)
    {
        if (prefab == null) return;
        
        GameObject enemy = Instantiate(prefab);
        EnemiesAlive++; // 적 숫자 증가

        if (enemy.TryGetComponent<EnemyMovement>(out EnemyMovement movement))
        {
            movement.SetPath(path);
        }
    }

    // 적이 죽을 때 호출 (Enemy 스크립트에서 호출)
    public void OnEnemyDied()
    {
        EnemiesAlive--;
    }

    void UpdateWaveUI()
    {
        if (WaveInfoText != null)
        {
            if (_currentWaveIndex < Waves.Count)
                WaveInfoText.text = $"Wave: {_currentWaveIndex + 1} / {Waves.Count}";
            else
                WaveInfoText.text = "All Cleared!";
        }
    }
}