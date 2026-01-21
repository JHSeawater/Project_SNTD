using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("Tower Prefabs")]
    public GameObject BasicTowerPrefab;
    public GameObject RapidTowerPrefab;
    public GameObject SniperTowerPrefab;

    public void SelectBasicTower()
    {
        Debug.Log("상점: 기본 타워 선택");
        BuildManager.Instance.SelectTower(BasicTowerPrefab);
    }

    public void SelectRapidTower()
    {
        Debug.Log("상점: 속사 타워 선택");
        BuildManager.Instance.SelectTower(RapidTowerPrefab);
    }

    public void SelectSniperTower()
    {
        Debug.Log("상점: 스나이퍼 타워 선택");
        BuildManager.Instance.SelectTower(SniperTowerPrefab);
    }
}
