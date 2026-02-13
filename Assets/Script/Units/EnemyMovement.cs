using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private List<Vector3> _pathPoints;
    private int _currentPointIndex = 0;
    
    [Header("Stats")]
    public float Speed = 2f;
    public int Damage = 1;

    // 외부(스포너)에서 경로를 넣어주는 함수
    public void SetPath(List<Vector3> newPath)
    {
        _pathPoints = newPath;
        _currentPointIndex = 0;
        
        // 시작 위치를 경로의 첫 번째(꼬리 있던 곳)로 이동
        if (_pathPoints != null && _pathPoints.Count > 0)
        {
            transform.position = _pathPoints[0];
        }
    }

    void Update()
    {
        if (_pathPoints == null || _currentPointIndex >= _pathPoints.Count) return;

        // 목표 지점 (다음 웨이포인트)
        Vector3 target = _pathPoints[_currentPointIndex];
        
        // 이동 (MoveTowards 사용)
        transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);

        // 목표 지점에 거의 도달했으면 다음 인덱스로 넘김
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            _currentPointIndex++;
            if (_currentPointIndex >= _pathPoints.Count)
            {
                ReachGoal();
            }
        }
    }

    void ReachGoal()
    {
        // 목적지 도착 시 데미지 처리
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(Damage);
        }
        else
        {
            Debug.LogWarning("GameManager 인스턴스를 찾을 수 없습니다.");
        }

        // [버그 수정] 적이 죽지 않고 도착해서 사라질 때도 카운트를 줄여야 함
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDied();
        }

        Destroy(gameObject);
    }
}