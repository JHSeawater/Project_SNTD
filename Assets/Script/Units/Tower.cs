using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Stats")]
    public string TowerName = "Basic Tower";
    public int Cost = 50;
    public float Range = 3f;
    public float Damage = 10f;
    public float FireRate = 1f;

    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint; // 발사 위치 (옵션, 없으면 타워 중심)

    private Transform _target;
    private float _fireCountdown = 0f;
    
    // [최적화] GC 방지를 위한 충돌체 검사 버퍼 (최대 20개까지만 검사)
    private Collider2D[] _targetBuffer = new Collider2D[20];

    void Start()
    {
        // 0.5초마다 타겟 갱신 (매 프레임 체크는 비효율적)
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    void UpdateTarget()
    {
        // 1. 사거리 내의 콜라이더 검출 (NonAlloc 사용으로 메모리 최적화)
        // Physics2D.OverlapCircleAll 대신 미리 할당된 배열을 재사용
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, Range, _targetBuffer);
        
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _targetBuffer[i];
            
            if (hit.CompareTag(Define.Tags.Enemy))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = hit.gameObject;
                }
            }
        }

        // 2. 가장 가까운 적을 타겟으로 설정
        if (nearestEnemy != null && shortestDistance <= Range)
        {
            _target = nearestEnemy.transform;
            // Debug.Log($"타겟 설정 완료: {_target.name}");
        }
        else
        {
            _target = null;
        }
    }

    void Update()
    {
        if (_target == null) return;

        // 공격 쿨타임 체크
        if (_fireCountdown <= 0f)
        {
            Shoot();
            _fireCountdown = 1f / FireRate; // 공속에 따른 쿨타임 재설정
        }

        _fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogError("타워에 총알(Projectile Prefab)이 연결되지 않았습니다! Inspector를 확인하세요.");
            return;
        }

        // 발사체 생성
        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
        GameObject bulletGO = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
        Debug.Log("총알 발사!");

        // 투사체 세팅
        Projectile projectile = bulletGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(_target, Damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 에디터에서 사거리를 시각적으로 확인하기 위함
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
