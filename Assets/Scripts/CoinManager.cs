using UnityEngine;

/// <summary>
/// コイン残高を管理する。PlayerPrefs で永続化。
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    const string KEY = "PrimeRunner_Coins";

    public int Coins { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Coins = PlayerPrefs.GetInt(KEY, 0);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        PlayerPrefs.SetInt(KEY, Coins);
        UIManager.Instance?.UpdateCoinDisplay(Coins);
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        PlayerPrefs.SetInt(KEY, Coins);
        UIManager.Instance?.UpdateCoinDisplay(Coins);
        return true;
    }
}
