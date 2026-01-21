using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _maxHealth = 30f;
    private float _currentHealth;

    [Header("Rewards")]
    [SerializeField] private int _goldReward = 15; // 처치 시 획득 골드

    private bool _isDead = false; // 중복 사망 처리 방지 플래그

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        // 체력바 UI 등이 있다면 여기서 갱신

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        _isDead = true;
        
        // 골드 지급
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(_goldReward);
        }

        // 사망 이펙트가 있다면 여기서 생성 (Instantiate)
        
        Destroy(gameObject);
    }
}
