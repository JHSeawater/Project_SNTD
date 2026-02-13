using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; } //싱글톤 & 접근 제어

    [Header("References")]
    [SerializeField] private GameObject _applePrefab;
    [SerializeField] private GameObject _goldPrefab; // 골드 프리팹 추가
    [SerializeField] private Tilemap _floorTilemap;

    private bool _isGoldSpawned = false; // 현재 맵에 골드가 있는지 확인

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (_floorTilemap == null)
        {
            Debug.LogError("SpawnManager: Floor Tilemap이 할당되지 않았습니다!");
        }

        SpawnApple();
        TrySpawnGold(); // 시작할 때 첫 골드 생성 시도
    }

    // 골드 존재 여부 설정 (SnakeController에서 먹었을 때 false로 바꿈)
    public void SetGoldStatus(bool exists)
    {
        _isGoldSpawned = exists;
    }

    // 사과를 먹었을 때 호출될 메서드
    public void TrySpawnGold()
    {
        if (!_isGoldSpawned)
        {
            SpawnItem(_goldPrefab);
            _isGoldSpawned = true;
        }
    }

    public void SpawnApple()
    {
        SpawnItem(_applePrefab);
    }

    // 아이템 생성 공용 로직 (사과, 골드 공용)
    private void SpawnItem(GameObject prefab)
    {
        if (_floorTilemap == null || prefab == null) return;

        Vector3 spawnPos = Vector3.zero;
        bool isValid = false;
        int maxAttempts = 100;
        int attempts = 0;

        BoundsInt bounds = _floorTilemap.cellBounds;
        
        while (!isValid && attempts < maxAttempts)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            if (_floorTilemap.HasTile(cellPos))
            {
                spawnPos = new Vector3(x, y, 0);
                Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.4f);
                if (hit == null) isValid = true;
            }
            attempts++;
        }

        if (isValid)
        {
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}