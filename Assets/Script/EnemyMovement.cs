using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private List<Vector3> pathPoints;
    private int currentPointIndex = 0;
    public float speed = 2f;

    // 외부(스포너)에서 경로를 넣어주는 함수
    public void SetPath(List<Vector3> newPath)
    {
        pathPoints = newPath;
        currentPointIndex = 0;
        
        // 시작 위치를 경로의 첫 번째(꼬리 있던 곳)로 이동
        if (pathPoints != null && pathPoints.Count > 0)
        {
            transform.position = pathPoints[0];
        }
    }

    void Update()
    {
        if (pathPoints == null || currentPointIndex >= pathPoints.Count) return;

        // 목표 지점 (다음 웨이포인트)
        Vector3 target = pathPoints[currentPointIndex];
        
        // 이동 (MoveTowards 사용)
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // 목표 지점에 거의 도달했으면 다음 인덱스로 넘김
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Count)
            {
                Debug.Log("적군이 목적지(뱀 머리)에 도착했습니다! (대미지 입음)");
                Destroy(gameObject); // 일단은 사라지게 처리
            }
        }
    }
}