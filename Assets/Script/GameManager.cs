using UnityEngine;
using TMPro; // TextMeshPro UI 사용 시

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Resources")]
    public int currentGold = 0;
    public int currentScore = 0;
    public int playerHealth = 10; // 기지 체력

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI healthText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        UpdateUI();
    }

    // 골드 추가 (사과 먹을 때, 적 죽일 때)
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    // 골드 사용 (타워 지을 때) -> 성공하면 true, 돈 부족하면 false 반환
    public bool UseGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            // 여기에 '띠링~' 하는 경고음 재생 로직 추가 가능
            return false;
        }
    }

    // 기지 데미지 처리
    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        UpdateUI();

        if (playerHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (goldText != null) goldText.text = $"Gold: {currentGold}";
        if (healthText != null) healthText.text = $"HP: {playerHealth}";
    }

    void GameOver()
    {
        Debug.Log("게임 오버!");
        Time.timeScale = 0;
        // 게임 오버 UI 팝업 띄우기
    }
}