using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// PrimeRunner > Setup Scene を実行すると
/// シーン・プレハブ・マテリアルを自動構築する。
/// </summary>
public class AutoSetupTownRun : EditorWindow
{
    // ═══════════════════════════════════════════════════
    //  エントリポイント
    // ═══════════════════════════════════════════════════
    [MenuItem("PrimeRunner/Setup Scene")]
    public static void Setup()
    {
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets/Materials", "TownRun");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets/Prefabs", "TownRun");

        ClearScene();

        // ── フォント（プレハブ作成より先に準備）──────────────
        var jpFont = SetupJapaneseFont();

        // ── マテリアル ─────────────────────────────────────
        Material matRoad      = SaveMat("Road",      new Color(0.05f, 0.05f, 0.05f));
        Material matRoadJoint = SaveMat("RoadJoint", new Color(0.48f, 0.46f, 0.43f));
        Material matLaneLine  = SaveMat("LaneLine",  new Color(0.95f, 0.93f, 0.88f));
        Material matSidewalk  = SaveMat("Sidewalk",  new Color(0.80f, 0.78f, 0.73f));
        Material matCurb      = SaveMat("Curb",      new Color(0.58f, 0.56f, 0.52f));
        Material matLightPole = SaveMat("LightPole", new Color(0.30f, 0.30f, 0.32f));
        Material matLightBulb = SaveMat("LightBulb", new Color(1.00f, 0.96f, 0.70f));

        Material matSkin  = SaveMat("Skin",  new Color(1.00f, 0.75f, 0.56f));
        Material matShirt = SaveMat("Shirt", new Color(0.90f, 0.25f, 0.15f));
        Material matPants = SaveMat("Pants", new Color(0.18f, 0.22f, 0.55f));
        Material matHair  = SaveMat("Hair",  new Color(0.22f, 0.14f, 0.06f));
        Material matShoe  = SaveMat("Shoe",  new Color(0.18f, 0.10f, 0.04f));

        // ゲートマテリアル（左右同色：数字だけで判断させる）
        Material matGate = SaveMatEmissive("Gate", new Color(1f, 1f, 1f));
        Material matLine = SaveMat("GateLine", Color.black);

        AssetDatabase.SaveAssets();

        // ── プレハブ ───────────────────────────────────────
        var cityTilePrefab = CreateCityTilePrefab(
            matRoad, matRoadJoint, matLaneLine, matSidewalk, matCurb,
            matLightPole, matLightBulb);

        var gatePairPrefab = CreateGatePairPrefab(matGate, matGate, matLine);

        // ── シーン構築 ────────────────────────────────────
        SetupLight();
        var camObj  = SetupCamera();
        SetupInputManager();
        var gameMgr = SetupGameManager();
        var player  = SetupPlayer(null, null, null, null, null);
        SetupCityLevelGenerator(player, cityTilePrefab);
        camObj.AddComponent<CameraFollow>().target = player.transform;

        // GameManager に参照をセット
        var gm = gameMgr.GetComponent<GameManager>();
        gm.player         = player.GetComponent<PlayerController>();
        gm.gatePairPrefab = gatePairPrefab;

