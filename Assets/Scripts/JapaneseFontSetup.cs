using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ゲーム起動時（シーンロード前）に自動実行。
/// TMP_FontAsset.CreateFontAsset(familyName, styleName) は
/// 内部で FontEngine.TryGetSystemFontReference → ファイルパス直接ロード (DynamicOS) に
/// なるため「Include Font Data」エラーが発生しない。
/// MonoBehaviour 不要 / AddComponent 不要 / 自動起動。
/// </summary>
public static class JapaneseFontSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        TMP_FontAsset fa = null;

        string[][] pairs =
        {
            new[] { "Yu Gothic",  "Regular" },
            new[] { "Yu Gothic",  "Light"   },
            new[] { "Meiryo",     "Regular" },
            new[] { "MS Gothic",  "Regular" },
        };

        foreach (var p in pairs)
        {
            fa = TMP_FontAsset.CreateFontAsset(p[0], p[1]);
            if (fa != null)
            {
                Debug.Log($"[JapaneseFontSetup] フォントロード成功: {p[0]} {p[1]}");
                break;
            }
        }

        if (fa == null)
        {
            Debug.LogError("[JapaneseFontSetup] 日本語フォントの生成に失敗しました。");
            return;
        }

        var def = TMP_Settings.defaultFontAsset;
        if (def == null)
        {
            Debug.LogWarning("[JapaneseFontSetup] TMP_Settings.defaultFontAsset が null です。");
            return;
        }

        if (def.fallbackFontAssetTable == null)
            def.fallbackFontAssetTable = new List<TMP_FontAsset>();

        if (!def.fallbackFontAssetTable.Contains(fa))
            def.fallbackFontAssetTable.Add(fa);

        Debug.Log("[JapaneseFontSetup] 日本語フォント フォールバック登録完了");
    }
}
