// Minimal version to prove the Tools menu works.
// File: Assets/Editor/ProjectHealthReportWindow.cs
#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ProjectHealthReportWindow : EditorWindow
{
    private Vector2 _scroll;
    private string _reportText = "Click 'Run Audit' to scan.";

    [MenuItem("Tools/Project Health Report/Run Audit")]
    public static void OpenAndRun()
    {
        var w = GetWindow<ProjectHealthReportWindow>("Project Health");
        w.minSize = new Vector2(600, 400);
        w.RunAudit();
        w.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Run Audit", GUILayout.Height(28)))
            RunAudit();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_reportText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void RunAudit()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Project Health Report");
        sb.AppendLine(DateTime.Now.ToString("u"));
        sb.AppendLine();
        sb.AppendLine("## Top-level under Assets");
        foreach (var f in AssetDatabase.GetSubFolders("Assets"))
            sb.AppendLine(" - " + f);
        _reportText = sb.ToString();
        Repaint();
    }
}
#endif
