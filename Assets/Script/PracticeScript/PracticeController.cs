using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class PracticeController : MonoBehaviour
{
    [SerializeField] private float speed = 0.2f;
    private Vector2Int direction = Vector2Int.zero; // 현재 이동 방향
    private Queue<Vector2Int> PQueue = new Queue<Vector2Int>();

    void Start()
    {
        StartCoroutine(move());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            direction = Vector2Int.zero;
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            EnqueueDirection(Vector2Int.left);
        } else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            EnqueueDirection(Vector2Int.right);
        } else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            EnqueueDirection(Vector2Int.up);
        } else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            EnqueueDirection(Vector2Int.down);
        }
    }

    IEnumerator move()
    {
        while (true)
        {
            if(direction == Vector2Int.zero)
            {
                yield return null;
                continue;
            }
            yield return new WaitForSeconds(speed);
            transform.position += (Vector3Int)direction;
        }
    }

    private void EnqueueDirection(Vector2Int newDirection)
    {
        // 현재 방향과 반대 방향으로는 이동할 수 없음
        if (PQueue.Count == 0)
        {
            if (newDirection + direction == Vector2Int.zero) return;
        }
        else
        {
            if (newDirection + PQueue.Last() == Vector2Int.zero) return;
        }

        // 입력 버퍼에 새 방향 추가
        PQueue.Enqueue(newDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Wall")
        {
            Debug.Log("벽 충돌");
        }

        transform.position = Vector3.zero;
        direction = Vector2Int.zero;
    }
}