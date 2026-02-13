using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject _basicTowerPrefab;
    [SerializeField] private Tilemap _floorTilemap; // 건설 가능 영역 확인용
    [SerializeField] private Tilemap _islandTilemap; // 섬/언덕 (건설 O, 뱀 X)
    [SerializeField] private SpriteRenderer _ghostRenderer; // 건설 미리보기용 렌더러

    private Tower _selectedTower; // 현재 선택된 타워

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 고스트 렌더러가 없다면 동적으로 생성
        if (_ghostRenderer == null)
        {
            GameObject ghostObj = new GameObject("BuildGhost");
            _ghostRenderer = ghostObj.AddComponent<SpriteRenderer>();
            ghostObj.transform.SetParent(transform); // 매니저 하위에 둠
            _ghostRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 반투명
            _ghostRenderer.sortingOrder = 10; // 가장 위에 보이게
            ghostObj.SetActive(false);
        }
    }

    void Start()
    {
        // [수정] GameObject.Find는 이름 변경에 취약하므로 Inspector에서 할당하는 것을 권장합니다.
        if (_floorTilemap == null || _islandTilemap == null)
        {
            Debug.LogWarning("BuildManager: 타일맵 참조가 누락되었습니다. Inspector에서 할당해주세요.");
        }

        _selectedTower = null;
    }

    void Update()
    {
        // 디펜스 페이즈가 아니면 건설 불가
        if (GameManager.Instance.CurrentPhase != GamePhase.Defense)
        {
            if (_ghostRenderer.gameObject.activeSelf) _ghostRenderer.gameObject.SetActive(false);
            return;
        }

        // 우클릭이나 ESC로 건설 취소 기능
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();
            return;
        }

        UpdateGhost(); // 미리보기 갱신

        // 마우스 클릭 감지 (좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            // 선택된 타워가 없으면 무시
            if (_selectedTower == null) return;

            // UI(버튼 등) 위를 클릭했다면 건설하지 않음
            if (EventSystem.current.IsPointerOverGameObject()) return;

            BuildTowerAtMousePos();
        }
    }

    // 미리보기(Ghost) 업데이트
    void UpdateGhost()
    {
        if (_selectedTower == null)
        {
            if (_ghostRenderer.gameObject.activeSelf) _ghostRenderer.gameObject.SetActive(false);
            return;
        }

        if (!_ghostRenderer.gameObject.activeSelf) _ghostRenderer.gameObject.SetActive(true);

        // 마우스 위치 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y), 0);
        
        // 위치 이동
        _ghostRenderer.transform.position = gridPos;

        // 건설 가능 여부에 따라 색상 변경 (가능: 초록/흰색, 불가능: 빨강)
        bool possible = CanBuild(gridPos);
        _ghostRenderer.color = possible ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        
        // 스프라이트 갱신 (선택된 타워의 모양으로)
        if (_selectedTower.TryGetComponent<SpriteRenderer>(out SpriteRenderer towerSprite))
        {
            _ghostRenderer.sprite = towerSprite.sprite;
        }
    }

    // 건설 취소 함수
    public void DeselectTower()
    {
        _selectedTower = null;
        if (_ghostRenderer != null) _ghostRenderer.gameObject.SetActive(false);
        Debug.Log("건설 취소");
    }

    void BuildTowerAtMousePos()
    {
        if (_selectedTower == null) return;

        // 마우스 좌표를 월드 좌표로 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 정수 좌표로 스냅 (Grid 시스템)
        Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y), 0);
        
        // 1. 건설 가능한지 확인
        if (CanBuild(gridPos))
        {
            // 2. 돈 확인
            if (GameManager.Instance.UseGold(_selectedTower.Cost))
            {
                // 3. 건설 (선택된 타워의 프리팹을 생성)
                Instantiate(_selectedTower.gameObject, gridPos, Quaternion.identity);
                Debug.Log($"타워 건설 완료! 위치: {gridPos}");
            }
        }
    }

    bool CanBuild(Vector3Int pos)
    {
        // 0. 유효한 지형 위인지 확인 (Floor OR Island)
        bool isFloor = (_floorTilemap != null && _floorTilemap.HasTile(pos));
        bool isIsland = (_islandTilemap != null && _islandTilemap.HasTile(pos));

        if (!isFloor && !isIsland)
        {
            // Debug.Log("건설 불가: 바닥이나 섬이 없는 허공입니다.");
            return false;
        }

        // 1. 이미 타워나 장애물이 있는지 확인 (Physics2D)
        // 타워 크기(1x1)를 고려하여 Box로 영역 체크 (0.9f로 살짝 여유를 둠)
        // [수정] OverlapBox -> OverlapBoxAll: 겹쳐있는 모든 콜라이더를 검사해야 함
        Collider2D[] hits = Physics2D.OverlapBoxAll(new Vector2(pos.x, pos.y), new Vector2(0.9f, 0.9f), 0f);
        
        foreach (Collider2D hit in hits)
        {
            // Island는 건설 가능 영역이므로 무시하고 계속 검사
            if (hit.gameObject.name.Contains("Island")) 
            {
                continue;
            }

            // 그 외의 모든 충돌체(Wall, Tower 등)가 하나라도 감지되면 건설 불가
            Debug.Log($"건설 불가: 장애물({hit.name})이 있습니다.");
            return false;
        }

        // 2. 뱀의 경로(몸통) 위인지 확인
        // SnakeController의 FinalPath 리스트를 확인
        foreach (Vector3 pathPos in SnakeController.Instance.FinalPath)
        {
            // 부동소수점 오차 고려하여 반올림 비교
            if (Mathf.RoundToInt(pathPos.x) == pos.x && Mathf.RoundToInt(pathPos.y) == pos.y)
            {
                Debug.Log("건설 불가: 적의 이동 경로입니다.");
                return false;
            }
        }

        return true;
    }

    // 상점 버튼 클릭 시 호출
    public void SelectTower(GameObject towerPrefab)
    {
        if (towerPrefab != null)
        {
            _selectedTower = towerPrefab.GetComponent<Tower>();
            Debug.Log($"타워 선택됨: {_selectedTower.TowerName}");
        }
    }
}
