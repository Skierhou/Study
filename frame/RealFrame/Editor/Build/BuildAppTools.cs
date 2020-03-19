using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class BuildAppTools
{
    private static string APPNAME = EditorConfig.GetEditorConfig().APPName;
    private static string IOSPATH = Application.dataPath + "/../BuildTarget/IOS/";
    private static string ADNROIDPATH = Application.dataPath + "/../BuildTarget/Adnroid/";
    private static string WINDOWPATH = Application.dataPath + "/../BuildTarget/Windows/";

    [MenuItem("Tools/Build/标准Build")]
    public static void Build()
    {
        string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        Copy(abPath, Application.streamingAssetsPath);
        string buildPath = "";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            buildPath = IOSPATH + APPNAME + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            buildPath = ADNROIDPATH + APPNAME + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {
            buildPath = WINDOWPATH + APPNAME + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, APPNAME);
        }
        string[] sceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        BuildPipeline.BuildPlayer(sceneList, buildPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

        DelectDir(Application.streamingAssetsPath);
    }
    public static void Copy(string srcPath,string targetPath)
    {
        try
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            string srcDir = Path.Combine(targetPath, Path.GetFileName(srcPath));
            Debug.Log(srcDir);
            if (Directory.Exists(srcPath))
            {
                srcDir += Path.DirectorySeparatorChar;
            }
            Debug.Log(srcDir);

            string[] files = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    Copy(file, srcDir);
                }
                else
                {
                    File.Copy(file, srcDir + Path.GetFileName(file), true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("无法从：" + srcPath + " 复制到：" + targetPath);
        }
    }
    public static void DelectDir(string srcPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)            //判断是否文件夹
                {
                    DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                    subdir.Delete(true);          //删除子目录和文件
                }
                else
                {
                    File.Delete(i.FullName);      //删除指定文件
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
}