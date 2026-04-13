// IOSPostBuild.cs — iOSビルド後に GADApplicationIdentifier を Info.plist へ自動注入
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class IOSPostBuild
{
    const string GAD_APP_ID = "ca-app-pub-8388601065600220~2483206060";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // GADApplicationIdentifier — AdMob App ID
        plist.root.SetString("GADApplicationIdentifier", GAD_APP_ID);

        // 輸出コンプライアンス
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        // 日本語のみ対応
        plist.root.SetString("CFBundleDevelopmentRegion", "ja");
        if (!plist.root.values.ContainsKey("CFBundleLocalizations"))
        {
            var langs = plist.root.CreateArray("CFBundleLocalizations");
            langs.AddString("ja");
        }

        // ATT フレームワーク不使用を明示（審査官向け）
        // ※ NSUserTrackingUsageDescription は意図的に設定しない（ATT 未使用のため）

        // iOS 14+ SKAdNetwork（AdMob 審査通過率向上）
        if (!plist.root.values.ContainsKey("SKAdNetworkItems"))
        {
            var skItems = plist.root.CreateArray("SKAdNetworkItems");
            var item = skItems.AddDict();
            item.SetString("SKAdNetworkIdentifier", "cstr6suwn9.skadnetwork"); // Google
        }

        plist.WriteToFile(plistPath);
        UnityEngine.Debug.Log("[IOSPostBuild] Info.plist を設定しました（GADAppID / 輸出コンプライアンス / SKAdNetwork）。");
    }
}
#endif