        SetupUIManager(jpFont);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // シーンを Assets/Scenes/TitleScene.unity に自動保存
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string scenePath = "Assets/Scenes/TitleScene.unity";
        EnsureFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("★ PrimeRunner Setup Complete!  Press Play to start. ★");
    }

    // ═══════════════════════════════════════════════════
    //  プレハブ: CityTile（直線タイル・障害物なし）
    // ═══════════════════════════════════════════════════
    static GameObject CreateCityTilePrefab(
        Material matRoad, Material matJoint, Material matLane,
        Material matSidewalk, Material matCurb,
        Material matPole, Material matBulb)
    {
        var root = BuildRoadStructure("CityTile",
            matRoad, matJoint, matLane, matSidewalk, matCurb, matPole, matBulb);

        var tile = root.AddComponent<CityTile>();
        tile.tileLength = 30f;

        return SavePrefab(root, "CityTile");
    }

    // ═══════════════════════════════════════════════════
    //  プレハブ: GatePair（左右ゲート1組）
    // ═══════════════════════════════════════════════════
    static GameObject CreateGatePairPrefab(Material matLeft, Material matRight, Material matLine)
    {
        var root = new GameObject("GatePair");
        root.AddComponent<GatePair>();

        var leftGate  = CreateGateChild(root.transform, "LeftGate",  -1.5f, matLeft);
        var rightGate = CreateGateChild(root.transform, "RightGate", +1.5f, matRight);

        var pair = root.GetComponent<GatePair>();
        pair.leftGate  = leftGate.GetComponent<Gate>();
        pair.rightGate = rightGate.GetComponent<Gate>();

        // 板と板の間の黒いセンターライン
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "CenterLine";
        line.transform.SetParent(root.transform);
        line.transform.localPosition = new Vector3(0f, 7f, 0f);
        line.transform.localScale    = new Vector3(0.5f, 14f, 0.6f);
        line.GetComponent<Renderer>().sharedMaterial = matLine;
        Object.DestroyImmediate(line.GetComponent<Collider>());

        return SavePrefab(root, "GatePair");
    }

    static GameObject CreateGateChild(Transform parent, string goName, float x, Material mat)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent);
        go.transform.localPosition = new Vector3(x, 0f, 0f);

        var gate = go.AddComponent<Gate>();

        // フレーム（板）：幅8m × 高さ14m × 奥行き0.3m
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Frame";
        frame.transform.SetParent(go.transform);
        frame.transform.localPosition = new Vector3(0f, 7f, 0f);
        frame.transform.localScale    = new Vector3(8f, 14f, 0.3f);
        frame.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(frame.GetComponent<Collider>());
        gate.frameRenderer = frame.GetComponent<MeshRenderer>();

        // 数字テキストは Gate.Start() でランタイム生成（TMP シリアライズ問題を回避）

        return go;
    }

    // ═══════════════════════════════════════════════════
    //  道路構造物ビルダー（CityTile 共通）
    // ═══════════════════════════════════════════════════
    static GameObject BuildRoadStructure(
        string name,
        Material matRoad, Material matJoint, Material matLane,
        Material matSidewalk, Material matCurb,
        Material matPole, Material matBulb)
    {
        var root = new GameObject(name);
        float len = 30f;
        float rW  = 9f;
        float swW = 2.2f;

        // 道路面（コライダー付き）
        Prim(PrimitiveType.Cube, root.transform,
            new Vector3(0, -0.05f, len * 0.5f),
            new Vector3(rW, 0.10f, len), matRoad, keepCol: true).name = "Road";

        // コンクリート膨張目地（5m ごとに横一本）
        float[] jointZ = { 5f, 10f, 15f, 20f, 25f };
        foreach (float jz in jointZ)
            Prim(PrimitiveType.Cube, root.transform,
                new Vector3(0, 0.01f, jz),
                new Vector3(rW, 0.02f, 0.12f), matJoint).name = "Joint";

        // 歩道
        foreach (float sx in new[] { -(rW + swW) * 0.5f, (rW + swW) * 0.5f })
        {
            Prim(PrimitiveType.Cube, root.transform,
                new Vector3(sx, 0.10f, len * 0.5f),
                new Vector3(swW, 0.20f, len), matSidewalk).name = "Sidewalk";
            float[] swJoints = { 2.5f, 5f, 7.5f, 10f, 12.5f, 15f, 17.5f, 20f, 22.5f, 25f, 27.5f };
            foreach (float jz in swJoints)
                Prim(PrimitiveType.Cube, root.transform,
                    new Vector3(sx, 0.21f, jz),
                    new Vector3(swW, 0.01f, 0.06f), matJoint).name = "SwJoint";
        }

        // 縁石
        foreach (float cx in new[] { -rW * 0.5f, rW * 0.5f })
            Prim(PrimitiveType.Cube, root.transform,
                new Vector3(cx, 0.06f, len * 0.5f),
                new Vector3(0.18f, 0.12f, len), matCurb).name = "Curb";

        // レーンライン（破線）
        float[] dotZ = { 3f, 9f, 15f, 21f, 27f };
        foreach (float dz in dotZ)
            Prim(PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.01f, dz),
                new Vector3(0.12f, 0.02f, 3.5f), matLane).name = "LaneDot";

        // 街灯
        float[] lzArr = { 7f, 22f };
        foreach (float z in lzArr)
            foreach (float sx in new[] { -(rW * 0.5f + swW * 0.5f), rW * 0.5f + swW * 0.5f })
            {
                float armDir = sx < 0 ? 1f : -1f;
                Prim(PrimitiveType.Cylinder, root.transform,
                    new Vector3(sx, 2.0f, z), new Vector3(0.10f, 2.0f, 0.10f), matPole).name = "Pole";
                Prim(PrimitiveType.Cube, root.transform,
                    new Vector3(sx + armDir * 0.55f, 4.1f, z), new Vector3(1.1f, 0.09f, 0.09f), matPole).name = "Arm";
                Prim(PrimitiveType.Sphere, root.transform,
                    new Vector3(sx + armDir * 1.1f, 3.85f, z), new Vector3(0.32f, 0.32f, 0.32f), matBulb).name = "Bulb";
            }

        return root;
    }

    // ═══════════════════════════════════════════════════
    //  シーンオブジェクト
    // ═══════════════════════════════════════════════════

    static void SetupLight()
    {
        var go = new GameObject("Directional Light");
        var l  = go.AddComponent<Light>();
        l.type      = LightType.Directional;
        l.intensity = 1.3f;
        l.color     = new Color(1f, 0.96f, 0.88f);
        go.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    static GameObject SetupCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.AddComponent<Camera>();
        go.AddComponent<AudioListener>();
        go.transform.position = new Vector3(0, 7, -13);
        go.transform.rotation = Quaternion.Euler(20, 0, 0);
        return go;
    }

    static void SetupInputManager() =>
        new GameObject("InputManager").AddComponent<InputManager>();

    static GameObject SetupGameManager()
    {
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        new GameObject("CoinManager").AddComponent<CoinManager>();
        new GameObject("AdManager").AddComponent<AdManager>();
        new GameObject("ShopManager").AddComponent<ShopManager>();
        new GameObject("StaminaManager").AddComponent<StaminaManager>();
        return go;
    }

    // ─── プレイヤーキャラクター ────────────────────────────────────
    // ジオメトリは SkinController.Start() が生成するため、ここではスケルトンのみ作成。
    static GameObject SetupPlayer(
        Material matSkin, Material matShirt, Material matPants,
        Material matHair, Material matShoe)
    {
        var root = new GameObject("Player");
        root.transform.position = new Vector3(-1.5f, 0f, 0f);

        var cc        = root.AddComponent<CharacterController>();
        cc.height     = 1.80f;
        cc.radius     = 0.32f;
        cc.center     = new Vector3(0, 0.90f, 0);
        cc.skinWidth  = 0.05f;
        cc.stepOffset = 0.4f;
        root.AddComponent<PlayerController>();

        var vis  = new GameObject("Visual");
        vis.transform.SetParent(root.transform);
        vis.transform.localPosition = Vector3.zero;

        var anim = vis.AddComponent<ProceduralRunAnimation>();
        var skin = vis.AddComponent<SkinController>();

        // 4つのピボット GO（空）── SkinController が中身を生成する
        Transform MakePivot(string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(vis.transform);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        var lArmPivot = MakePivot("L_Arm_Pivot", new Vector3(-0.33f, 1.08f, 0));
        var rArmPivot = MakePivot("R_Arm_Pivot", new Vector3( 0.33f, 1.08f, 0));
        var lLegPivot = MakePivot("L_Leg_Pivot", new Vector3(-0.13f, 0.56f, 0));
        var rLegPivot = MakePivot("R_Leg_Pivot", new Vector3( 0.13f, 0.56f, 0));

        // ProceduralRunAnimation と SkinController に同じ参照をセット
        anim.leftArmPivot  = skin.leftArmPivot  = lArmPivot;
        anim.rightArmPivot = skin.rightArmPivot = rArmPivot;
        anim.leftLegPivot  = skin.leftLegPivot  = lLegPivot;
        anim.rightLegPivot = skin.rightLegPivot = rLegPivot;

        return root;
    }

    static void SetupCityLevelGenerator(GameObject playerObj, GameObject tilePrefab)
    {
        var go  = new GameObject("CityLevelGenerator");
        var clg = go.AddComponent<CityLevelGenerator>();
        clg.tilePrefab      = tilePrefab;
        clg.playerTransform = playerObj.transform;
        clg.tileLength      = 30f;
        clg.numberOfTiles   = 7;
    }

    // ─── UIManager（タイトル / ゲーム中 / ゲームオーバー）──────────
    static void SetupUIManager(TMP_FontAsset jpFont)
    {
        var canvasObj = new GameObject("Canvas");
        var canvas    = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = canvasObj.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ─── TitlePanel ─────────────────────────────────────
        var titlePanel = new GameObject("TitlePanel");
        titlePanel.transform.SetParent(canvasObj.transform, false);
        titlePanel.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.16f, 1f);
        SetFullScreen(titlePanel);

        MakeTMP(titlePanel.transform, "TitleText", "素数ランナー", 90f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 580f), new Vector2(900f, 180f)).GetComponent<TextMeshProUGUI>().color
            = new Color(0f, 1f, 0.67f);

        MakeTMP(titlePanel.transform, "DescText",
            "素数のゲートを選べ！\n左右スワイプで移動", 38f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 350f), new Vector2(800f, 150f));

        var titleHighScore = MakeTMP(titlePanel.transform, "TitleHighScoreText", "", 40f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 160f), new Vector2(700f, 70f));
        titleHighScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0f);

        var titleCoinText = MakeTMP(titlePanel.transform, "TitleCoinText", "コイン: 0", 36f,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -90f), new Vector2(360f, 55f));
        titleCoinText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0f);

        var startBtn = MakeButton(titlePanel.transform, "StartButton", "スタート", 56f,
            new Vector2(0f, -180f), new Vector2(700f, 130f), new Color(0.13f, 0.62f, 0.22f));

        var shopBtn = MakeButton(titlePanel.transform, "ShopButton", "ショップ", 46f,
            new Vector2(0f, -360f), new Vector2(560f, 110f), new Color(0.20f, 0.30f, 0.55f));

        // ─── GamePanel ──────────────────────────────────────
        var gamePanel = new GameObject("GamePanel");
        gamePanel.transform.SetParent(canvasObj.transform, false);
        SetFullScreen(gamePanel);
        gamePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0);

        var scoreText = MakeTMP(gamePanel.transform, "ScoreText", "スコア：0", 38f,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -20f), new Vector2(400f, 55f));

        var gameCoinText = MakeTMP(gamePanel.transform, "GameCoinText", "コイン: 0", 32f,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -65f), new Vector2(320f, 48f));
        gameCoinText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0f);

        var hpText = MakeTMP(gamePanel.transform, "HpText", "●●●", 42f,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(220f, 55f));
        hpText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.4f);

        var effectText = MakeTMP(gamePanel.transform, "EffectText", "", 62f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 100f), new Vector2(900f, 120f));

        var comboText = MakeTMP(gamePanel.transform, "ComboText", "", 48f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -20f), new Vector2(700f, 80f));
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0f);

        var previewText = MakeTMP(gamePanel.transform, "PreviewText", "", 44f,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 120f), new Vector2(900f, 70f));

        // ─── GameOverPanel ──────────────────────────────────
        var goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        SetFullScreen(goPanel);

        MakeTMP(goPanel.transform, "GOTitle", "ゲームオーバー", 80f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 420f), new Vector2(800f, 120f)).GetComponent<TextMeshProUGUI>().color
            = new Color(1f, 0.3f, 0.3f);

        var goScoreText = MakeTMP(goPanel.transform, "GOScoreText", "スコア：0", 56f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 250f), new Vector2(700f, 90f));

        var goHighScoreText = MakeTMP(goPanel.transform, "GOHighScoreText", "ハイスコア：0", 44f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 150f), new Vector2(700f, 75f));
        goHighScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0f);

        var goEarnedCoinText = MakeTMP(goPanel.transform, "GOEarnedCoinText", "今回獲得：+0 コイン", 42f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 55f), new Vector2(700f, 68f));
        goEarnedCoinText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);

        var goTotalCoinText = MakeTMP(goPanel.transform, "GOTotalCoinText", "合計：0 コイン", 36f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -25f), new Vector2(700f, 58f));
        goTotalCoinText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var continueBtn = MakeButton(goPanel.transform, "ContinueButton", "広告でコンティニュー（残り2回）", 40f,
            new Vector2(0f, -120f), new Vector2(700f, 110f), new Color(0.80f, 0.45f, 0.05f));

        var coinBoostBtn = MakeButton(goPanel.transform, "CoinBoostButton", "広告でコイン×2（次のゲーム）", 38f,
            new Vector2(0f, -250f), new Vector2(700f, 100f), new Color(0.40f, 0.10f, 0.65f));

        var retryBtn = MakeButton(goPanel.transform, "RetryButton", "もう一度", 52f,
            new Vector2(0f, -380f), new Vector2(600f, 110f), new Color(0.13f, 0.62f, 0.22f));

        var titleBtn = MakeButton(goPanel.transform, "TitleButton", "タイトルへ", 44f,
            new Vector2(0f, -510f), new Vector2(500f, 100f), new Color(0.30f, 0.30f, 0.35f));

        // ─── UIManager ──────────────────────────────────────
        var uiMgrObj = new GameObject("UIManager");
        uiMgrObj.transform.SetParent(canvasObj.transform, false);
        var uiMgr = uiMgrObj.AddComponent<UIManager>();

        uiMgr.titlePanel    = titlePanel;
        uiMgr.gamePanel     = gamePanel;
        uiMgr.gameOverPanel = goPanel;

        uiMgr.scoreText   = scoreText.GetComponent<TextMeshProUGUI>();
        uiMgr.hpText      = hpText.GetComponent<TextMeshProUGUI>();
        uiMgr.effectText  = effectText.GetComponent<TextMeshProUGUI>();
        uiMgr.comboText   = comboText.GetComponent<TextMeshProUGUI>();
        uiMgr.previewText = previewText.GetComponent<TextMeshProUGUI>();

        uiMgr.goScoreText      = goScoreText.GetComponent<TextMeshProUGUI>();
        uiMgr.goHighScoreText  = goHighScoreText.GetComponent<TextMeshProUGUI>();
        uiMgr.goEarnedCoinText = goEarnedCoinText.GetComponent<TextMeshProUGUI>();
        uiMgr.goTotalCoinText  = goTotalCoinText.GetComponent<TextMeshProUGUI>();

        uiMgr.titleHighScoreText = titleHighScore.GetComponent<TextMeshProUGUI>();
        uiMgr.titleCoinText      = titleCoinText.GetComponent<TextMeshProUGUI>();
        uiMgr.gameCoinText       = gameCoinText.GetComponent<TextMeshProUGUI>();
        uiMgr.continueBtn  = continueBtn;
        uiMgr.coinBoostBtn = coinBoostBtn;
        uiMgr.shopBtn      = shopBtn;

        // ボタンのリスナーを登録
        UnityEventTools.AddPersistentListener(startBtn.onClick,    uiMgr.OnStartButton);
        UnityEventTools.AddPersistentListener(retryBtn.onClick,    uiMgr.OnRetryButton);
        UnityEventTools.AddPersistentListener(titleBtn.onClick,    uiMgr.OnTitleButton);
        UnityEventTools.AddPersistentListener(continueBtn.onClick,  uiMgr.OnContinueButton);
        UnityEventTools.AddPersistentListener(coinBoostBtn.onClick, uiMgr.OnCoinBoostButton);
        UnityEventTools.AddPersistentListener(shopBtn.onClick,      uiMgr.OnShopButton);

        // 初期状態: タイトルのみ表示
        titlePanel.SetActive(true);
        gamePanel.SetActive(false);
        goPanel.SetActive(false);

        // 日本語フォントを全 TMP テキストに適用
        if (jpFont != null)
            foreach (var tmp in canvasObj.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.font = jpFont;
    }

    // ─── 日本語 TMP_FontAsset をエディタで生成・保存 ──────────────
    static TMP_FontAsset SetupJapaneseFont()
    {
        EnsureFolder("Assets", "Fonts");
        string assetPath = "Assets/Fonts/JapaneseTMPFont.asset";

        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null)
        {
            Debug.Log("[JpFont] 既存アセット再利用: " + assetPath);
            return existing;
        }

        TMP_FontAsset fa = null;

        string[][] pairs = {
            new[] { "HGMaruGothicMPRO", "Regular" },  // 丸ゴシック（最優先）
            new[] { "BIZ UDGothic",     "Regular" },  // BIZ UD ゴシック
            new[] { "Meiryo UI",        "Regular" },  // メイリオ UI（丸め）
            new[] { "Meiryo",           "Regular" },
            new[] { "Yu Gothic",        "Regular" },
            new[] { "MS Gothic",        "Regular" },
        };
        foreach (var p in pairs)
        {
            fa = TMP_FontAsset.CreateFontAsset(p[0], p[1]);
            if (fa != null) { Debug.Log($"[JpFont] ファミリー名で生成成功: {p[0]} {p[1]}"); break; }
        }

        if (fa == null)
        {
            string[] files = {
                @"C:\Windows\Fonts\HGMaruGothicMPRO.ttf",  // 丸ゴシック
                @"C:\Windows\Fonts\BIZUDGothic-Regular.ttf",
                @"C:\Windows\Fonts\meiryo.ttc",
                @"C:\Windows\Fonts\meiryob.ttc",
                @"C:\Windows\Fonts\YuGothR.ttc",
                @"C:\Windows\Fonts\YuGothM.ttc",
                @"C:\Windows\Fonts\msgothic.ttc",
            };
            foreach (var f in files)
            {
                if (!File.Exists(f)) continue;
                fa = TMP_FontAsset.CreateFontAsset(f, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
                if (fa != null) { Debug.Log($"[JpFont] ファイルパスで生成成功: {f}"); break; }
            }
        }

        if (fa == null)
        {
            Debug.LogError("[JpFont] 日本語フォントの生成に失敗しました。");
            return null;
        }

        fa.name = "JapaneseTMPFont";
        AssetDatabase.CreateAsset(fa, assetPath);

        if (fa.atlasTextures != null)
            foreach (var tex in fa.atlasTextures)
                if (tex != null)
                {
                    tex.name = "JapaneseTMPFont Atlas";
                    AssetDatabase.AddObjectToAsset(tex, assetPath);
                }

        if (fa.material != null)
        {
            fa.material.name = "JapaneseTMPFont Material";
            AssetDatabase.AddObjectToAsset(fa.material, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

        var def = TMP_Settings.defaultFontAsset;
        if (def != null)
        {
            if (def.fallbackFontAssetTable == null)
                def.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!def.fallbackFontAssetTable.Contains(fa))
                def.fallbackFontAssetTable.Add(fa);
            EditorUtility.SetDirty(def);
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[JpFont] フォントアセット保存完了: {assetPath}");
        return fa;
    }

    // ═══════════════════════════════════════════════════
    //  ユーティリティ
    // ═══════════════════════════════════════════════════

    static GameObject Prim(PrimitiveType type, Transform parent,
        Vector3 localPos, Vector3 scale, Material mat, bool keepCol = false)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!keepCol)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
        }
        return go;
    }

    static Shader s_litShader;
    static Shader GetLitShader()
    {
        if (s_litShader != null) return s_litShader;
        var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s_litShader = tmp.GetComponent<Renderer>().sharedMaterial.shader;
        DestroyImmediate(tmp);
        return s_litShader;
    }

    static Material SaveMat(string name, Color color)
    {
        string path = $"Assets/Materials/TownRun/{name}.mat";
        var mat = new Material(GetLitShader());
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color);
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    static Material SaveMatEmissive(string name, Color color)
    {
        string path = $"Assets/Materials/TownRun/{name}.mat";
        var mat = new Material(GetLitShader());
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color * 0.5f);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color * 0.5f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", color * 3f);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        string path   = $"Assets/Prefabs/TownRun/{name}.prefab";
        var    prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    static GameObject MakeTMP(Transform parent, string goName, string text, float fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go  = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        var rt = tmp.rectTransform;
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        return go;
    }

    static Button MakeButton(Transform parent, string goName, string label,
        float fontSize, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        var go  = new GameObject(goName);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        var btn = go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
        MakeTMP(go.transform, "Label", label, fontSize,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        return btn;
    }

    static void SetFullScreen(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            AssetDatabase.CreateFolder(parent, name);
    }

    static void ClearScene()
    {
        // ルートオブジェクトを全削除（Missing Script の残骸も含め完全にゼロから構築）
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                               .GetRootGameObjects();
        foreach (var obj in roots)
            if (obj != null) DestroyImmediate(obj);
    }
}
