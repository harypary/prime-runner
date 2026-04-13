using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("パネル")]
    public GameObject titlePanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    [Header("ゲーム中 HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI previewText;

    [Header("ゲームオーバー")]
    public TextMeshProUGUI goScoreText;
    public TextMeshProUGUI goHighScoreText;
    public TextMeshProUGUI goEarnedCoinText;
    public TextMeshProUGUI goTotalCoinText;

    [Header("ハイスコア表示（タイトル）")]
    public TextMeshProUGUI titleHighScoreText;

    [Header("コイン・広告ボタン（AutoSetupが設定）")]
    public TextMeshProUGUI titleCoinText;
    public TextMeshProUGUI gameCoinText;
    public Button          continueBtn;
    public Button          coinBoostBtn;   // 旧 scoreX2Btn → コイン×2
    public Button          shopBtn;

    // ── 内部 ──────────────────────────────────────────────────────────────────
    Coroutine         previewCoroutine;
    Image             _flashOverlay;
    TitleScreenEffect _titleEffect;
    UIRunnerCharacter _runner;

    // タイトル背景
    bool                 _titleBgBuilt = false;

    // スタミナ UI
    TextMeshProUGUI      _staminaText;
    TextMeshProUGUI      _staminaTimerText;
    GameObject           _staminaDialog;
    Coroutine            _staminaTimerCo;

    // トースト通知（全画面共通）
    CanvasGroup          _toastGroup;
    TextMeshProUGUI      _toastTMP;
    bool                 _toastBuilt;

    // ショップパネル
    Sprite               _roundedSprite;
    GameObject           _shopPanel;
    TextMeshProUGUI      _shopCoinLabel;
    GameObject           _shopSkinContent;
    GameObject           _shopCityContent;
    bool                 _shopShowSkins = true;

    struct ShopItemUI { public Button btn; public TextMeshProUGUI priceText; }
    ShopItemUI[] _skinItems = new ShopItemUI[5];
    ShopItemUI[] _cityItems = new ShopItemUI[5];

    TMP_FontAsset _jpFont;

    // ═══════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (effectText)  effectText.alpha  = 0f;
        if (previewText) previewText.alpha = 0f;
        if (comboText)   comboText.text    = "";

        // フォントを既存テキストから取得（日本語フォントを runtime 生成 TMP に使う）
        _jpFont = scoreText?.font;

        // シングルトン生成（AutoSetupTownRun で作られている場合はスキップ）
        if (CoinManager.Instance    == null) new GameObject("CoinManager").AddComponent<CoinManager>();
        if (AdManager.Instance      == null) new GameObject("AdManager").AddComponent<AdManager>();
        if (ShopManager.Instance    == null) new GameObject("ShopManager").AddComponent<ShopManager>();
        if (StaminaManager.Instance == null) new GameObject("StaminaManager").AddComponent<StaminaManager>();

        // タイトル演出（浮遊する素数）
        var effectGO = new GameObject("TitleScreenEffect");
        effectGO.transform.SetParent(transform, false);
        _titleEffect = effectGO.AddComponent<TitleScreenEffect>();
        effectGO.SetActive(false);

        // タイトル演出（横向き走るキャラクター）
        var runnerGO = new GameObject("UIRunnerCharacter");
        runnerGO.transform.SetParent(transform, false);
        _runner = runnerGO.AddComponent<UIRunnerCharacter>();
        runnerGO.SetActive(false);

        // 死亡フラッシュ用フルスクリーンオーバーレイ（最前面）
        var flashGO = new GameObject("DeathFlash");
        flashGO.transform.SetParent(transform, false);
        _flashOverlay = flashGO.AddComponent<Image>();
        _flashOverlay.color = new Color(1f, 0f, 0f, 0f);
        _flashOverlay.raycastTarget = false;
        var rt = _flashOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // 角丸スプライト生成
        _roundedSprite = MakeRoundedSprite(128, 26);

        // ショップパネル構築
        BuildShopPanel();

        // ボタンコールバック（AutoSetupTownRun で設定されない場合のフォールバック）
        if (continueBtn)  continueBtn.onClick.AddListener(OnContinueButton);
        if (coinBoostBtn) coinBoostBtn.onClick.AddListener(OnCoinBoostButton);
        if (shopBtn)      shopBtn.onClick.AddListener(OnShopButton);

        // スタミナダイアログ構築
        BuildStaminaDialog();

        // Canvas 内の全ボタンに角丸適用（StartButton・RetryButton なども含む）
        var rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas)
            foreach (var btn in rootCanvas.GetComponentsInChildren<Button>(true))
                ApplyRoundedImg(btn.GetComponent<Image>());
    }

    void Start()
    {
        ShowTitle();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ショップパネル構築
    // ═══════════════════════════════════════════════════════════════════════════
    void BuildShopPanel()
    {
        // ── パネル本体 ───────────────────────────────────────────────────────
        _shopPanel = new GameObject("ShopPanel");
        _shopPanel.transform.SetParent(transform, false);
        var panelImg = _shopPanel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.06f, 0.12f, 0.97f);
        var panelRt = _shopPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;
        _shopPanel.SetActive(false);

        var cv  = _shopPanel.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 100;
        _shopPanel.AddComponent<GraphicRaycaster>();

        // ── タイトル ─────────────────────────────────────────────────────────
        RtTMP(_shopPanel.transform, "ShopTitle", "ショップ", 72f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, 840f), new Vector2(600f, 110f))
            .color = new Color(0f, 1f, 0.67f);

        // ── コイン表示 ───────────────────────────────────────────────────────
        _shopCoinLabel = RtTMP(_shopPanel.transform, "ShopCoin", "コイン: 0", 44f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, 730f), new Vector2(500f, 65f));
        _shopCoinLabel.color = new Color(1f, 0.85f, 0f);

        // ── タブボタン ───────────────────────────────────────────────────────
        var skinTab = RtButton(_shopPanel.transform, "SkinTab", "キャラスキン", 42f,
            new Vector2(-210f, 620f), new Vector2(380f, 80f), new Color(0.15f, 0.50f, 0.20f));
        skinTab.onClick.AddListener(() => { _shopShowSkins = true; UpdateShopTabs(); });

        var cityTab = RtButton(_shopPanel.transform, "CityTab", "街並み", 42f,
            new Vector2(210f, 620f), new Vector2(380f, 80f), new Color(0.20f, 0.30f, 0.55f));
        cityTab.onClick.AddListener(() => { _shopShowSkins = false; UpdateShopTabs(); });

        // ── スキンコンテンツ ─────────────────────────────────────────────────
        _shopSkinContent = new GameObject("SkinContent");
        _shopSkinContent.transform.SetParent(_shopPanel.transform, false);
        var scRt = _shopSkinContent.AddComponent<RectTransform>();
        scRt.anchorMin = scRt.anchorMax = new Vector2(0.5f, 0.5f);
        scRt.sizeDelta = new Vector2(1000f, 800f);
        scRt.anchoredPosition = new Vector2(0f, 150f);

        for (int i = 0; i < 5; i++)
        {
            float y = 320f - i * 160f;
            var s = SkinDef.All[i];
            _skinItems[i] = BuildShopRow(_shopSkinContent.transform, "SkinRow" + i,
                s.shirt, s.name, y, true, i);
        }

        // ── 街並みコンテンツ ─────────────────────────────────────────────────
        _shopCityContent = new GameObject("CityContent");
        _shopCityContent.transform.SetParent(_shopPanel.transform, false);
        var ccRt = _shopCityContent.AddComponent<RectTransform>();
        ccRt.anchorMin = ccRt.anchorMax = new Vector2(0.5f, 0.5f);
        ccRt.sizeDelta = new Vector2(1000f, 800f);
        ccRt.anchoredPosition = new Vector2(0f, 150f);

        for (int i = 0; i < 5; i++)
        {
            float y = 320f - i * 160f;
            var c = CityDef.All[i];
            _cityItems[i] = BuildShopRow(_shopCityContent.transform, "CityRow" + i,
                c.previewColor, c.name, y, false, i);
        }

        // ── 閉じるボタン ─────────────────────────────────────────────────────
        var closeBtn = RtButton(_shopPanel.transform, "CloseBtn", "閉じる", 46f,
            new Vector2(0f, -730f), new Vector2(440f, 100f), new Color(0.40f, 0.10f, 0.10f));
        closeBtn.onClick.AddListener(HideShop);

        UpdateShopTabs();
    }

    ShopItemUI BuildShopRow(Transform parent, string name, Color swatch,
        string itemName, float y, bool isSkin, int index)
    {
        var row = new GameObject(name);
        row.transform.SetParent(parent, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0f, y);
        rowRt.sizeDelta = new Vector2(960f, 140f);

        // スウォッチ
        var sw = new GameObject("Swatch");
        sw.transform.SetParent(row.transform, false);
        sw.AddComponent<Image>().color = swatch;
        var swRt = sw.GetComponent<RectTransform>();
        swRt.anchorMin = swRt.anchorMax = new Vector2(0f, 0.5f);
        swRt.pivot = new Vector2(0f, 0.5f);
        swRt.anchoredPosition = new Vector2(10f, 0f);
        swRt.sizeDelta = new Vector2(90f, 90f);

        // 名前
        var nameTmp = RtTMP(row.transform, "NameTmp", itemName, 40f,
            new Vector2(0f,0.5f), new Vector2(0f,0.5f), new Vector2(0f,0.5f),
            new Vector2(120f, 0f), new Vector2(320f, 60f));
        nameTmp.alignment = TextAlignmentOptions.Left;

        // 価格テキスト
        var priceGO = new GameObject("PriceTmp");
        priceGO.transform.SetParent(row.transform, false);
        var pRt = priceGO.AddComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = new Vector2(10f, 0f);
        pRt.sizeDelta = new Vector2(240f, 60f);
        var pTmp = priceGO.AddComponent<TextMeshProUGUI>();
        pTmp.text = "---";
        pTmp.fontSize = 36f;
        pTmp.alignment = TextAlignmentOptions.Center;
        pTmp.color = new Color(1f, 0.85f, 0f);
        if (_jpFont) pTmp.font = _jpFont;

        // ボタン
        int capturedIndex = index;
        bool capturedIsSkin = isSkin;
        var btn = RtButton(row.transform, "ActionBtn", "購入", 38f,
            new Vector2(380f, 0f), new Vector2(220f, 80f), new Color(0.15f, 0.50f, 0.20f));
        btn.onClick.AddListener(() => OnShopItemAction(capturedIsSkin, capturedIndex));

        return new ShopItemUI { btn = btn, priceText = pTmp };
    }

    void UpdateShopTabs()
    {
        if (_shopSkinContent) _shopSkinContent.SetActive(_shopShowSkins);
        if (_shopCityContent) _shopCityContent.SetActive(!_shopShowSkins);
        RefreshShopButtons();
    }

    void RefreshShopButtons()
    {
        if (_shopCoinLabel && CoinManager.Instance != null)
            _shopCoinLabel.text = $"コイン: {CoinManager.Instance.Coins}";

        var shop = ShopManager.Instance;
        if (shop == null || CoinManager.Instance == null) return;
        int coins = CoinManager.Instance.Coins;

        for (int i = 0; i < 5; i++)
        {
            UpdateShopItem(_skinItems[i], shop.IsSkinOwned(i), shop.SelectedSkin == i,
                           SkinDef.All[i].price, coins);
            UpdateShopItem(_cityItems[i], shop.IsCityOwned(i), shop.SelectedCity == i,
                           CityDef.All[i].price, coins);
        }
    }

    void UpdateShopItem(ShopItemUI item, bool owned, bool selected, int price, int coins)
    {
        if (item.btn == null) return;
        var label = item.btn.GetComponentInChildren<TextMeshProUGUI>();
        if (selected)
        {
            item.priceText.text = "使用中";
            item.btn.interactable = false;
            if (label) label.text = "✓";
        }
        else if (owned)
        {
            item.priceText.text = "所持済";
            item.btn.interactable = true;
            if (label) label.text = "選択";
        }
        else
        {
            item.priceText.text = $"{price}コイン";
            item.btn.interactable = coins >= price;
            if (label) label.text = "購入";
        }
    }

    void OnShopItemAction(bool isSkin, int index)
    {
        var shop = ShopManager.Instance;
        if (shop == null) return;

        if (isSkin)
        {
            if (!shop.IsSkinOwned(index)) { if (shop.BuySkin(index)) shop.SelectSkin(index); }
            else shop.SelectSkin(index);
        }
        else
        {
            if (!shop.IsCityOwned(index)) { if (shop.BuyCity(index)) shop.SelectCity(index); }
            else shop.SelectCity(index);
        }
        RefreshShopButtons();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  画面切り替え
    // ═══════════════════════════════════════════════════════════════════════════
    public void ShowTitle()
    {
        BuildTitleBackground();   // 初回のみ実行
        BuildStaminaHUD();        // スタミナ表示を titlePanel に追加（初回のみ）
        titlePanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        int hs = GameManager.Instance.highScore;
        if (titleHighScoreText) titleHighScoreText.text = hs > 0 ? $"ハイスコア：{hs}" : "";
        _titleEffect?.StartEffect();
        if (_runner) _runner.gameObject.SetActive(true);
        if (CoinManager.Instance != null) UpdateCoinDisplay(CoinManager.Instance.Coins);
        UpdateStaminaDisplay();
        // スタミナ回復タイマーを開始
        if (_staminaTimerCo != null) StopCoroutine(_staminaTimerCo);
        _staminaTimerCo = StartCoroutine(StaminaTimerLoop());
    }

    public void ShowGame()
    {
        if (_staminaTimerCo != null) { StopCoroutine(_staminaTimerCo); _staminaTimerCo = null; }
        _titleEffect?.StopEffect();
        if (_runner) _runner.gameObject.SetActive(false);
        titlePanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        UpdateScore(0);
        UpdateHp(3);
        UpdateCombo(0);
        if (previewText) previewText.alpha = 0f;
        if (gameCoinText) gameCoinText.text = "コイン: 0";  // セッション開始時リセット
        // スコア×2中の場合はゲームパネルに表示
    }

    /// <summary>コンティニュー時：スコアをリセットせずゲームパネルを表示する</summary>
    public void ResumeGame()
    {
        titlePanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        if (previewText) previewText.alpha = 0f;
    }

    public void ShowGameOver(int score, int highScore, int earnedCoins = 0)
    {
        titlePanel.SetActive(false);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        goScoreText.text     = $"スコア：{score}";
        goHighScoreText.text = $"ハイスコア：{highScore}";
        int totalCoins = CoinManager.Instance?.Coins ?? 0;
        if (goEarnedCoinText) goEarnedCoinText.text = $"今回獲得：+{earnedCoins} コイン";
        if (goTotalCoinText)  goTotalCoinText.text  = $"合計：{totalCoins} コイン";
        // コンティニューは残り回数がある場合のみ表示
        if (continueBtn)
        {
            bool canCont = GameManager.Instance.CanContinue;
            continueBtn.gameObject.SetActive(canCont);
            if (canCont)
            {
                int rem = GameManager.MaxContinue - GameManager.Instance.ContinueCount;
                var lbl = continueBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl) lbl.text = $"広告でコンティニュー（残り{rem}回）";
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  HUD 更新
    // ═══════════════════════════════════════════════════════════════════════════
    public void UpdateScore(int score) => scoreText.text = $"スコア：{score}";

    public void UpdateHp(int hp)
    {
        if (!hpText) return;
        hpText.text = new string('●', Mathf.Max(0, hp)) + new string('○', Mathf.Max(0, 3 - hp));
    }

    public void UpdateCombo(int combo)
    {
        if (!comboText) return;
        comboText.text  = combo >= 2 ? $"コンボ ×{combo}!" : "";
        comboText.color = new Color(1f, 0.85f, 0f);
    }

    // タイトル・ショップのコイン表示（トータル）
    public void UpdateCoinDisplay(int totalCoins)
    {
        string txt = $"コイン: {totalCoins}";
        if (titleCoinText)  titleCoinText.text  = txt;
        if (_shopCoinLabel) _shopCoinLabel.text  = txt;
    }

    // ゲーム中のコイン表示（このゲームで獲得した分のみ）
    public void UpdateSessionCoinDisplay(int sessionCoins)
    {
        if (gameCoinText) gameCoinText.text = $"コイン: +{sessionCoins}";
        RefreshShopButtons();
    }

    // ─── 次のゲート予告 ──────────────────────────────────────────────────────
    public void ShowPreview(int left, int right)
    {
        if (!previewText) return;
        previewText.text = $"← {left}          {right} →";
        if (previewCoroutine != null) StopCoroutine(previewCoroutine);
        previewCoroutine = StartCoroutine(FadePreview());
    }

    IEnumerator FadePreview()
    {
        previewText.alpha = 1f;
        yield return new WaitForSeconds(2.5f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            previewText.alpha = 1f - t;
            yield return null;
        }
        previewText.alpha = 0f;
    }

    // ─── 正解/ミスエフェクト ──────────────────────────────────────────────────
    public void ShowEffect(string text, bool correct)
    {
        StopCoroutine(nameof(FadeEffect));
        effectText.text  = text;
        effectText.color = correct ? new Color(0f, 1f, 0.67f) : new Color(1f, 0.27f, 0.4f);
        effectText.alpha = 1f;
        StartCoroutine(nameof(FadeEffect));
    }

    IEnumerator FadeEffect()
    {
        yield return new WaitForSeconds(0.8f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            effectText.alpha = 1f - t;
            yield return null;
        }
        effectText.alpha = 0f;
    }

    // ─── トースト通知（画面上部・全パネル共通） ─────────────────────────────
    /// <summary>「広告がありません」など短いメッセージを全画面上に浮かせる</summary>
    public void ShowToast(string msg)
    {
        if (!_toastBuilt) BuildToast();
        StopCoroutine(nameof(ToastRoutine));
        _toastTMP.text = msg;
        _toastGroup.alpha = 1f;
        StartCoroutine(nameof(ToastRoutine));
    }

    void BuildToast()
    {
        _toastBuilt = true;
        var go = new GameObject("Toast");
        go.transform.SetParent(transform, false);

        // 独自 Canvas で最前面
        var cv = go.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 999;
        go.AddComponent<GraphicRaycaster>();

        _toastGroup = go.AddComponent<CanvasGroup>();
        _toastGroup.alpha = 0f;
        _toastGroup.blocksRaycasts = false;

        // 半透明背景
        var bg = new GameObject("Bg");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.78f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.1f, 0.82f);
        bgRt.anchorMax = new Vector2(0.9f, 0.92f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        bgImg.sprite   = _roundedSprite;
        bgImg.type     = Image.Type.Sliced;

        // テキスト
        _toastTMP = RtTMP(go.transform, "ToastText", "", 36f,
            new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.92f),
            new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(0f, 0f));
        _toastTMP.GetComponent<RectTransform>().offsetMin = _toastTMP.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        _toastTMP.color = new Color(1f, 0.85f, 0.3f);
    }

    IEnumerator ToastRoutine()
    {
        yield return new WaitForSecondsRealtime(2f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 2f;
            _toastGroup.alpha = 1f - t;
            yield return null;
        }
        _toastGroup.alpha = 0f;
    }

    // ─── 死亡時赤フラッシュ ──────────────────────────────────────────────────
    public void StartDeathFlash() => StartCoroutine(DeathFlashRoutine());

    IEnumerator DeathFlashRoutine()
    {
        for (int i = 0; i < 4; i++)
        {
            float alpha = i == 0 ? 0.75f : 0.5f;
            _flashOverlay.color = new Color(1f, 0.05f, 0.05f, alpha);
            yield return new WaitForSecondsRealtime(0.1f);
            _flashOverlay.color = new Color(1f, 0.05f, 0.05f, 0f);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // ─── エフェクト全リセット ────────────────────────────────────────────────
    public void ResetEffects()
    {
        StopAllCoroutines();
        if (_flashOverlay) _flashOverlay.color = new Color(1f, 0f, 0f, 0f);
        if (effectText)    effectText.alpha     = 0f;
        if (previewText)   previewText.alpha    = 0f;
        if (comboText)     comboText.text       = "";
        previewCoroutine = null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ショップ表示
    // ═══════════════════════════════════════════════════════════════════════════
    public void ShowShop()
    {
        if (_shopPanel) _shopPanel.SetActive(true);
        // タイトル画面を隠す
        if (titlePanel) titlePanel.SetActive(false);
        _titleEffect?.StopEffect();
        if (_runner) _runner.gameObject.SetActive(false);
        RefreshShopButtons();
    }

    public void HideShop()
    {
        if (_shopPanel) _shopPanel.SetActive(false);
        // タイトルフェーズ中なら元に戻す
        if (GameManager.Instance?.phase == GamePhase.Title)
            ShowTitle();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ボタンコールバック
    // ═══════════════════════════════════════════════════════════════════════════
    public void OnStartButton()
    {
        var sm = StaminaManager.Instance;
        if (sm != null && sm.GetStamina() <= 0)
        {
            ShowStaminaDialog();
            return;
        }
        sm?.UseStamina();
        GameManager.Instance.StartGame();
    }

    public void OnRetryButton()
    {
        var sm = StaminaManager.Instance;
        if (sm != null && sm.GetStamina() <= 0)
        {
            ShowStaminaDialog();
            return;
        }
        sm?.UseStamina();
        GameManager.Instance.StartGame();
    }

    public void OnTitleButton()
    {
        GameManager.Instance.StopAllCoroutines();
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;
        GameManager.Instance.phase = GamePhase.Title;
        ResetEffects();
        ShowTitle();
    }

    public void OnContinueButton()
    {
        if (continueBtn) continueBtn.gameObject.SetActive(false);
        AdManager.Instance?.ShowContinueAd(
            onReward: () => GameManager.Instance.Continue(),
            onNoAd:   () => ShowToast("広告がありません")
        );
    }

    public void OnCoinBoostButton()
    {
        AdManager.Instance?.ShowCoinBoostAd(
            onReward: () =>
            {
                GameManager.Instance.ApplyCoinX2();
                // ゲームオーバー画面の表示を更新
                int earned = GameManager.Instance.sessionCoins;
                int total  = CoinManager.Instance?.Coins ?? 0;
                if (goEarnedCoinText) goEarnedCoinText.text = $"今回獲得：+{earned} コイン  (×2適用!)";
                if (goTotalCoinText)  goTotalCoinText.text  = $"合計：{total} コイン";
                if (coinBoostBtn)     coinBoostBtn.gameObject.SetActive(false); // 2回押し防止
            },
            onNoAd: () => ShowToast("広告がありません")
        );
    }

    public void OnShopButton() => ShowShop();

    // ═══════════════════════════════════════════════════════════════════════════
    //  Runtime UI ヘルパー
    // ═══════════════════════════════════════════════════════════════════════════
    TextMeshProUGUI RtTMP(Transform parent, string name, string text, float fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (_jpFont) tmp.font = _jpFont;
        return tmp;
    }

    Button RtButton(Transform parent, string name, string label, float fontSize,
        Vector2 pos, Vector2 size, Color bgColor)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        ApplyRoundedImg(img);
        var btn = go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var lbl = RtTMP(go.transform, "Label", label, fontSize,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        lbl.color = Color.white;
        return btn;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  スタミナ UI
    // ═══════════════════════════════════════════════════════════════════════════
    bool _staminaHudBuilt = false;

    void BuildStaminaHUD()
    {
        if (_staminaHudBuilt || !titlePanel) return;
        _staminaHudBuilt = true;

        // スタミナ●●● — ショップボタン(y=-360, h=110)の下
        _staminaText = RtTMP(titlePanel.transform, "StaminaText", "●●●", 50f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, -490f), new Vector2(600f, 70f));
        _staminaText.color = new Color(1f, 0.25f, 0.40f);

        // 回復タイマー（スタミナ不足時のみ表示）
        _staminaTimerText = RtTMP(titlePanel.transform, "StaminaTimer", "", 30f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, -550f), new Vector2(500f, 48f));
        _staminaTimerText.color = new Color(0.9f, 0.7f, 0.3f);
    }

    public void UpdateStaminaDisplay()
    {
        if (_staminaText == null) return;
        var sm = StaminaManager.Instance;
        if (sm == null) return;

        int cur = sm.GetStamina();
        int max = sm.MaxStamina;
        // ●（赤）= 所持、○（暗灰）= 消費済み
        _staminaText.text = new string('●', cur) + new string('○', max - cur);
        _staminaText.color = cur > 0 ? new Color(1f, 0.25f, 0.40f) : new Color(0.5f, 0.5f, 0.55f);

        // スタミナが満タンでない場合のみ回復タイマーを表示
        if (_staminaTimerText)
        {
            if (cur < max)
            {
                string timerStr = sm.NextRegenTimeString();
                _staminaTimerText.text = timerStr.Length > 0 ? $"回復まで {timerStr}" : "";
            }
            else
            {
                _staminaTimerText.text = "";
            }
        }
    }

    IEnumerator StaminaTimerLoop()
    {
        while (true)
        {
            UpdateStaminaDisplay();
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    // ── スタミナ不足ダイアログ ────────────────────────────────────────────
    void BuildStaminaDialog()
    {
        _staminaDialog = new GameObject("StaminaDialog");
        _staminaDialog.transform.SetParent(transform, false);

        // 半透明黒背景
        var bg = _staminaDialog.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        var rt = _staminaDialog.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var cv = _staminaDialog.AddComponent<Canvas>();
        cv.overrideSorting = true; cv.sortingOrder = 200;
        _staminaDialog.AddComponent<GraphicRaycaster>();

        // タイトル
        RtTMP(_staminaDialog.transform, "Title", "スタミナ不足", 64f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, 300f), new Vector2(700f, 100f))
            .color = new Color(1f, 0.35f, 0.35f);

        // ハート表示
        var hearts = RtTMP(_staminaDialog.transform, "Hearts", "○○○", 72f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, 160f), new Vector2(500f, 100f));
        hearts.color = new Color(0.5f, 0.5f, 0.55f);

        // タイマーテキスト
        var timerTmp = RtTMP(_staminaDialog.transform, "DialogTimer", "", 40f,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0f, 60f), new Vector2(600f, 60f));
        timerTmp.color = new Color(0.9f, 0.75f, 0.3f);

        // 広告ボタン
        var adBtn = RtButton(_staminaDialog.transform, "StaminaAdBtn",
            "広告でスタミナ1回復", 44f,
            new Vector2(0f, -80f), new Vector2(680f, 110f), new Color(0.10f, 0.55f, 0.20f));
        adBtn.onClick.AddListener(() =>
        {
            AdManager.Instance?.ShowStaminaAd(
                onReward: () =>
                {
                    StaminaManager.Instance?.AddStamina(1);
                    HideStaminaDialog();
                },
                onNoAd: () => ShowToast("広告がありません")
            );
        });

        // 閉じるボタン
        var closeBtn = RtButton(_staminaDialog.transform, "StaminaCloseBtn",
            "閉じる", 40f,
            new Vector2(0f, -230f), new Vector2(440f, 90f), new Color(0.30f, 0.30f, 0.35f));
        closeBtn.onClick.AddListener(HideStaminaDialog);

        _staminaDialog.SetActive(false);

        // タイマーテキストを毎秒更新するコルーチン（ダイアログ表示中のみ）
        StartCoroutine(DialogTimerLoop(timerTmp));
    }

    IEnumerator DialogTimerLoop(TextMeshProUGUI timerTmp)
    {
        while (true)
        {
            if (_staminaDialog != null && _staminaDialog.activeSelf && timerTmp != null)
            {
                var sm = StaminaManager.Instance;
                if (sm != null)
                {
                    string t = sm.NextRegenTimeString();
                    timerTmp.text = t.Length > 0 ? $"次の回復まで {t}" : "スタミナ回復可能！";
                }
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    void ShowStaminaDialog()
    {
        if (_staminaDialog) _staminaDialog.SetActive(true);
        UpdateStaminaDisplay();
    }

    void HideStaminaDialog()
    {
        if (_staminaDialog) _staminaDialog.SetActive(false);
        UpdateStaminaDisplay();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  タイトル背景デザイン
    // ═══════════════════════════════════════════════════════════════════════════
    void BuildTitleBackground()
    {
        if (_titleBgBuilt || !titlePanel) return;
        _titleBgBuilt = true;

        // titlePanel の背景をより暗くする
        var titleImg = titlePanel.GetComponent<Image>();
        if (titleImg) titleImg.color = new Color(0.02f, 0.03f, 0.08f, 1f);

        var bg = new GameObject("TitleBG");
        bg.transform.SetParent(titlePanel.transform, false);
        bg.transform.SetAsFirstSibling();   // 全 UI の後ろへ

        // ── グラデーション帯 ──────────────────────────────────────────────
        // 上部（濃い青紫）
        BgRect(bg.transform, "GradTop",    new Color(0.04f, 0.04f, 0.18f, 0.85f),
               new Vector2(0f, 0.62f), new Vector2(1f, 1f));
        // 中央グロー（明るい青帯）
        BgRect(bg.transform, "GlowMid",   new Color(0.05f, 0.18f, 0.55f, 0.10f),
               new Vector2(0f, 0.44f), new Vector2(1f, 0.62f));
        // 地平線ライン（シアン細線）
        BgRect(bg.transform, "Horizon",   new Color(0.25f, 0.70f, 1.00f, 0.35f),
               new Vector2(0f, 0.290f), new Vector2(1f, 0.298f));

        // ── 道路パース線（4本、画面下部中央から発散）────────────────────
        // anchoredPosition の pivot = 下中央 (0.5, 0)
        // 画面幅 1080、高さ 1920 を基準に傾き計算
        AddRoadLine(bg.transform, "RL0", -480f, 5f, 0.13f);
        AddRoadLine(bg.transform, "RL1", -220f, 3f, 0.22f);
        AddRoadLine(bg.transform, "RL2",  220f, 3f, 0.22f);
        AddRoadLine(bg.transform, "RL3",  480f, 5f, 0.13f);

        // ── 道路面（下部 15%）────────────────────────────────────────────
        BgRect(bg.transform, "RoadSurface", new Color(0.03f, 0.05f, 0.09f, 1f),
               new Vector2(0f, 0f), new Vector2(1f, 0.155f));
        // 道路中央線（点線風）
        BgRect(bg.transform, "RoadLine", new Color(0.55f, 0.55f, 0.58f, 0.20f),
               new Vector2(0.494f, 0f), new Vector2(0.506f, 0.295f));

        // ── 都市シルエット（ビル 15棟）────────────────────────────────────
        float[] bH = { 0.20f, 0.13f, 0.24f, 0.10f, 0.18f, 0.28f, 0.15f, 0.22f,
                        0.12f, 0.21f, 0.09f, 0.19f, 0.25f, 0.11f, 0.16f };
        Color bldgCol = new Color(0.024f, 0.034f, 0.072f, 1f);
        float bW = 1f / bH.Length;
        for (int i = 0; i < bH.Length; i++)
        {
            float xMin = i * bW + 0.003f, xMax = (i + 1) * bW - 0.003f;
            BgRect(bg.transform, "Bldg" + i, bldgCol,
                   new Vector2(xMin, 0.14f), new Vector2(xMax, 0.14f + bH[i]));

            // ビルの窓（1棟あたり最大 12個）
            var rng = new System.Random(i * 17 + 3);
            int wCols = Mathf.Max(1, (int)(bW * 1080f / 22f));
            int wRows = Mathf.Max(1, (int)(bH[i] * 1920f / 30f));
            int maxWin = Mathf.Min(wCols * wRows, 12);
            int placed = 0;
            for (int wy = 0; wy < wRows && placed < maxWin; wy++)
            for (int wx = 0; wx < wCols && placed < maxWin; wx++)
            {
                if (rng.NextDouble() > 0.38) continue;
                float nx = xMin + (wx + 0.5f) / wCols * (xMax - xMin);
                float ny = 0.14f + (wy + 0.5f) / wRows * bH[i] * 0.82f;
                Color wc = rng.NextDouble() > 0.45f
                    ? new Color(1.00f, 0.92f, 0.55f, 0.60f)   // 暖色窓
                    : new Color(0.30f, 0.60f, 1.00f, 0.45f);  // 青白窓
                BgRect(bg.transform, $"W{i}_{wx}_{wy}", wc,
                       new Vector2(nx - 0.004f, ny - 0.005f),
                       new Vector2(nx + 0.004f, ny + 0.005f));
                placed++;
            }
        }
    }

    // anchorMin/Max を直接指定して Image を貼る
    void BgRect(Transform parent, string name, Color col,
                Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // 道路パース線（下端の xOffset ピクセルから消失点へ傾く細い矩形）
    void AddRoadLine(Transform parent, string name,
                     float bottomXOffset, float lineW, float alpha)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.20f, 0.55f, 1.00f, alpha);
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(bottomXOffset, 0f);
        // 消失点は画面中央 y ≈ 30% = 576px から発散
        float angle = Mathf.Atan2(-bottomXOffset, 576f) * Mathf.Rad2Deg;
        go.transform.localEulerAngles = new Vector3(0f, 0f, angle);
        rt.sizeDelta = new Vector2(lineW, 960f);
    }

    // ── 角丸ヘルパー ─────────────────────────────────────────────────────────
    void ApplyRoundedImg(Image img)
    {
        if (img == null || _roundedSprite == null) return;
        img.sprite = _roundedSprite;
        img.type   = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;
    }

    static Sprite MakeRoundedSprite(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color32[size * size];
        byte full = 255, none = 0;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = 0f, dy = 0f;
            bool corner = false;
            if      (x < radius     && y < radius)           { dx = radius - x - 0.5f; dy = radius - y - 0.5f;           corner = true; }
            else if (x >= size-radius && y < radius)         { dx = x-(size-radius)+0.5f; dy = radius - y - 0.5f;         corner = true; }
            else if (x < radius     && y >= size-radius)     { dx = radius - x - 0.5f; dy = y-(size-radius)+0.5f;         corner = true; }
            else if (x >= size-radius && y >= size-radius)   { dx = x-(size-radius)+0.5f; dy = y-(size-radius)+0.5f;      corner = true; }
            bool inside = !corner || (dx*dx + dy*dy <= (float)radius * radius);
            byte a = inside ? full : none;
            pixels[y * size + x] = new Color32(255, 255, 255, a);
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        var border = new Vector4(radius, radius, radius, radius);
        return Sprite.Create(tex, new Rect(0, 0, size, size),
                             new Vector2(0.5f, 0.5f), 100f, 0,
                             SpriteMeshType.FullRect, border);
    }
}
