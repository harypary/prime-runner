using UnityEngine;

/// <summary>
/// 街並みタイル。themeIndex で 5 種類の外観を切り替え。
///   0: 普通の街  ── 雑多なビル・緑の街路樹
///   1: 夜の街    ── 暗い超高層・ネオン看板・ネオン街灯
///   2: 砂漠の町  ── 低いアドベ建築・ドーム・ミナレット・サボテン
///   3: 未来都市  ── 超高層ガラスタワー・接続チューブ・ホバープラットフォーム
///   4: 黄金都市  ── 大理石宮殿・三角尖塔・黄金の柱・噴水
/// </summary>
public class CityTile : MonoBehaviour
{
    [HideInInspector] public int themeIndex = 0;
    public float tileLength = 30f;

    // ── 共通色 ──────────────────────────────────────────────────────────────
    static readonly Color WinFrame = new Color(0.90f, 0.90f, 0.88f);

    void Start()
    {
        ApplyRoadColors();
        GenerateStreet();
    }

    // ── 道路・歩道の色をテーマに合わせて上書き ─────────────────────────────
    void ApplyRoadColors()
    {
        Color road, sw, curb;
        switch (themeIndex)
        {
            case 1: road=C(0.02f,0.02f,0.04f); sw=C(0.10f,0.08f,0.14f); curb=C(0.16f,0.12f,0.20f); break;
            case 2: road=C(0.72f,0.60f,0.38f); sw=C(0.78f,0.68f,0.52f); curb=C(0.60f,0.50f,0.32f); break;
            case 3: road=C(0.10f,0.13f,0.20f); sw=C(0.20f,0.22f,0.30f); curb=C(0.28f,0.30f,0.38f); break;
            case 4: road=C(0.90f,0.88f,0.84f); sw=C(0.94f,0.92f,0.88f); curb=C(0.78f,0.74f,0.68f); break;
            default: return;
        }
        foreach (Transform child in transform)
        {
            var n = child.name;
            if      (n == "Road")     SetColor(child.gameObject, road);
            else if (n == "Sidewalk") SetColor(child.gameObject, sw);
            else if (n == "Curb")     SetColor(child.gameObject, curb);
        }
    }

    void GenerateStreet()
    {
        var saved = Random.state;
        Random.InitState(Mathf.RoundToInt(transform.position.magnitude * 17 + 1));

        float swEdge = 6.5f;
        GenerateBuildingRow( 1f, swEdge, tileLength);
        GenerateBuildingRow(-1f, swEdge, tileLength);
        GenerateSidewalkProps( 1f, 5.2f, tileLength);
        GenerateSidewalkProps(-1f, 5.2f, tileLength);

        Random.state = saved;
    }

    // ── 建物列の生成 ─────────────────────────────────────────────────────────
    void GenerateBuildingRow(float side, float startX, float tileLen)
    {
        // テーマごとに高さ・密度を変える
        float heightScale = themeIndex switch { 1=>1.6f, 2=>0.55f, 3=>2.8f, 4=>1.4f, _=>1.0f };
        float gapScale    = themeIndex switch { 2=>0.5f, 3=>0.6f, 4=>0.7f, _=>1.0f };

        float z = 0f;
        while (z < tileLen - 2f)
        {
            float depth   = Random.Range(5f, 9f);
            float width   = Random.Range(4f, 8f);
            float height  = Random.Range(6f, 20f) * heightScale;
            float gap     = Random.Range(0.3f, 1.5f) * gapScale;
            float centerZ = z + depth * 0.5f;
            if (centerZ + depth * 0.5f > tileLen) break;

            float cx    = side * (startX + width * 0.5f + Random.Range(0f, 1.2f));
            int   style = PickStyle();
            CreateBuilding(cx, centerZ, width, height, depth, side, style);
            z += depth + gap;
        }
    }

    int PickStyle()
    {
        // テーマ3(未来)・テーマ1(夜)はガラスビル多め
        if (themeIndex == 3) return Random.value < 0.7f ? 2 : Random.Range(0, 4);
        if (themeIndex == 1) return Random.value < 0.5f ? 2 : Random.Range(0, 4);
        if (themeIndex == 4) return Random.value < 0.6f ? 0 : Random.Range(0, 4); // オフィス多め
        return Random.Range(0, 4);
    }

