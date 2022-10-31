using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;

namespace LiteNinja.Pooling.Editor
{
    /// <summary>
    /// Draw a window to debug the pooling system.
    /// Print for each pool the number of objects in the pool and the number of objects in use.
    /// </summary>
    public class PoolingDebugWindow : EditorWindow
    {
        

        private const long UPDATE_INTERVAL = 1000;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly List<DebugPoolData> poolData = new List<DebugPoolData>();


        [MenuItem("LiteNinja/Pooling/Debugger")]
        private static void OpenWindow()
        {
            var window = GetWindow<PoolingDebugWindow>("Pooling Debugger");
            window.Show();
        }

        private void OnEnable()
        {
            UpdateMonitors();
            stopwatch.Start();
            EditorApplication.update += WindowUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= WindowUpdate;
            stopwatch.Stop();
        }

        private void WindowUpdate()
        {
            if (stopwatch.ElapsedMilliseconds < UPDATE_INTERVAL) return;
            stopwatch.Reset();
            stopwatch.Start();
            UpdateMonitors();
        }

        private void UpdateMonitors()
        {
            var pools = PoolManager.GetPools();
            poolData.Clear();
            foreach (var pool in pools)
            {
                poolData.Add(new DebugPoolData(pool.Key.name, pool.Value.ActiveObjectCount, pool.Value.InactiveObjectCount));
            }

            Repaint();
        }

        private void OnGUI()
        {
            DisplayHeader();

            if (Application.isPlaying)
            {
                DisplayRuntimeGUI();
            }
            else
            {
                GUILayout.Label("Start playmode to start debugging pools and pooled objects.");
            }
        }

        private void DisplayHeader()
        {
            GUILayout.BeginHorizontal();
            DrawTitle("LiteNinja Pooling");
            GUILayout.FlexibleSpace();
            DrawTitle("Debug Window");
            GUILayout.EndHorizontal();
        }

        private void DisplayRuntimeGUI()
        {
            DisplayVisibilityButton();
            DisplayPoolStatistics();
        }

        private static void DisplayVisibilityButton()
        {
            DrawTitle("Pooled object visibility");

            GUILayout.BeginHorizontal();
            var previousColor = GUI.color;

            GUILayout.Label("You can make pooled objects visible to debug their state.");

            string buttonText;
            if (PoolManager.ObjectsAreVisibleInHierarchy)
            {
                buttonText = "Hide pooled instances";
                GUI.color *= new Color(1, 0.5f, 0.5f, 1);
            }
            else
            {
                buttonText = "Show pooled instances";
                GUI.color *= new Color(0.5f, 1, 0.5f, 1);
            }

            if (GUILayout.Button(buttonText, GUILayout.Width(150), GUILayout.Height(50)))
            {
                PoolManager.SetEditorObjectVisibility(!PoolManager.ObjectsAreVisibleInHierarchy);
                RepaintHierarchyWindow();
            }

            GUI.color = previousColor;
            GUILayout.EndHorizontal();
        }

        private static void RepaintHierarchyWindow()
        {
            try
            {
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            catch
            {
            }
        }

        private void DisplayPoolStatistics()
        {
            DrawTitle("Current pooling activity");
            if (poolData.Count > 0)
            {
                DrawTableRow("", "Active", "Inactive");
                foreach (var pool in poolData)
                {
                    DrawTableRow(pool.title, pool.activeObjectCount + "", pool.inactiveObjectCount + "");
                }
            }
            else
            {
                GUILayout.Label("There are no active pools.");
            }
        }

        private static void DrawTitle(string s)
        {
            GUILayout.Label(s, EditorStyles.boldLabel);
        }

        private void DrawTableRow(string prefix, string column0, string column1)
        {
            const float prefixWidth = 250;
            var columnWidth = (this.position.width - prefixWidth) / 2;

            GUILayout.BeginHorizontal();
            GUILayout.Label(prefix, EditorStyles.boldLabel, GUILayout.Width(prefixWidth));
            GUILayout.Label(column0, GUILayout.Width(columnWidth));
            GUILayout.Label(column1, GUILayout.Width(columnWidth));
            GUILayout.EndHorizontal();
        }
    }
}