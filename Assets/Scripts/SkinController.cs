using UnityEngine;

/// <summary>
/// Visual GO にアタッチ。
/// Apply(skinIndex) を呼ぶたびにキャラのジオメトリを完全に作り直す。
/// AutoSetupTownRun が作った4つの Pivot GO はそのまま残し、
/// その子と Visual 直下の見た目オブジェクトだけを再生成する。
/// </summary>
public class SkinController : MonoBehaviour
{
    // AutoSetupTownRun が設定（ProceduralRunAnimation と共有）
    [HideInInspector] public Transform leftArmPivot;
    [HideInInspector] public Transform rightArmPivot;
    [HideInInspector] public Transform leftLegPivot;
    [HideInInspector] public Transform rightLegPivot;

    void Start()
    {
        // ProceduralRunAnimation からフォールバック取得
        var anim = GetComponent<ProceduralRunAnimation>();
        if (anim != null)
        {
            if (!leftArmPivot)  leftArmPivot  = anim.leftArmPivot;
            if (!rightArmPivot) rightArmPivot = anim.rightArmPivot;
            if (!leftLegPivot)  leftLegPivot  = anim.leftLegPivot;
            if (!rightLegPivot) rightLegPivot = anim.rightLegPivot;
        }
        Apply(ShopManager.Instance?.SelectedSkin ?? 0);
    }

