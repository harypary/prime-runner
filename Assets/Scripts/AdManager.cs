// ──────────────────────────────────────────────────────────────────────────
//  AdManager — AdMob リワード広告（3枠）
//
//  SDK導入手順:
//    Package Manager → Add package by name:
//    "com.google.ads.mobile" (Google Mobile Ads Unity Plugin)
//
//  SDKインストール後、Player Settings → Other Settings → Scripting Define Symbols に
//    GOOGLE_MOBILE_ADS
//  を追加すると本番広告が有効になります。
//
//  SDK未インストール時: 広告なし扱い（onFailed を即コール）。
//  審査対応: 広告ロード失敗 → onFailed 呼び出しのみ。報酬は付与しない。
// ──────────────────────────────────────────────────────────────────────────
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    // ── 本番 Ad Unit ID ─────────────────────────────────────────────────
    // App ID (AndroidManifest / Info.plist に設定): ca-app-pub-8388601065600220~2483206060
    const string UNIT_STAMINA  = "ca-app-pub-8388601065600220/8392353301";
    const string UNIT_CONTINUE = "ca-app-pub-8388601065600220/2186037602";
    const string UNIT_COIN     = "ca-app-pub-8388601065600220/2597313172";

#if GOOGLE_MOBILE_ADS
    RewardedAd _staminaAd;
    RewardedAd _continueAd;
    RewardedAd _coinAd;
    bool       _initialized;
#endif

    // ═════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
#if GOOGLE_MOBILE_ADS
        // デフォルト設定を適用（各フィールドは読み取り専用のため new のみ）
        MobileAds.SetRequestConfiguration(new RequestConfiguration());

        MobileAds.Initialize(status =>
        {
            _initialized = true;
            Debug.Log("[AdManager] AdMob 初期化完了");
            LoadAll();
        });
#else
        Debug.Log("[AdManager] Google Mobile Ads SDK 未インストール。広告は無効です。");
#endif
    }

#if GOOGLE_MOBILE_ADS
    // ── ロード ────────────────────────────────────────────────────────────
    void LoadAll()
    {
        LoadUnit(UNIT_STAMINA,  ad => _staminaAd  = ad);
        LoadUnit(UNIT_CONTINUE, ad => _continueAd = ad);
        LoadUnit(UNIT_COIN,     ad => _coinAd     = ad);
    }

    static AdRequest BuildRequest()
    {
        // npa=1: 非パーソナライズ広告（ユーザー追跡なし・ATT未使用）
        var req = new AdRequest();
        req.Extras = new Dictionary<string, string> { { "npa", "1" } };
        return req;
    }

    void LoadUnit(string unitId, Action<RewardedAd> setter)
    {
        RewardedAd.Load(unitId, BuildRequest(), (ad, err) =>
        {
            if (err != null || ad == null)
            {
                Debug.LogWarning($"[AdManager] ロード失敗 {unitId}: {err?.GetMessage()}");
                return;
            }
            setter(ad);
            Debug.Log($"[AdManager] ロード成功: {unitId}");
        });
    }

    // ── 表示 ──────────────────────────────────────────────────────────────
    // onNoAd : 広告を表示できなかった旨のUI通知（常に onReward も呼ばれる）
    void ShowUnit(ref RewardedAd adRef, string unitId,
                  Action onReward, Action onNoAd)
    {
        var ad = adRef;
        if (ad == null)
        {
            Debug.Log($"[AdManager] 広告未ロード: {unitId}");
            onNoAd?.Invoke();
            onReward?.Invoke();             // 広告なしでも恩恵付与
            LoadUnit(unitId, a => adRef = a);
            return;
        }

        ad.OnAdFullScreenContentClosed += () =>
        {
            adRef = null;
            LoadUnit(unitId, a => adRef = a);
        };
        ad.OnAdFullScreenContentFailed += err =>
        {
            Debug.LogWarning($"[AdManager] 表示失敗: {err.GetMessage()}");
            adRef = null;
            onNoAd?.Invoke();
            onReward?.Invoke();             // 表示失敗でも恩恵付与
            LoadUnit(unitId, a => adRef = a);
        };

        ad.Show(_ => onReward?.Invoke());
    }
#endif

    // ═════════════════════════════════════════════════════════════════════
    //  公開 API
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>スタミナ回復広告。広告なし時も onReward を呼ぶ。onNoAd は通知用。</summary>
    public void ShowStaminaAd(Action onReward, Action onNoAd = null)
    {
#if GOOGLE_MOBILE_ADS
        ShowUnit(ref _staminaAd, UNIT_STAMINA, onReward, onNoAd);
#else
        onNoAd?.Invoke();
        onReward?.Invoke();
#endif
    }

    /// <summary>コンティニュー広告。広告なし時も onReward を呼ぶ。</summary>
    public void ShowContinueAd(Action onReward, Action onNoAd = null)
    {
#if GOOGLE_MOBILE_ADS
        ShowUnit(ref _continueAd, UNIT_CONTINUE, onReward, onNoAd);
#else
        onNoAd?.Invoke();
        onReward?.Invoke();
#endif
    }

    /// <summary>コイン×2広告。広告なし時も onReward を呼ぶ。</summary>
    public void ShowCoinBoostAd(Action onReward, Action onNoAd = null)
    {
#if GOOGLE_MOBILE_ADS
        ShowUnit(ref _coinAd, UNIT_COIN, onReward, onNoAd);
#else
        onNoAd?.Invoke();
        onReward?.Invoke();
#endif
    }
}
