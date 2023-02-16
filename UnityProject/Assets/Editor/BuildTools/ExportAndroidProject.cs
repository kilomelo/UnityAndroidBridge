using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace kilomelo.Editor
{
    public class ExportAndroidProject : UnityEditor.Editor
    {
        // 启动场景
        private static string _startScene = "Assets/Example/ExampleEntrance.unity";
        // 安卓工程导出临时目录
        private static string _exportProjectTempDirRelativePath = "Export/Android";
        // unity lib 拷贝目标目录
        private static string _androidProjectRelativePath = "../AndroidProject";

        [MenuItem("Build/Export android project %#m", priority = 0)]
        public static void Export()
        {
            Build(_exportProjectTempDirRelativePath, true, false, false);
            Copy2AndroidProject();
        }

        [MenuItem("Build/Clean android project cache %#e", priority = 1)]
        public static void Clean()
        {
            var destPath = Path.Combine(_androidProjectRelativePath, "unityLibrary");
            FileTools.DeleteFolder(_exportProjectTempDirRelativePath);
            FileTools.DeleteFolder(destPath);
            Debug.Log($"ExportAndroidProject.Clean, buildPath: [{_exportProjectTempDirRelativePath}], destPath: [{destPath}]");
        }

        private static void Build(string exportPath, bool enableDevelopment, bool useMono, bool debug)
        {
            if (!useMono && debug)
            {
                Debug.LogError("ExportAndroidProject.Export error: not support build il2cpp with AllowDebugging.");
                throw new ArgumentException("not support build il2cpp with AllowDebugging");
            }
            var startTime = DateTimeUtils.GetNowSeconds();


            var backEnd = useMono ? "MONO" : "IL2CPP";
            Debug.Log($"ExportAndroidProject.Export start build, exportPath = {exportPath}.");
            Debug.Log($"ExportAndroidProject.Export enableDevelopment = {enableDevelopment}, backend = {backEnd}, debug = {debug}.");
            var buildPlayerOptions = new BuildPlayerOptions
            {
                // scenes = GetBuildScenes(),
                locationPathName = exportPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            if (enableDevelopment) buildPlayerOptions.options |= BuildOptions.Development;
            if (debug) buildPlayerOptions.options |= BuildOptions.AllowDebugging;
            //buildPlayerOptions.options = BuildOptions.ConnectWithProfiler | BuildOptions.Development | BuildOptions.EnableDeepProfilingSupport;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, useMono ? ScriptingImplementation.Mono2x : ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = useMono ? AndroidArchitecture.ARMv7 : AndroidArchitecture.ARM64;
            PlayerSettings.preserveFramebufferAlpha = true;

            var prevStackTraceLog = PlayerSettings.GetStackTraceLogType(LogType.Log);
            var prevStackTraceWarning = PlayerSettings.GetStackTraceLogType(LogType.Warning);
            var prevStackTraceError = PlayerSettings.GetStackTraceLogType(LogType.Error);
            var prevStackTraceException = PlayerSettings.GetStackTraceLogType(LogType.Exception);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            // PlayerSettings.SplashScreen.show = false;
            
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            PlayerSettings.SetStackTraceLogType(LogType.Log, prevStackTraceLog);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, prevStackTraceWarning);
            PlayerSettings.SetStackTraceLogType(LogType.Error, prevStackTraceError);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, prevStackTraceException);
            
            var summary = report.summary;
            var useTime = DateTimeUtils.GetNowSeconds() - startTime;
            Debug.Log($"Build time: {FormatUseTime(useTime)}");

            if (summary.result == BuildResult.Succeeded)
            {
                foreach (var step in report.steps)
                {
                    var stepSeconds = step.duration.TotalSeconds;
                    if (stepSeconds > 3)
                    {
                        Debug.Log($"Step [{step.name}] cost {FormatUseTime((long)stepSeconds)}");
                    }
                }
                Debug.Log($"Build finish with result succeeded, package size: {(float)summary.totalSize / 1024 / 1024} mb。");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.Log($"Build finish with result failed。");
                throw new Exception("Export Android Project Failed.");
            }
        }

        private static void Copy2AndroidProject()
        {
            var srcPath = Path.Combine(_exportProjectTempDirRelativePath, "unityLibrary");
            var destPath = Path.Combine(_androidProjectRelativePath, "unityLibrary");
            Debug.Log($"ExportAndroidProject.Copy2AndroidProject, srcPath: [{srcPath}], destPath: [{destPath}]");
            FileTools.DeleteFolder(destPath);
            FileTools.CheckAndCreateFolder(destPath);
            FileTools.CopyDirectory(srcPath, destPath);
        }
        /// <summary>
        /// Gets the build scenes.
        /// </summary>
        /// <returns>The build scenes.</returns>
        private static string[] GetBuildScenes()
        {       
            var builtInScenes = new List<string> {
                // 添加默认启动场景
                _startScene };

            return builtInScenes.ToArray(); 
        }
        
        private static string FormatUseTime(long timeSec)
        {
            if (timeSec < 60)
            {
                return timeSec + " s";
            }
            var formattedTime = new StringBuilder();
            if (timeSec >= 3600)
            {
                formattedTime.Append(timeSec / 3600).Append(" h ");
            }

            formattedTime.Append(timeSec / 60 % 60).Append(" m ");
            formattedTime.Append(timeSec % 60).Append(" s ");
            formattedTime.Append($"({timeSec} s)");
            return formattedTime.ToString();
        }
    }
}