    public void Apply(int skinIndex)
    {
        ClearAllSkinGeometry();
        if (skinIndex < 0 || skinIndex >= SkinDef.All.Length) skinIndex = 0;
        switch (skinIndex)
        {
            case 1: BuildHero();  break;
            case 2: BuildNinja(); break;
            case 3: BuildRobot(); break;
            case 4: BuildKing();  break;
            default: BuildRunner(); break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Skin 0 ── ランナー（標準）
    // ─────────────────────────────────────────────────────────────────────────
    void BuildRunner()
    {
        var s = SkinDef.All[0];
        MovePivots(-0.33f,1.08f, 0.33f,1.08f, -0.13f,0.56f, 0.13f,0.56f);

        P(PrimitiveType.Capsule, T, new Vector3(0,0.85f,0),    new Vector3(0.44f,0.28f,0.28f), s.shirt); // torso
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.44f,0),    new Vector3(0.38f,0.38f,0.38f), s.skin);  // head
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.62f,0),    new Vector3(0.40f,0.22f,0.40f), s.hair);  // hair
        P(PrimitiveType.Cylinder,T, new Vector3(0,1.20f,0),    new Vector3(0.14f,0.12f,0.14f), s.skin);  // neck

        // arms
        P(PrimitiveType.Capsule, LA, new Vector3(0,-0.24f,0),  new Vector3(0.14f,0.24f,0.14f), s.shirt);
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.52f,0),  new Vector3(0.13f,0.13f,0.13f), s.skin);
        P(PrimitiveType.Capsule, RA, new Vector3(0,-0.24f,0),  new Vector3(0.14f,0.24f,0.14f), s.shirt);
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.52f,0),  new Vector3(0.13f,0.13f,0.13f), s.skin);

        // legs
        P(PrimitiveType.Capsule, LL, new Vector3(0,-0.37f,0),  new Vector3(0.17f,0.37f,0.17f), s.pants);
        P(PrimitiveType.Capsule, RL, new Vector3(0,-0.37f,0),  new Vector3(0.17f,0.37f,0.17f), s.pants);

        // shoes
        P(PrimitiveType.Cube, T, new Vector3(-0.13f,0.09f,0.06f), new Vector3(0.17f,0.12f,0.30f), s.shoe);
        P(PrimitiveType.Cube, T, new Vector3( 0.13f,0.09f,0.06f), new Vector3(0.17f,0.12f,0.30f), s.shoe);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Skin 1 ── 勇者（広い胸鎧・ヘルメット・剣・ケープ）
    // ─────────────────────────────────────────────────────────────────────────
    void BuildHero()
    {
        var s    = SkinDef.All[1];
        var gold = C(0.90f,0.75f,0.10f);
        var silv = C(0.78f,0.80f,0.84f);
        var dark = C(0.10f,0.08f,0.06f);
        var red  = C(0.80f,0.10f,0.08f);
        MovePivots(-0.42f,1.12f, 0.42f,1.12f, -0.15f,0.55f, 0.15f,0.55f);

        // 広い胸鎧
        P(PrimitiveType.Capsule, T, new Vector3(0,0.88f,0),      new Vector3(0.66f,0.32f,0.40f), s.shirt);
        P(PrimitiveType.Cube,    T, new Vector3(0.34f,0.88f,0),  new Vector3(0.05f,0.32f,0.30f), gold);   // 胸板
        P(PrimitiveType.Cube,    T, new Vector3(0.34f,0.96f,0),  new Vector3(0.06f,0.06f,0.22f), gold);   // 上リブ

        // 顔
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.50f,0),      new Vector3(0.36f,0.36f,0.36f), s.skin);
        // ヘルメット（球体ベース）
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.62f,0),      new Vector3(0.52f,0.38f,0.52f), gold);
        // バイザー
        P(PrimitiveType.Cube,    T, new Vector3(0.27f,1.54f,0),  new Vector3(0.06f,0.20f,0.34f), dark);
        // 兜の羽飾り
        P(PrimitiveType.Cube,    T, new Vector3(0,1.88f,0),      new Vector3(0.08f,0.24f,0.38f), red);
        // 首
        P(PrimitiveType.Cylinder,T, new Vector3(0,1.25f,0),      new Vector3(0.18f,0.14f,0.18f), s.skin);
        // ケープ
        P(PrimitiveType.Cube,    T, new Vector3(0,0.82f,-0.24f), new Vector3(0.54f,0.78f,0.06f), red);
        // 肩ガード
        P(PrimitiveType.Sphere,  T, new Vector3(-0.46f,1.14f,0), new Vector3(0.28f,0.24f,0.28f), gold);
        P(PrimitiveType.Sphere,  T, new Vector3( 0.46f,1.14f,0), new Vector3(0.28f,0.24f,0.28f), gold);
        // 剣（右側に装備）
        P(PrimitiveType.Cube,    T, new Vector3(0.64f,0.64f,0),  new Vector3(0.07f,0.74f,0.06f), silv);
        P(PrimitiveType.Cube,    T, new Vector3(0.64f,1.02f,0),  new Vector3(0.32f,0.06f,0.08f), gold);  // 鍔
        P(PrimitiveType.Cube,    T, new Vector3(0.64f,0.26f,0),  new Vector3(0.10f,0.14f,0.10f), dark);  // 柄頭

        // 腕（鎧付き）
        P(PrimitiveType.Capsule, LA, new Vector3(0,-0.27f,0),    new Vector3(0.18f,0.27f,0.18f), s.shirt);
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.18f,0),    new Vector3(0.24f,0.20f,0.24f), gold);  // 肘
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.57f,0),    new Vector3(0.16f,0.16f,0.16f), s.skin);
        P(PrimitiveType.Capsule, RA, new Vector3(0,-0.27f,0),    new Vector3(0.18f,0.27f,0.18f), s.shirt);
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.18f,0),    new Vector3(0.24f,0.20f,0.24f), gold);
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.57f,0),    new Vector3(0.16f,0.16f,0.16f), s.skin);

        // 脚（鎧付き）
        P(PrimitiveType.Capsule, LL, new Vector3(0,-0.38f,0),    new Vector3(0.22f,0.38f,0.22f), s.pants);
        P(PrimitiveType.Sphere,  LL, new Vector3(0,-0.24f,0),    new Vector3(0.28f,0.24f,0.28f), gold);  // 膝
        P(PrimitiveType.Capsule, RL, new Vector3(0,-0.38f,0),    new Vector3(0.22f,0.38f,0.22f), s.pants);
        P(PrimitiveType.Sphere,  RL, new Vector3(0,-0.24f,0),    new Vector3(0.28f,0.24f,0.28f), gold);

        // 重装ブーツ
        P(PrimitiveType.Cube, T, new Vector3(-0.15f,0.10f,0.04f), new Vector3(0.24f,0.16f,0.40f), s.shoe);
        P(PrimitiveType.Cube, T, new Vector3( 0.15f,0.10f,0.04f), new Vector3(0.24f,0.16f,0.40f), s.shoe);
        P(PrimitiveType.Cube, T, new Vector3(-0.15f,0.22f,0),     new Vector3(0.26f,0.12f,0.24f), gold); // ブーツ上
        P(PrimitiveType.Cube, T, new Vector3( 0.15f,0.22f,0),     new Vector3(0.26f,0.12f,0.24f), gold);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Skin 2 ── 忍者（低姿勢・幅広袴・長スカーフ・二刀・大手裏剣）
    // ─────────────────────────────────────────────────────────────────────────
    void BuildNinja()
    {
        var s    = SkinDef.All[2];
        var dark = C(0.05f,0.05f,0.07f);   // ほぼ黒
        var purp = C(0.20f,0.04f,0.30f);   // 濃紫スカーフ
        var ste  = C(0.68f,0.70f,0.74f);   // 刃の鋼
        var red  = C(0.88f,0.05f,0.05f);   // 赤アクセント
        var wrap = C(0.10f,0.10f,0.13f);   // 濃い布
        var gold = C(0.88f,0.78f,0.08f);   // 金の目
        // 低い重心・広い腰幅
        MovePivots(-0.30f,0.96f, 0.30f,0.96f, -0.20f,0.46f, 0.20f,0.46f);

        // ─── 胴体（スリム）────────────────────────────────────────────────
        P(PrimitiveType.Capsule, T, new Vector3(0,0.70f,0),            new Vector3(0.28f,0.24f,0.24f), dark);
        // 帯（幅広の赤ベルト）
        P(PrimitiveType.Cube,    T, new Vector3(0,0.50f,0),            new Vector3(0.40f,0.11f,0.30f), red);
        // 帯の金具
        P(PrimitiveType.Cube,    T, new Vector3(0.21f,0.50f,0.16f),    new Vector3(0.09f,0.09f,0.05f), ste);

        // ─── 首 ────────────────────────────────────────────────────────────
        P(PrimitiveType.Cylinder,T, new Vector3(0,0.96f,0),            new Vector3(0.10f,0.08f,0.10f), dark);

        // ─── 頭（多面体マスク）────────────────────────────────────────────
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.27f,0),            new Vector3(0.30f,0.30f,0.30f), s.skin);
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.32f,0),            new Vector3(0.37f,0.31f,0.37f), dark); // 頭巾全体
        P(PrimitiveType.Cube,    T, new Vector3(0.20f,1.37f,0),        new Vector3(0.06f,0.14f,0.30f), dark); // ひたい部
        P(PrimitiveType.Cube,    T, new Vector3(0.20f,1.21f,0),        new Vector3(0.05f,0.14f,0.30f), dark); // 口覆い
        // 肌露出（目鼻部分だけ）
        P(PrimitiveType.Cube,    T, new Vector3(0.21f,1.29f,0),        new Vector3(0.04f,0.08f,0.14f), s.skin);
        // 黄金の鋭い目（スリット×2）
        P(PrimitiveType.Cube,    T, new Vector3(0.25f,1.33f, 0.09f),   new Vector3(0.04f,0.038f,0.11f), gold);
        P(PrimitiveType.Cube,    T, new Vector3(0.25f,1.33f,-0.09f),   new Vector3(0.04f,0.038f,0.11f), gold);
        // 幅広ハチマキ
        P(PrimitiveType.Cube,    T, new Vector3(0,1.40f,0),            new Vector3(0.39f,0.065f,0.39f), red);
        // ハチマキの後ろたれ（2本）
        P(PrimitiveType.Cube,    T, new Vector3(-0.06f,1.34f,-0.26f),  new Vector3(0.10f,0.09f,0.08f), red);
        P(PrimitiveType.Cube,    T, new Vector3(-0.06f,1.24f,-0.30f),  new Vector3(0.07f,0.09f,0.07f), red);

        // ─── 長い流れるスカーフ（7段 → 首元から背中に垂れる）────────────
        P(PrimitiveType.Cube, T, new Vector3(0,0.93f,-0.22f),          new Vector3(0.28f,0.18f,0.07f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.04f,0.76f,-0.26f),      new Vector3(0.24f,0.15f,0.06f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.06f,0.61f,-0.28f),      new Vector3(0.20f,0.13f,0.05f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.07f,0.47f,-0.27f),      new Vector3(0.16f,0.12f,0.05f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.06f,0.34f,-0.25f),      new Vector3(0.12f,0.11f,0.04f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.04f,0.22f,-0.22f),      new Vector3(0.09f,0.10f,0.04f), purp);
        P(PrimitiveType.Cube, T, new Vector3(0.02f,0.12f,-0.18f),      new Vector3(0.06f,0.09f,0.03f), purp);

        // ─── 背中に二刀（X字クロス）──────────────────────────────────────
        P(PrimitiveType.Cube, T, new Vector3( 0.14f,0.80f,-0.20f),     new Vector3(0.05f,0.72f,0.05f), ste);
        P(PrimitiveType.Cube, T, new Vector3( 0.14f,1.14f,-0.20f),     new Vector3(0.20f,0.05f,0.06f), dark); // 鍔
        P(PrimitiveType.Sphere, T, new Vector3(0.14f,0.44f,-0.20f),    new Vector3(0.08f,0.08f,0.08f), dark); // 柄頭
        P(PrimitiveType.Cube, T, new Vector3(-0.14f,0.80f,-0.20f),     new Vector3(0.05f,0.72f,0.05f), ste);
        P(PrimitiveType.Cube, T, new Vector3(-0.14f,1.14f,-0.20f),     new Vector3(0.20f,0.05f,0.06f), dark);
        P(PrimitiveType.Sphere, T, new Vector3(-0.14f,0.44f,-0.20f),   new Vector3(0.08f,0.08f,0.08f), dark);

        // ─── 大手裏剣×2（腰サイドに装備）────────────────────────────────
        P(PrimitiveType.Cube,   T, new Vector3(0.30f,0.52f,0.18f),     new Vector3(0.22f,0.22f,0.04f), ste);
        P(PrimitiveType.Cube,   T, new Vector3(0.30f,0.52f,0.18f),     new Vector3(0.04f,0.22f,0.22f), ste);
        P(PrimitiveType.Sphere, T, new Vector3(0.30f,0.52f,0.18f),     new Vector3(0.08f,0.08f,0.08f), dark);
        P(PrimitiveType.Cube,   T, new Vector3(0.34f,0.40f,0.18f),     new Vector3(0.17f,0.17f,0.03f), ste);
        P(PrimitiveType.Cube,   T, new Vector3(0.34f,0.40f,0.18f),     new Vector3(0.03f,0.17f,0.17f), ste);

        // ─── 腕（前腕に小手ガード）────────────────────────────────────────
        P(PrimitiveType.Capsule, LA, new Vector3(0,-0.18f,0), new Vector3(0.11f,0.18f,0.11f), dark);
        P(PrimitiveType.Cube,    LA, new Vector3(0,-0.29f,0), new Vector3(0.19f,0.15f,0.19f), wrap); // 小手
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.41f,0), new Vector3(0.11f,0.11f,0.11f), s.skin);
        P(PrimitiveType.Capsule, RA, new Vector3(0,-0.18f,0), new Vector3(0.11f,0.18f,0.11f), dark);
        P(PrimitiveType.Cube,    RA, new Vector3(0,-0.29f,0), new Vector3(0.19f,0.15f,0.19f), wrap);
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.41f,0), new Vector3(0.11f,0.11f,0.11f), s.skin);

        // ─── 脚（幅広の袴 ── 大きくフレア）──────────────────────────────
        P(PrimitiveType.Cube,    LL, new Vector3(0,-0.14f,0), new Vector3(0.34f,0.28f,0.32f), dark); // 袴上
        P(PrimitiveType.Capsule, LL, new Vector3(0,-0.38f,0), new Vector3(0.15f,0.24f,0.15f), dark); // 脛
        P(PrimitiveType.Cube,    RL, new Vector3(0,-0.14f,0), new Vector3(0.34f,0.28f,0.32f), dark);
        P(PrimitiveType.Capsule, RL, new Vector3(0,-0.38f,0), new Vector3(0.15f,0.24f,0.15f), dark);

        // ─── 足袋（地下足袋スタイル）──────────────────────────────────────
        P(PrimitiveType.Cube, T, new Vector3(-0.20f,0.08f,0.04f),  new Vector3(0.16f,0.10f,0.30f), dark);
        P(PrimitiveType.Cube, T, new Vector3( 0.20f,0.08f,0.04f),  new Vector3(0.16f,0.10f,0.30f), dark);
        P(PrimitiveType.Cube, T, new Vector3(-0.20f,0.08f,0.14f),  new Vector3(0.08f,0.08f,0.10f), C(0.50f,0.50f,0.55f));
        P(PrimitiveType.Cube, T, new Vector3( 0.20f,0.08f,0.14f),  new Vector3(0.08f,0.08f,0.10f), C(0.50f,0.50f,0.55f));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Skin 3 ── ロボット（四角ボディ・アンテナ・バイザー・関節球）
    // ─────────────────────────────────────────────────────────────────────────
    void BuildRobot()
    {
        var s    = SkinDef.All[3];
        var cyan = C(0.20f,0.88f,1.00f);
        var dark = C(0.10f,0.10f,0.12f);
        var red  = C(0.90f,0.10f,0.05f);
        MovePivots(-0.34f,1.06f, 0.34f,1.06f, -0.14f,0.52f, 0.14f,0.52f);

        // 四角い胴体
        P(PrimitiveType.Cube,    T, new Vector3(0,0.86f,0),       new Vector3(0.56f,0.42f,0.38f), s.shirt);
        P(PrimitiveType.Cube,    T, new Vector3(0.30f,0.86f,0.20f), new Vector3(0.06f,0.30f,0.26f), dark);   // 胸ベンチ
        P(PrimitiveType.Cube,    T, new Vector3(0.30f,0.90f,0.20f), new Vector3(0.08f,0.06f,0.22f), cyan);   // シアングロー
        P(PrimitiveType.Cube,    T, new Vector3(0,0.66f,0.20f),   new Vector3(0.40f,0.06f,0.06f), s.pants);  // 腰ライン

        // 四角い頭
        P(PrimitiveType.Cube,    T, new Vector3(0,1.46f,0),       new Vector3(0.44f,0.40f,0.40f), s.shirt);
        // バイザー（目のストライプ）
        P(PrimitiveType.Cube,    T, new Vector3(0.23f,1.48f,0),   new Vector3(0.06f,0.16f,0.36f), cyan);
        // 赤い目×2
        P(PrimitiveType.Sphere,  T, new Vector3(0.25f,1.50f, 0.12f), new Vector3(0.08f,0.08f,0.08f), red);
        P(PrimitiveType.Sphere,  T, new Vector3(0.25f,1.50f,-0.12f), new Vector3(0.08f,0.08f,0.08f), red);
        // アンテナ
        P(PrimitiveType.Cube,    T, new Vector3(0,1.76f,0),       new Vector3(0.05f,0.22f,0.05f), s.shirt);
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.89f,0),       new Vector3(0.11f,0.11f,0.11f), red);
        // 首（四角関節）
        P(PrimitiveType.Cube,    T, new Vector3(0,1.24f,0),       new Vector3(0.22f,0.10f,0.20f), s.pants);

        // 四角い腕＋関節球
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.02f,0),     new Vector3(0.24f,0.22f,0.24f), s.pants); // 肩球
        P(PrimitiveType.Cube,    LA, new Vector3(0,-0.27f,0),     new Vector3(0.18f,0.28f,0.18f), s.shirt);
        P(PrimitiveType.Cube,    LA, new Vector3(0,-0.54f,0),     new Vector3(0.20f,0.12f,0.20f), s.pants); // クランプ手
        P(PrimitiveType.Cube,    LA, new Vector3(0,-0.22f,0),     new Vector3(0.20f,0.08f,0.20f), cyan);    // 肘ライン
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.02f,0),     new Vector3(0.24f,0.22f,0.24f), s.pants);
        P(PrimitiveType.Cube,    RA, new Vector3(0,-0.27f,0),     new Vector3(0.18f,0.28f,0.18f), s.shirt);
        P(PrimitiveType.Cube,    RA, new Vector3(0,-0.54f,0),     new Vector3(0.20f,0.12f,0.20f), s.pants);
        P(PrimitiveType.Cube,    RA, new Vector3(0,-0.22f,0),     new Vector3(0.20f,0.08f,0.20f), cyan);

        // 四角い脚＋関節球
        P(PrimitiveType.Sphere,  LL, new Vector3(0,-0.02f,0),     new Vector3(0.26f,0.24f,0.26f), s.pants); // 腰球
        P(PrimitiveType.Cube,    LL, new Vector3(0,-0.36f,0),     new Vector3(0.22f,0.36f,0.22f), s.shirt);
        P(PrimitiveType.Cube,    LL, new Vector3(0,-0.22f,0),     new Vector3(0.24f,0.08f,0.24f), cyan);    // 膝ライン
        P(PrimitiveType.Sphere,  RL, new Vector3(0,-0.02f,0),     new Vector3(0.26f,0.24f,0.26f), s.pants);
        P(PrimitiveType.Cube,    RL, new Vector3(0,-0.36f,0),     new Vector3(0.22f,0.36f,0.22f), s.shirt);
        P(PrimitiveType.Cube,    RL, new Vector3(0,-0.22f,0),     new Vector3(0.24f,0.08f,0.24f), cyan);

        // 重厚な金属足
        P(PrimitiveType.Cube, T, new Vector3(-0.14f,0.08f,0.06f), new Vector3(0.24f,0.14f,0.38f), s.pants);
        P(PrimitiveType.Cube, T, new Vector3( 0.14f,0.08f,0.06f), new Vector3(0.24f,0.14f,0.38f), s.pants);
        P(PrimitiveType.Cube, T, new Vector3(-0.14f,0.16f,0),     new Vector3(0.26f,0.08f,0.26f), dark);
        P(PrimitiveType.Cube, T, new Vector3( 0.14f,0.16f,0),     new Vector3(0.26f,0.08f,0.26f), dark);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Skin 4 ── 王様（太い体・ローブ・王冠・笏）
    // ─────────────────────────────────────────────────────────────────────────
    void BuildKing()
    {
        var s      = SkinDef.All[4];
        var gold   = C(0.90f,0.74f,0.10f);
        var red    = C(0.72f,0.08f,0.08f);
        var white  = C(0.95f,0.95f,0.95f);  // アーミン（白毛皮）
        var jewel  = C(0.20f,0.15f,0.85f);  // 青宝石
        MovePivots(-0.44f,1.04f, 0.44f,1.04f, -0.16f,0.50f, 0.16f,0.50f);

        // 太い丸い胴体
        P(PrimitiveType.Capsule, T, new Vector3(0,0.84f,0),       new Vector3(0.76f,0.34f,0.54f), s.shirt);
        // ローブ（下半身の広がり）
        P(PrimitiveType.Cylinder,T, new Vector3(0,0.44f,0),       new Vector3(0.72f,0.24f,0.72f), s.shirt);
        // ローブ裾のアーミン
        P(PrimitiveType.Cylinder,T, new Vector3(0,0.22f,0),       new Vector3(0.78f,0.04f,0.78f), white);
        // 首元のアーミン
        P(PrimitiveType.Cylinder,T, new Vector3(0,1.08f,0),       new Vector3(0.60f,0.07f,0.60f), white);
        // ケープ（後ろ）
        P(PrimitiveType.Cube,    T, new Vector3(0,0.80f,-0.30f),  new Vector3(0.62f,0.84f,0.06f), red);
        P(PrimitiveType.Cube,    T, new Vector3(0,0.40f,-0.30f),  new Vector3(0.64f,0.07f,0.07f), white); // ケープ裾

        // 大きな丸い頭
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.46f,0),       new Vector3(0.48f,0.46f,0.46f), s.skin);
        // ぷっくり頬
        P(PrimitiveType.Sphere,  T, new Vector3(-0.22f,1.40f,0.20f), new Vector3(0.20f,0.18f,0.16f), s.skin);
        P(PrimitiveType.Sphere,  T, new Vector3( 0.22f,1.40f,0.20f), new Vector3(0.20f,0.18f,0.16f), s.skin);
        // 口ひげ
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.30f,0.24f),   new Vector3(0.26f,0.12f,0.10f), s.hair);
        // 髪
        P(PrimitiveType.Sphere,  T, new Vector3(0,1.68f,0),       new Vector3(0.46f,0.18f,0.46f), s.hair);
        // 首（太い）
        P(PrimitiveType.Cylinder,T, new Vector3(0,1.23f,0),       new Vector3(0.24f,0.13f,0.24f), s.skin);

        // 王冠 ── 台座
        P(PrimitiveType.Cylinder,T, new Vector3(0,1.84f,0),       new Vector3(0.46f,0.16f,0.46f), gold);
        // 王冠スパイク×5
        float[] angles = { 0f, 72f, 144f, 216f, 288f };
        foreach (float a in angles)
        {
            float r = a * Mathf.Deg2Rad;
            P(PrimitiveType.Cube, T,
                new Vector3(Mathf.Sin(r)*0.16f, 2.04f, Mathf.Cos(r)*0.16f),
                new Vector3(0.10f,0.28f,0.10f), gold);
        }
        // 宝石
        P(PrimitiveType.Sphere, T, new Vector3(0,1.88f,0.23f),    new Vector3(0.11f,0.11f,0.11f), jewel);
        P(PrimitiveType.Sphere, T, new Vector3(0,1.88f,-0.23f),   new Vector3(0.09f,0.09f,0.09f), new Color(0.85f,0.10f,0.10f));

        // 笏
        P(PrimitiveType.Cylinder,T, new Vector3(0.70f,0.72f,0),   new Vector3(0.07f,0.52f,0.07f), gold);
        P(PrimitiveType.Sphere,  T, new Vector3(0.70f,1.26f,0),   new Vector3(0.16f,0.16f,0.16f), gold);
        P(PrimitiveType.Sphere,  T, new Vector3(0.70f,1.26f,0),   new Vector3(0.11f,0.11f,0.11f), jewel);

        // 太い短い腕
        P(PrimitiveType.Capsule, LA, new Vector3(0,-0.20f,0), new Vector3(0.24f,0.20f,0.24f), s.shirt);
        P(PrimitiveType.Sphere,  LA, new Vector3(0,-0.44f,0), new Vector3(0.20f,0.20f,0.20f), s.skin);
        P(PrimitiveType.Capsule, RA, new Vector3(0,-0.20f,0), new Vector3(0.24f,0.20f,0.24f), s.shirt);
        P(PrimitiveType.Sphere,  RA, new Vector3(0,-0.44f,0), new Vector3(0.20f,0.20f,0.20f), s.skin);

        // 太い短い脚
        P(PrimitiveType.Capsule, LL, new Vector3(0,-0.28f,0), new Vector3(0.26f,0.28f,0.26f), s.pants);
        P(PrimitiveType.Capsule, RL, new Vector3(0,-0.28f,0), new Vector3(0.26f,0.28f,0.26f), s.pants);

        // 靴（バックル付き）
        P(PrimitiveType.Cube, T, new Vector3(-0.16f,0.10f,0.04f), new Vector3(0.26f,0.16f,0.38f), s.shoe);
        P(PrimitiveType.Cube, T, new Vector3( 0.16f,0.10f,0.04f), new Vector3(0.26f,0.16f,0.38f), s.shoe);
        P(PrimitiveType.Cube, T, new Vector3(-0.16f,0.18f,0.18f), new Vector3(0.16f,0.12f,0.09f), gold);
        P(PrimitiveType.Cube, T, new Vector3( 0.16f,0.18f,0.18f), new Vector3(0.16f,0.12f,0.09f), gold);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ユーティリティ
    // ─────────────────────────────────────────────────────────────────────────
    Transform T  => transform;
    Transform LA => leftArmPivot;
    Transform RA => rightArmPivot;
    Transform LL => leftLegPivot;
    Transform RL => rightLegPivot;

    static Color C(float r, float g, float b) => new Color(r, g, b);

    void ClearAllSkinGeometry()
    {
        var pivots = new System.Collections.Generic.HashSet<Transform>
            { leftArmPivot, rightArmPivot, leftLegPivot, rightLegPivot };

        foreach (var p in pivots)
            if (p != null)
                for (int i = p.childCount - 1; i >= 0; i--)
                    Destroy(p.GetChild(i).gameObject);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (!pivots.Contains(c))
                Destroy(c.gameObject);
        }
    }

    void MovePivots(float lax, float lay, float rax, float ray,
                    float llx, float lly, float rlx, float rly)
    {
        if (leftArmPivot)  leftArmPivot.localPosition  = new Vector3(lax, lay, 0);
        if (rightArmPivot) rightArmPivot.localPosition = new Vector3(rax, ray, 0);
        if (leftLegPivot)  leftLegPivot.localPosition  = new Vector3(llx, lly, 0);
        if (rightLegPivot) rightLegPivot.localPosition = new Vector3(rlx, rly, 0);
    }

    Transform P(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color col)
    {
        if (parent == null) return null;
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = scale;
        Destroy(go.GetComponent<Collider>());
        var r = go.GetComponent<Renderer>();
        if (r != null)
        {
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", col);
            block.SetColor("_Color",     col);
            r.SetPropertyBlock(block);
        }
        return go.transform;
    }
}