    // ── テーマごとの壁色 ─────────────────────────────────────────────────────
    void GetBuildingColors(int theme, int style,
        out Color[] wallPalette, out Color baseCol, out Color winCol, out Color cornCol)
    {
        switch (theme)
        {
            case 1: // 夜の街 ─── 暗壁・ネオン窓
                wallPalette = style switch {
                    1 => new[]{ C(0.12f,0.06f,0.08f), C(0.10f,0.05f,0.06f), C(0.14f,0.07f,0.09f) },
                    2 => new[]{ C(0.05f,0.08f,0.14f), C(0.04f,0.06f,0.12f), C(0.06f,0.09f,0.16f) },
                    3 => new[]{ C(0.10f,0.08f,0.14f), C(0.08f,0.06f,0.12f), C(0.12f,0.10f,0.16f) },
                    _ => new[]{ C(0.06f,0.08f,0.14f), C(0.05f,0.07f,0.12f), C(0.08f,0.10f,0.18f) },
                };
                baseCol = C(0.04f,0.04f,0.08f);
                winCol  = C(0.10f,0.88f,1.00f);
                cornCol = C(0.20f,0.10f,0.30f);
                break;

            case 2: // 砂漠の町 ─── 砂岩色
                wallPalette = style switch {
                    1 => new[]{ C(0.80f,0.55f,0.30f), C(0.74f,0.50f,0.26f), C(0.84f,0.60f,0.34f) },
                    2 => new[]{ C(0.72f,0.52f,0.30f), C(0.66f,0.46f,0.26f), C(0.76f,0.56f,0.34f) },
                    3 => new[]{ C(0.90f,0.78f,0.55f), C(0.84f,0.72f,0.50f), C(0.94f,0.82f,0.60f) },
                    _ => new[]{ C(0.86f,0.70f,0.44f), C(0.80f,0.64f,0.38f), C(0.90f,0.74f,0.48f) },
                };
                baseCol = C(0.68f,0.52f,0.30f);
                winCol  = C(0.60f,0.44f,0.22f);
                cornCol = C(0.60f,0.44f,0.22f);
                break;

            case 3: // 未来都市 ─── 金属・シアン
                wallPalette = style switch {
                    1 => new[]{ C(0.18f,0.22f,0.30f), C(0.15f,0.18f,0.26f), C(0.20f,0.25f,0.34f) },
                    2 => new[]{ C(0.22f,0.28f,0.38f), C(0.18f,0.24f,0.34f), C(0.26f,0.32f,0.42f) },
                    3 => new[]{ C(0.38f,0.42f,0.50f), C(0.32f,0.36f,0.44f), C(0.44f,0.48f,0.56f) },
                    _ => new[]{ C(0.28f,0.34f,0.44f), C(0.24f,0.30f,0.40f), C(0.32f,0.38f,0.48f) },
                };
                baseCol = C(0.12f,0.14f,0.20f);
                winCol  = C(0.30f,0.85f,1.00f);
                cornCol = C(0.42f,0.50f,0.64f);
                break;

            case 4: // 黄金都市 ─── 金・大理石
                wallPalette = style switch {
                    1 => new[]{ C(0.88f,0.75f,0.22f), C(0.82f,0.68f,0.18f), C(0.92f,0.80f,0.28f) },
                    2 => new[]{ C(0.95f,0.92f,0.80f), C(0.90f,0.86f,0.74f), C(0.98f,0.95f,0.84f) },
                    3 => new[]{ C(0.82f,0.65f,0.15f), C(0.76f,0.58f,0.12f), C(0.86f,0.70f,0.20f) },
                    _ => new[]{ C(0.90f,0.80f,0.35f), C(0.85f,0.74f,0.28f), C(0.94f,0.84f,0.42f) },
                };
                baseCol = C(0.72f,0.55f,0.14f);
                winCol  = C(0.98f,0.90f,0.68f);
                cornCol = C(0.80f,0.65f,0.20f);
                break;

            default: // 普通の街
                wallPalette = style switch {
                    1 => new[]{ C(0.72f,0.38f,0.24f), C(0.65f,0.34f,0.22f), C(0.60f,0.42f,0.28f) },
                    2 => new[]{ C(0.28f,0.32f,0.38f), C(0.22f,0.28f,0.36f), C(0.32f,0.35f,0.40f) },
                    3 => new[]{ C(0.88f,0.82f,0.70f), C(0.82f,0.78f,0.65f), C(0.92f,0.86f,0.72f) },
                    _ => new[]{ C(0.72f,0.72f,0.74f), C(0.66f,0.68f,0.72f), C(0.76f,0.74f,0.70f) },
                };
                baseCol = style switch {
                    1=>C(0.82f,0.78f,0.68f), 2=>C(0.18f,0.20f,0.24f), 3=>C(0.70f,0.65f,0.54f), _=>C(0.40f,0.42f,0.46f)
                };
                winCol = style switch {
                    1=>C(0.88f,0.82f,0.68f), 2=>C(0.55f,0.70f,0.90f), 3=>C(0.78f,0.74f,0.60f), _=>C(0.48f,0.62f,0.80f)
                };
                cornCol = style switch {
                    1=>C(0.55f,0.30f,0.18f), 2=>C(0.38f,0.40f,0.44f), 3=>C(0.75f,0.68f,0.55f), _=>C(0.55f,0.55f,0.58f)
                };
                break;
        }
    }

