using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.Linq;
using UnityEngine;

public class BuildScript
{
    public static void BuildiOS()
    {
        string buildPath = GetArg("-customBuildPath") ?? "ios_build";

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray(),
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log($"[BuildScript] Build succeeded: {buildPath}");
        }
    }

    static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
}
