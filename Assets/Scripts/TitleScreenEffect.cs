using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// タイトル画面：素数が画面全体にまばらに漂うエフェクト
/// ・1個ずつ 0.8〜1.6秒間隔でスポーン
/// ・スポーン可能セクターを5か所に限定（ボタン・テキストを完全回避）
/// ・同時表示最大 10個
/// </summary>
public class TitleScreenEffect : MonoBehaviour
{
    static readonly int[] Primes =
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
        31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
        73, 79, 83, 89, 97, 101, 103, 107, 109, 113
    };

    static readonly Color[] NeonColors =
    {
        new Color(0.10f, 0.90f, 1.00f),  // シアン
        new Color(1.00f, 0.15f, 0.60f),  // マゼンタ
        new Color(0.20f, 1.00f, 0.30f),  // グリーン
        new Color(1.00f, 0.85f, 0.10f),  // イエロー
        new Color(0.70f, 0.15f, 1.00f),  // パープル
        new Color(1.00f, 0.45f, 0.10f),  // オレンジ
        new Color(0.10f, 0.60f, 1.00f),  // ブルー
    };

    // ── スポーン許可セクター（正規化座標 x,y,w,h）────────────────────────
    // キャンバス 1080×1920、センターアンカー基準
    //   StartButton  center y = (960-180)/1920 = 0.406  size h = 130/1920 = 0.068
    //   ShopButton   center y = (960-360)/1920 = 0.313  size h = 110/1920 = 0.057
    //   DescText     center y = (960+350)/1920 = 0.682
    //   TitleText    center y = (960+580)/1920 = 0.802
    //
    //   ボタン＋テキスト占有帯: y ≈ 0.28〜0.90  （バッファ込み）
    //   → 下部・左右ストリップ・上部だけを許可
    static readonly Rect[] SpawnSectors =
    {
        new Rect(0.04f, 0.03f, 0.92f, 0.22f),  // [0] 下部（ショップより下）
        new Rect(0.02f, 0.28f, 0.12f, 0.62f),  // [1] 左ストリップ
        new Rect(0.86f, 0.28f, 0.12f, 0.62f),  // [2] 右ストリップ
        new Rect(0.04f, 0.48f, 0.92f, 0.13f),  // [3] ボタン〜説明文の間の帯
        new Rect(0.04f, 0.90f, 0.92f, 0.08f),  // [4] 上部（タイトル上）
    };

    const int MaxAlive = 10;

    Coroutine         _spawnLoop;
    int               _lastSector = -1;
    readonly List<GameObject> _alive = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────
    public void StartEffect()
    {
        gameObject.SetActive(true);
        if (_spawnLoop != null) StopCoroutine(_spawnLoop);
        _spawnLoop = StartCoroutine(SpawnLoop());
    }

    public void StopEffect()
    {
        if (_spawnLoop != null) { StopCoroutine(_spawnLoop); _spawnLoop = null; }
        foreach (Transform child in transform) Destroy(child.gameObject);
        _alive.Clear();
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 上限超えたら古いものを削除
            _alive.RemoveAll(g => g == null);
            if (_alive.Count >= MaxAlive)
            {
                Destroy(_alive[0]);
                _alive.RemoveAt(0);
            }

            SpawnOne();
            yield return new WaitForSecondsRealtime(Random.Range(0.8f, 1.6f));
        }
    }

    void SpawnOne()
    {
        // 前回と異なるセクターをランダムに選ぶ
        int sec;
        do { sec = Random.Range(0, SpawnSectors.Length); }
        while (sec == _lastSector && SpawnSectors.Length > 1);
        _lastSector = sec;

        Rect r  = SpawnSectors[sec];
        float nx = r.x + Random.Range(0f, r.width);
        float ny = r.y + Random.Range(0f, r.height);

        var go = new GameObject("FloatNum");
        go.transform.SetParent(transform, false);
        _alive.Add(go);

        var tmp       = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = Primes[Random.Range(0, Primes.Length)].ToString();
        tmp.fontSize  = Random.Range(30f, 96f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        Color nc  = NeonColors[Random.Range(0, NeonColors.Length)];
        tmp.color = new Color(nc.r, nc.g, nc.b, 0f);

        var rt        = go.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(150f, 100f);
        rt.anchorMin  = rt.anchorMax = new Vector2(nx, ny);
        rt.anchoredPosition = Vector2.zero;

        StartCoroutine(Animate(tmp, rt));
    }

    IEnumerator Animate(TextMeshProUGUI tmp, RectTransform rt)
    {
        float duration = Random.Range(5f, 9f);
        float angle    = Random.Range(0f, 360f);
        float speed    = Random.Range(15f, 45f);     // ゆっくり漂う
        float rotSpeed = Random.Range(-35f, 35f);    // 穏やかな回転

        Vector2 dir      = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                       Mathf.Sin(angle * Mathf.Deg2Rad));
        float elapsed    = 0f;
        Color baseCol    = tmp.color;
        Vector2 startPos = rt.anchoredPosition;
        Vector3 startRot = rt.localEulerAngles;

        while (elapsed < duration)
        {
            if (tmp == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // フェードイン(20%) → 維持 → フェードアウト(25%)
            float alpha = t < 0.20f ? t / 0.20f
                        : t > 0.75f ? (1f - t) / 0.25f
                        : 1f;
            tmp.color = new Color(baseCol.r, baseCol.g, baseCol.b, alpha * 0.75f);

            rt.anchoredPosition = startPos + dir * speed * elapsed;
            rt.localEulerAngles = startRot + new Vector3(0f, 0f, rotSpeed * elapsed);

            yield return null;
        }

        if (tmp != null) Destroy(tmp.gameObject);
    }
}
