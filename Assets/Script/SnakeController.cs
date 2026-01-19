using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    private Vector2Int direction = Vector2Int.zero; // 현재 이동 방향
    private Vector2Int lastInputDirection = Vector2Int.right; // 마지막 입력 방향 (반대 방향 전환 방지)
    private List<Transform> bodyParts = new List<Transform>(); // 몸통 마디들을 담을 리스트

    [Header("Settings")]
    [SerializeField] private float moveInterval = 0.2f;
    [SerializeField] private GameObject bodyPrefab;

    [Header("Path Settings")]
    [SerializeField] private Color roadColor = new Color(0.4f, 0.4f, 0.4f, 1f); // 길로 변했을 때의 색 (회색)
    public List<Vector3> finalPath = new List<Vector3>(); // 적들이 참고할 최종 경로 데이터

    void Start()
    {
        bodyParts.Clear();
        bodyParts.Add(this.transform); // 머리를 리스트의 첫 번째로 추가
        StartCoroutine(MoveRoutine()); // 일정 시간마다 Move 함수를 호출하는 코루틴 시작
    }

    void Update()
    {
        if(direction == Vector2Int.zero)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
                direction = Vector2Int.up;
            else if(Input.GetKeyDown(KeyCode.DownArrow))
                direction = Vector2Int.down;
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
                direction = Vector2Int.left;
            else if(Input.GetKeyDown(KeyCode.RightArrow))
                direction = Vector2Int.right;
        }

        else
        {
            // 반대 방향 전환 방지
            if (Input.GetKeyDown(KeyCode.UpArrow) && lastInputDirection != Vector2Int.down)
                direction = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow) && lastInputDirection != Vector2Int.up)
                direction = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && lastInputDirection != Vector2Int.right)
                direction = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) && lastInputDirection != Vector2Int.left)
                direction = Vector2Int.right;
        }
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            if(direction == Vector2Int.zero)
            {
                yield return null;
                continue;
            }
            yield return new WaitForSeconds(moveInterval); // 기본 0.2초

            lastInputDirection = direction;
            
            // [핵심] 꼬리부터 앞 마디의 위치로 한 칸씩 이동 (역순 루프)
            for (int i = bodyParts.Count - 1; i > 0; i--)
            {
                bodyParts[i].position = bodyParts[i - 1].position;
            }

            // 머리 이동
            transform.position += (Vector3Int)direction;
        }
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
            // 시작 직후(0.1초 이내) 발생하는 충돌은 무시하는 안전장치를 넣을 수도 있습니다.
            if (Time.timeSinceLevelLoad < 0.1f) return; 

            Debug.Log($"게임 오버! 부딪힌 대상: {collision.gameObject.tag}");
            Time.timeScale = 0;
        }
    }

    void FinishSnakeGame()
    {
        Debug.Log("목적지 도착! 이제 타워 디펜스를 시작합니다.");
    
        // 1. 이동 코루틴 중지
        StopAllCoroutines();
    
        BakePath();

        WaveManager.Instance.StartWave();
    }

    private void BakePath()
    {
        finalPath.Clear();

        // 1. 데이터 추출 (중요: 적은 꼬리 -> 머리 방향으로 옵니다)
        // 현재 bodyParts[0]은 머리(Goal 위치), 마지막은 꼬리입니다.
        // 따라서 역순으로 돌지 않고, 정순으로 담은 뒤 나중에 적이 거꾸로 쓰거나,
        // 여기서 아예 꼬리부터 머리 순서로 뒤집어서 저장합니다.

        for (int i = bodyParts.Count - 1; i >= 0; i--)
        {
            finalPath.Add(bodyParts[i].position);
        }

        foreach (Transform part in bodyParts)
        {
            // 색상 변경 (SpriteRenderer가 있다면)
            if (part.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
            {
                sprite.color = roadColor; // 회색 등으로 변경
                sprite.sortingOrder = -1; // 적이나 타워보다 뒤에 보이도록 순서 내리기
            }
        // 충돌체 끄기 (타워 설치 클릭 등에 방해되지 않게)
            if (part.TryGetComponent<Collider2D>(out Collider2D col))
            {
                col.enabled = false;
            }
        }

        Debug.Log("🐍 뱀 경로 베이킹 완료! 경로 길이: " + finalPath.Count);
    }

    void Grow()
    {
        // 새 몸통 생성
        GameObject newPart = Instantiate(bodyPrefab);
        // 화면 밖(-100, -100)에 임시로 생성
        newPart.transform.position = new Vector3(-100, -100, 0);
        // 리스트에 추가
        bodyParts.Add(newPart.transform);
    }

    public List<Vector3> GetSnakePath()
{
    List<Vector3> path = new List<Vector3>();
    // 적이 꼬리에서 머리 방향으로 오게 하려면 리스트를 그대로 사용하거나 역순으로 담습니다.
    foreach (Transform part in bodyParts)
    {
        path.Add(part.position);
    }
    return path;
}
}