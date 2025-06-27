using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO; // to read files when checking scripts
using System.Linq; // to use Where and Select functions

public class UnusedAssetFinderWindow : EditorWindow
{
    private List<string> unusedAssets = new List<string>();
    private HashSet<string> selectedAssetsToDelete = new HashSet<string>();
    private Vector2 scroll;
    private bool includeScripts = false;

    [MenuItem("Tools/Unused Assets Finder")]
    public static void ShowWindow()
    {
        // open the window from the unity tools menu
        GetWindow<UnusedAssetFinderWindow>("Unused Assets Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unused Assets Finder", EditorStyles.boldLabel);

        // toggle to include .cs scripts in the search or not
        includeScripts = GUILayout.Toggle(includeScripts, "Include unused .cs (script) files");

        if (GUILayout.Button("Find Unused Assets"))
        {
            FindUnusedAssets();
        }

        if (unusedAssets.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Unused Assets Found: " + unusedAssets.Count, EditorStyles.boldLabel);

            if (GUILayout.Button("Delete Selected Assets"))
            {
                if (selectedAssetsToDelete.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Selection", "Please select assets to delete.", "OK");
                }
                else
                {
                    // ask for confirmation before deleting assets
                    bool confirm = EditorUtility.DisplayDialog("Confirm Deletion",
                        "Are you sure you want to delete " + selectedAssetsToDelete.Count + " selected assets?",
                        "Yes", "Cancel");

                    if (confirm)
                    {
                        foreach (string path in selectedAssetsToDelete)
                        {
                            AssetDatabase.DeleteAsset(path); // delete the asset from project
                            Debug.Log("Deleted: " + path);
                        }

                        AssetDatabase.Refresh(); // refresh the editor to update project view

                        FindUnusedAssets(); // find again to update the list
                    }
                }
            }

            scroll = GUILayout.BeginScrollView(scroll);

            foreach (string asset in unusedAssets)
            {
                GUILayout.BeginHorizontal();

                // checkbox to select asset for deletion
                bool isSelected = selectedAssetsToDelete.Contains(asset);
                bool newSelected = GUILayout.Toggle(isSelected, "", GUILayout.Width(20));

                if (newSelected != isSelected)
                {
                    if (newSelected)
                        selectedAssetsToDelete.Add(asset);
                    else
                        selectedAssetsToDelete.Remove(asset);
                }

                GUILayout.Label(asset, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
                    EditorGUIUtility.PingObject(obj); // highlight asset in project window
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    private void FindUnusedAssets()
    {
        unusedAssets.Clear();
        selectedAssetsToDelete.Clear();

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths(); // get all assets in project

        // get enabled scenes in build settings
        string[] scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        // get all assets referenced by the scenes
        string[] sceneDependencies = AssetDatabase.GetDependencies(scenePaths);
        HashSet<string> usedAssets = new HashSet<string>(sceneDependencies);

        foreach (string path in allAssetPaths)
        {
            // skip non-assets or folders
            if (!path.StartsWith("Assets/") || Directory.Exists(path))
                continue;

            // skip scripts if not included
            if (!includeScripts && path.EndsWith(".cs"))
                continue;

            // skip if asset is used in any scene
            if (usedAssets.Contains(path))
                continue;

            // for scripts, check references by scanning files content
            if (path.EndsWith(".cs"))
            {
                if (!ScriptIsReferenced(path, allAssetPaths))
                {
                    unusedAssets.Add(path);
                }
            }
            else
            {
                unusedAssets.Add(path);
            }
        }

        Debug.Log("Found " + unusedAssets.Count + " unused assets.");
    }

    private bool ScriptIsReferenced(string scriptPath, string[] allAssetPaths)
    {
        string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

        // check if script name appears in any other asset file
        foreach (string path in allAssetPaths)
        {
            if (path == scriptPath || path.EndsWith(".cs") || path.EndsWith(".meta"))
                continue;

            try
            {
                string content = File.ReadAllText(path);
                if (content.Contains(scriptName))
                    return true;
            }
            catch
            {
                // ignore files that can't be read
            }
        }

        return false;
    }
}