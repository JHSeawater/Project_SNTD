using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class PracticeController : MonoBehaviour
{
    [SerializeField] private float speed = 0.2f;
    private Vector2Int direction = Vector2Int.zero; // 현재 이동 방향

    void Start()
    {
        StartCoroutine(move());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector2Int.left;
        } else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector2Int.right;
        } else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2Int.up;
        } else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector2Int.down;
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