using UnityEngine;
using TMPro;

/// <summary>
/// ゲートの見た目と数字テキストを管理する。
/// テキストは Start() でランタイム生成（TMP をエディタスクリプトで
/// プレハブに保存するとフォントマテリアルが失われる問題を回避）。
/// </summary>
public class Gate : MonoBehaviour
{
    [Header("表示する数字")]
    public int  number;
    public bool isPrimeGate;

    [Header("UI参照")]
    public MeshRenderer frameRenderer;

    private TextMeshPro _frontText;
    private TextMeshPro _backText;

    void Start()
    {
        // 前面テキスト：プレイヤーが接近してくる方向（-Z 方向）に向ける
        // yRot=180 で TMP を -Z 向きにし、scaleX=-0.01 で左右反転を補正する
        if (_frontText == null)
            _frontText = CreateText(
                "NumberText",
                new Vector3(0f, 7f, -0.16f),
                180f,
                new Vector3(-0.03f, 0.03f, 0.03f));

        // 背面テキスト：通過後の方向
        if (_backText == null)
            _backText = CreateText(
                "NumberTextBack",
                new Vector3(0f, 7f, +0.16f),
                0f,
                new Vector3(0.03f, 0.03f, 0.03f));

        UpdateText();
    }

    /// <summary>GatePair.Setup() からも手動で呼べる初期化処理。</summary>
    public void Init()
    {
        // Start() より前に呼ばれた場合は number だけ保持し、Start() 内で表示する
        UpdateText();
    }

    void UpdateText()
    {
        string s = number.ToString();
        if (_frontText != null) _frontText.text = s;
        if (_backText  != null) _backText.text  = s;
    }

    /// <summary>指定の姿勢で TextMeshPro を生成して返す。</summary>
    TextMeshPro CreateText(string goName, Vector3 localPos, float yRot, Vector3 scale)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
        go.transform.localScale    = scale;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text             = number.ToString();
        tmp.fontStyle        = FontStyles.Bold;
        tmp.color            = Color.black;          // 青いボードに黒文字で高コントラスト
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin      = 50f;
        tmp.fontSizeMax      = 400f;                 // 桁数に合わせて自動サイズ調整

        // sizeDelta: canvas 座標（scale 0.03 → 世界座標 7.5m × 13.2m、板いっぱい）
        tmp.rectTransform.sizeDelta = new Vector2(250f, 440f);

        return tmp;
    }
}