    // ── 建物本体の生成（テーマ別ディスパッチ）────────────────────────────────
    void CreateBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        if (themeIndex == 1) { CreateNightBuilding(cx, cz, w, h, d, side, style);   return; }
        if (themeIndex == 2) { CreateDesertBuilding(cx, cz, w, h, d, side, style);  return; }
        if (themeIndex == 3) { CreateFutureBuilding(cx, cz, w, h, d, side, style);  return; }
        if (themeIndex == 4) { CreateGoldBuilding(cx, cz, w, h, d, side, style);    return; }
        CreateNormalBuilding(cx, cz, w, h, d, side, style);
    }

    void CreateNormalBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        GetBuildingColors(themeIndex, style, out var wallPalette, out var baseCol, out var winCol, out var cornCol);
        Color wallCol = wallPalette[Random.Range(0, wallPalette.Length)];
        float floorH  = Mathf.Clamp(h * 0.18f, 2.8f, 4.5f);

        // メインボディ
        SetColor(Box(cx, h*0.5f, cz, w, h, d), wallCol);
        SetColor(Box(cx, floorH*0.5f, cz, w+0.04f, floorH, d+0.04f), baseCol);

        // 1F 壁面
        float faceX_base = cx + side * (-w*0.5f - 0.06f);
        SetColor(Box(faceX_base, floorH*0.4f, cz, 0.06f, floorH*0.75f, w*0.22f),
                 C(0.18f,0.18f,0.18f));

        // 1F ショーウィンドウ
        int shopWins = Mathf.Max(1, (int)(d/3.5f));
        for (int i = 0; i < shopWins; i++)
        {
            float wz = cz - d*0.5f + (i+0.5f)*(d/shopWins);
            SetColor(Box(faceX_base, floorH*0.6f, wz, 0.06f, floorH*0.55f, (d/shopWins)*0.65f), winCol);
        }

        // 上階窓
        float faceX  = cx + side * (-w*0.5f - 0.05f);
        int winRows  = Mathf.Max(1, Mathf.RoundToInt((h-floorH)/3.2f));
        int winCols  = Mathf.Max(1, (int)(d/3.5f));
        float winH   = Mathf.Clamp((h-floorH)/winRows*0.55f, 0.7f, 1.8f);
        float winD   = Mathf.Min(1.8f, d/(winCols+0.5f)*0.65f);

        for (int row = 0; row < winRows; row++)
        {
            float wy = floorH + (h-floorH)*((row+0.5f)/winRows);
            for (int col = 0; col < winCols; col++)
            {
                float wz = cz - d*0.5f + (col+0.5f)*(d/winCols);
                SetColor(Box(faceX, wy, wz, 0.05f, winH+0.14f, winD+0.14f), WinFrame);
                SetColor(Box(faceX, wy, wz, 0.06f, winH, winD), winCol);
            }
        }

        // ガラスビルの縦フィン
        if (style == 2)
        {
            int fins = Mathf.Max(1, Mathf.RoundToInt(d/2.5f));
            for (int i = 0; i <= fins; i++)
            {
                float fz = cz - d*0.5f + i*(d/fins);
                SetColor(Box(faceX-side*0.05f, (h+floorH)*0.5f, fz, 0.12f, h-floorH, 0.12f), cornCol);
            }
        }

        // パラペット
        SetColor(Box(cx, h+0.22f, cz, w+0.20f, 0.45f, d+0.20f), cornCol);

        // ── テーマ別屋根装飾 ──────────────────────────────────────────────
        AddRoofDecoration(cx, cz, w, h, d, side, cornCol, winCol);
    }

    // ── テーマ1: 夜の街（超高層・ネオンバンド・アンテナ塔）─────────────────
    void CreateNightBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        GetBuildingColors(1, style, out var wallPalette, out var baseCol, out var winCol, out var cornCol);
        Color wallCol = wallPalette[Random.Range(0, wallPalette.Length)];
        float floorH  = Mathf.Clamp(h * 0.10f, 1.8f, 3.0f);
        float faceX   = cx + side * (-w * 0.5f - 0.05f);

        // 超暗い外壁（超高層ガラス）
        SetColor(Box(cx, h * 0.5f, cz, w, h, d), wallCol);
        SetColor(Box(cx, floorH * 0.5f, cz, w + 0.06f, floorH, d + 0.06f), baseCol);

        // ネオン水平バンド（4〜6本）
        Color[] neons = { C(0.10f,0.90f,1.00f), C(1.0f,0.15f,0.60f), C(0.80f,0.10f,1.00f), C(0.10f,1.0f,0.45f) };
        int nBands = Random.Range(4, 7);
        for (int i = 0; i < nBands; i++)
        {
            float bandY = floorH + (h - floorH) * ((i + 0.5f) / nBands);
            Color nc = neons[i % neons.Length];
            SetColor(Box(faceX - side * 0.04f, bandY, cz, 0.07f, 0.14f, d), nc);
        }

        // 縦ネオン看板（正面に大きく）
        SetColor(Box(faceX - side * 0.10f, h * 0.55f, cz, 0.12f, h * 0.32f, d * 0.42f), winCol);
        // 側面ネオン看板
        if (Random.value > 0.4f)
            SetColor(Box(faceX - side * 0.08f, h * 0.40f, cz + d * 0.28f, 0.10f, h * 0.24f, d * 0.25f),
                     neons[Random.Range(0, neons.Length)]);

        // 上階縦スリット窓
        int winCols = Mathf.Max(1, (int)(d / 3.5f));
        float winH  = Mathf.Clamp((h - floorH) * 0.68f, 1.0f, h * 0.65f);
        float winD  = Mathf.Min(1.6f, d / (winCols + 0.5f) * 0.60f);
        for (int col = 0; col < winCols; col++)
        {
            float wz = cz - d * 0.5f + (col + 0.5f) * (d / winCols);
            float wy = floorH + (h - floorH) * 0.5f;
            SetColor(Box(faceX, wy, wz, 0.06f, winH, winD), winCol);
        }

        // パラペット
        SetColor(Box(cx, h + 0.22f, cz, w + 0.20f, 0.45f, d + 0.20f), cornCol);

        // アンテナアレイ（3本）
        for (int i = 0; i < 3; i++)
            SetColor(Box(cx + Random.Range(-w * 0.3f, w * 0.3f), h + Random.Range(1.0f, 2.8f),
                         cz + Random.Range(-d * 0.3f, d * 0.3f), 0.05f, 1.8f, 0.05f),
                     C(0.50f, 0.52f, 0.55f));
        // 頂部の赤い航空灯
        SetColor(Box(cx, h + 3.2f, cz, 0.14f, 0.14f, 0.14f), C(1.0f, 0.08f, 0.08f));
    }

    // ── テーマ2: 砂漠の町（幅広低層・アーチ・ドーム必須・ミナレット）────────
    void CreateDesertBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        GetBuildingColors(2, style, out var wallPalette, out var baseCol, out var winCol, out var cornCol);
        Color wallCol = wallPalette[Random.Range(0, wallPalette.Length)];
        float faceX = cx + side * (-w * 0.5f - 0.04f);

        // 幅広・低いアドベ本体
        SetColor(Box(cx, h * 0.5f, cz, w, h, d), wallCol);
        // 基礎台（盛り土風）
        SetColor(Box(cx, h * 0.06f, cz, w + 0.30f, h * 0.12f, d + 0.30f), baseCol);

        // アーチ入口
        float archW = Mathf.Min(w * 0.32f, d * 0.50f);
        float archH = Mathf.Min(h * 0.50f, 3.2f);
        SetColor(Box(faceX, archH * 0.5f, cz, 0.07f, archH, archW), C(0.40f, 0.28f, 0.14f));
        // アーチの半円（球体行）
        int aSegs = 6;
        for (int i = 0; i < aSegs; i++)
        {
            float ang = Mathf.PI * i / (aSegs - 1);
            float bz  = cz + (archW * 0.5f) * Mathf.Cos(ang);
            float by  = archH + (archW * 0.48f) * Mathf.Sin(ang);
            var ball = Box(faceX, by, bz, 0.32f, 0.32f, 0.32f);
            SwapMesh(ball, PrimitiveType.Sphere); SetColor(ball, wallCol);
        }

        // 上部の小窓
        int winCols = Mathf.Max(1, (int)(d / 4.2f));
        float winH  = h * 0.20f;
        float winD  = Mathf.Min(1.3f, d / (winCols + 1f));
        for (int col = 0; col < winCols; col++)
        {
            float wz = cz - d * 0.5f + (col + 0.5f) * (d / winCols);
            SetColor(Box(faceX, h * 0.73f, wz, 0.06f, winH, winD), winCol);
        }

        // 屋上段差
        SetColor(Box(cx, h + 0.22f, cz, w + 0.24f, 0.44f, d + 0.24f), cornCol);

        // 必ずドームを追加（目立つ大きさ）
        float domeR = Mathf.Min(w, d) * 0.50f;
        var dome = Box(cx, h + domeR * 0.40f, cz, domeR * 2.0f, domeR * 1.0f, domeR * 2.0f);
        SwapMesh(dome, PrimitiveType.Sphere); SetColor(dome, cornCol);
        // ドーム頂飾り
        SetColor(Box(cx, h + domeR * 0.92f + 0.22f, cz, 0.15f, 0.45f, 0.15f), C(0.58f, 0.42f, 0.20f));

        // ミナレット（高確率・常に隣接）
        if (Random.value > 0.25f)
        {
            float mx  = cx + side * (-w * 0.42f);
            float mh  = h * 0.95f;
            float mfz = cz + (Random.value > 0.5f ? d * 0.30f : -d * 0.30f);
            SetColor(Box(mx, mh * 0.5f, mfz, 0.65f, mh, 0.65f), cornCol);
            var mDome = Box(mx, mh + 0.55f, mfz, 0.90f, 0.90f, 0.90f);
            SwapMesh(mDome, PrimitiveType.Sphere); SetColor(mDome, cornCol);
            SetColor(Box(mx, mh + 1.15f, mfz, 0.10f, 0.55f, 0.10f), C(0.52f, 0.38f, 0.16f));
        }
    }

    // ── テーマ3: 未来都市（3段テーパー・発光ライン・スカイブリッジ）─────────
    void CreateFutureBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        GetBuildingColors(3, style, out var wallPalette, out var baseCol, out var winCol, out var cornCol);
        Color wallCol = wallPalette[Random.Range(0, wallPalette.Length)];
        var glowCol = C(0.28f, 0.88f, 1.00f);

        // 3段テーパー構造
        const int tiers = 3;
        float tierH  = h / tiers;
        float ww = w, dd = d;
        for (int i = 0; i < tiers; i++)
        {
            float tY = tierH * i + tierH * 0.5f;
            SetColor(Box(cx, tY, cz, ww, tierH - 0.18f, dd), wallCol);
            // 段の縁に発光ライン
            SetColor(Box(cx, tierH * (i + 1), cz, ww + 0.12f, 0.16f, dd + 0.12f), glowCol);
            ww *= 0.72f; dd *= 0.78f;
        }

        // 正面2本の縦光柱
        float faceX = cx + side * (-w * 0.5f - 0.05f);
        SetColor(Box(faceX - side * 0.05f, h * 0.5f, cz - d * 0.28f, 0.09f, h * 1.02f, 0.09f), glowCol);
        SetColor(Box(faceX - side * 0.05f, h * 0.5f, cz + d * 0.28f, 0.09f, h * 1.02f, 0.09f), glowCol);

        // ホログラフィック窓（細い横スリット）
        int winRows = Mathf.Max(2, Mathf.RoundToInt(h * 0.25f));
        int winCols = Mathf.Max(1, (int)(d / 3.5f));
        float winH  = 0.38f;
        float curW  = w;
        for (int row = 0; row < winRows; row++)
        {
            float wy   = 0.7f + row * (h / winRows);
            float frac = wy / h;
            curW = w * (1.0f - frac * 0.45f);
            float fX   = cx + side * (-curW * 0.5f - 0.06f);
            float winD = Mathf.Min(1.4f, d / (winCols + 1f) * 0.55f);
            for (int col = 0; col < winCols; col++)
            {
                float wz = cz - d * 0.5f + (col + 0.5f) * (d / winCols);
                SetColor(Box(fX, wy, wz, 0.06f, winH, winD), winCol);
            }
        }

        // 上部スパイア
        float spireBase = ww * 0.5f;
        SetColor(Box(cx, h + 0.9f, cz, spireBase, 1.8f, spireBase * 0.8f), cornCol);
        SetColor(Box(cx, h + 2.0f, cz, spireBase * 0.5f, 0.9f, spireBase * 0.4f), glowCol);

        // スカイブリッジ（高確率）
        if (Random.value > 0.35f)
        {
            float brgH = h * Random.Range(0.50f, 0.72f);
            float brgW = 2.4f;
            SetColor(Box(cx + side * (w * 0.5f + brgW * 0.5f), brgH, cz,
                         brgW, 0.65f, d * 0.42f), C(0.20f, 0.26f, 0.36f));
            SetColor(Box(cx + side * (w * 0.5f + brgW * 0.5f), brgH + 0.38f, cz,
                         brgW + 0.1f, 0.09f, d * 0.42f + 0.1f), glowCol);
        }
    }

    // ── テーマ4: 黄金都市（常に柱・ペディメント・豪華装飾）──────────────────
    void CreateGoldBuilding(float cx, float cz, float w, float h, float d, float side, int style)
    {
        GetBuildingColors(4, style, out var wallPalette, out var baseCol, out var winCol, out var cornCol);
        Color wallCol = wallPalette[Random.Range(0, wallPalette.Length)];
        var gold   = C(0.90f, 0.75f, 0.12f);
        var marble = C(0.93f, 0.89f, 0.83f);

        // 基壇（大理石の台）
        SetColor(Box(cx, h * 0.035f, cz, w + 0.90f, h * 0.07f, d + 0.90f), marble);
        // 胴体
        SetColor(Box(cx, h * 0.5f, cz, w, h, d), wallCol);

        // 正面窓（アーチ風枠付き）
        float faceX = cx + side * (-w * 0.5f - 0.05f);
        int winCols = Mathf.Max(1, (int)(d / 4.2f));
        int winRows = Mathf.Max(1, (int)((h - 3.5f) / 4.5f));
        float winH  = Mathf.Clamp((h - 3.5f) / Mathf.Max(1, winRows) * 0.52f, 0.9f, 2.4f);
        float winD  = Mathf.Min(2.0f, d / (winCols + 0.5f) * 0.68f);
        for (int row = 0; row < winRows; row++)
        {
            float wy = 3.5f + (h - 3.5f) * ((row + 0.5f) / winRows);
            for (int col = 0; col < winCols; col++)
            {
                float wz = cz - d * 0.5f + (col + 0.5f) * (d / winCols);
                SetColor(Box(faceX, wy, wz, 0.05f, winH + 0.26f, winD + 0.26f), WinFrame);
                SetColor(Box(faceX, wy, wz, 0.06f, winH, winD), winCol);
                // アーチ頂（小さい球）
                var arch = Box(faceX, wy + winH * 0.5f + 0.14f, wz, 0.06f, 0.30f, winD * 0.38f);
                SwapMesh(arch, PrimitiveType.Sphere); SetColor(arch, marble);
            }
        }

        // 正面の柱×2（コーナー）
        float colX = cx + side * (-w * 0.5f - 0.14f);
        foreach (float pz in new[] { cz - d * 0.38f, cz + d * 0.38f })
        {
            var shaft = Box(colX, h * 0.5f, pz, 0.42f, h + 0.3f, 0.42f);
            SwapMesh(shaft, PrimitiveType.Cylinder); SetColor(shaft, marble);
            SetColor(Box(colX, h + 0.32f, pz, 0.65f, 0.34f, 0.65f), marble); // 柱頭
            SetColor(Box(colX, 0.14f,      pz, 0.65f, 0.28f, 0.65f), marble); // 柱脚
            SetColor(Box(colX, h + 0.50f,  pz, 0.68f, 0.14f, 0.68f), gold);   // 金帯
        }

        // コーニス（豪華軒蛇腹）
        SetColor(Box(cx, h + 0.24f, cz, w + 0.55f, 0.56f, d + 0.55f), gold);

        // ペディメント（段階的三角形）
        float pw = w + 0.4f, pd = d + 0.4f;
        SetColor(Box(cx, h + 0.80f, cz, pw,        0.22f, pd),        cornCol);
        SetColor(Box(cx, h + 1.22f, cz, pw * 0.74f, 0.22f, pd * 0.74f), cornCol);
        SetColor(Box(cx, h + 1.60f, cz, pw * 0.50f, 0.22f, pd * 0.50f), gold);
        SetColor(Box(cx, h + 1.96f, cz, pw * 0.28f, 0.22f, pd * 0.28f), gold);
        // 頂部の黄金球
        var orb = Box(cx, h + 2.35f, cz, pw * 0.18f, pw * 0.18f, pw * 0.18f);
        SwapMesh(orb, PrimitiveType.Sphere); SetColor(orb, C(0.96f, 0.84f, 0.12f));
    }

    void AddRoofDecoration(float cx, float cz, float w, float h, float d,
                           float side, Color cornCol, Color winCol)
    {
        switch (themeIndex)
        {
            case 0: // 普通 ── 屋上設備・アンテナ
                if (Random.value > 0.4f)
                    SetColor(Box(cx+Random.Range(-w*0.3f,w*0.3f), h+0.55f,
                                 cz+Random.Range(-d*0.3f,d*0.3f), 1.1f, 0.7f, 1.5f),
                             C(0.55f,0.56f,0.58f));
                if (Random.value > 0.5f)
                    SetColor(Box(cx, h+1.5f, cz+d*0.3f, 0.06f, 2.0f, 0.06f),
                             C(0.35f,0.35f,0.38f));
                break;

            case 1: // 夜の街 ── ネオン看板・尖塔
                // 光る縦看板
                SetColor(Box(cx+side*(-w*0.5f-0.12f), h*0.7f, cz+d*0.2f,
                             0.10f, h*0.25f, d*0.35f), winCol);
                // アンテナアレイ
                for (int i = 0; i < 3; i++)
                    SetColor(Box(cx+Random.Range(-w*0.3f,w*0.3f), h+Random.Range(0.8f,2.4f),
                                 cz+Random.Range(-d*0.3f,d*0.3f), 0.05f, 1.2f, 0.05f),
                             C(0.55f,0.56f,0.58f));
                // 頂部の赤い航空障害灯
                SetColor(Box(cx, h+2.6f, cz, 0.12f, 0.12f, 0.12f),
                         C(1.0f,0.1f,0.1f));
                break;

            case 2: // 砂漠 ── ドーム・ミナレット
                // ドーム
                var dome = Box(cx, h+w*0.22f, cz, w*0.60f, w*0.45f, w*0.60f);
                SwapMesh(dome, PrimitiveType.Sphere);
                SetColor(dome, cornCol);
                // ミナレット（小塔）
                if (Random.value > 0.4f)
                {
                    float mx = cx + side*(-w*0.38f);
                    SetColor(Box(mx, h*0.5f+h*0.3f, cz+d*0.3f, 0.70f, h*0.6f, 0.70f),
                             cornCol); // 細い塔
                    var mDome = Box(mx, h+h*0.3f+0.50f, cz+d*0.3f, 0.90f, 0.90f, 0.90f);
                    SwapMesh(mDome, PrimitiveType.Sphere);
                    SetColor(mDome, cornCol);
                }
                break;

            case 3: // 未来都市 ── ピラミッド頂部・ホバープラットフォーム
                // 先端のピラミッド型キャップ
                SetColor(Box(cx, h+w*0.25f, cz, w*0.70f, w*0.50f, d*0.70f), cornCol);
                SetColor(Box(cx, h+w*0.50f, cz, w*0.35f, w*0.25f, d*0.35f), winCol);
                // ホバープラットフォーム（水平フィン）
                if (Random.value > 0.5f)
                    SetColor(Box(cx, h*0.65f, cz, w+2.0f, 0.15f, d+2.0f),
                             C(0.42f,0.48f,0.60f));
                // 縦方向の光るストライプ
                for (int i = 0; i < 2; i++)
                    SetColor(Box(cx+side*(w*0.4f*(i==0?1f:-1f)), h*0.5f, cz,
                                 0.08f, h, 0.08f), winCol);
                break;

            case 4: // 黄金都市 ── 三角尖塔・大理石柱
                // 三角尖塔
                SetColor(Box(cx, h+w*0.4f, cz, w*0.50f, w*0.80f, d*0.50f), cornCol);
                SetColor(Box(cx, h+w*0.80f, cz, w*0.18f, w*0.40f, d*0.18f),
                         C(0.96f,0.86f,0.50f));
                // 両端の柱
                float[] pxArr = { cx + side*(-w*0.44f), cx + side*(-w*0.44f+0.8f) };
                foreach (float px in pxArr)
                    SetColor(Box(px, h*0.5f, cz, 0.45f, h, 0.45f), cornCol);
                // 金の球（尖塔頂部）
                var orb = Box(cx, h+w*1.18f, cz, 0.60f, 0.60f, 0.60f);
                SwapMesh(orb, PrimitiveType.Sphere);
                SetColor(orb, C(0.96f,0.82f,0.10f));
                break;
        }
    }

    // ── 歩道プロップ（テーマ別） ─────────────────────────────────────────────
    void GenerateSidewalkProps(float side, float propX, float tileLen)
    {
        float z = 3f;
        while (z < tileLen - 3f)
        {
            float tx = side * propX;
            PlaceProp(tx, z);
            z += Random.Range(6f, 10f);
        }
    }

    void PlaceProp(float tx, float z)
    {
        switch (themeIndex)
        {
            case 0: PlaceTree(tx, z, C(0.42f,0.28f,0.14f), C(0.22f,0.56f,0.20f)); break;
            case 1: PlaceNeonLamp(tx, z); break;
            case 2: PlaceCactus(tx, z);  break;
            case 3: PlaceHoloPillar(tx, z); break;
            case 4: PlaceColumn(tx, z); break;
        }
    }

    // テーマ0: 緑の街路樹
    void PlaceTree(float tx, float tz, Color trunk, Color leaves)
    {
        var t = Box(tx, 0.9f, tz, 0.18f, 1.8f, 0.18f);
        SwapMesh(t, PrimitiveType.Cylinder); SetColor(t, trunk);
        var l = Box(tx, 2.6f+Random.Range(0f,0.5f), tz, 1.1f, 1.1f, 1.1f);
        SwapMesh(l, PrimitiveType.Sphere); SetColor(l, leaves*(0.85f+Random.value*0.3f));
        if (Random.value > 0.5f)
        {
            var l2 = Box(tx+Random.Range(-0.3f,0.3f), 2.3f, tz+Random.Range(-0.3f,0.3f), 0.65f, 0.65f, 0.65f);
            SwapMesh(l2, PrimitiveType.Sphere); SetColor(l2, leaves*0.8f);
        }
    }

    // テーマ1: ネオン街灯 + 暗い植込み
    void PlaceNeonLamp(float tx, float tz)
    {
        // 支柱
        var pole = Box(tx, 1.5f, tz, 0.10f, 3.0f, 0.10f);
        SwapMesh(pole, PrimitiveType.Cylinder); SetColor(pole, C(0.20f,0.20f,0.24f));
        // 水平アーム
        float sign = tx > 0 ? -1f : 1f;
        SetColor(Box(tx+sign*0.4f, 3.1f, tz, 0.8f, 0.08f, 0.08f), C(0.20f,0.20f,0.24f));
        // ネオン球
        var bulb = Box(tx+sign*0.8f, 2.9f, tz, 0.35f, 0.35f, 0.35f);
        SwapMesh(bulb, PrimitiveType.Sphere); SetColor(bulb, C(0.10f,0.90f,1.00f));
        // 地面の暗い植込み
        var base_ = Box(tx, 0.12f, tz, 0.70f, 0.24f, 0.70f);
        SetColor(base_, C(0.12f,0.10f,0.14f));
        // 小さいネオン看板（ランダム）
        if (Random.value > 0.5f)
            SetColor(Box(tx+sign*0.55f, 2.4f, tz, 0.08f, 0.50f, 0.90f), C(1.0f,0.2f,0.6f));
    }

    // テーマ2: サボテン
    void PlaceCactus(float tx, float tz)
    {
        var trunkC = C(0.24f,0.48f,0.18f);
        // 幹
        var trunk = Box(tx, 1.0f, tz, 0.28f, 2.0f, 0.28f);
        SwapMesh(trunk, PrimitiveType.Cylinder); SetColor(trunk, trunkC);
        // 左腕
        SetColor(Box(tx-0.40f, 1.4f, tz, 0.22f, 0.80f, 0.22f), trunkC);
        SetColor(Box(tx-0.40f, 1.80f, tz, 0.22f, 0.50f, 0.22f), trunkC); // 垂直部
        // 右腕
        SetColor(Box(tx+0.40f, 1.6f, tz, 0.22f, 0.70f, 0.22f), trunkC);
        SetColor(Box(tx+0.40f, 2.00f, tz, 0.22f, 0.45f, 0.22f), trunkC);
        // トゲ（小さい球）
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f * Mathf.Deg2Rad;
            var sp = Box(tx+Mathf.Sin(a)*0.18f, 0.8f+i*0.5f, tz+Mathf.Cos(a)*0.18f,
                         0.08f, 0.08f, 0.08f);
            SwapMesh(sp, PrimitiveType.Sphere); SetColor(sp, C(0.20f,0.42f,0.14f));
        }
        // 砂の盛り
        SetColor(Box(tx, 0.06f, tz, 1.2f, 0.12f, 1.2f), C(0.78f,0.65f,0.40f));
    }

    // テーマ3: ホログラム柱
    void PlaceHoloPillar(float tx, float tz)
    {
        // 台座
        SetColor(Box(tx, 0.12f, tz, 0.60f, 0.24f, 0.60f), C(0.18f,0.22f,0.32f));
        // 柱
        var col = Box(tx, 1.4f, tz, 0.15f, 2.8f, 0.15f);
        SwapMesh(col, PrimitiveType.Cylinder); SetColor(col, C(0.30f,0.34f,0.44f));
        // ホロ球（上部）
        var orb = Box(tx, 3.0f, tz, 0.55f, 0.55f, 0.55f);
        SwapMesh(orb, PrimitiveType.Sphere); SetColor(orb, C(0.25f,0.80f,1.00f));
        // 内側の小さい球
        var inner = Box(tx, 3.0f, tz, 0.30f, 0.30f, 0.30f);
        SwapMesh(inner, PrimitiveType.Sphere); SetColor(inner, C(0.60f,0.95f,1.00f));
        // 横方向のリング（キューブで近似）
        SetColor(Box(tx, 3.0f, tz, 0.70f, 0.06f, 0.06f), C(0.30f,0.85f,1.00f));
        SetColor(Box(tx, 3.0f, tz, 0.06f, 0.06f, 0.70f), C(0.30f,0.85f,1.00f));
    }

    // テーマ4: 大理石の柱と小オベリスク
    void PlaceColumn(float tx, float tz)
    {
        var marble = C(0.92f,0.88f,0.82f);
        var gold   = C(0.88f,0.74f,0.18f);
        // 台座
        SetColor(Box(tx, 0.15f, tz, 0.80f, 0.30f, 0.80f), marble);
        // 柱身
        var shaft = Box(tx, 1.6f, tz, 0.36f, 3.2f, 0.36f);
        SwapMesh(shaft, PrimitiveType.Cylinder); SetColor(shaft, marble);
        // 柱頭（キャピタル）
        SetColor(Box(tx, 3.3f, tz, 0.55f, 0.28f, 0.55f), marble);
        SetColor(Box(tx, 3.50f, tz, 0.60f, 0.12f, 0.60f), gold);

        // 小オベリスク（ランダム配置）
        if (Random.value > 0.5f)
        {
            float ox = tx + Random.Range(-0.6f, 0.6f);
            float oz = tz + Random.Range(-0.6f, 0.6f);
            SetColor(Box(ox, 0.10f, oz, 0.40f, 0.20f, 0.40f), marble);
            SetColor(Box(ox, 0.85f, oz, 0.28f, 1.5f, 0.28f), marble);
            SetColor(Box(ox, 1.68f, oz, 0.22f, 0.50f, 0.22f), marble);
            SetColor(Box(ox, 1.96f, oz, 0.10f, 0.20f, 0.10f), gold);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ユーティリティ
    // ─────────────────────────────────────────────────────────────────────────
    static Color C(float r, float g, float b) => new Color(r, g, b);

    GameObject Box(float x, float y, float z, float sx, float sy, float sz)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, y, z);
        go.transform.localScale    = new Vector3(sx, sy, sz);
        go.transform.localRotation = Quaternion.identity;
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    void SwapMesh(GameObject go, PrimitiveType type)
    {
        var tmp = GameObject.CreatePrimitive(type);
        go.GetComponent<MeshFilter>().sharedMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tmp);
    }

    static void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color",     color);
        r.SetPropertyBlock(block);
    }
}
