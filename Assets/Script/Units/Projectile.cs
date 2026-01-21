using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform _target;
    private float _speed = 10f; // 탄속
    private float _damage = 0f;

    // 타워에서 호출하여 목표 설정
    public void Seek(Transform target, float damage)
    {
        _target = target;
        _damage = damage;
    }

    void Update()
    {
        // 타겟이 날아가는 도중 사라지거나 죽었을 경우
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 이동 방향 계산
        Vector3 dir = _target.position - transform.position;
        float distanceThisFrame = _speed * Time.deltaTime;

        // 이번 프레임에 이동할 거리가 남은 거리보다 크다면 명중으로 판정
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // 이동
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        
        // 투사체가 타겟을 바라보게 회전 (선택 사항)
        // transform.up = dir.normalized; // 2D 스프라이트 기준
    }

    void HitTarget()
    {
        // 적에게 데미지 전달
        Enemy enemy = _target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
        }

        // 피격 이펙트가 있다면 여기서 생성
        
        Destroy(gameObject); // 투사체 소멸
    }
}
