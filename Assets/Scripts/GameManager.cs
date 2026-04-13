using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { Title, Game, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲートPrefab")]
    public GameObject gatePairPrefab;

    [Header("スポーン設定")]
    public float spawnAhead   = 130f;
    public float gateInterval = 120f;
    public int   preloadCount = 3;

    [Header("参照")]
    public PlayerController player;

    [HideInInspector] public GamePhase phase        = GamePhase.Title;
    [HideInInspector] public int       score        = 0;
    [HideInInspector] public int       highScore    = 0;
    [HideInInspector] public int       hp           = 3;
    [HideInInspector] public int       combo        = 0;
    [HideInInspector] public int       coinMultiplier = 1;   // 将来拡張用（現在は常に1）
    [HideInInspector] public int       sessionCoins   = 0;   // このゲームで獲得したコイン

    // コンティニュー回数（1ゲーム中最大2回）
    public int ContinueCount { get; private set; } = 0;
    public const int MaxContinue = 2;
    public bool CanContinue => ContinueCount < MaxContinue;

    private float nextGateZ;
    private float speedTimer     = 0f;

    // FindObjectsByType を毎フレーム呼ぶのをやめてリストで管理（がたつき解消）
    private readonly List<GatePair> activePairs = new List<GatePair>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (phase != GamePhase.Game) return;

        speedTimer += Time.deltaTime;
        player.forwardSpeed = 21f + Mathf.Floor(speedTimer / 15f) * 2.25f + score * 0.15f;

        float playerZ = player.transform.position.z;

        while (nextGateZ < playerZ + spawnAhead)
        {
            SpawnGateAt(nextGateZ);
            nextGateZ += gateInterval;
        }

        // リストを逆順ループして安全に削除
        for (int i = activePairs.Count - 1; i >= 0; i--)
        {
            var pair = activePairs[i];
            if (pair == null) { activePairs.RemoveAt(i); continue; }

            float relZ = pair.transform.position.z - playerZ;

            if (!pair.previewed && relZ > 0f && relZ <= 50f)
            {
                pair.previewed = true;
                UIManager.Instance.ShowPreview(pair.leftNumber, pair.rightNumber);
            }

            if (!pair.judged && relZ <= 0f)
            {
                pair.judged = true;
                OnGatePassed(pair);
            }

            if (relZ < -10f)
            {
                activePairs.RemoveAt(i);
                Destroy(pair.gameObject);
            }
        }
    }

    public void StartGame()
    {
        // 実行中のコルーチン（GameOverSequence等）を全停止してから初期化
        StopAllCoroutines();

        // 時間を必ず1倍に戻す（スローモー中にリトライされた場合の対策）
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;

        foreach (var g in activePairs)
            if (g != null) Destroy(g.gameObject);
        activePairs.Clear();

        // UIのコルーチン・エフェクトもリセット
        UIManager.Instance?.ResetEffects();

        score          = 0;
        hp             = 3;
        combo          = 0;
        sessionCoins   = 0;
        speedTimer     = 0f;
        coinMultiplier = 1;
        ContinueCount  = 0;
        phase          = GamePhase.Game;
        PrimeMath.ResetHistory(); // 数字の使用履歴をリセット

        player.forwardSpeed = 21f;
        player.ResetOnSpawn(); // CC無効化→位置セット→CC有効化まで含む

        // キャラクター姿勢リセット（前のめりアニメが残らないように）
        player.GetComponentInChildren<ProceduralRunAnimation>()?.ResetPose();

        if (InputManager.Instance == null)
        {
            var im = new GameObject("InputManager");
            im.AddComponent<InputManager>();
        }

        nextGateZ = player.transform.position.z + spawnAhead * 0.5f;
        for (int i = 0; i < preloadCount; i++)
        {
            SpawnGateAt(nextGateZ);
            nextGateZ += gateInterval;
        }

        UIManager.Instance.ShowGame();
        UIManager.Instance.UpdateScore(0);
        UIManager.Instance.UpdateHp(3);

        if (CityLevelGenerator.Instance != null)
            CityLevelGenerator.Instance.StartGeneration();
    }

    void SpawnGateAt(float z)
    {
        var (prime, composite) = PrimeMath.GenPair(score);
        bool leftIsPrime = Random.value < 0.5f;

        var obj = Instantiate(gatePairPrefab);
        obj.transform.position = new Vector3(0f, 0f, z);

        var pair = obj.GetComponent<GatePair>();
        activePairs.Add(pair);
        pair.Setup(leftIsPrime ? prime : composite,
                   leftIsPrime ? composite : prime,
                   leftIsPrime);
    }

    void OnGatePassed(GatePair pair)
    {
        int  chosenNum = player.transform.position.x < 0f ? pair.leftNumber : pair.rightNumber;
        bool correct   = PrimeMath.IsPrime(chosenNum);

        if (correct)
        {
            combo  = Mathf.Min(combo + 1, 10);
            score += 1;                              // スコア = 通過数（+1）
            int earned = combo * coinMultiplier;     // コイン = コンボ数 × 倍率
            CoinManager.Instance?.AddCoins(earned);
            sessionCoins += earned;
            UIManager.Instance?.UpdateSessionCoinDisplay(sessionCoins);
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateCombo(combo);
            string x2tag = coinMultiplier > 1 ? " コイン×2!" : "";
            string msg = combo >= 2
                ? $"×{combo}コンボ!  +{earned}コイン{x2tag}"
                : $"正解!  +{earned}コイン{x2tag}";
            UIManager.Instance.ShowEffect(msg, true);
        }
        else
        {
            combo = 0;
            UIManager.Instance.UpdateCombo(0);
            hp--;
            UIManager.Instance.UpdateHp(hp);
            string factored = PrimeMath.Factorize(chosenNum);

            if (hp <= 0)
            {
                phase     = GamePhase.GameOver;
                highScore = Mathf.Max(highScore, score);
                StartCoroutine(GameOverSequence($"ドボン！  {chosenNum}＝{factored}"));
            }
            else
            {
                UIManager.Instance.ShowEffect($"ミス！  {chosenNum}={factored}  残り{hp}回", false);
            }
        }
    }

    /// <summary>リワード広告視聴後にコンティニュー（HP1で復活）最大2回</summary>
    public void Continue()
    {
        StopAllCoroutines();
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;

        ContinueCount++;
        hp    = 1;
        combo = 0;
        phase = GamePhase.Game;

        player.GetComponentInChildren<ProceduralRunAnimation>()?.ResetPose();
        ShopManager.Instance?.ApplySkin();

        UIManager.Instance.ResumeGame();
        UIManager.Instance.UpdateHp(1);
        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.UpdateCombo(0);
    }

    /// <summary>今回獲得したコインを2倍にする（広告視聴後にゲームオーバー画面から呼ぶ）</summary>
    public void ApplyCoinX2()
    {
        if (sessionCoins <= 0) return;
        CoinManager.Instance?.AddCoins(sessionCoins); // sessionCoins分追加 = 合計2倍
        sessionCoins *= 2;
    }

    IEnumerator GameOverSequence(string deathMsg)
    {
        // スローモーション
        Time.timeScale      = 0.15f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        UIManager.Instance.ShowEffect(deathMsg, false);

        // カメラシェイク（UnscaledTimeで動く）
        Camera.main?.GetComponent<CameraFollow>()?.Shake(1.8f, 0.7f);

        // 1.2秒リアルタイム待機
        yield return new WaitForSecondsRealtime(1.2f);

        // 赤フラッシュ
        UIManager.Instance.StartDeathFlash();

        yield return new WaitForSecondsRealtime(0.9f);

        // 通常速度に戻してゲームオーバー画面表示
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;

        UIManager.Instance.ShowGameOver(score, highScore, sessionCoins);
    }
}
