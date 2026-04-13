using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面の横向き走行キャラクター（UI Image で構成）
/// TitleScreenEffect と同じ GameObject にアタッチして使う
/// </summary>
public class UIRunnerCharacter : MonoBehaviour
{
    static readonly Color Skin  = new Color(1.00f, 0.75f, 0.56f);
    static readonly Color Hair  = new Color(0.22f, 0.14f, 0.06f);
    static readonly Color Shirt = new Color(0.90f, 0.25f, 0.15f);
    static readonly Color Pants = new Color(0.18f, 0.22f, 0.55f);
    static readonly Color Shoe  = new Color(0.15f, 0.08f, 0.03f);

    RectTransform _root;
    RectTransform _lLeg, _rLeg, _lArm, _rArm;
    RectTransform _headRt;

    float _cycle;
    const float Speed = 150f;   // px/sec
    const float XMin  = -700f;
    const float XMax  = 800f;

    void Awake() => Build();

    void Build()
    {
        // ── キャラルート（スタートボタンの下あたりに配置）──────────
        var rootGO = new GameObject("RunnerRoot");
        rootGO.transform.SetParent(transform, false);
        _root            = rootGO.AddComponent<RectTransform>();
        _root.anchorMin  = _root.anchorMax = new Vector2(0f, 0.15f); // 画面下15%
        _root.pivot      = new Vector2(0.5f, 0f);
        _root.sizeDelta  = new Vector2(50f, 110f);
        _root.anchoredPosition = new Vector2(XMin + 100f, 0f);

        // ── 各パーツ ─────────────────────────────────────────────
        // 胴体
        Img(rootGO.transform, "Body",
            pos: new Vector2(0, 38), size: new Vector2(22, 32), col: Shirt,
            pivot: new Vector2(0.5f, 0f));

        // 頭
        _headRt = Img(rootGO.transform, "Head",
            pos: new Vector2(0, 72), size: new Vector2(26, 26), col: Skin,
            pivot: new Vector2(0.5f, 0f));
        Img(rootGO.transform, "Hair",
            pos: new Vector2(0, 92), size: new Vector2(28, 12), col: Hair,
            pivot: new Vector2(0.5f, 0f));

        // 腕 ── ピボット上端（肩）で回転
        _lArm = Img(rootGO.transform, "LAm",
            pos: new Vector2(-15, 70), size: new Vector2(9, 26), col: Shirt,
            pivot: new Vector2(0.5f, 1f));
        _rArm = Img(rootGO.transform, "RAm",
            pos: new Vector2(15, 70), size: new Vector2(9, 26), col: Shirt,
            pivot: new Vector2(0.5f, 1f));

        // 脚 ── ピボット上端（腰）で回転
        _lLeg = Img(rootGO.transform, "LLg",
            pos: new Vector2(-7, 36), size: new Vector2(11, 34), col: Pants,
            pivot: new Vector2(0.5f, 1f));
        _rLeg = Img(rootGO.transform, "RLg",
            pos: new Vector2(7, 36), size: new Vector2(11, 34), col: Pants,
            pivot: new Vector2(0.5f, 1f));

        // 靴 ── 脚の子として配置
        Img(_lLeg, "LSh",
            pos: new Vector2(3, -34), size: new Vector2(17, 8), col: Shoe,
            pivot: new Vector2(0f, 1f));
        Img(_rLeg, "RSh",
            pos: new Vector2(3, -34), size: new Vector2(17, 8), col: Shoe,
            pivot: new Vector2(0f, 1f));
    }

    RectTransform Img(Transform parent, string name,
                      Vector2 pos, Vector2 size, Color col, Vector2 pivot)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = col;
        return rt;
    }

    void Update()
    {
        if (_root == null) return;

        float dt = Time.unscaledDeltaTime;
        _cycle += dt * 9f;

        float swing = Mathf.Sin(_cycle) * 32f;
        float bob   = Mathf.Abs(Mathf.Sin(_cycle * 2f)) * 4f;

        // 脚の振り
        if (_lLeg) _lLeg.localRotation = Quaternion.Euler(0, 0, swing);
        if (_rLeg) _rLeg.localRotation = Quaternion.Euler(0, 0, -swing);
        // 腕は脚と逆位相
        if (_lArm) _lArm.localRotation = Quaternion.Euler(0, 0, -swing * 0.65f);
        if (_rArm) _rArm.localRotation = Quaternion.Euler(0, 0,  swing * 0.65f);
        // 上下バウンド
        if (_headRt)
        {
            var hp = _headRt.anchoredPosition;
            hp.y = 72f + bob;
            _headRt.anchoredPosition = hp;
        }

        // 左→右へ移動し、右端で左端にループ
        var p = _root.anchoredPosition;
        p.x += Speed * dt;
        if (p.x > XMax) p.x = XMin;
        _root.anchoredPosition = p;
    }
}
