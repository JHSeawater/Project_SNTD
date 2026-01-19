using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; } //싱글톤 & 접근 제어

    [Header("References")]
    [SerializeField] private GameObject _applePrefab;
    [SerializeField] private Tilemap _floorTilemap; // 길 타일맵 참조

    void Awake()
    {
        // 2. 싱글톤 중복 체크 로직
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (_floorTilemap == null)
        {
            Debug.LogError("SpawnManager: Floor Tilemap이 할당되지 않았습니다! Inspector에서 할당해주세요.");
        }

        SpawnApple();
    }

    public void SpawnApple()
    {
        if (_floorTilemap == null) return;

        Vector3 spawnPos = Vector3.zero;
        bool isValid = false;
        int maxAttempts = 100; // 무한 루프 방지용 안전 장치
        int attempts = 0;

        BoundsInt bounds = _floorTilemap.cellBounds;
        
        while (!isValid && attempts < maxAttempts)
        {
            // 1. 영역 내에서 랜덤 좌표 추출 후 반올림하여 정수 격자에 맞춤
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            // [핵심 로직] "이 좌표에 바닥 타일이 깔려 있니?" 확인
            if (_floorTilemap.HasTile(cellPos))
            {
                spawnPos = new Vector3(x, y, 0);

                // 2. 그 자리에 장애물(벽, 몸통)이 없는지 2차 확인
                Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.4f);
                if (hit == null)
                {
                    isValid = true;
                }
                else
                {
                    // [추가] 누가 방해하는지 콘솔에 범인의 이름을 찍어봅니다.
                    Debug.Log("방해꾼 발견: " + hit.name);
                }
            }
            attempts++;
        }

        if (isValid)
        {
            Instantiate(_applePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("사과 생성 실패! (빈 공간 없음)");
        }
    }
}