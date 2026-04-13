using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 素数判定・数字ペア生成・素因数分解のユーティリティ
/// </summary>
public static class PrimeMath
{
    // 300以下の素数セット
    private static readonly HashSet<int> PrimeSet = new HashSet<int>
    {
        2,3,5,7,11,13,17,19,23,29,31,37,41,43,47,53,59,61,67,71,73,79,83,89,97,
        101,103,107,109,113,127,131,137,139,149,151,157,163,167,173,179,181,191,
        193,197,199,211,223,227,229,233,239,241,251,257,263,269,271,277,281,283,293
    };

    // 素数に見えやすい合成数（2・5の倍数を含まず判断が難しい）
    private static readonly int[] TrickyComposites =
    {
        21, 27, 33, 35, 49, 51, 57, 63, 65, 77, 85, 87, 91,
        95, 119, 121, 133, 143, 145, 161, 169, 177, 187,
        201, 203, 209, 217, 221, 247, 253, 259, 287, 289
    };

    // ─── 直近使用数の履歴管理（同じ数字が繰り返し出ないようにする）───
    private static readonly HashSet<int> recentlyUsed = new HashSet<int>();
    private static readonly Queue<int>   recentQueue  = new Queue<int>();
    private const int HISTORY_SIZE = 6; // 直近3ペア分（6個）を記憶

    /// <summary>ゲーム開始時に履歴をリセットする</summary>
    public static void ResetHistory()
    {
        recentlyUsed.Clear();
        recentQueue.Clear();
    }

    static void AddToHistory(int n)
    {
        if (recentlyUsed.Contains(n)) return;
        recentQueue.Enqueue(n);
        recentlyUsed.Add(n);
        while (recentQueue.Count > HISTORY_SIZE)
            recentlyUsed.Remove(recentQueue.Dequeue());
    }

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (PrimeSet.Contains(n)) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        for (int i = 5; i * i <= n; i += 6)
            if (n % i == 0 || n % (i + 2) == 0) return false;
        return true;
    }

    /// <summary>合成数 n の素因数分解を "3×17" 形式で返す</summary>
    public static string Factorize(int n)
    {
        for (int i = 2; i * i <= n; i++)
            if (n % i == 0) return $"{i}×{n / i}";
        return n.ToString();
    }

    /// <summary>難易度に応じた数字ペアを生成（素数1個 + 合成数1個、直近の数字は除外）</summary>
    public static (int prime, int composite) GenPair(int score)
    {
        int maxNum = Mathf.Clamp(30 + score * 4, 30, 300);

        // ── 素数を乱択（直近使用済みを除外・候補ゼロなら制限なしで選ぶ）──
        var primes = new List<int>();
        for (int n = 2; n <= maxNum; n++)
            if (IsPrime(n) && !recentlyUsed.Contains(n)) primes.Add(n);
        if (primes.Count == 0) // フォールバック
            for (int n = 2; n <= maxNum; n++)
                if (IsPrime(n)) primes.Add(n);
        int prime = primes[Random.Range(0, primes.Count)];

        // ── 合成数を乱択（直近使用済み・選んだ素数と同じ数を除外）─────
        int composite;

        // スコア3以上: 55%の確率でひっかけ合成数を優先
        if (score >= 3 && Random.value < 0.55f)
        {
            var pool = System.Array.FindAll(TrickyComposites,
                n => n <= maxNum && !recentlyUsed.Contains(n) && n != prime);
            if (pool.Length == 0) // 制限なしフォールバック
                pool = System.Array.FindAll(TrickyComposites,
                    n => n <= maxNum && n != prime);
            if (pool.Length > 0)
            {
                composite = pool[Random.Range(0, pool.Length)];
                AddToHistory(prime);
                AddToHistory(composite);
                return (prime, composite);
            }
        }

        // 通常の合成数を乱択
        int tries = 0;
        do
        {
            composite = Random.Range(4, maxNum + 1);
            tries++;
        }
        while ((IsPrime(composite) || recentlyUsed.Contains(composite) || composite == prime)
               && tries < 300);

        AddToHistory(prime);
        AddToHistory(composite);
        return (prime, composite);
    }
}
