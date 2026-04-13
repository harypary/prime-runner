using System;
using UnityEngine;

/// <summary>
/// スタミナ管理（最大3、5分で1回復）
/// </summary>
public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance { get; private set; }

    const int   MAX_STAMINA     = 3;
    const float REGEN_SECONDS   = 300f;   // 5分
    const string KEY_STAMINA    = "PrimeRunner_Stamina";
    const string KEY_REGEN_TICK = "PrimeRunner_StaminaRegenTick"; // 最後に「満タンでない」になった時刻

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int MaxStamina => MAX_STAMINA;

    // ── スタミナ取得（回復計算込み）───────────────────────────────────────
    public int GetStamina()
    {
        int saved = PlayerPrefs.GetInt(KEY_STAMINA, MAX_STAMINA);
        if (saved >= MAX_STAMINA) return MAX_STAMINA;

        string tickStr = PlayerPrefs.GetString(KEY_REGEN_TICK, "");
        if (string.IsNullOrEmpty(tickStr)) return Mathf.Max(0, saved);

        if (!long.TryParse(tickStr, out long ticks)) return Mathf.Max(0, saved);

        DateTime regenStart = new DateTime(ticks, DateTimeKind.Utc);
        double elapsedSec   = (DateTime.UtcNow - regenStart).TotalSeconds;
        int    recovered    = Mathf.FloorToInt((float)elapsedSec / REGEN_SECONDS);

        if (recovered <= 0) return Mathf.Max(0, saved);

        int newVal = Mathf.Min(saved + recovered, MAX_STAMINA);

        // 回復分だけ基準時刻を進める
        DateTime newBase = regenStart.AddSeconds(recovered * REGEN_SECONDS);
        SaveState(newVal, newVal < MAX_STAMINA ? newBase.Ticks.ToString() : "");

        return newVal;
    }

    // ── スタミナ消費（ゲーム開始時）──────────────────────────────────────
    public bool UseStamina()
    {
        int current = GetStamina();
        if (current <= 0) return false;

        int newVal = current - 1;
        // 満タン→不満になった瞬間にタイマー開始
        string tickStr = PlayerPrefs.GetString(KEY_REGEN_TICK, "");
        string newTick = (newVal < MAX_STAMINA && string.IsNullOrEmpty(tickStr))
                         ? DateTime.UtcNow.Ticks.ToString()
                         : tickStr;
        // current が MAX_STAMINA だった（タイマーが止まっていた）場合は今から開始
        if (current == MAX_STAMINA)
            newTick = DateTime.UtcNow.Ticks.ToString();

        SaveState(newVal, newVal < MAX_STAMINA ? newTick : "");
        UIManager.Instance?.UpdateStaminaDisplay();
        return true;
    }

    // ── スタミナ追加（広告報酬など）──────────────────────────────────────
    public void AddStamina(int amount = 1)
    {
        int newVal = Mathf.Min(GetStamina() + amount, MAX_STAMINA);
        string tick = newVal < MAX_STAMINA
                      ? PlayerPrefs.GetString(KEY_REGEN_TICK, DateTime.UtcNow.Ticks.ToString())
                      : "";
        SaveState(newVal, tick);
        UIManager.Instance?.UpdateStaminaDisplay();
    }

    // ── 次回回復まで残り秒数 ──────────────────────────────────────────────
    public float SecondsUntilNextRegen()
    {
        if (GetStamina() >= MAX_STAMINA) return 0f;
        string tickStr = PlayerPrefs.GetString(KEY_REGEN_TICK, "");
        if (string.IsNullOrEmpty(tickStr)) return 0f;
        if (!long.TryParse(tickStr, out long ticks)) return 0f;

        DateTime regenStart = new DateTime(ticks, DateTimeKind.Utc);
        double elapsed = (DateTime.UtcNow - regenStart).TotalSeconds;
        float  rem     = REGEN_SECONDS - (float)(elapsed % REGEN_SECONDS);
        return Mathf.Max(0f, rem);
    }

    public string NextRegenTimeString()
    {
        float s = SecondsUntilNextRegen();
        if (s <= 0f) return "";
        int m = (int)(s / 60);
        int sec = (int)(s % 60);
        return $"{m:D2}:{sec:D2}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    void SaveState(int stamina, string regenTick)
    {
        PlayerPrefs.SetInt(KEY_STAMINA, stamina);
        PlayerPrefs.SetString(KEY_REGEN_TICK, regenTick);
        PlayerPrefs.Save();
    }
}
