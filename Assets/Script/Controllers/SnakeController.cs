using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 재시작용
using System.Linq; // Queue.Last() 사용을 위해 추가

public class SnakeController : MonoBehaviour
{
    public static SnakeController Instance { get; private set; }

    private Vector2Int _currentDirection = Vector2Int.zero; // 현재 이동 방향
    private Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>(); // 입력 버퍼

    private List<Transform> _bodyParts = new List<Transform>(); // 몸통 마디들을 담을 리스트
    private bool _isPaused = false; // 일시정지 상태

    [Header("Settings")]
    [SerializeField] private float _moveInterval = 0.2f;
    [SerializeField] private GameObject _bodyPrefab;
    [SerializeField] private bool _enablePause = true; // 일시정지 기능 활성화 여부

    [Header("Path Settings")]
    [SerializeField] private Color _roadColor = new Color(0.4f, 0.4f, 0.4f, 1f); // 길로 변했을 때의 색 (회색)
    public List<Vector3> FinalPath = new List<Vector3>(); // 적들이 참고할 최종 경로 데이터

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _bodyParts.Clear();
        _bodyParts.Add(this.transform); // 머리를 리스트의 첫 번째로 추가
        StartCoroutine(MoveRoutine()); // 일정 시간마다 Move 함수를 호출하는 코루틴 시작
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 일시정지 토글 (스페이스바)
        if (_enablePause && Input.GetKeyDown(KeyCode.Space))
        {
            _isPaused = true;
            Debug.Log("일시정지: 방향키를 누르면 재개합니다.");
        }

        // 방향키 입력 감지
        if (Input.GetKeyDown(KeyCode.UpArrow)) EnqueueDirection(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) EnqueueDirection(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) EnqueueDirection(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) EnqueueDirection(Vector2Int.right);
    }

    // 입력 버퍼에 방향 추가 시도
    void EnqueueDirection(Vector2Int newDir)
    {
        // 정지 상태였다면 해제하고 즉시 이동 처리
        if (_isPaused)
        {
            _isPaused = false;
            // 버퍼 비우고 현재 입력 즉시 적용
            _inputQueue.Clear();
            _inputQueue.Enqueue(newDir);
            return;
        }

        // 버퍼가 너무 많이 쌓이면 반응이 느려지므로 최대 2개까지만 예약
        if (_inputQueue.Count >= 2) return;

        // 검증 기준: 버퍼에 예약된 게 있다면 그 마지막 예약 방향, 없다면 현재 이동 방향
        Vector2Int lastPlannedDir = _inputQueue.Count > 0 ? _inputQueue.Last() : _currentDirection;

        // 1. 반대 방향 전환 방지 (180도 턴 불가)
        if (newDir == -lastPlannedDir) return;

        // 2. 같은 방향 중복 입력 방지
        if (newDir == lastPlannedDir) return;

        // 유효하면 큐에 추가
        _inputQueue.Enqueue(newDir);
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            // 일시정지 상태면 대기
            if (_isPaused)
            {
                yield return null;
                continue;
            }

            // 첫 시작(방향 없음)이면 대기하되, 입력이 들어오면 시작
            if (_currentDirection == Vector2Int.zero && _inputQueue.Count == 0)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(_moveInterval); // 기본 0.2초

            // 다시 한 번 일시정지 체크
            if (_isPaused) continue;

            // 버퍼에 입력된 다음 방향이 있다면 꺼내서 적용
            if (_inputQueue.Count > 0)
            {
                _currentDirection = _inputQueue.Dequeue();
            }

            MoveSnake();
        }
    }

    void MoveSnake()
    {
        // 꼬리부터 앞 마디의 위치로 한 칸씩 이동 (역순 루프)
        for (int i = _bodyParts.Count - 1; i > 0; i--)
        {
            _bodyParts[i].position = _bodyParts[i - 1].position;
        }

        // 머리 이동
        transform.position += (Vector3Int)_currentDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Apple"))
        {
            Collider2D appleCollider = collision.GetComponent<Collider2D>();
            if (appleCollider.enabled == false) return;
            appleCollider.enabled = false;
            
            Grow();
            SpawnManager.Instance.SpawnApple();
            Destroy(collision.gameObject);
        }

        else if (collision.CompareTag("Goal"))
        {
            FinishSnakeGame();
        }

        else if (collision.CompareTag("Wall") || collision.CompareTag("Body"))
        {
            // 시작 직후 안전장치
            if (Time.timeSinceLevelLoad < 0.1f) return; 

            Debug.Log($"충돌 발생 ({collision.tag}) -> 게임 재시작");
            // 현재 씬을 다시 로드하여 재시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void FinishSnakeGame()
    {
        Debug.Log("목적지 도착! 이제 타워 디펜스를 시작합니다.");
    
        // 1. 이동 코루틴 중지
        StopAllCoroutines();
    
        BakePath();

        // WaveManager 직접 호출 대신 GameManager를 통해 페이즈 전환
        GameManager.Instance.StartDefensePhase();
    }

    private void BakePath()
    {
        FinalPath.Clear();

        // 1. 데이터 추출 (중요: 적은 꼬리 -> 머리 방향으로 옵니다)
        // 현재 _bodyParts[0]은 머리(Goal 위치), 마지막은 꼬리입니다.
        // 따라서 역순으로 돌지 않고, 정순으로 담은 뒤 나중에 적이 거꾸로 쓰거나,
        // 여기서 아예 꼬리부터 머리 순서로 뒤집어서 저장합니다.

        for (int i = _bodyParts.Count - 1; i >= 0; i--)
        {
            FinalPath.Add(_bodyParts[i].position);
        }

        foreach (Transform part in _bodyParts)
        {
            // 색상 변경 (SpriteRenderer가 있다면)
            if (part.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
            {
                sprite.color = _roadColor; // 회색 등으로 변경
                sprite.sortingOrder = -1; // 적이나 타워보다 뒤에 보이도록 순서 내리기
            }
        // 충돌체 끄기 (타워 설치 클릭 등에 방해되지 않게)
            if (part.TryGetComponent<Collider2D>(out Collider2D col))
            {
                col.enabled = false;
            }
        }

        Debug.Log("🐍 뱀 경로 베이킹 완료! 경로 길이: " + FinalPath.Count);
    }

    void Grow()
    {
        // 새 몸통 생성
        GameObject newPart = Instantiate(_bodyPrefab);
        // 화면 밖(-100, -100)에 임시로 생성
        newPart.transform.position = new Vector3(-100, -100, 0);
        // 리스트에 추가
        _bodyParts.Add(newPart.transform);
        // 골드 추가
        GameManager.Instance.AddGold(10);
    }

    public List<Vector3> GetSnakePath()
    {
        List<Vector3> path = new List<Vector3>();
        // 적이 꼬리에서 머리 방향으로 오게 하려면 리스트를 그대로 사용하거나 역순으로 담습니다.
        foreach (Transform part in _bodyParts)
        {
            path.Add(part.position);
        }
        return path;
    }
}