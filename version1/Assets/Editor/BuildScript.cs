using UnityEditor;
using UnityEngine;

namespace SpaceMissionEditor
{
    public static class BuildScript
    {
        static string[] Scenes => new[]
        {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/SimLaunch.unity"
        };

        public static void BuildMac()
        {
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Mac/MyMission",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[BuildScript] Mac build: " + report.summary.result + " size=" + report.summary.totalSize);
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildWindows()
        {
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Windows/MyMission.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[BuildScript] Windows build: " + report.summary.result + " size=" + report.summary.totalSize);
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }
    }
}
